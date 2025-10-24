using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetHtml2Pdf.Layout.Display;
using NetHtml2Pdf.Layout.Engines;
using NetHtml2Pdf.Layout.FormattingContexts;
using NetHtml2Pdf.Layout.Pagination;
using NetHtml2Pdf.Parser;
using NetHtml2Pdf.Parser.Interfaces;
using NetHtml2Pdf.Renderer.Adapters;
using NetHtml2Pdf.Renderer.Interfaces;
using NetHtml2Pdf.Renderer.Inline;

namespace NetHtml2Pdf.Renderer;

/// <summary>
/// Internal static factory responsible for constructing the complete rendering pipeline.
/// This is the single composition root that owns all default creation logic and wires
/// the entire dependency graph.
/// </summary>
internal static class RendererComposition
{
    /// <summary>
    /// Creates a fully wired PDF renderer with the complete pipeline.
    /// </summary>
    /// <param name="options">Configuration options for the renderer</param>
    /// <param name="services">Optional service overrides (primarily for testing)</param>
    /// <returns>A configured PDF renderer instance</returns>
    public static IPdfRenderer CreateRenderer(RendererOptions options, RendererServices? services = null)
    {
        // Create all pipeline components with explicit dependencies
        var displayClassifier = services?.DisplayClassifier ?? CreateDisplayClassifier();
        var inlineFlowLayoutEngine = services?.InlineFlowLayoutEngine ?? CreateInlineFlowLayoutEngine();
        var formattingContextFactory = services?.FormattingContextFactory ?? CreateFormattingContextFactory(options);
        var layoutEngine = services?.LayoutEngine ?? CreateLayoutEngine(formattingContextFactory, displayClassifier);
        var paginationService = services?.PaginationService ?? CreatePaginationService();
        var rendererAdapter = services?.RendererAdapter ?? CreateRendererAdapter();
        var blockComposer = services?.BlockComposer ?? CreateBlockComposer();
        var logger = services?.Logger ?? CreateLogger();

        // Create the PDF renderer with all dependencies
        return new PdfRenderer(
            options,
            blockComposer,
            paginationService,
            rendererAdapter,
            layoutEngine);
    }

    /// <summary>
    /// Creates the internal dependencies that PdfBuilder needs.
    /// </summary>
    /// <param name="options">Configuration options</param>
    /// <param name="services">Optional service overrides</param>
    /// <returns>Tuple containing (IHtmlParser, IPdfRendererFactory)</returns>
    public static (IHtmlParser parser, IPdfRendererFactory rendererFactory) CreatePdfBuilderDependencies(
        RendererOptions options,
        RendererServices? services = null,
        ILogger? logger = null)
    {
        var parser = CreateHtmlParser(logger);
        var rendererFactory = CreatePdfRendererFactory(options, services, logger);
        
        return (parser, rendererFactory);
    }

    #region Default Service Creation

    private static DisplayClassifier CreateDisplayClassifier()
    {
        return new DisplayClassifier();
    }

    private static InlineFlowLayoutEngine CreateInlineFlowLayoutEngine()
    {
        return new InlineFlowLayoutEngine();
    }

    private static FormattingContextFactory CreateFormattingContextFactory(RendererOptions options)
    {
        var layoutOptions = LayoutEngineOptions.FromRendererOptions(options);
        return FormattingContextFactory.CreateDefault(layoutOptions);
    }

    private static LayoutEngine CreateLayoutEngine(FormattingContextFactory formattingContextFactory, DisplayClassifier displayClassifier)
    {
        return new LayoutEngine(
            displayClassifier,
            formattingContextFactory.GetBlockFormattingContext(),
            formattingContextFactory.GetInlineFormattingContext(),
            formattingContextFactory.GetInlineBlockFormattingContext(),
            formattingContextFactory.GetTableFormattingContext(),
            formattingContextFactory.GetFlexFormattingContext(),
            formattingContextFactory.Options);
    }

    private static PaginationService CreatePaginationService()
    {
        return new PaginationService();
    }

    private static IRendererAdapter CreateRendererAdapter()
    {
        return new QuestPdfAdapter();
    }

    private static BlockComposer CreateBlockComposer()
    {
        // Create the required dependencies for BlockComposer
        var inlineComposer = new InlineComposer();
        var spacingApplier = new BlockSpacingApplier();
        var listComposer = new ListComposer(inlineComposer, spacingApplier);
        var tableComposer = new TableComposer(inlineComposer, spacingApplier);
        
        return new BlockComposer(inlineComposer, listComposer, tableComposer, spacingApplier);
    }

    private static ILogger CreateLogger()
    {
        // Create a no-op logger by default
        return NullLogger.Instance;
    }

    private static IHtmlParser CreateHtmlParser(ILogger? logger = null)
    {
        var angleSharpParser = new AngleSharp.Html.Parser.HtmlParser();
        var declarationParser = new CssDeclarationParser();
        var declarationUpdater = new CssStyleUpdater();
        var classStyleExtractor = new CssClassStyleExtractor(declarationParser, declarationUpdater);
        
        // Create a fallback element handler that logs warnings
        Action<string>? onFallbackElement = logger != null 
            ? (tagName) => logger.LogWarning("Unsupported HTML element encountered: {TagName}", tagName)
            : null;
            
        return new HtmlParser(angleSharpParser, classStyleExtractor, onFallbackElement);
    }

    private static IPdfRendererFactory CreatePdfRendererFactory(RendererOptions options, RendererServices? services, ILogger? logger = null)
    {
        var blockComposer = CreateBlockComposer();
        return new PdfRendererFactory(blockComposer);
    }

    #endregion
}

