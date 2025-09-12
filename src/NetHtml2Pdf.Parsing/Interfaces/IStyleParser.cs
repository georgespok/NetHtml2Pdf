using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;

namespace NetHtml2Pdf.Parsing.Interfaces
{
    /// <summary>
    /// Interface for parsing CSS styles from HTML elements
    /// </summary>
    public interface IStyleParser
    {
        /// <summary>
        /// Parses inline styles from an HTML element and applies them to a document node
        /// </summary>
        /// <param name="element">The HTML element containing styles</param>
        /// <param name="node">The document node to apply styles to</param>
        void ApplyInlineStyles(IElement element, DocumentNode node);

        /// <summary>
        /// Parses a CSS size value (e.g., "10px", "5pt") to a float
        /// </summary>
        /// <param name="size">The CSS size string</param>
        /// <returns>The parsed size value, or null if parsing fails</returns>
        float? ParseSize(string size);
    }
}
