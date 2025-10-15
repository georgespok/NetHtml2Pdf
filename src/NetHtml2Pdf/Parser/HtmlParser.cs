using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Parser.Interfaces;

namespace NetHtml2Pdf.Parser;

internal class HtmlParser : IHtmlParser
{
    private readonly AngleSharp.Html.Parser.HtmlParser _angleSharpParser;
    private readonly ICssClassStyleExtractor _classStyleExtractor;
    private readonly Action<string>? _onFallbackElement;

    internal HtmlParser(
        AngleSharp.Html.Parser.HtmlParser angleSharpParser,
        ICssClassStyleExtractor classStyleExtractor,
        Action<string>? onFallbackElement)
    {
        ArgumentNullException.ThrowIfNull(angleSharpParser);
        ArgumentNullException.ThrowIfNull(classStyleExtractor);

        _angleSharpParser = angleSharpParser;
        _classStyleExtractor = classStyleExtractor;
        _onFallbackElement = onFallbackElement;
    }

    public DocumentNode Parse(string html, ILogger? logger = null)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            throw new ArgumentException("HTML content cannot be null or empty", nameof(html));
        }

        var angleSharpDocument = _angleSharpParser.ParseDocument(html);
        var classStyles = _classStyleExtractor.Extract(angleSharpDocument, logger);
        var styleResolver = new CssStyleResolver(classStyles, _classStyleExtractor.DeclarationParser, _classStyleExtractor.DeclarationUpdater);
        var nodeConverter = new HtmlNodeConverter(styleResolver, _onFallbackElement);

        var root = new DocumentNode(DocumentNodeType.Document);
        var body = angleSharpDocument.Body;
        if (body is null)
        {
            return root;
        }

        foreach (var child in body.ChildNodes)
        {
            var node = nodeConverter.Convert(child, CssStyleMap.Empty, logger);
            if (node != null)
            {
                root.AddChild(node);
            }
        }

        return root;
    }
}
