using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;

namespace NetHtml2Pdf.RenderModel
{
    /// <summary>
    /// Represents a text run with formatting
    /// </summary>
    public class TextRunNode : RenderNode
    {
        /// <summary>
        /// The text content
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Whether this text is bold
        /// </summary>
        public bool IsBold { get; set; }

        /// <summary>
        /// Whether this text is italic
        /// </summary>
        public bool IsItalic { get; set; }

        /// <summary>
        /// Text color
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// Font size override (if different from paragraph)
        /// </summary>
        public float? FontSize { get; set; }

        public override void Render(IContainer container)
        {
            // This method is called from ParagraphNode, so we don't render here
            // The actual rendering is done in the Render(ITextDescriptor) method
        }

        /// <summary>
        /// Renders this text run to a text descriptor
        /// </summary>
        /// <param name="textDescriptor">The text descriptor to add this run to</param>
        public void Render(QuestPDF.Fluent.TextDescriptor textDescriptor)
        {
            var styledText = textDescriptor.Span(Text);

            if (IsBold)
                styledText = styledText.Bold();

            if (IsItalic)
                styledText = styledText.Italic();

            if (!string.IsNullOrEmpty(Color))
                styledText = styledText.FontColor(Color);

            if (FontSize.HasValue)
                styledText = styledText.FontSize(FontSize.Value);
        }
    }
}
