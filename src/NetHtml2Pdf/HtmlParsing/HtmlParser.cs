using AngleSharp;
using AngleSharp.Dom;
using NetHtml2Pdf.RenderModel;

namespace NetHtml2Pdf.HtmlParsing
{
    /// <summary>
    /// Parses HTML content into the intermediate render model
    /// </summary>
    public class HtmlParser
    {
        /// <summary>
        /// Parses HTML content into a render model
        /// </summary>
        /// <param name="html">The HTML content to parse</param>
        /// <returns>A list of render nodes representing the parsed content</returns>
        public async Task<List<RenderNode>> ParseAsync(string html)
        {
            if (string.IsNullOrEmpty(html))
                return new List<RenderNode>();

            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html));

            var renderNodes = new List<RenderNode>();
            
            if (document.Body != null)
            {
                foreach (var child in document.Body.Children)
                {
                    var renderNode = ConvertElement(child);
                    if (renderNode != null)
                    {
                        renderNodes.Add(renderNode);
                    }
                }
            }

            return renderNodes;
        }

        /// <summary>
        /// Converts an HTML element to a render node
        /// </summary>
        /// <param name="element">The HTML element to convert</param>
        /// <returns>A render node, or null if the element is not supported</returns>
        private RenderNode? ConvertElement(IElement element)
        {
            var tagName = element.TagName.ToLowerInvariant();

            return tagName switch
            {
                "p" => ConvertParagraph(element),
                "h1" or "h2" or "h3" or "h4" or "h5" or "h6" => ConvertHeading(element),
                "div" or "section" or "article" => ConvertBlock(element),
                "table" => ConvertTable(element),
                "ul" or "ol" => ConvertList(element),
                "img" => ConvertImage(element),
                "br" => ConvertLineBreak(),
                "strong" or "b" => ConvertStrong(element),
                "em" or "i" => ConvertEmphasis(element),
                "span" => ConvertSpan(element),
                _ => ConvertGenericBlock(element)
            };
        }

        private ParagraphNode ConvertParagraph(IElement element)
        {
            var paragraph = new ParagraphNode();
            ExtractTextRuns(element, paragraph.TextRuns);
            ApplyBasicStyling(element, paragraph);
            return paragraph;
        }

        private ParagraphNode ConvertHeading(IElement element)
        {
            var heading = new ParagraphNode();
            ExtractTextRuns(element, heading.TextRuns);
            
            // Set font size based on heading level
            var level = int.Parse(element.TagName.Substring(1));
            heading.FontSize = 24 - (level * 2); // h1=22, h2=20, h3=18, etc.
            heading.TextRuns.ForEach(run => run.IsBold = true);
            
            ApplyBasicStyling(element, heading);
            return heading;
        }

        private BlockNode ConvertBlock(IElement element)
        {
            var block = new BlockNode();
            
            foreach (var child in element.Children)
            {
                var childNode = ConvertElement(child);
                if (childNode != null)
                {
                    block.Children.Add(childNode);
                }
            }
            
            ApplyBasicStyling(element, block);
            return block;
        }

        private TableNode ConvertTable(IElement element)
        {
            var table = new TableNode();
            
            // Find header row
            var headerRow = element.QuerySelector("thead tr") ?? element.QuerySelector("tr");
            if (headerRow != null)
            {
                var headers = headerRow.QuerySelectorAll("th, td").Select(cell => cell.TextContent.Trim()).ToList();
                
                // Define columns
                foreach (var _ in headers)
                {
                    table.ColumnDefinitions.Add(new TableColumnDefinition { Type = TableColumnType.Relative });
                }
                
                // Add header row
                var headerRowNode = new TableRowNode { IsHeader = true };
                foreach (var header in headers)
                {
                    var cell = new TableCellNode();
                    cell.Content.Add(new ParagraphNode 
                    { 
                        TextRuns = new List<TextRunNode> 
                        { 
                            new TextRunNode { Text = header, IsBold = true } 
                        } 
                    });
                    headerRowNode.Cells.Add(cell);
                }
                table.Rows.Add(headerRowNode);
            }
            
            // Add data rows
            var dataRows = element.QuerySelectorAll("tbody tr").Concat(
                element.QuerySelectorAll("tr").Skip(headerRow != null ? 1 : 0));
                
            foreach (var row in dataRows)
            {
                var rowNode = new TableRowNode();
                var cells = row.QuerySelectorAll("td, th");
                
                foreach (var cell in cells)
                {
                    var cellNode = new TableCellNode();
                    ExtractTextRuns(cell, new List<TextRunNode>());
                    var paragraph = new ParagraphNode();
                    ExtractTextRuns(cell, paragraph.TextRuns);
                    cellNode.Content.Add(paragraph);
                    rowNode.Cells.Add(cellNode);
                }
                
                table.Rows.Add(rowNode);
            }
            
            ApplyBasicStyling(element, table);
            return table;
        }

        private ListNode ConvertList(IElement element)
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
                    var childNode = ConvertElement(child);
                    if (childNode != null)
                    {
                        listItem.Content.Add(childNode);
                    }
                    else if (child.NodeType == NodeType.Text)
                    {
                        var text = child.TextContent.Trim();
                        if (!string.IsNullOrEmpty(text))
                        {
                            listItem.Content.Add(new ParagraphNode
                            {
                                TextRuns = new List<TextRunNode> { new TextRunNode { Text = text } }
                            });
                        }
                    }
                }
                
                list.Items.Add(listItem);
            }
            
            ApplyBasicStyling(element, list);
            return list;
        }

        private ImageNode ConvertImage(IElement element)
        {
            var image = new ImageNode
            {
                Source = element.GetAttribute("src") ?? "",
                AltText = element.GetAttribute("alt")
            };
            
            // Parse width and height attributes
            if (float.TryParse(element.GetAttribute("width"), out var width))
                image.Width = width;
                
            if (float.TryParse(element.GetAttribute("height"), out var height))
                image.Height = height;
            
            ApplyBasicStyling(element, image);
            return image;
        }

        private ParagraphNode ConvertLineBreak()
        {
            return new ParagraphNode
            {
                TextRuns = new List<TextRunNode> { new TextRunNode { Text = "\n" } }
            };
        }

        private ParagraphNode ConvertStrong(IElement element)
        {
            var paragraph = new ParagraphNode();
            ExtractTextRuns(element, paragraph.TextRuns);
            paragraph.TextRuns.ForEach(run => run.IsBold = true);
            return paragraph;
        }

        private ParagraphNode ConvertEmphasis(IElement element)
        {
            var paragraph = new ParagraphNode();
            ExtractTextRuns(element, paragraph.TextRuns);
            paragraph.TextRuns.ForEach(run => run.IsItalic = true);
            return paragraph;
        }

        private ParagraphNode ConvertSpan(IElement element)
        {
            var paragraph = new ParagraphNode();
            ExtractTextRuns(element, paragraph.TextRuns);
            ApplyBasicStyling(element, paragraph);
            return paragraph;
        }

        private BlockNode ConvertGenericBlock(IElement element)
        {
            // For unsupported elements, try to convert their children
            var block = new BlockNode();
            
            foreach (var child in element.Children)
            {
                var childNode = ConvertElement(child);
                if (childNode != null)
                {
                    block.Children.Add(childNode);
                }
            }
            
            return block;
        }

        private void ExtractTextRuns(IElement element, List<TextRunNode> textRuns)
        {
            foreach (var child in element.ChildNodes)
            {
                if (child.NodeType == NodeType.Text)
                {
                    var text = child.TextContent.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        textRuns.Add(new TextRunNode { Text = text });
                    }
                }
                else if (child is IElement childElement)
                {
                    var tagName = childElement.TagName.ToLowerInvariant();
                    var textRun = new TextRunNode();
                    
                    // Extract text content from child element
                    var childText = childElement.TextContent.Trim();
                    if (!string.IsNullOrEmpty(childText))
                    {
                        textRun.Text = childText;
                        
                        // Apply formatting based on tag
                        switch (tagName)
                        {
                            case "strong" or "b":
                                textRun.IsBold = true;
                                break;
                            case "em" or "i":
                                textRun.IsItalic = true;
                                break;
                        }
                        
                        textRuns.Add(textRun);
                    }
                }
            }
        }

        private void ApplyBasicStyling(IElement element, RenderNode node)
        {
            var style = element.GetAttribute("style");
            if (string.IsNullOrEmpty(style))
                return;

            // Parse basic inline styles
            var styles = ParseInlineStyles(style);
            
            // Apply text alignment
            if (styles.TryGetValue("text-align", out var textAlign))
            {
                node.Alignment = textAlign.ToLowerInvariant() switch
                {
                    "center" => QuestPDF.Infrastructure.HorizontalAlignment.Center,
                    "right" => QuestPDF.Infrastructure.HorizontalAlignment.Right,
                    _ => QuestPDF.Infrastructure.HorizontalAlignment.Left
                };
            }
            
            // Apply margins and padding (simplified parsing)
            if (styles.TryGetValue("margin", out var margin))
            {
                var marginValue = ParseSize(margin);
                if (marginValue.HasValue)
                {
                    node.Margins = marginValue.Value;
                }
            }
            
            if (styles.TryGetValue("padding", out var padding))
            {
                var paddingValue = ParseSize(padding);
                if (paddingValue.HasValue)
                {
                    node.Padding = paddingValue.Value;
                }
            }
        }

        private Dictionary<string, string> ParseInlineStyles(string style)
        {
            var styles = new Dictionary<string, string>();
            var declarations = style.Split(';', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var declaration in declarations)
            {
                var parts = declaration.Split(':', 2);
                if (parts.Length == 2)
                {
                    var property = parts[0].Trim().ToLowerInvariant();
                    var value = parts[1].Trim();
                    styles[property] = value;
                }
            }
            
            return styles;
        }

        private float? ParseSize(string size)
        {
            if (string.IsNullOrEmpty(size))
                return null;
                
            var cleanSize = size.Replace("px", "").Replace("pt", "").Trim();
            if (float.TryParse(cleanSize, out var result))
            {
                return result;
            }
            
            return null;
        }
    }
}
