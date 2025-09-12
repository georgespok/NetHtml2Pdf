using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Parsing.Interfaces;
using NetHtml2Pdf.Parsing.Converters;
using NetHtml2Pdf.Parsing.Utilities;

namespace NetHtml2Pdf.Parsing.Factories
{
    /// <summary>
    /// Factory for creating HTML element converters
    /// </summary>
    public class HtmlElementConverterFactory : IHtmlElementConverterFactory
    {
        private readonly List<IHtmlElementConverter> _converters;
        private readonly IHtmlElementConverter _defaultConverter;

        public HtmlElementConverterFactory()
        {
            _converters = [];
            _defaultConverter = new DefaultElementConverter(this);
            RegisterDefaultConverters();
        }

        /// <summary>
        /// Gets the appropriate converter for the specified HTML element
        /// </summary>
        /// <param name="element">The HTML element to convert</param>
        /// <returns>The converter that can handle the element</returns>
        public IHtmlElementConverter GetConverter(IElement element)
        {
            var converter = _converters.FirstOrDefault(c => c.CanConvert(element));
            return converter ?? _defaultConverter;
        }

        /// <summary>
        /// Registers a converter for specific HTML element types
        /// </summary>
        /// <param name="converter">The converter to register</param>
        public void RegisterConverter(IHtmlElementConverter converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            _converters.Add(converter);
        }

        /// <summary>
        /// Registers the default converters for common HTML element types
        /// </summary>
        private void RegisterDefaultConverters()
        {
            var styleParser = new StyleParser();

            // Paragraph converter
            RegisterConverter(new TypedElementConverterWrapper<ParagraphNode>(
                new ParagraphElementConverter(styleParser), 
                ["p"]));

            // Heading converter
            RegisterConverter(new TypedElementConverterWrapper<ParagraphNode>(
                new HeadingElementConverter(styleParser), 
                ["h1", "h2", "h3", "h4", "h5", "h6"]));

            // Block converter
            RegisterConverter(new TypedElementConverterWrapper<BlockNode>(
                new BlockElementConverter(styleParser, this), 
                ["div", "section", "article"]));

            // Table converter
            RegisterConverter(new TypedElementConverterWrapper<TableNode>(
                new TableElementConverter(styleParser, this), 
                ["table"]));

            // List converter
            RegisterConverter(new TypedElementConverterWrapper<ListNode>(
                new ListElementConverter(styleParser, this), 
                ["ul", "ol"]));

            // Image converter
            RegisterConverter(new TypedElementConverterWrapper<ImageNode>(
                new ImageElementConverter(styleParser), 
                ["img"]));

            // Line break converter
            RegisterConverter(new TypedElementConverterWrapper<ParagraphNode>(
                new LineBreakElementConverter(), 
                ["br"]));

            // Inline element converter
            RegisterConverter(new TypedElementConverterWrapper<ParagraphNode>(
                new InlineElementConverter(styleParser), 
                ["strong", "b", "em", "i", "span"]));
        }
    }
}
