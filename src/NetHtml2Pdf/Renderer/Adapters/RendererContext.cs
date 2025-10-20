using System;
using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Layout.Model;

namespace NetHtml2Pdf.Renderer.Adapters;

internal sealed class RendererContext
{
    public RendererContext(
        RendererOptions rendererOptions,
        ILogger? logger,
        LayoutFragment? header,
        LayoutFragment? footer)
    {
        RendererOptions = rendererOptions ?? throw new ArgumentNullException(nameof(rendererOptions));
        Logger = logger;
        Header = header;
        Footer = footer;
    }

    public RendererOptions RendererOptions { get; }

    public ILogger? Logger { get; }

    public LayoutFragment? Header { get; }

    public LayoutFragment? Footer { get; }
}
