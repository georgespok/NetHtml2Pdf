using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Parsing.Interfaces;
using NetHtml2Pdf.Parsing.Utilities;

namespace NetHtml2Pdf.Parsing.Converters
{
    /// <summary>
    /// Converts HTML list elements (ul, ol) to ListNode
    /// </summary>
    public class ListElementConverter(IStyleParser styleParser, IHtmlElementConverterFactory converterFactory)
        : IHtmlElementConverter<ListNode>
    {
        private readonly IStyleParser _styleParser = styleParser ?? throw new ArgumentNullException(nameof(styleParser));
        private readonly IHtmlElementConverterFactory _converterFactory = converterFactory ?? throw new ArgumentNullException(nameof(converterFactory));

        public ListNode? Convert(IElement element)
        {
            var list = new ListNode
            {
                IsOrdered = element.TagName.ToLowerInvariant() == "ol"
            };
            
            var listItems = element.QuerySelectorAll("li");
            foreach (var item in listItems)
            {
                var listItem = new ListItemNode();
                
                foreach (var child in item.Children)
                {
                    var converter = _converterFactory.GetConverter(child);
                    var childNode = converter.Convert(child);
                    if (childNode != null)
                    {
                        listItem.Content.Add(childNode);
                    }
                    else if (child.NodeType == NodeType.Text)
                    {
                        var text = child.TextContent.Trim();
                        if (!string.IsNullOrEmpty(text))
                        {
                            var paragraph = new ParagraphNode();
                            paragraph.TextRuns.Add(new TextRunNode { Text = text });
                            listItem.Content.Add(paragraph);
                        }
                    }
                }
                
                list.Items.Add(listItem);
            }
            
            _styleParser.ApplyInlineStyles(element, list);
            return list;
        }
    }
}
