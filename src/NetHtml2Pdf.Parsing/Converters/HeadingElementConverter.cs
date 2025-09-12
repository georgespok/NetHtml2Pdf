using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Parsing.Interfaces;
using NetHtml2Pdf.Parsing.Utilities;

namespace NetHtml2Pdf.Parsing.Converters
{
    /// <summary>
    /// Converts HTML heading elements (h1-h6) to ParagraphNode
    /// </summary>
    public class HeadingElementConverter(IStyleParser styleParser)
        : IHtmlElementConverter<ParagraphNode>
    {
        private readonly IStyleParser _styleParser = styleParser ?? throw new ArgumentNullException(nameof(styleParser));

        public ParagraphNode? Convert(IElement element)
        {
            var heading = new ParagraphNode();
            heading.TextRuns = TextExtractor.ExtractTextRuns(element);
            
            // Set font size based on heading level
            var level = int.Parse(element.TagName.Substring(1));
            heading.FontSize = 24 - (level * 2); // h1=22, h2=20, h3=18, etc.
            heading.TextRuns.ForEach(run => run.IsBold = true);
            
            _styleParser.ApplyInlineStyles(element, heading);
            return heading;
        }
    }
}
