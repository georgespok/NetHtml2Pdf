namespace NetHtml2Pdf.Core.Models
{
    /// <summary>
    /// Abstract base class for all document nodes in the intermediate model
    /// Pure POCO with no external dependencies
    /// </summary>
    public abstract class DocumentNode
    {
        /// <summary>
        /// Common style attributes for this node
        /// </summary>
        public NodeStyle Style { get; set; } = new NodeStyle();

        // Backwards-compatible convenience properties mapping to Style
        public float Margins
        {
            get => Style.Margins;
            set => Style.Margins = value;
        }

        public float PaddingLeft
        {
            get => Style.PaddingLeft;
            set => Style.PaddingLeft = value;
        }

        public float PaddingRight
        {
            get => Style.PaddingRight;
            set => Style.PaddingRight = value;
        }

        public float PaddingTop
        {
            get => Style.PaddingTop;
            set => Style.PaddingTop = value;
        }

        public float PaddingBottom
        {
            get => Style.PaddingBottom;
            set => Style.PaddingBottom = value;
        }

        public TextAlignment Alignment
        {
            get => Style.Alignment;
            set => Style.Alignment = value;
        }

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
