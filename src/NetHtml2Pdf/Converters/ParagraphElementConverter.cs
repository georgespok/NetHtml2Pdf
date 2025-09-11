using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using HtmlElement = AngleSharp.Dom.IElement;

namespace NetHtml2Pdf.Converters
{
    /// <summary>
    /// Converter for HTML paragraph elements
    /// </summary>
    public class ParagraphElementConverter : BaseHtmlElementConverter
    {
        protected override string[] SupportedTags => ["p"];

        public override void Convert(HtmlElement element, IContainer container)
        {
            // Process children elements to handle <br> tags properly
            var textBuilder = new System.Text.StringBuilder();
            ProcessTextNodes(element, textBuilder);
            
            var text = textBuilder.ToString();
            if (!string.IsNullOrEmpty(text))
            {
                container.Text(text).FontSize(12).LineHeight(1.4f);
            }
        }
        
        private void ProcessTextNodes(HtmlElement element, System.Text.StringBuilder textBuilder)
        {
            foreach (var child in element.ChildNodes)
            {
                if (child.NodeType == AngleSharp.Dom.NodeType.Text)
                {
                    textBuilder.Append(child.TextContent);
                }
                else if (child is AngleSharp.Dom.IElement childElement && childElement.TagName?.ToLower() == "br")
                {
                    textBuilder.Append("\n");
                }
                else if (child.NodeType == AngleSharp.Dom.NodeType.Element)
                {
                    ProcessTextNodes((HtmlElement)child, textBuilder);
                }
            }
        }
    }
}
