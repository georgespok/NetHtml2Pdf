namespace NetHtml2Pdf.Core.Models
{
    /// <summary>
    /// Represents a block-level element (div, section, etc.)
    /// Pure POCO with no external dependencies
    /// </summary>
    public class BlockNode : DocumentNode
    {
        // Background and border now live in Style but keep passthroughs for compatibility
        public string? BackgroundColor { get => Style.BackgroundColor; set => Style.BackgroundColor = value; }
        public float BorderWidth { get => Style.BorderWidth ?? 0; set => Style.BorderWidth = value; }
        public string? BorderColor { get => Style.BorderColor; set => Style.BorderColor = value; }
    }
}
