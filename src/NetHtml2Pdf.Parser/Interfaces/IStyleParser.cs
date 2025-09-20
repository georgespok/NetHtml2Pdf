using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;

namespace NetHtml2Pdf.Parser.Interfaces
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
        /// Parses inline styles from an HTML element and applies them to a table cell node
        /// </summary>
        /// <param name="element">The HTML element containing styles</param>
        /// <param name="cell">The table cell node to apply styles to</param>
        void ApplyInlineStyles(IElement element, TableCellNode cell);

        /// <summary>
        /// Parses a CSS size value (e.g., "10px", "5pt") to a float
        /// </summary>
        /// <param name="size">The CSS size string</param>
        /// <returns>The parsed size value, or null if parsing fails</returns>
        float? ParseSize(string size);

        /// <summary>
        /// Parses border style (width and color) from inline styles dictionary
        /// </summary>
        /// <param name="styles">Parsed inline styles map</param>
        /// <param name="borderWidth">Resulting border width</param>
        /// <param name="borderColor">Resulting border color (hex or css name)</param>
        void ParseBorder(IDictionary<string, string> styles, out float? borderWidth, out string? borderColor);

        /// <summary>
        /// Converts CSS color names (e.g., 'black') to hex acceptable by renderer
        /// </summary>
        string ConvertColorToHex(string color);
    }
}
