namespace NetHtml2Pdf.Core.Models
{
    /// <summary>
    /// Represents a block-level element (div, section, etc.)
    /// Pure POCO with no external dependencies
    /// </summary>
    public class BlockNode : DocumentNode
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
    }
}
