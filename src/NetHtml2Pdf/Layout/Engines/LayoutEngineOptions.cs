using NetHtml2Pdf.Renderer;

namespace NetHtml2Pdf.Layout.Engines;

internal sealed class LayoutEngineOptions
{
    public bool EnableNewLayoutForTextBlocks { get; init; }

    public bool EnableDiagnostics { get; init; }

    public bool EnableInlineBlockContext { get; init; }

    public bool EnableTableContext { get; init; }

    public bool EnableTableBorderCollapse { get; init; }

    public bool EnableFlexContext { get; init; }

    public static LayoutEngineOptions Disabled { get; } = new()
    {
        EnableNewLayoutForTextBlocks = false,
        EnableDiagnostics = false,
        EnableInlineBlockContext = false,
        EnableTableContext = false,
        EnableTableBorderCollapse = false,
        EnableFlexContext = false
    };

    public static LayoutEngineOptions FromRendererOptions(RendererOptions rendererOptions)
    {
        ArgumentNullException.ThrowIfNull(rendererOptions);

        return new LayoutEngineOptions
        {
            EnableNewLayoutForTextBlocks = rendererOptions.EnableNewLayoutForTextBlocks,
            EnableDiagnostics = rendererOptions.EnableLayoutDiagnostics,
            EnableInlineBlockContext = rendererOptions.EnableInlineBlockContext,
            EnableTableContext = rendererOptions.EnableTableContext,
            EnableTableBorderCollapse = rendererOptions.EnableTableBorderCollapse,
            EnableFlexContext = rendererOptions.EnableFlexContext
        };
    }
}