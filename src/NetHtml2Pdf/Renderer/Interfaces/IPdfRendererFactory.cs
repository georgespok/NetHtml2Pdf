namespace NetHtml2Pdf.Renderer.Interfaces;

internal interface IPdfRendererFactory
{
    IPdfRenderer Create(RendererOptions options);
}