using NetHtml2Pdf.Core;

namespace NetHtml2Pdf.Renderer.Interfaces;

internal interface IPdfRenderer
{
    byte[] Render(DocumentNode document, DocumentNode? header = null, DocumentNode? footer = null);
    byte[] Render(IEnumerable<DocumentNode> pages, DocumentNode? header = null, DocumentNode? footer = null);
}
