using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using HtmlElement = AngleSharp.Dom.IElement;

namespace NetHtml2Pdf.Converters
{
    /// <summary>
    /// Converter for HTML line break elements
    /// </summary>
    public class LineBreakElementConverter : BaseHtmlElementConverter
    {
        protected override string[] SupportedTags => ["br"];

        public override void Convert(HtmlElement element, IContainer container)
        {
            // Create a line break by adding vertical space
            container.Text(string.Empty);
        }
    }
}
