using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Parser.Interfaces;

namespace NetHtml2Pdf.Parser;

internal class HtmlParser : IHtmlParser
{
    private readonly AngleSharp.Html.Parser.HtmlParser _angleSharpParser;
    private readonly ICssDeclarationParser _declarationParser;
    private readonly ICssDeclarationUpdater _declarationUpdater;
    private readonly ICssClassStyleExtractor _classStyleExtractor;

    public HtmlParser()
        : this(
            new AngleSharp.Html.Parser.HtmlParser(),
            new CssDeclarationParser(),
            new CssStyleUpdater(),
            null)
    {
    }

    internal HtmlParser(
        AngleSharp.Html.Parser.HtmlParser angleSharpParser,
        ICssDeclarationParser declarationParser,
        ICssDeclarationUpdater declarationUpdater,
        ICssClassStyleExtractor? classStyleExtractor)
    {
        ArgumentNullException.ThrowIfNull(angleSharpParser);
        ArgumentNullException.ThrowIfNull(declarationParser);
        ArgumentNullException.ThrowIfNull(declarationUpdater);

        _angleSharpParser = angleSharpParser;
        _declarationParser = declarationParser;
        _declarationUpdater = declarationUpdater;
        _classStyleExtractor = classStyleExtractor ?? new CssClassStyleExtractor(_declarationParser, _declarationUpdater);
    }

    public DocumentNode Parse(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            throw new ArgumentException("HTML content cannot be null or empty", nameof(html));
        }

        var angleSharpDocument = _angleSharpParser.ParseDocument(html);
        var classStyles = _classStyleExtractor.Extract(angleSharpDocument);
        var styleResolver = new CssStyleResolver(classStyles, _declarationParser, _declarationUpdater);
        var nodeConverter = new HtmlNodeConverter(styleResolver);

        var root = new DocumentNode(DocumentNodeType.Document);
        var body = angleSharpDocument.Body;
        if (body is null)
        {
            return root;
        }

        foreach (var child in body.ChildNodes)
        {
            var node = nodeConverter.Convert(child, CssStyleMap.Empty);
            if (node != null)
            {
                root.AddChild(node);
            }
        }

        return root;
    }
}
