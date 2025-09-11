using AngleSharp;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using NetHtml2Pdf.Interfaces;
using NetHtml2Pdf.Factories;
using Document = QuestPDF.Fluent.Document;
using HtmlElement = AngleSharp.Dom.IElement;

namespace NetHtml2Pdf
{
    /// <summary>
    /// Main converter class for converting HTML to PDF 
    /// </summary>
    public class HtmlConverter
    {
        private readonly IElementConverterFactory _converterFactory;
        private readonly string _fontPath;

        /// <summary>
        /// Initializes a new instance of the HtmlConverter class with default factory
        /// </summary>
        public HtmlConverter() : this(new ElementConverterFactory())
        {
        }

        /// <summary>
        /// Initializes a new instance of the HtmlConverter class with custom factory
        /// </summary>
        /// <param name="converterFactory">The factory for element converters</param>
        public HtmlConverter(IElementConverterFactory converterFactory)
        {
            _converterFactory = converterFactory ?? throw new ArgumentNullException(nameof(converterFactory));
            _fontPath = Path.Combine(AppContext.BaseDirectory, "fonts", "Inter-Regular.ttf");
        }

        /// <summary>
        /// Converts HTML content to PDF bytes
        /// </summary>
        /// <param name="html">The HTML content to convert</param>
        /// <returns>PDF bytes</returns>
        public async Task<byte[]> ConvertToPdfBytes(string html)
        {
            if (string.IsNullOrEmpty(html))
                throw new ArgumentException("HTML content cannot be null or empty", nameof(html));

            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html));

            SetupQuestPdf();

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Content().Column(column =>
                    {
                        ConvertHtmlElements(document.Body!, column);
                    });
                });
            }).GeneratePdf();
        }

        /// <summary>
        /// Registers a custom element converter
        /// </summary>
        /// <param name="converter">The converter to register</param>
        public void RegisterConverter(IHtmlElementConverter converter)
        {
            _converterFactory.RegisterConverter(converter);
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

        /// <summary>
        /// Recursively converts HTML elements to QuestPDF elements
        /// </summary>
        /// <param name="parentElement">The parent HTML element</param>
        /// <param name="column">The QuestPDF column container</param>
        private void ConvertHtmlElements(HtmlElement parentElement, QuestPDF.Fluent.ColumnDescriptor column)
        {
            foreach (var child in parentElement.Children.ToList())
            {
                var converter = _converterFactory.GetConverter(child);
                if (converter != null)
                {
                    column.Item().Element(container => converter.Convert(child, container));
                }
                else
                {
                    // For unsupported elements, try to convert their children
                    ConvertHtmlElements(child, column);
                }
            }
        }
    }
}
