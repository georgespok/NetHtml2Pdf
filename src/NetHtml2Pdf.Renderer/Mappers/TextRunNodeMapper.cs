using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Renderer.Interfaces;
using NetHtml2Pdf.Renderer.Utilities;

namespace NetHtml2Pdf.Renderer.Mappers
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
                TextStylingHelper.ApplyTextRunStyling(textRunNode, text);
            });
        }
    }
}
