using NetHtml2Pdf.Core;
using NetHtml2Pdf.Parser;
using Shouldly;

namespace NetHtml2Pdf.Test.Parser;

public class HtmlParserTests
{
    private readonly HtmlParser _parser = new();

    [Fact]
    public void ParagraphsWithInlineStyles_ProducesInlineNodes()
    {
        const string html = """
            <div class="body">
              <p>Welcome to <strong>NetHtml2Pdf</strong>.</p>
              <p><span class="em" style="font-style: italic;">Iteration 1</span> handles <br />line breaks.</p>
            </div>
            """;

        var document = _parser.Parse(html);

        document.Children.Count.ShouldBe(1);
        var body = document.Children.Single();
        body.NodeType.ShouldBe(DocumentNodeType.Div);

        var paragraph1 = body.Children[0];
        paragraph1.NodeType.ShouldBe(DocumentNodeType.Paragraph);
        paragraph1.Children.Any(child => child.NodeType == DocumentNodeType.Text).ShouldBeTrue();
        paragraph1.Children.Any(child => child.NodeType == DocumentNodeType.Strong).ShouldBeTrue();

        var paragraph2 = body.Children[1];
        paragraph2.NodeType.ShouldBe(DocumentNodeType.Paragraph);
        paragraph2.Children.Any(child => child.NodeType == DocumentNodeType.Span).ShouldBeTrue();
        paragraph2.Children.Any(child => child.NodeType == DocumentNodeType.Text).ShouldBeTrue();
        paragraph2.Children.Any(child => child.NodeType == DocumentNodeType.LineBreak).ShouldBeTrue();

        var span = paragraph2.Children.First(child => child.NodeType == DocumentNodeType.Span);
        span.Styles.FontStyle.ShouldBe(FontStyle.Italic);
    }

    [Fact]
    public void ListsAndSection_CreateListStructureWithStyles()
    {
        const string html = """
            <html>
              <head>
                <style>
                  .article { margin-top: 12px; padding-left: 16px; }
                  .bullet { text-decoration: underline; }
                </style>
              </head>
              <body>
                <section class="article">
                  <ul class="article">
                    <li>First</li>
                    <li class="bullet">Second</li>
                  </ul>
                  <ol style="margin-top: 8px">
                    <li>One</li>
                  </ol>
                </section>
              </body>
            </html>
            """;

        var document = _parser.Parse(html);

        document.Children.Count.ShouldBe(1);
        var section = document.Children.Single();
        section.NodeType.ShouldBe(DocumentNodeType.Section);
        section.Styles.Margin.Top.ShouldBe(12);
        section.Styles.Padding.Left.ShouldBe(16);

        section.Children.Count.ShouldBe(2);
        var unordered = section.Children[0];
        unordered.NodeType.ShouldBe(DocumentNodeType.List);
        unordered.Children.Count.ShouldBe(2);

        unordered.Children[0].NodeType.ShouldBe(DocumentNodeType.ListItem);
        unordered.Children[0].Children.Single().TextContent.ShouldBe("First");

        var secondItem = unordered.Children[1];
        secondItem.NodeType.ShouldBe(DocumentNodeType.ListItem);
        secondItem.Children.Single().Styles.TextDecoration.ShouldBe(TextDecorationStyle.Underline);

        var ordered = section.Children[1];
        ordered.NodeType.ShouldBe(DocumentNodeType.OrderedList);
        ordered.Styles.Margin.Top.ShouldBe(8);
        ordered.Children.Single().Children.Single().TextContent.ShouldBe("One");
    }
}
