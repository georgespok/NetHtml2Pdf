using NetHtml2Pdf.Layout.Pagination;

namespace NetHtml2Pdf.Renderer.Adapters;

internal interface IRendererAdapter
{
    void BeginDocument(PaginatedDocument document, RendererContext context);

    void Render(PageFragmentTree page, RendererContext context);

    byte[] EndDocument(RendererContext context);
}