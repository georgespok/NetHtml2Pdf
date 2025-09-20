namespace NetHtml2Pdf.Core.Models
{
    /// <summary>
    /// Represents a text run with formatting
    /// Pure POCO with no external dependencies
    /// </summary>
    public class TextRunNode : DocumentNode
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

    }
}
