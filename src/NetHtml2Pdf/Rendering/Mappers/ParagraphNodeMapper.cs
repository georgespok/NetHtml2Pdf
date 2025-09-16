using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Rendering.Interfaces;
using NetHtml2Pdf.Rendering.Utilities;

namespace NetHtml2Pdf.Rendering.Mappers
{
    /// <summary>
    /// Maps ParagraphNode to QuestPDF elements
    /// </summary>
    public class ParagraphNodeMapper(IDocumentNodeMapperFactory mapperFactory) : IDocumentNodeMapper<ParagraphNode>
    {
        public void Map(ParagraphNode paragraphNode, IContainer container)
        {
            if (paragraphNode.TextRuns.Count == 0)
                return;

            container.Text(text =>
            {
                text.DefaultTextStyle(style =>
                {
                    style.FontSize(paragraphNode.FontSize);
                    style.LineHeight(paragraphNode.LineHeight);
                    return style;
                });

                foreach (var run in paragraphNode.TextRuns)
                {
                    MapTextRun(run, text);
                }
            });
        }

        private static void MapTextRun(TextRunNode textRun, TextDescriptor textDescriptor)
        {
            // Handle line breaks specially - they should create new lines in the text flow
            if (textRun.Text == "\n")
            {
                textDescriptor.Span(Environment.NewLine);
                return;
            }

            // Use the common styling helper to eliminate code duplication
            TextStylingHelper.ApplyTextRunStyling(textRun, textDescriptor);
        }

    }
}
