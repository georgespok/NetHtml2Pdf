using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NetHtml2Pdf.RenderModel
{
    /// <summary>
    /// Represents an image element
    /// </summary>
    public class ImageNode : RenderNode
    {
        /// <summary>
        /// Image source (URL or base64 data)
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Alt text for the image
        /// </summary>
        public string? AltText { get; set; }

        /// <summary>
        /// Image width
        /// </summary>
        public float? Width { get; set; }

        /// <summary>
        /// Image height
        /// </summary>
        public float? Height { get; set; }

        /// <summary>
        /// How the image should fit within its bounds
        /// </summary>
        public ImageFitMode FitMode { get; set; } = ImageFitMode.FitWidth;

        public override void Render(IContainer container)
        {
            // For now, just render a placeholder text for images
            // In a real implementation, you would need to handle image loading
            container.Text($"Image: {AltText ?? "Image"}");
        }
    }

    /// <summary>
    /// Defines how an image should fit within its bounds
    /// </summary>
    public enum ImageFitMode
    {
        FitWidth,
        FitHeight,
        FitArea
    }
}
