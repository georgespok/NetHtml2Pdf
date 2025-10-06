using AngleSharpHtmlParser = AngleSharp.Html.Parser.HtmlParser;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Parser;
using Shouldly;

namespace NetHtml2Pdf.Test.Parser;

public class CssStyleResolverTests
{
    private readonly CssDeclarationParser _declarationParser = new();
    private readonly CssStyleUpdater _styleUpdater = new();

    [Fact]
    public void Resolve_ShouldLayerInheritedClassAndInlineStyles()
    {
        var classStyles = new Dictionary<string, CssStyleMap>(StringComparer.OrdinalIgnoreCase)
        {
            ["base"] = CssStyleMap.Empty.WithFontStyle(FontStyle.Italic).WithMarginTop(5),
            ["pad"] = CssStyleMap.Empty.WithPadding(BoxSpacing.FromAll(3))
        };

        const string html = "<div class=\"base pad\" style=\"font-style: normal; padding-left: 8px\"></div>";
        var element = new AngleSharpHtmlParser().ParseDocument(html).Body!.FirstElementChild!;

        var resolver = new CssStyleResolver(classStyles, _declarationParser, _styleUpdater);
        var inherited = CssStyleMap.Empty.WithMarginLeft(2);

        var resolved = resolver.Resolve(element, inherited);

        resolved.FontStyle.ShouldBe(FontStyle.Normal); // inline overrides class
        resolved.Margin.Top.ShouldBe(5);
        resolved.Margin.Left.ShouldBe(2);
        resolved.Padding.Left.ShouldBe(8);
        resolved.Padding.Top.ShouldBe(3);
    }
}
