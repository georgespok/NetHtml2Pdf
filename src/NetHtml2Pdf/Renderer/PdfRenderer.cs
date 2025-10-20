using System;
using System.Collections.Generic;
using System.Linq;
using NetHtml2Pdf.Core;
using Microsoft.Extensions.Logging.Abstractions;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Contexts;
using NetHtml2Pdf.Layout.Display;
using NetHtml2Pdf.Layout.Engines;
using NetHtml2Pdf.Layout.Model;
using NetHtml2Pdf.Layout.Pagination;
using NetHtml2Pdf.Renderer.Adapters;
using NetHtml2Pdf.Renderer.Inline;
using NetHtml2Pdf.Renderer.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NetHtml2Pdf.Renderer;

internal class PdfRenderer : IPdfRenderer
{
    private readonly RendererOptions _options;
    private readonly IBlockComposer _blockComposer;
    private readonly PaginationService _paginationService;
    private readonly IRendererAdapter _rendererAdapter;
    private readonly ILayoutEngine _layoutEngine;

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

    public byte[] Render(DocumentNode document, DocumentNode? header = null, DocumentNode? footer = null) =>
        Render([document], header, footer);

    public byte[] Render(IEnumerable<DocumentNode> pages,
        DocumentNode? header = null, DocumentNode? footer = null)
    {
        ArgumentNullException.ThrowIfNull(pages);

        var pageList = pages as IList<DocumentNode> ?? pages.ToList();

        if (_options.EnablePagination
            && _options.EnableQuestPdfAdapter
            && TryRenderWithAdapter(pageList, header, footer, out var adapterBytes))
        {
            return adapterBytes;
        }

        return RenderWithLegacyPipeline(pageList, header, footer);
    }

    private bool TryRenderWithAdapter(
        IList<DocumentNode> pages,
        DocumentNode? header,
        DocumentNode? footer,
        out byte[] result)
    {
        result = [];

        if (_layoutEngine is null || !pages.Any())
        {
            return false;
        }

        var pageConstraints = CreateDefaultPageConstraints();
        var layoutConstraints = CreateLayoutConstraints(pageConstraints);
        var layoutOptions = new LayoutEngineOptions
        {
            EnableNewLayoutForTextBlocks = _options.EnableNewLayoutForTextBlocks,
            EnableDiagnostics = _options.EnableLayoutDiagnostics
        };

        var layoutFragments = new List<LayoutFragment>();
        foreach (var page in pages)
        {
            if (!TryCollectLayoutFragments(page, layoutFragments, layoutConstraints, layoutOptions))
            {
                return false;
            }
        }

        if (layoutFragments.Count == 0)
        {
            return false;
        }

        if (!TryBuildStaticFragment(header, layoutConstraints, layoutOptions, "Header", out var headerFragment))
        {
            return false;
        }

        if (!TryBuildStaticFragment(footer, layoutConstraints, layoutOptions, "Footer", out var footerFragment))
        {
            return false;
        }

        var paginationOptions = PaginationOptions.FromRendererOptions(_options);
        var paginatedDocument = _paginationService.Paginate(layoutFragments, pageConstraints, paginationOptions, NullLogger.Instance);

        var context = new RendererContext(_options, NullLogger.Instance, headerFragment, footerFragment);
        _rendererAdapter.BeginDocument(paginatedDocument, context);
        foreach (var page in paginatedDocument.Pages)
        {
            _rendererAdapter.Render(page, context);
        }

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

        var inlineFormattingContext = new InlineFormattingContext(inlineFlowLayoutEngine);
        var blockFormattingContext = new BlockFormattingContext(inlineFormattingContext);
        var layoutEngine = new LayoutEngine(displayClassifier, blockFormattingContext, inlineFormattingContext);

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
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.UseEnvironmentFonts = false;

        using var fontStream = File.OpenRead(_options.FontPath);
        QuestPDF.Drawing.FontManager.RegisterFont(fontStream);
    }

    private IDocument CreateMultiPageDocument(IEnumerable<DocumentNode> pages, DocumentNode? header, DocumentNode? footer)
    {
        return Document.Create(container =>
        {
            foreach (var pageDocument in pages)
            {
                container.Page(page =>
                {
                    page.Margin(40);

                    // Add header if provided
                    if (header != null)
                    {
                        page.Header().Column(column =>
                        {
                            foreach (var child in header.Children)
                            {
                                if (IsDisplayNone(child))
                                    continue;

                                _blockComposer.Compose(column, child);
                            }
                        });
                    }

                    // Add footer if provided
                    if (footer != null)
                    {
                        page.Footer().Column(column =>
                        {
                            foreach (var child in footer.Children)
                            {
                                if (IsDisplayNone(child))
                                    continue;

                                _blockComposer.Compose(column, child);
                            }
                        });
                    }

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
            }
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
            if (!result.IsSuccess)
            {
                return false;
            }

            foreach (var fragment in result.Fragments)
            {
                fragments.Add(fragment);
            }

            return true;
        }

        if (node.Children.Count == 0)
        {
            return true;
        }

        foreach (var child in node.Children)
        {
            if (!TryCollectLayoutFragments(child, fragments, constraints, layoutOptions))
            {
                return false;
            }
        }

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
        if (node is null)
        {
            return true;
        }

        var fragments = new List<LayoutFragment>();
        if (!TryCollectLayoutFragments(node, fragments, constraints, layoutOptions))
        {
            return false;
        }

        if (fragments.Count == 0)
        {
            return true;
        }

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

    internal static ILayoutEngine CreateDefaultLayoutEngine(RendererOptions? options = null)
    {
        options ??= RendererOptions.CreateDefault();

        var inlineFlowLayoutEngine = new InlineFlowLayoutEngine();
        var inlineFormattingContext = new InlineFormattingContext(inlineFlowLayoutEngine);
        var blockFormattingContext = new BlockFormattingContext(inlineFormattingContext);
        var displayClassifier = new DisplayClassifier(options: options);

        return new LayoutEngine(displayClassifier, blockFormattingContext, inlineFormattingContext);
    }

    private static PageConstraints CreateDefaultPageConstraints()
    {
        return new PageConstraints(595f, 842f, BoxSpacing.Empty, 0f, 0f);
    }

    private static LayoutConstraints CreateLayoutConstraints(PageConstraints pageConstraints)
    {
        return new LayoutConstraints(
            inlineMin: 0f,
            inlineMax: pageConstraints.ContentWidth,
            blockMin: 0f,
            blockMax: pageConstraints.ContentHeight,
            pageRemainingBlockSize: pageConstraints.ContentHeight,
            allowBreaks: true);
    }
}
