using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Parsing.Interfaces;

namespace NetHtml2Pdf.Parsing.Converters
{
    /// <summary>
    /// Converts HTML block elements (div, section, article) to BlockNode
    /// </summary>
    public class BlockElementConverter(IStyleParser styleParser, IHtmlElementConverterFactory converterFactory)
        : IHtmlElementConverter<BlockNode>
    {
        private readonly IStyleParser _styleParser = styleParser ?? throw new ArgumentNullException(nameof(styleParser));
        private readonly IHtmlElementConverterFactory _converterFactory = converterFactory ?? throw new ArgumentNullException(nameof(converterFactory));

        public BlockNode? Convert(IElement element)
        {
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
            
            _styleParser.ApplyInlineStyles(element, block);
            return block;
        }
    }
}
