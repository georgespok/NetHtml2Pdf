using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Renderer.Interfaces;

namespace NetHtml2Pdf.Renderer.Mappers
{
    /// <summary>
    /// Maps ImageNode to QuestPDF elements
    /// </summary>
    public class ImageNodeMapper : IDocumentNodeMapper<ImageNode>
    {
        public void Map(ImageNode imageNode, IContainer container)
        {
            // For now, just render a placeholder text for images
            // In a real implementation, you would need to handle image loading
            container.Text($"Image: {imageNode.AltText ?? "Image"}");
        }
    }
}
