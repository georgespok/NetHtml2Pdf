using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NetHtml2Pdf.RenderModel
{
    /// <summary>
    /// Represents a block-level element (div, section, etc.)
    /// </summary>
    public class BlockNode : RenderNode
    {
        /// <summary>
        /// Background color for this block
        /// </summary>
        public string? BackgroundColor { get; set; }

        /// <summary>
        /// Border width for this block
        /// </summary>
        public float BorderWidth { get; set; } = 0;

        /// <summary>
        /// Border color for this block
        /// </summary>
        public string? BorderColor { get; set; }

        public override void Render(IContainer container)
        {
            container.Column(column =>
            {
                foreach (var child in Children)
                {
                    column.Item().Element(childContainer =>
                    {
                        // Apply margins and padding
                        var styledContainer = ApplyStyling(childContainer);
                        child.Render(styledContainer);
                    });
                }
            });
        }

        private IContainer ApplyStyling(IContainer container)
        {
            var styledContainer = container;

            if (Margins > 0)
            {
                styledContainer = styledContainer.Padding(Margins);
            }

            if (Padding > 0)
            {
                styledContainer = styledContainer.Padding(Padding);
            }

            if (!string.IsNullOrEmpty(BackgroundColor))
            {
                // Background color would be applied here
                // styledContainer = styledContainer.BackgroundColor(BackgroundColor);
            }

            if (BorderWidth > 0 && !string.IsNullOrEmpty(BorderColor))
            {
                styledContainer = styledContainer.Border(BorderWidth).BorderColor(BorderColor);
            }

            return styledContainer;
        }
    }
}
