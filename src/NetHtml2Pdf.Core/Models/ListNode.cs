namespace NetHtml2Pdf.Core.Models
{
    /// <summary>
    /// Represents a list element (ul or ol)
    /// Pure POCO with no external dependencies
    /// </summary>
    public class ListNode : DocumentNode
    {
        /// <summary>
        /// List items
        /// </summary>
        public List<ListItemNode> Items { get; set; } = new List<ListItemNode>();

        /// <summary>
        /// Whether this is an ordered list
        /// </summary>
        public bool IsOrdered { get; set; }

        /// <summary>
        /// Bullet character for unordered lists
        /// </summary>
        public string BulletCharacter { get; set; } = "•";
    }

    /// <summary>
    /// Represents a list item
    /// Pure POCO with no external dependencies
    /// </summary>
    public class ListItemNode
    {
        /// <summary>
        /// Content of this list item
        /// </summary>
        public List<DocumentNode> Content { get; set; } = new List<DocumentNode>();
    }
}
