using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Rendering.Interfaces;

namespace NetHtml2Pdf.Rendering.Mappers
{
    /// <summary>
    /// Maps TextRunNode to QuestPDF text elements
    /// </summary>
    public class TextRunNodeMapper : IDocumentNodeMapper<TextRunNode>
    {
        public void Map(TextRunNode textRunNode, IContainer container)
        {
            container.Text(text =>
            {
                var styledText = text.Span(textRunNode.Text);

                if (textRunNode.IsBold)
                    styledText = styledText.Bold();

                if (textRunNode.IsItalic)
                    styledText = styledText.Italic();

                if (!string.IsNullOrEmpty(textRunNode.Color))
                    styledText = styledText.FontColor(textRunNode.Color);

                if (textRunNode.FontSize.HasValue)
                    styledText = styledText.FontSize(textRunNode.FontSize.Value);
            });
        }
    }
}
