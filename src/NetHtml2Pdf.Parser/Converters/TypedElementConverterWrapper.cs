using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Parser.Interfaces;

namespace NetHtml2Pdf.Parser.Converters
{
    /// <summary>
    /// Wrapper class to adapt generic typed converters to the non-generic interface
    /// </summary>
    /// <typeparam name="T">The document node type</typeparam>
    public class TypedElementConverterWrapper<T>(IHtmlElementConverter<T> typedConverter, string[] supportedTags)
        : IHtmlElementConverter where T : DocumentNode
    {
        private readonly IHtmlElementConverter<T> _typedConverter = typedConverter ?? throw new ArgumentNullException(nameof(typedConverter));
        private readonly string[] _supportedTags = supportedTags ?? throw new ArgumentNullException(nameof(supportedTags));

        public DocumentNode? Convert(IElement element)
        {
            return _typedConverter.Convert(element);
        }

        public bool CanConvert(IElement element)
        {
            return _supportedTags.Contains(element.TagName.ToLowerInvariant());
        }
    }
}
