using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NetHtml2Pdf.RenderModel
{
    /// <summary>
    /// Represents a paragraph element with text runs
    /// </summary>
    public class ParagraphNode : RenderNode
    {
        /// <summary>
        /// Text runs that make up this paragraph
        /// </summary>
        public List<TextRunNode> TextRuns { get; set; } = new List<TextRunNode>();

        /// <summary>
        /// Font size for this paragraph
        /// </summary>
        public float FontSize { get; set; } = 12;

        /// <summary>
        /// Line height for this paragraph
        /// </summary>
        public float LineHeight { get; set; } = 1.4f;

        public override void Render(IContainer container)
        {
            if (TextRuns.Count == 0)
                return;

            container.Text(text =>
            {
                text.DefaultTextStyle(style =>
                {
                    style.FontSize(FontSize);
                    style.LineHeight(LineHeight);
                    return style;
                });

                foreach (var run in TextRuns)
                {
                    run.Render(text);
                }
            });
        }
    }
}
