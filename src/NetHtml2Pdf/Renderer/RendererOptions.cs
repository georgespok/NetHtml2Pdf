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

    public static RendererOptions CreateDefault() => new();

    internal static string DetermineDefaultFontPath()
    {
        var basePath = AppContext.BaseDirectory;
        return Path.Combine(basePath, "Fonts", "Inter-Regular.ttf");
    }
}
