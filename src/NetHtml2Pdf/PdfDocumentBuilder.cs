using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NetHtml2Pdf.Parser;
using NetHtml2Pdf.Renderer;

namespace NetHtml2Pdf
{
    /// <summary>
    /// Builder for creating multi-page PDFs from multiple HTML fragments using explicit pagination.
    /// </summary>
    public class PdfDocumentBuilder
    {
        private readonly HtmlParser _htmlParser;
        private readonly PdfRenderer _pdfRenderer;
        private readonly List<string> _htmlPages = new();
        private readonly string _fontPath;
        private string? _headerHtml;
        private string? _footerHtml;

        public PdfDocumentBuilder()
        {
            _htmlParser = new HtmlParser();
            _pdfRenderer = new PdfRenderer();
            _fontPath = Path.Combine(AppContext.BaseDirectory, "Fonts", "Inter-Regular.ttf");
        }

        /// <summary>
        /// Adds a page represented by an HTML fragment. Pages are rendered in the order added.
        /// </summary>
        public void AddPdfPage(string html)
        {
            if (string.IsNullOrEmpty(html))
                throw new ArgumentException("HTML content cannot be null or empty", nameof(html));
            _htmlPages.Add(html);
        }

        /// <summary>
        /// Sets a global header HTML fragment rendered on each page.
        /// </summary>
        public void SetHeaderHtml(string html) => _headerHtml = html;

        /// <summary>
        /// Sets a global footer HTML fragment rendered on each page.
        /// </summary>
        public void SetFooterHtml(string html) => _footerHtml = html;

        /// <summary>
        /// Renders all added pages into a single PDF document and returns the bytes.
        /// </summary>
        public async Task<byte[]> RenderAsync()
        {
            if (_htmlPages.Count == 0)
                throw new InvalidOperationException("No pages have been added. Call AddPdfPage(html) before rendering.");

            _pdfRenderer.Setup(_fontPath);

            var pageNodes = new List<List<Core.Models.DocumentNode>>(_htmlPages.Count);
            foreach (var html in _htmlPages)
            {
                var nodes = await _htmlParser.ParseAsync(html);
                pageNodes.Add(nodes);
            }

            var headerNodes = _headerHtml != null ? await _htmlParser.ParseAsync(_headerHtml) : null;
            var footerNodes = _footerHtml != null ? await _htmlParser.ParseAsync(_footerHtml) : null;

            return _pdfRenderer.RenderPagesToPdf(pageNodes, headerNodes, footerNodes);
        }
    }
}
