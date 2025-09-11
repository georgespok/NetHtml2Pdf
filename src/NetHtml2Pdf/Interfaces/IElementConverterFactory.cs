using AngleSharp.Dom;

namespace NetHtml2Pdf.Interfaces
{
    /// <summary>
    /// Factory interface for creating HTML element converters
    /// </summary>
    public interface IElementConverterFactory
    {
        /// <summary>
        /// Gets the appropriate converter for the specified HTML element
        /// </summary>
        /// <param name="element">The HTML element to convert</param>
        /// <returns>The converter that can handle the element, or null if none found</returns>
        IHtmlElementConverter? GetConverter(IElement element);

        /// <summary>
        /// Registers a new converter for specific HTML elements
        /// </summary>
        /// <param name="converter">The converter to register</param>
        void RegisterConverter(IHtmlElementConverter converter);
    }
}
