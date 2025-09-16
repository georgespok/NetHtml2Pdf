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
                
                // Check if the li element has direct text content (no child elements)
                if (item.Children.Length == 0 && !string.IsNullOrWhiteSpace(item.TextContent))
                {
                    var text = item.TextContent.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        listItem.Content.Add(new TextRunNode { Text = text });
                    }
                }
                else
                {
                    // Process child nodes for mixed content or nested elements
                    foreach (var child in item.ChildNodes)
                    {
                        if (child.NodeType == NodeType.Text)
                        {
                            var text = child.TextContent.Trim();
                            if (!string.IsNullOrEmpty(text))
                            {
                                listItem.Content.Add(new TextRunNode { Text = text });
                            }
                        }
                        else if (child is IElement childElement)
                        {
                            var converter = _converterFactory.GetConverter(childElement);
                            var childNode = converter.Convert(childElement);
                            if (childNode != null)
                            {
                                listItem.Content.Add(childNode);
                            }
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
