using AngleSharp.Dom;
using QuestPDF.Infrastructure;
using HtmlElement = AngleSharp.Dom.IElement;

namespace NetHtml2Pdf.Interfaces
{
    /// <summary>
    /// Interface for converting HTML elements to QuestPDF elements
    /// </summary>
    public interface IHtmlElementConverter
    {
        /// <summary>
        /// Determines if this converter can handle the specified HTML element
        /// </summary>
        /// <param name="element">The HTML element to check</param>
        /// <returns>True if this converter can handle the element</returns>
        bool CanConvert(HtmlElement element);

        /// <summary>
        /// Converts an HTML element to a QuestPDF element
        /// </summary>
        /// <param name="element">The HTML element to convert</param>
        /// <param name="container">The QuestPDF container to add the converted element to</param>
        void Convert(HtmlElement element, IContainer container);
    }
}
