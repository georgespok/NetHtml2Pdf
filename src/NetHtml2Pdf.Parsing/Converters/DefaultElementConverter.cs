using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Parsing.Interfaces;

namespace NetHtml2Pdf.Parsing.Converters
{
    /// <summary>
    /// Default converter for unknown HTML element types
    /// </summary>
    public class DefaultElementConverter(IHtmlElementConverterFactory converterFactory)
        : IHtmlElementConverter
    {
        private readonly IHtmlElementConverterFactory _converterFactory = converterFactory ?? throw new ArgumentNullException(nameof(converterFactory));

        public DocumentNode? Convert(IElement element)
        {
            // For unsupported elements, try to convert their children
            var block = new BlockNode();
            
            foreach (var child in element.Children)
            {
                var converter = _converterFactory.GetConverter(child);
                var childNode = converter.Convert(child);
                if (childNode != null)
                {
                    block.Children.Add(childNode);
                }
            }
            
            return block;
        }

        public bool CanConvert(IElement element)
        {
            // This converter can handle any element type as a fallback
            return true;
        }
    }
}
