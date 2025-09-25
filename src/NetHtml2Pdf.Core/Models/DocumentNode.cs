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
        public NodeStyle Style { get; } = new();

        /// <summary>
        /// Child nodes of this document node
        /// </summary>
        public List<DocumentNode> Children { get; } = [];
    }
}
