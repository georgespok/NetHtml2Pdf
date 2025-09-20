using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Parser.Interfaces;
using NetHtml2Pdf.Parser.Utilities;

namespace NetHtml2Pdf.Parser.Converters
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
            var heading = new ParagraphNode
            {
                TextRuns = TextExtractor.ExtractTextRuns(element)
            };

            // Set browser-like defaults for font size and margins based on heading level
            var level = int.Parse(element.TagName[1..]);
            var defaultFontSizes = new Dictionary<int, float>
            {
                { 1, 32f },
                { 2, 24f },
                { 3, 18.72f },
                { 4, 16f },
                { 5, 13.28f },
                { 6, 10.72f }
            };

            var defaultMarginsEm = new Dictionary<int, float>
            {
                { 1, 0.67f },
                { 2, 0.83f },
                { 3, 1.0f },
                { 4, 1.33f },
                { 5, 1.67f },
                { 6, 2.33f }
            };

            if (defaultFontSizes.TryGetValue(level, out var fontSize))
            {
                heading.Style.Text.FontSize = fontSize;
                var em = defaultMarginsEm[level];
                var marginPx = em * fontSize;
                heading.Style.Box.MarginTop = marginPx;
                heading.Style.Box.MarginBottom = marginPx;
            }
            heading.TextRuns.ForEach(run => run.IsBold = true);
            
            _styleParser.ApplyInlineStyles(element, heading);
            return heading;
        }
    }
}
