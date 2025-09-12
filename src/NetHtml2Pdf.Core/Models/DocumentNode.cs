namespace NetHtml2Pdf.Core.Models
{
    /// <summary>
    /// Abstract base class for all document nodes in the intermediate model
    /// Pure POCO with no external dependencies
    /// </summary>
    public abstract class DocumentNode
    {
        /// <summary>
        /// Margins for this node
        /// </summary>
        public float Margins { get; set; } = 0;

        /// <summary>
        /// Padding for this node
        /// </summary>
        public float Padding { get; set; } = 0;

        /// <summary>
        /// Text alignment for this node
        /// </summary>
        public TextAlignment Alignment { get; set; } = TextAlignment.Left;

        /// <summary>
        /// Child nodes of this document node
        /// </summary>
        public List<DocumentNode> Children { get; set; } = new List<DocumentNode>();
    }

    /// <summary>
    /// Text alignment options
    /// </summary>
    public enum TextAlignment
    {
        Left,
        Center,
        Right,
        Justify
    }
}
