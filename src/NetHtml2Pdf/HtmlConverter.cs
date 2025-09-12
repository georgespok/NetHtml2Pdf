using QuestPDF.Infrastructure;
using NetHtml2Pdf.Parsing;
using NetHtml2Pdf.Rendering;

namespace NetHtml2Pdf
{
    /// <summary>
    /// Main converter class for converting HTML to PDF using the three-layer architecture
    /// </summary>
    public class HtmlConverter
    {
        private readonly HtmlParser _htmlParser;
        private readonly PdfRenderer _pdfRenderer;
        private readonly string _fontPath;

        /// <summary>
        /// Initializes a new instance of the HtmlConverter class
        /// </summary>
        public HtmlConverter()
        {
            _htmlParser = new HtmlParser();
            _pdfRenderer = new PdfRenderer();
            _fontPath = Path.Combine(AppContext.BaseDirectory, "fonts", "Inter-Regular.ttf");
        }

        /// <summary>
        /// Converts HTML content to PDF bytes using the three-layer architecture:
        /// HTML → Document Model → QuestPDF
        /// </summary>
        /// <param name="html">The HTML content to convert</param>
        /// <returns>PDF bytes</returns>
        public async Task<byte[]> ConvertToPdfBytes(string html)
        {
            if (string.IsNullOrEmpty(html))
                throw new ArgumentException("HTML content cannot be null or empty", nameof(html));

            SetupQuestPdf();

            // Layer 1: Parse HTML into intermediate document model
            var documentNodes = await _htmlParser.ParseAsync(html);

            // Layer 2: Convert document model to QuestPDF elements
            return _pdfRenderer.RenderToPdf(documentNodes);
        }

        /// <summary>
        /// Sets up QuestPDF configuration
        /// </summary>
        private void SetupQuestPdf()
        {
            if (!File.Exists(_fontPath))
                throw new ApplicationException($"Missing font at {_fontPath}. Ensure it's copied to output.");

            QuestPDF.Settings.UseEnvironmentFonts = false;
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Drawing.FontManager.RegisterFont(File.OpenRead(_fontPath));
        }
    }
}
