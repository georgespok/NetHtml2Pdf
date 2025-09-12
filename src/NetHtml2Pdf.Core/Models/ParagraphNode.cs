namespace NetHtml2Pdf.Core.Models
{
    /// <summary>
    /// Represents a paragraph element with text runs
    /// Pure POCO with no external dependencies
    /// </summary>
    public class ParagraphNode : DocumentNode
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
    }
}
