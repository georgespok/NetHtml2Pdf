namespace NetHtml2Pdf.Layout.Engines;

internal sealed class LayoutEngineOptions
{
    public bool EnableNewLayoutForTextBlocks { get; init; }

    public bool EnableDiagnostics { get; init; }

    public static LayoutEngineOptions Disabled { get; } = new LayoutEngineOptions
    {
        EnableNewLayoutForTextBlocks = false,
        EnableDiagnostics = false
    };
}
