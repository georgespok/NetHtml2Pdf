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
        /// Left padding for this node
        /// </summary>
        public float PaddingLeft { get; set; } = 0;

        /// <summary>
        /// Right padding for this node
        /// </summary>
        public float PaddingRight { get; set; } = 0;

        /// <summary>
        /// Top padding for this node
        /// </summary>
        public float PaddingTop { get; set; } = 0;

        /// <summary>
        /// Bottom padding for this node
        /// </summary>
        public float PaddingBottom { get; set; } = 0;

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
