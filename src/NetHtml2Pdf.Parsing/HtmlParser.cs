using AngleSharp;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Parsing.Interfaces;
using NetHtml2Pdf.Parsing.Factories;

namespace NetHtml2Pdf.Parsing
{
    /// <summary>
    /// Parses HTML content into the intermediate document model using a factory pattern
    /// </summary>
    public class HtmlParser(IHtmlElementConverterFactory? converterFactory = null)
    {
        private readonly IHtmlElementConverterFactory _converterFactory = converterFactory ?? new HtmlElementConverterFactory();

        /// <summary>
        /// Parses HTML content into a document model
        /// </summary>
        /// <param name="html">The HTML content to parse</param>
        /// <returns>A list of document nodes representing the parsed content</returns>
        public async Task<List<DocumentNode>> ParseAsync(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return new List<DocumentNode>();
            }

            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html));

            var documentNodes = new List<DocumentNode>();
            
            if (document.Body != null)
            {
                foreach (var child in document.Body.Children)
                {
                    var converter = _converterFactory.GetConverter(child);
                    var documentNode = converter.Convert(child);
                    if (documentNode != null)
                    {
                        documentNodes.Add(documentNode);
                    }
                }
            }

            return documentNodes;
        }

        /// <summary>
        /// Registers a custom converter for specific HTML element types
        /// </summary>
        /// <param name="converter">The converter to register</param>
        public void RegisterConverter(IHtmlElementConverter converter)
        {
            _converterFactory.RegisterConverter(converter);
        }
    }
}