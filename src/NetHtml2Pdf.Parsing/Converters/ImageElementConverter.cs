using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Parsing.Interfaces;

namespace NetHtml2Pdf.Parsing.Converters
{
    /// <summary>
    /// Converts HTML image elements to ImageNode
    /// </summary>
    public class ImageElementConverter(IStyleParser styleParser)
        : IHtmlElementConverter<ImageNode>
    {
        private readonly IStyleParser _styleParser = styleParser ?? throw new ArgumentNullException(nameof(styleParser));

        public ImageNode? Convert(IElement element)
        {
            var image = new ImageNode
            {
                Source = element.GetAttribute("src") ?? "",
                AltText = element.GetAttribute("alt")
            };
            
            // Parse width and height attributes
            if (float.TryParse(element.GetAttribute("width"), out var width))
                image.Width = width;
                
            if (float.TryParse(element.GetAttribute("height"), out var height))
                image.Height = height;
            
            _styleParser.ApplyInlineStyles(element, image);
            return image;
        }
    }
}
