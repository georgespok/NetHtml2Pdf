using Microsoft.Extensions.Logging;

namespace NetHtml2Pdf.Renderer.Adapters;

internal interface IRendererAdapterFactory
{
    IRendererAdapter Create(RendererOptions options, ILogger? logger = null);
}