using NetHtml2Pdf.Renderer.Interfaces;

namespace NetHtml2Pdf.Renderer;

internal sealed class PdfRendererFactory(IBlockComposer blockComposer) : IPdfRendererFactory
{
    private readonly IBlockComposer _blockComposer = blockComposer ?? throw new ArgumentNullException(nameof(blockComposer));

    public IPdfRenderer Create(RendererOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return new PdfRenderer(options, _blockComposer);
    }
}
