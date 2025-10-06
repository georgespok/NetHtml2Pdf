using AngleSharpHtmlParser = AngleSharp.Html.Parser.HtmlParser;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Parser;
using Shouldly;

namespace NetHtml2Pdf.Test.Parser;

public class HtmlNodeConverterTests
{
    private readonly CssDeclarationParser _declarationParser = new();
    private readonly CssStyleUpdater _styleUpdater = new();

    [Fact]
    public void Convert_ShouldMapElementHierarchyAndStyles()
    {
        var classStyles = new Dictionary<string, CssStyleMap>(StringComparer.OrdinalIgnoreCase)
        {
            ["content"] = CssStyleMap.Empty.WithMarginTop(12),
            ["accent"] = CssStyleMap.Empty.WithBold()
        };

        const string html = """
            <section class="content">
              <p>Hello<br />world</p>
              <span class="accent" style="font-style: italic;">Styled</span>
            </section>
            """;

        var element = new AngleSharpHtmlParser().ParseDocument(html).Body!.FirstElementChild!;
        var resolver = new CssStyleResolver(classStyles, _declarationParser, _styleUpdater);
        var converter = new HtmlNodeConverter(resolver);

        var sectionNode = converter.Convert(element, CssStyleMap.Empty)!;

        sectionNode.NodeType.ShouldBe(DocumentNodeType.Section);
        sectionNode.Styles.Margin.Top.ShouldBe(12);
        sectionNode.Children.Count.ShouldBe(2);

        var paragraph = sectionNode.Children[0];
        paragraph.NodeType.ShouldBe(DocumentNodeType.Paragraph);
        paragraph.Children.ShouldContain(child => child.NodeType == DocumentNodeType.Text && child.TextContent!.Contains("Hello"));
        paragraph.Children.ShouldContain(child => child.NodeType == DocumentNodeType.LineBreak);

        var span = sectionNode.Children[1];
        span.NodeType.ShouldBe(DocumentNodeType.Span);
        span.Styles.Bold.ShouldBeTrue();
        span.Styles.FontStyle.ShouldBe(FontStyle.Italic);
        span.Children.Single().TextContent.ShouldBe("Styled");
    }
}
