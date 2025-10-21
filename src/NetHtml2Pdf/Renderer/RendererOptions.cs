namespace NetHtml2Pdf.Renderer;

public class RendererOptions
{
    public string FontPath { get; set; } = DetermineDefaultFontPath();

    /// <summary>
    ///     Enables trace logging for DisplayClassifier operations.
    ///     When enabled, detailed classification decisions are logged.
    ///     Default: false (disabled)
    /// </summary>
    public bool EnableClassifierTraceLogging { get; set; } = false;

    /// <summary>
    ///     Enables the Phase 2 layout pipeline for paragraphs/headings/spans.
    /// </summary>
    public bool EnableNewLayoutForTextBlocks { get; set; } = false;

    /// <summary>
    ///     Enables structured layout diagnostics (fragment logging).
    /// </summary>
    public bool EnableLayoutDiagnostics { get; set; } = false;

    /// <summary>
    ///     Enables the pagination pass that slices layout fragments into page-scoped trees.
    /// </summary>
    public bool EnablePagination { get; set; } = false;

    /// <summary>
    ///     Routes rendering through the QuestPDF adapter instead of legacy composers (requires pagination).
    /// </summary>
    public bool EnableQuestPdfAdapter { get; set; } = false;

    /// <summary>
    ///     Enables detailed pagination diagnostics (per-page and fragment logging).
    /// </summary>
    public bool EnablePaginationDiagnostics { get; set; } = false;

    /// <summary>
    ///     Enables the inline-block formatting context introduced in Phase 4.
    /// </summary>
    public bool EnableInlineBlockContext { get; set; } = false;

    /// <summary>
    ///     Enables the table formatting context introduced in Phase 4.
    /// </summary>
    public bool EnableTableContext { get; set; } = false;

    /// <summary>
    ///     Enables advanced table border-collapse handling. Requires table context.
    /// </summary>
    public bool EnableTableBorderCollapse { get; set; } = false;

    /// <summary>
    ///     Enables the preview flex formatting context (guarded release).
    /// </summary>
    public bool EnableFlexContext { get; set; } = false;

    public static RendererOptions CreateDefault()
    {
        return new RendererOptions();
    }

    internal static string DetermineDefaultFontPath()
    {
        var basePath = AppContext.BaseDirectory;
        return Path.Combine(basePath, "Fonts", "Inter-Regular.ttf");
    }
}