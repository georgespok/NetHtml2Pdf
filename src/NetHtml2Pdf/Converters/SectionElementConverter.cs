using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using HtmlElement = AngleSharp.Dom.IElement;

namespace NetHtml2Pdf.Converters
{
    /// <summary>
    /// Converter for HTML section elements
    /// </summary>
    public class SectionElementConverter : BaseHtmlElementConverter
    {
        protected override string[] SupportedTags => ["section"];

        public override void Convert(HtmlElement element, IContainer container)
        {
            var text = GetTextContent(element);
            if (!string.IsNullOrEmpty(text))
            {
                container.Text(text).FontSize(12).LineHeight(1.4f);
            }
        }
    }
}
