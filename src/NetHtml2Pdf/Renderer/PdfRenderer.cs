using Microsoft.Extensions.Logging.Abstractions;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Display;
using NetHtml2Pdf.Layout.Engines;
using NetHtml2Pdf.Layout.FormattingContexts;
using NetHtml2Pdf.Layout.Model;
using NetHtml2Pdf.Layout.Pagination;
using NetHtml2Pdf.Renderer.Adapters;
using NetHtml2Pdf.Renderer.Inline;
using NetHtml2Pdf.Renderer.Interfaces;
using QuestPDF;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NetHtml2Pdf.Renderer;

internal class PdfRenderer : IPdfRenderer
{
    private static readonly HashSet<DocumentNodeType> LayoutEligibleNodeTypes =
    [
        DocumentNodeType.Paragraph,
        DocumentNodeType.Heading1,
        DocumentNodeType.Heading2,
        DocumentNodeType.Heading3,
        DocumentNodeType.Heading4,
        DocumentNodeType.Heading5,
        DocumentNodeType.Heading6
    ];

    private readonly IBlockComposer _blockComposer;
    private readonly ILayoutEngine _layoutEngine;
    private readonly RendererOptions _options;
    private readonly PaginationService _paginationService;
    private readonly IRendererAdapter _rendererAdapter;

    internal PdfRenderer(
        RendererOptions? options = null,
        IBlockComposer? blockComposer = null,
        PaginationService? paginationService = null,
        IRendererAdapter? rendererAdapter = null,
        ILayoutEngine? layoutEngine = null)
    {
        _options = options ?? RendererOptions.CreateDefault();
        _blockComposer = blockComposer ?? CreateDefaultBlockComposer(_options);
        _paginationService = paginationService ?? new PaginationService();
        _rendererAdapter = rendererAdapter ?? new QuestPdfAdapter();
        _layoutEngine = layoutEngine ?? CreateDefaultLayoutEngine(_options);
    }

    public byte[] Render(DocumentNode document, DocumentNode? header = null, DocumentNode? footer = null)
    {
        return Render([document], header, footer);
    }

    public byte[] Render(IEnumerable<DocumentNode> pages,
        DocumentNode? header = null, DocumentNode? footer = null)
    {
        ArgumentNullException.ThrowIfNull(pages);

        var pageList = pages as IList<DocumentNode> ?? [.. pages];

        if (_options.EnablePagination
            && _options.EnableQuestPdfAdapter
            && TryRenderWithAdapter(pageList, header, footer, out var adapterBytes))
            return adapterBytes;

        return RenderWithLegacyPipeline(pageList, header, footer);
    }

    private bool TryRenderWithAdapter(
        IList<DocumentNode> pages,
        DocumentNode? header,
        DocumentNode? footer,
        out byte[] result)
    {
        result = [];

        if (_layoutEngine is null || !pages.Any()) return false;

        var pageConstraints = CreateDefaultPageConstraints();
        var layoutConstraints = CreateLayoutConstraints(pageConstraints);
        var layoutOptions = LayoutEngineOptions.FromRendererOptions(_options);

        var layoutFragments = new List<LayoutFragment>();
        foreach (var page in pages)
            if (!TryCollectLayoutFragments(page, layoutFragments, layoutConstraints, layoutOptions))
                return false;

        if (layoutFragments.Count == 0) return false;

        if (!TryBuildStaticFragment(header, layoutConstraints, layoutOptions, "Header", out var headerFragment))
            return false;

        if (!TryBuildStaticFragment(footer, layoutConstraints, layoutOptions, "Footer", out var footerFragment))
            return false;

        var paginationOptions = PaginationOptions.FromRendererOptions(_options);
        var paginatedDocument =
            _paginationService.Paginate(layoutFragments, pageConstraints, paginationOptions, NullLogger.Instance);

        var context = new RendererContext(_options, NullLogger.Instance, headerFragment, footerFragment);
        _rendererAdapter.BeginDocument(paginatedDocument, context);
        foreach (var page in paginatedDocument.Pages) _rendererAdapter.Render(page, context);

        result = _rendererAdapter.EndDocument(context);
        return true;
    }

    private byte[] RenderWithLegacyPipeline(IEnumerable<DocumentNode> pages, DocumentNode? header, DocumentNode? footer)
    {
        ConfigureQuestPdf();
        var pdfDocument = CreateMultiPageDocument(pages, header, footer);

        using var stream = new MemoryStream();
        pdfDocument.GeneratePdf(stream);
        return stream.ToArray();
    }

    internal static IBlockComposer CreateDefaultBlockComposer(RendererOptions? options = null)
    {
        options ??= RendererOptions.CreateDefault();

        var spacingApplier = new BlockSpacingApplier();
        var inlineFlowLayoutEngine = new InlineFlowLayoutEngine();
        var displayClassifier = new DisplayClassifier(options: options);
        var inlineComposer = new InlineComposer(displayClassifier, inlineFlowLayoutEngine);
        var listComposer = new ListComposer(inlineComposer, spacingApplier);
        var tableComposer = new TableComposer(inlineComposer, spacingApplier);

        var layoutEngineOptions = LayoutEngineOptions.FromRendererOptions(options);
        var formattingContextFactory =
            FormattingContextFactory.CreateDefault(layoutEngineOptions, inlineFlowLayoutEngine);
        var layoutEngine = new LayoutEngine(
            displayClassifier,
            formattingContextFactory.GetBlockFormattingContext(),
            formattingContextFactory.GetInlineFormattingContext(),
            formattingContextFactory.GetInlineBlockFormattingContext(),
            formattingContextFactory.GetTableFormattingContext(),
            formattingContextFactory.GetFlexFormattingContext(),
            formattingContextFactory.Options);

        return new BlockComposer(
            inlineComposer,
            listComposer,
            tableComposer,
            spacingApplier,
            options,
            displayClassifier,
            layoutEngine);
    }

    private void ConfigureQuestPdf()
    {
        Settings.License = LicenseType.Community;
        var useEnvFonts = string.IsNullOrWhiteSpace(_options.FontPath) || !File.Exists(_options.FontPath);
        Settings.UseEnvironmentFonts = useEnvFonts;

        if (!useEnvFonts)
        {
            using var fontStream = File.OpenRead(_options.FontPath);
            FontManager.RegisterFont(fontStream);
        }
    }

    private IDocument CreateMultiPageDocument(IEnumerable<DocumentNode> pages, DocumentNode? header,
        DocumentNode? footer)
    {
        return Document.Create(container =>
        {
            foreach (var pageDocument in pages)
                container.Page(page =>
                {
                    page.Margin(40);

                    // Add header if provided
                    if (header != null)
                        page.Header().Column(column =>
                        {
                            foreach (var child in header.Children)
                            {
                                if (IsDisplayNone(child))
                                    continue;

                                _blockComposer.Compose(column, child);
                            }
                        });

                    // Add footer if provided
                    if (footer != null)
                        page.Footer().Column(column =>
                        {
                            foreach (var child in footer.Children)
                            {
                                if (IsDisplayNone(child))
                                    continue;

                                _blockComposer.Compose(column, child);
                            }
                        });

                    // Add page content
                    page.Content().Column(column =>
                    {
                        foreach (var child in pageDocument.Children)
                        {
                            if (IsDisplayNone(child))
                                continue;

                            _blockComposer.Compose(column, child);
                        }
                    });
                });
        });
    }

    private static bool IsDisplayNone(DocumentNode node)
    {
        return node.Styles.DisplaySet && node.Styles.Display == CssDisplay.None;
    }

    private bool TryCollectLayoutFragments(
        DocumentNode node,
        ICollection<LayoutFragment> fragments,
        LayoutConstraints constraints,
        LayoutEngineOptions layoutOptions)
    {
        if (LayoutEligibleNodeTypes.Contains(node.NodeType))
        {
            var result = _layoutEngine.Layout(node, constraints, layoutOptions);
            if (!result.IsSuccess) return false;

            foreach (var fragment in result.Fragments)
            {
                fragments.Add(fragment);

                // When inline-block context is enabled, also surface nested inline-block fragments
                // so adapter-side pagination can reason about them directly (contract test expectation).
                if (_options.EnableInlineBlockContext)
                    foreach (var inlineBlock in ExtractInlineBlockFragments(fragment))
                        fragments.Add(CreateProbeFragment(inlineBlock));
            }

            return true;
        }

        if (node.Children.Count == 0) return true;

        foreach (var child in node.Children)
            if (!TryCollectLayoutFragments(child, fragments, constraints, layoutOptions))
                return false;

        return true;
    }

    private bool TryBuildStaticFragment(
        DocumentNode? node,
        LayoutConstraints constraints,
        LayoutEngineOptions layoutOptions,
        string nodePath,
        out LayoutFragment? fragment)
    {
        fragment = null;
        if (node is null) return true;

        var fragments = new List<LayoutFragment>();
        if (!TryCollectLayoutFragments(node, fragments, constraints, layoutOptions)) return false;

        if (fragments.Count == 0) return true;

        fragment = CombineFragments(node, fragments, constraints, nodePath);
        return true;
    }

    private static LayoutFragment CombineFragments(
        DocumentNode sourceNode,
        IReadOnlyList<LayoutFragment> fragments,
        LayoutConstraints constraints,
        string nodePath)
    {
        var width = fragments.Count == 0 ? constraints.InlineMax : fragments.Max(fragment => fragment.Width);
        var height = fragments.Sum(fragment => fragment.Height);
        var spacing = LayoutSpacing.FromStyles(sourceNode.Styles);
        var box = new LayoutBox(
            sourceNode,
            DisplayClass.Block,
            sourceNode.Styles,
            spacing,
            nodePath,
            fragments.Select(fragment => fragment.Box).ToArray());

        var diagnostics = new LayoutDiagnostics(
            "CompositeFragment",
            constraints,
            width,
            height);

        return LayoutFragment.CreateBlock(box, width, height, fragments, diagnostics);
    }

    private static IEnumerable<LayoutFragment> ExtractInlineBlockFragments(LayoutFragment root)
    {
        var stack = new Stack<LayoutFragment>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            if (current.Diagnostics.ContextName == "InlineBlockFormattingContext") yield return current;

            foreach (var child in current.Children) stack.Push(child);
        }
    }

    private static LayoutFragment CreateProbeFragment(LayoutFragment source)
    {
        // Create a zero-height, zero-width probe so pagination/adapter can inspect diagnostics
        // without producing duplicate visible text content.
        var box = new LayoutBox(
            source.Node,
            source.Display,
            source.Box.Style,
            source.Box.Spacing,
            source.NodePath + "/probe",
            source.Box.Children);

        var diagnostics = new LayoutDiagnostics(
            source.Diagnostics.ContextName,
            source.Diagnostics.AppliedConstraints,
            0,
            0,
            source.Diagnostics.Metadata);

        return new LayoutFragment(
            source.Kind,
            box,
            0,
            0,
            null,
            [],
            diagnostics);
    }

    internal static ILayoutEngine CreateDefaultLayoutEngine(RendererOptions? options = null)
    {
        options ??= RendererOptions.CreateDefault();

        var layoutOptions = LayoutEngineOptions.FromRendererOptions(options);
        var formattingContextFactory = FormattingContextFactory.CreateDefault(layoutOptions);
        var displayClassifier = new DisplayClassifier(options: options);

        return new LayoutEngine(
            displayClassifier,
            formattingContextFactory.GetBlockFormattingContext(),
            formattingContextFactory.GetInlineFormattingContext(),
            formattingContextFactory.GetInlineBlockFormattingContext(),
            formattingContextFactory.GetTableFormattingContext(),
            formattingContextFactory.GetFlexFormattingContext(),
            formattingContextFactory.Options);
    }

    private static PageConstraints CreateDefaultPageConstraints()
    {
        return new PageConstraints(595f, 842f, BoxSpacing.Empty, 0f, 0f);
    }

    private static LayoutConstraints CreateLayoutConstraints(PageConstraints pageConstraints)
    {
        return new LayoutConstraints(
            0f,
            pageConstraints.ContentWidth,
            0f,
            pageConstraints.ContentHeight,
            pageConstraints.ContentHeight,
            true);
    }
}