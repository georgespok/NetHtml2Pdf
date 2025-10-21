using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Layout.Model;

namespace NetHtml2Pdf.Renderer.Adapters;

internal sealed class RendererContext(
    RendererOptions rendererOptions,
    ILogger? logger,
    LayoutFragment? header,
    LayoutFragment? footer)
{
    public RendererOptions RendererOptions { get; } =
        rendererOptions ?? throw new ArgumentNullException(nameof(rendererOptions));

    public ILogger? Logger { get; } = logger;

    public LayoutFragment? Header { get; } = header;

    public LayoutFragment? Footer { get; } = footer;
}