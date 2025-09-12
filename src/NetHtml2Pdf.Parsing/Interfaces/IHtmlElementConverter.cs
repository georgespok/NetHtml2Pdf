using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;

namespace NetHtml2Pdf.Parsing.Interfaces
{
    /// <summary>
    /// Interface for converting HTML elements to document nodes
    /// </summary>
    /// <typeparam name="T">The type of document node this converter produces</typeparam>
    public interface IHtmlElementConverter<out T> where T : DocumentNode
    {
        /// <summary>
        /// Converts an HTML element to a document node
        /// </summary>
        /// <param name="element">The HTML element to convert</param>
        /// <returns>A document node, or null if the element cannot be converted</returns>
        T? Convert(IElement element);
    }

    /// <summary>
    /// Non-generic interface for HTML element converters
    /// </summary>
    public interface IHtmlElementConverter
    {
        /// <summary>
        /// Converts an HTML element to a document node
        /// </summary>
        /// <param name="element">The HTML element to convert</param>
        /// <returns>A document node, or null if the element cannot be converted</returns>
        DocumentNode? Convert(IElement element);

        /// <summary>
        /// Determines if this converter can handle the specified element
        /// </summary>
        /// <param name="element">The HTML element to check</param>
        /// <returns>True if this converter can handle the element</returns>
        bool CanConvert(IElement element);
    }
}
