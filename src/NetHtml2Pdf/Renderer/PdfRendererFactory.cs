using NetHtml2Pdf.Renderer.Adapters;
using NetHtml2Pdf.Renderer.Interfaces;

namespace NetHtml2Pdf.Renderer;

internal sealed class PdfRendererFactory : IPdfRendererFactory
{
    private readonly IBlockComposer _blockComposer;
    private readonly IRendererAdapterFactory _adapterFactory;

    public PdfRendererFactory(IBlockComposer blockComposer, IRendererAdapterFactory? adapterFactory = null)
    {
        _blockComposer = blockComposer ?? throw new ArgumentNullException(nameof(blockComposer));
        _adapterFactory = adapterFactory ?? new RendererAdapterFactory();
    }

    public IPdfRenderer Create(RendererOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var adapter = _adapterFactory.Create(options);
        return new PdfRenderer(options, _blockComposer, rendererAdapter: adapter);
    }
}
