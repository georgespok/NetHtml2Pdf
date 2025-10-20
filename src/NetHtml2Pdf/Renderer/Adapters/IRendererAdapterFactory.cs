using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core;

namespace NetHtml2Pdf.Renderer.Adapters;

internal interface IRendererAdapterFactory
{
    IRendererAdapter Create(RendererOptions options, ILogger? logger = null);
}
