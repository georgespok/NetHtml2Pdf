namespace NetHtml2Pdf.Renderer;

public class RendererOptions
{
    public string FontPath { get; set; } = DetermineDefaultFontPath();

    public static RendererOptions CreateDefault() => new();

    internal static string DetermineDefaultFontPath()
    {
        var basePath = AppContext.BaseDirectory;
        return Path.Combine(basePath, "Fonts", "Inter-Regular.ttf");
    }
}
