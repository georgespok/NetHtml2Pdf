using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Parsing.Interfaces;

namespace NetHtml2Pdf.Parsing.Converters
{
    /// <summary>
    /// Converts HTML line break elements to ParagraphNode
    /// </summary>
    public class LineBreakElementConverter : IHtmlElementConverter<ParagraphNode>
    {
        public ParagraphNode? Convert(IElement element)
        {
            return new ParagraphNode
            {
                TextRuns = [new TextRunNode { Text = "\n" }]
            };
        }
    }
}
