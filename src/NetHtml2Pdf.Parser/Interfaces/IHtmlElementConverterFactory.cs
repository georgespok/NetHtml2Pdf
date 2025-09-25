using AngleSharp.Dom;

namespace NetHtml2Pdf.Parser.Interfaces
{
    /// <summary>
    /// Factory interface for creating HTML element converters
    /// </summary>
    public interface IHtmlElementConverterFactory
    {
        /// <summary>
        /// Gets the appropriate converter for the specified HTML element
        /// </summary>
        /// <param name="element">The HTML element to convert</param>
        /// <returns>The converter that can handle the element</returns>
        IHtmlElementConverter GetConverter(IElement element);

        /// <summary>
        /// Registers a converter for specific HTML element types
        /// </summary>
        /// <param name="converter">The converter to register</param>
        void RegisterConverter(IHtmlElementConverter converter);
    }
}
