using QuestPDF.Infrastructure;
using QuestPDF.Helpers;

namespace NetHtml2Pdf.RenderModel
{
    /// <summary>
    /// Abstract base class for all render nodes in the intermediate model
    /// </summary>
    public abstract class RenderNode
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
        public HorizontalAlignment Alignment { get; set; } = HorizontalAlignment.Left;

        /// <summary>
        /// Child nodes of this render node
        /// </summary>
        public List<RenderNode> Children { get; set; } = new List<RenderNode>();

        /// <summary>
        /// Converts this render node to QuestPDF elements
        /// </summary>
        /// <param name="container">The QuestPDF container to add elements to</param>
        public abstract void Render(IContainer container);
    }
}
