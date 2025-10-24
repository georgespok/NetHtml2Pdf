using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Layout.Display;
using NetHtml2Pdf.Layout.FormattingContexts;
using NetHtml2Pdf.Layout.Engines;
using NetHtml2Pdf.Layout.Pagination;
using NetHtml2Pdf.Renderer.Adapters;
using NetHtml2Pdf.Renderer.Inline;

namespace NetHtml2Pdf.Renderer;

/// <summary>
/// Internal container for selective service overrides (primarily for testing).
/// Allows tests to override specific pipeline components without affecting others.
/// </summary>
internal class RendererServices
{
    public PaginationService? PaginationService { get; init; }
    public ILogger? Logger { get; init; }
    public DisplayClassifier? DisplayClassifier { get; init; }
    public InlineFlowLayoutEngine? InlineFlowLayoutEngine { get; init; }
    public FormattingContextFactory? FormattingContextFactory { get; init; }
    public LayoutEngine? LayoutEngine { get; init; }
    public IRendererAdapter? RendererAdapter { get; init; }
    public BlockComposer? BlockComposer { get; init; }

    /// <summary>
    /// Creates a RendererServices instance for testing with no overrides.
    /// </summary>
    public static RendererServices ForTests() => new();

    /// <summary>
    /// Creates a new RendererServices instance with the specified overrides.
    /// Any parameter that is null will use the default service from the composition root.
    /// </summary>
    /// <param name="pagination">Override for PaginationService</param>
    /// <param name="logger">Override for Logger</param>
    /// <param name="displayClassifier">Override for DisplayClassifier</param>
    /// <param name="inlineFlowLayoutEngine">Override for InlineFlowLayoutEngine</param>
    /// <param name="formattingContextFactory">Override for FormattingContextFactory</param>
    /// <param name="layoutEngine">Override for LayoutEngine</param>
    /// <param name="rendererAdapter">Override for RendererAdapter</param>
    /// <param name="blockComposer">Override for BlockComposer</param>
    /// <returns>New RendererServices instance with specified overrides</returns>
    public RendererServices With(
        PaginationService? pagination = null,
        ILogger? logger = null,
        DisplayClassifier? displayClassifier = null,
        InlineFlowLayoutEngine? inlineFlowLayoutEngine = null,
        FormattingContextFactory? formattingContextFactory = null,
        LayoutEngine? layoutEngine = null,
        IRendererAdapter? rendererAdapter = null,
        BlockComposer? blockComposer = null)
    {
        return new RendererServices
        {
            PaginationService = pagination ?? PaginationService,
            Logger = logger ?? Logger,
            DisplayClassifier = displayClassifier ?? DisplayClassifier,
            InlineFlowLayoutEngine = inlineFlowLayoutEngine ?? InlineFlowLayoutEngine,
            FormattingContextFactory = formattingContextFactory ?? FormattingContextFactory,
            LayoutEngine = layoutEngine ?? LayoutEngine,
            RendererAdapter = rendererAdapter ?? RendererAdapter,
            BlockComposer = blockComposer ?? BlockComposer
        };
    }

    private RendererServices()
    {
        // Private constructor for ForTests()
    }
}
