using AngleSharp.Dom;
using NetHtml2Pdf.Interfaces;
using NetHtml2Pdf.Converters;

namespace NetHtml2Pdf.Factories
{
    /// <summary>
    /// Factory for creating HTML element converters
    /// </summary>
    public class ElementConverterFactory : IElementConverterFactory
    {
        private readonly List<IHtmlElementConverter> _converters;

        public ElementConverterFactory()
        {
            _converters = [];
            RegisterDefaultConverters();
        }

        /// <summary>
        /// Gets the appropriate converter for the specified HTML element
        /// </summary>
        /// <param name="element">The HTML element to convert</param>
        /// <returns>The converter that can handle the element, or null if none found</returns>
        public IHtmlElementConverter? GetConverter(IElement element)
        {
            return _converters.FirstOrDefault(converter => converter.CanConvert(element));
        }

        /// <summary>
        /// Registers a new converter for specific HTML elements
        /// </summary>
        /// <param name="converter">The converter to register</param>
        public void RegisterConverter(IHtmlElementConverter converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            _converters.Add(converter);
        }

        /// <summary>
        /// Registers the default converters for common HTML elements
        /// </summary>
        private void RegisterDefaultConverters()
        {
            RegisterConverter(new TableElementConverter());
            RegisterConverter(new ParagraphElementConverter());
            RegisterConverter(new LineBreakElementConverter());
            RegisterConverter(new DivElementConverter());
            RegisterConverter(new SectionElementConverter());
        }
    }
}
