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
        public List<TextRunNode> TextRuns { get; set; } = [];
    }
}
