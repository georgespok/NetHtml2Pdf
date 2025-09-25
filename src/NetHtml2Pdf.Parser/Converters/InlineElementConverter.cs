using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Parser.Interfaces;

namespace NetHtml2Pdf.Parser.Converters
{
    /// <summary>
    /// Converts HTML inline elements (strong, em, span) to ParagraphNode
    /// </summary>
    public class InlineElementConverter(IStyleParser styleParser, IHtmlElementConverterFactory converterFactory)
        : IHtmlElementConverter<ParagraphNode>
    {
        private readonly IStyleParser _styleParser = styleParser ?? throw new ArgumentNullException(nameof(styleParser));
        private readonly IHtmlElementConverterFactory _converterFactory = converterFactory ?? throw new ArgumentNullException(nameof(converterFactory));

        public ParagraphNode Convert(IElement element)
        {
            var paragraph = new ParagraphNode();
            
            // Process child nodes recursively
            foreach (var child in element.ChildNodes)
            {
                if (child.NodeType == NodeType.Text)
                {
                    var text = child.TextContent;
                    if (!string.IsNullOrEmpty(text))
                    {
                        var textRun = new TextRunNode { Text = text };
                        ApplyFormatting(element, textRun);
                        paragraph.TextRuns.Add(textRun);
                    }
                }
                else if (child is IElement childElement)
                {
                    var converter = _converterFactory.GetConverter(childElement);
                    var childNode = converter.Convert(childElement);
                    
                    if (childNode is ParagraphNode childParagraph)
                    {
                        // Apply formatting to all text runs from child paragraph
                        foreach (var textRun in childParagraph.TextRuns)
                        {
                            ApplyFormatting(element, textRun);
                            paragraph.TextRuns.Add(textRun);
                        }
                    }
                }
            }
            
            _styleParser.ApplyInlineStyles(element, paragraph);
            return paragraph;
        }
        
        private static void ApplyFormatting(IElement element, TextRunNode textRun)
        {
            var tagName = element.TagName.ToLowerInvariant();
            switch (tagName)
            {
                case "strong" or "b":
                    textRun.IsBold = true;
                    break;
                case "em" or "i":
                    textRun.IsItalic = true;
                    break;
                // span and other elements don't have default formatting
            }
        }
    }
}
