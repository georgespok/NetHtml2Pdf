using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Parsing.Interfaces;

namespace NetHtml2Pdf.Parsing.Converters
{
    /// <summary>
    /// Converts HTML paragraph elements to ParagraphNode with support for nested inline elements
    /// </summary>
    public class ParagraphElementConverter(IStyleParser styleParser, IHtmlElementConverterFactory converterFactory)
        : IHtmlElementConverter<ParagraphNode>
    {
        private readonly IStyleParser _styleParser = styleParser ?? throw new ArgumentNullException(nameof(styleParser));
        private readonly IHtmlElementConverterFactory _converterFactory = converterFactory ?? throw new ArgumentNullException(nameof(converterFactory));

        public ParagraphNode? Convert(IElement element)
        {
            var paragraph = new ParagraphNode();
            
            // Process all child nodes recursively using existing converters
            foreach (var child in element.ChildNodes)
            {
                if (child.NodeType == NodeType.Text)
                {
                    // Handle text nodes directly
                    var text = child.TextContent;
                    if (!string.IsNullOrEmpty(text))
                    {
                        paragraph.TextRuns.Add(new TextRunNode { Text = text });
                    }
                }
                else if (child is IElement childElement)
                {
                    // Use existing converters for element nodes
                    var converter = _converterFactory.GetConverter(childElement);
                    var childNode = converter.Convert(childElement);
                    
                    if (childNode is ParagraphNode childParagraph)
                    {
                        // Merge text runs from child paragraph (e.g., from <br>, <strong>, <em>, <span>)
                        paragraph.TextRuns.AddRange(childParagraph.TextRuns);
                    }
                    else if (childNode is ImageNode imageNode)
                    {
                        // Handle images within paragraphs by converting to text placeholder
                        paragraph.TextRuns.Add(new TextRunNode { Text = $"[Image: {imageNode.AltText ?? "image"}]" });
                    }
                    // Other node types (BlockNode, TableNode, ListNode) are not typically found within paragraphs
                    // but if they are, we can handle them as needed
                }
            }
            
            _styleParser.ApplyInlineStyles(element, paragraph);
            return paragraph;
        }
    }
}
