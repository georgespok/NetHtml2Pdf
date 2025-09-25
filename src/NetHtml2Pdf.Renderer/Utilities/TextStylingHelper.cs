using QuestPDF.Fluent;
using NetHtml2Pdf.Core.Models;

namespace NetHtml2Pdf.Renderer.Utilities
{
    /// <summary>
    /// Helper class for common text styling operations
    /// </summary>
    public static class TextStylingHelper
    {
        /// <summary>
        /// Applies styling to a text run using the provided TextDescriptor
        /// </summary>
        /// <param name="textRunNode">The text run node containing styling information</param>
        /// <param name="textDescriptor">The QuestPDF TextDescriptor to apply styling to</param>
        public static void ApplyTextRunStyling(TextRunNode textRunNode, TextDescriptor textDescriptor)
        {
            var styledText = textDescriptor.Span(textRunNode.Text);

            if (textRunNode.IsBold)
                styledText = styledText.Bold();

            if (textRunNode.IsItalic)
                styledText = styledText.Italic();

            if (!string.IsNullOrEmpty(textRunNode.Color))
                styledText = styledText.FontColor(textRunNode.Color);

            if (textRunNode.FontSize.HasValue)
                styledText = styledText.FontSize(textRunNode.FontSize.Value);
        }
    }
}
