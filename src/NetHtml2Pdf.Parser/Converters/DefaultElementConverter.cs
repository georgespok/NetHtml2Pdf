using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Parser.Interfaces;

namespace NetHtml2Pdf.Parser.Converters
{
    /// <summary>
    /// Default converter for unknown HTML element types
    /// </summary>
    public class DefaultElementConverter(IHtmlElementConverterFactory converterFactory)
        : IHtmlElementConverter
    {
        private readonly IHtmlElementConverterFactory _converterFactory = converterFactory ?? throw new ArgumentNullException(nameof(converterFactory));

        public DocumentNode Convert(IElement element)
        {
            // For unsupported elements, preserve inner text and convert supported descendants
            var block = new BlockNode();

            foreach (var child in element.ChildNodes)
            {
                if (child.NodeType == AngleSharp.Dom.NodeType.Text)
                {
                    var text = child.TextContent;
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        // Append text into the nearest paragraph to preserve flow
                        if (block.Children.LastOrDefault() is ParagraphNode lastPara)
                        {
                            lastPara.TextRuns.Add(new TextRunNode { Text = text });
                        }
                        else
                        {
                            var paragraph = new ParagraphNode();
                            paragraph.TextRuns.Add(new TextRunNode { Text = text });
                            block.Children.Add(paragraph);
                        }
                    }
                    continue;
                }

                if (child is IElement childElement)
                {
                    var converter = _converterFactory.GetConverter(childElement);
                    var childNode = converter.Convert(childElement);
                    if (childNode != null)
                        block.Children.Add(childNode);
                }
            }

            return block;
        }

        public bool CanConvert(IElement element)
        {
            // This converter can handle any element type as a fallback
            return true;
        }
    }
}
