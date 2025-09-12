using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Parsing.Interfaces;
using NetHtml2Pdf.Parsing.Utilities;

namespace NetHtml2Pdf.Parsing.Converters
{
    /// <summary>
    /// Converts HTML paragraph elements to ParagraphNode
    /// </summary>
    public class ParagraphElementConverter(IStyleParser styleParser)
        : IHtmlElementConverter<ParagraphNode>
    {
        private readonly IStyleParser _styleParser = styleParser ?? throw new ArgumentNullException(nameof(styleParser));

        public ParagraphNode? Convert(IElement element)
        {
            var paragraph = new ParagraphNode();
            paragraph.TextRuns = TextExtractor.ExtractTextRuns(element);
            _styleParser.ApplyInlineStyles(element, paragraph);
            return paragraph;
        }
    }
}
