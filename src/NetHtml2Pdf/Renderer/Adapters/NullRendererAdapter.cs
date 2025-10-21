using NetHtml2Pdf.Layout.Pagination;

namespace NetHtml2Pdf.Renderer.Adapters;

/// <summary>
///     Placeholder adapter used when the adapter pipeline is disabled.
/// </summary>
internal sealed class NullRendererAdapter : IRendererAdapter
{
    private PaginatedDocument? _document;

    public void BeginDocument(PaginatedDocument document, RendererContext context)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
    }

    public void Render(PageFragmentTree page, RendererContext context)
    {
        // Intentionally no-op. Acts as a sink when adapter pipeline is disabled.
        ArgumentNullException.ThrowIfNull(page);
    }

    public byte[] EndDocument(RendererContext context)
    {
        _document = null;
        return [];
    }
}