using NetHtml2Pdf.Parser;
using Shouldly;
using AngleSharpHtmlParser = AngleSharp.Html.Parser.HtmlParser;

namespace NetHtml2Pdf.Test.Parser;

public class CssClassStyleExtractorTests
{
    private readonly CssDeclarationParser _declarationParser = new();
    private readonly CssStyleUpdater _styleUpdater = new();

    [Fact]
    public void Extract_ShouldMergeClassDeclarationsAcrossBlocks()
    {
        const string html = """
                            <html>
                              <head>
                                <style>
                                  .title { font-weight: bold; }
                                </style>
                                <style>
                                  .title { margin-top: 10px; }
                                  .body { padding: 4px 8px; }
                                </style>
                              </head>
                            </html>
                            """;

        var document = new AngleSharpHtmlParser().ParseDocument(html);
        var extractor = new CssClassStyleExtractor(_declarationParser, _styleUpdater);

        var styles = extractor.Extract(document);

        styles.Count.ShouldBe(2);
        styles.ContainsKey("title").ShouldBeTrue();
        styles["title"].Bold.ShouldBeTrue();
        styles["title"].Margin.Top.ShouldBe(10);
        styles.ContainsKey("body").ShouldBeTrue();
        styles["body"].Padding.Top.ShouldBe(4);
        styles["body"].Padding.Right.ShouldBe(8);
    }

    [Fact]
    public void Extract_ShouldReturnEmptyWhenNoStyleBlocks()
    {
        var document = new AngleSharpHtmlParser().ParseDocument("<html></html>");
        var extractor = new CssClassStyleExtractor(_declarationParser, _styleUpdater);

        extractor.Extract(document).ShouldBeEmpty();
    }
}