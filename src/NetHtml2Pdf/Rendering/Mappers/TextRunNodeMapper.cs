using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Rendering.Interfaces;
using NetHtml2Pdf.Rendering.Utilities;

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
                TextStylingHelper.ApplyTextRunStyling(textRunNode, text);
            });
        }
    }
}
