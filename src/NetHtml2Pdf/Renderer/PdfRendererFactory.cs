using NetHtml2Pdf.Renderer.Adapters;
using NetHtml2Pdf.Renderer.Interfaces;

namespace NetHtml2Pdf.Renderer;

internal sealed class PdfRendererFactory(IBlockComposer blockComposer, IRendererAdapterFactory? adapterFactory = null)
    : IPdfRendererFactory
{
    private readonly IRendererAdapterFactory _adapterFactory = adapterFactory ?? new RendererAdapterFactory();

    private readonly IBlockComposer _blockComposer =
        blockComposer ?? throw new ArgumentNullException(nameof(blockComposer));

    public IPdfRenderer Create(RendererOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var adapter = _adapterFactory.Create(options);
        return new PdfRenderer(options, _blockComposer, rendererAdapter: adapter);
    }
}