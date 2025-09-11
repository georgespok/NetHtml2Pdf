using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using HtmlElement = AngleSharp.Dom.IElement;

namespace NetHtml2Pdf.Converters
{
    /// <summary>
    /// Base class for HTML element converters providing common functionality
    /// </summary>
    public abstract class BaseHtmlElementConverter : Interfaces.IHtmlElementConverter
    {
        /// <summary>
        /// Gets the tag names this converter can handle
        /// </summary>
        protected abstract string[] SupportedTags { get; }

        /// <summary>
        /// Determines if this converter can handle the specified HTML element
        /// </summary>
        /// <param name="element">The HTML element to check</param>
        /// <returns>True if this converter can handle the element</returns>
        public virtual bool CanConvert(HtmlElement element)
        {
            return SupportedTags.Contains(element.TagName.ToLowerInvariant());
        }

        /// <summary>
        /// Converts an HTML element to a QuestPDF element
        /// </summary>
        /// <param name="element">The HTML element to convert</param>
        /// <param name="container">The QuestPDF container to add the converted element to</param>
        public abstract void Convert(HtmlElement element, IContainer container);

        /// <summary>
        /// Gets the text content from an element, handling nested elements
        /// </summary>
        /// <param name="element">The element to extract text from</param>
        /// <returns>The text content</returns>
        protected virtual string GetTextContent(HtmlElement element)
        {
            return element.TextContent.Trim();
        }

        /// <summary>
        /// Applies default styling to a container
        /// </summary>
        /// <param name="container">The container to style</param>
        /// <returns>The styled container</returns>
        protected virtual IContainer ApplyDefaultStyle(IContainer container)
        {
            return container.Padding(5);
        }
    }
}
