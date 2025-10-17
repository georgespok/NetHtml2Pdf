namespace NetHtml2Pdf.Renderer;

public class RendererOptions
{
    public string FontPath { get; set; } = DetermineDefaultFontPath();
    
    /// <summary>
    /// Enables trace logging for DisplayClassifier operations.
    /// When enabled, detailed classification decisions are logged.
    /// Default: false (disabled)
    /// </summary>
    public bool EnableClassifierTraceLogging { get; set; } = false;

    /// <summary>
    /// Enables the Phase 2 layout pipeline for paragraphs/headings/spans.
    /// </summary>
    public bool EnableNewLayoutForTextBlocks { get; set; } = false;

    /// <summary>
    /// Enables structured layout diagnostics (fragment logging).
    /// </summary>
    public bool EnableLayoutDiagnostics { get; set; } = false;

    public static RendererOptions CreateDefault() => new();

    internal static string DetermineDefaultFontPath()
    {
        var basePath = AppContext.BaseDirectory;
        return Path.Combine(basePath, "Fonts", "Inter-Regular.ttf");
    }
}
