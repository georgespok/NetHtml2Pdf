using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NetHtml2Pdf.RenderModel
{
    /// <summary>
    /// Represents a list element (ul or ol)
    /// </summary>
    public class ListNode : RenderNode
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

        public override void Render(IContainer container)
        {
            container.Column(column =>
            {
                for (int i = 0; i < Items.Count; i++)
                {
                    var item = Items[i];
                    column.Item().Row(row =>
                    {
                        // Add bullet or number
                        var bulletText = IsOrdered ? $"{i + 1}." : BulletCharacter;
                        row.RelativeItem(0.1f).Text(bulletText).FontSize(12);
                        
                        // Add content
                        row.RelativeItem(0.9f).Element(contentContainer =>
                        {
                            item.Render(contentContainer);
                        });
                    });
                }
            });
        }
    }

    /// <summary>
    /// Represents a list item
    /// </summary>
    public class ListItemNode
    {
        /// <summary>
        /// Content of this list item
        /// </summary>
        public List<RenderNode> Content { get; set; } = new List<RenderNode>();

        public void Render(IContainer container)
        {
            container.Column(column =>
            {
                foreach (var node in Content)
                {
                    column.Item().Element(nodeContainer =>
                    {
                        node.Render(nodeContainer);
                    });
                }
            });
        }
    }
}
