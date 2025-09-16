using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Rendering.Interfaces;

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
            var styledText = textDescriptor.Span(textRun.Text);

            if (textRun.IsBold)
                styledText = styledText.Bold();

            if (textRun.IsItalic)
                styledText = styledText.Italic();

            if (!string.IsNullOrEmpty(textRun.Color))
                styledText = styledText.FontColor(textRun.Color);

            if (textRun.FontSize.HasValue)
                styledText = styledText.FontSize(textRun.FontSize.Value);
        }
    }
}
