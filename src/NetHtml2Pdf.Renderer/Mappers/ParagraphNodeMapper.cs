using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Renderer.Interfaces;
using NetHtml2Pdf.Renderer.Utilities;

namespace NetHtml2Pdf.Renderer.Mappers
{
    /// <summary>
    /// Maps ParagraphNode to QuestPDF elements
    /// </summary>
    public class ParagraphNodeMapper() : IDocumentNodeMapper<ParagraphNode>
    {
        public void Map(ParagraphNode paragraphNode, IContainer container)
        {
            if (paragraphNode.TextRuns.Count == 0)
                return;

            container
                      .PaddingTop(paragraphNode.Style.PaddingTop)
                      .PaddingRight(paragraphNode.Style.PaddingRight)
                      .PaddingBottom(paragraphNode.Style.PaddingBottom)
                      .PaddingLeft(paragraphNode.Style.PaddingLeft)
                      .PaddingTop(paragraphNode.Style.Box.MarginTop.GetValueOrDefault() + paragraphNode.Style.PaddingTop)
                      .PaddingBottom(paragraphNode.Style.Box.MarginBottom.GetValueOrDefault() + paragraphNode.Style.PaddingBottom)
                      .Text(text =>
            {
                text.DefaultTextStyle(style =>
                {
                    if (paragraphNode.Style.Text.FontSize.HasValue)
                        style = style.FontSize(paragraphNode.Style.Text.FontSize.Value);
                    if (paragraphNode.Style.Text.LineHeight.HasValue)
                        style = style.LineHeight(paragraphNode.Style.Text.LineHeight.Value);
                    return style;
                });

                var inheritedFontSize = paragraphNode.Style.Text.FontSize;
                foreach (var run in paragraphNode.TextRuns)
                {
                    if (inheritedFontSize.HasValue && !run.FontSize.HasValue)
                        run.FontSize = inheritedFontSize.Value;
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
