namespace NetHtml2Pdf.Core.Models
{
    /// <summary>
    /// Represents an image element
    /// Pure POCO with no external dependencies
    /// </summary>
    public class ImageNode : DocumentNode
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
