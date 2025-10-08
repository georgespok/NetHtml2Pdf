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
        unordered.NodeType.ShouldBe(DocumentNodeType.UnorderedList);
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

    [Theory]
    [InlineData("h1", DocumentNodeType.Heading1)]
    [InlineData("h2", DocumentNodeType.Heading2)]
    [InlineData("h3", DocumentNodeType.Heading3)]
    [InlineData("h4", DocumentNodeType.Heading4)]
    [InlineData("h5", DocumentNodeType.Heading5)]
    [InlineData("h6", DocumentNodeType.Heading6)]
    public void Headings_ShouldParseToCorrectNodeType(string tag, DocumentNodeType expectedType)
    {
        // Arrange
        var html = $"<{tag}>Heading Text</{tag}>";

        // Act
        var document = _parser.Parse(html);

        // Assert
        document.Children.Single().NodeType.ShouldBe(expectedType);
    }

    [Theory]
    [InlineData("b", DocumentNodeType.Bold)]
    [InlineData("i", DocumentNodeType.Italic)]
    public void TextEmphasis_ShouldParseToCorrectNodeType(string tag, DocumentNodeType expectedType)
    {
        // Arrange
        var html = $"<p>Text with <{tag}>emphasis</{tag}>.</p>";

        // Act
        var document = _parser.Parse(html);

        // Assert
        var paragraph = document.Children.Single();
        paragraph.Children.Any(c => c.NodeType == expectedType).ShouldBeTrue();
    }

    [Fact]
    public void InlineStyles_WithColor_ShouldParseColorProperty()
    {
        // Arrange
        const string html = """<p style="color: red">Red text</p>""";

        // Act
        var document = _parser.Parse(html);

        // Assert
        var paragraph = document.Children.Single();
        paragraph.Styles.Color.ShouldBe("red");
    }

    [Fact]
    public void InlineStyles_WithBackgroundColor_ShouldParseBackgroundColorProperty()
    {
        // Arrange
        const string html = """<p style="background-color: yellow">Highlighted text</p>""";

        // Act
        var document = _parser.Parse(html);

        // Assert
        var paragraph = document.Children.Single();
        paragraph.Styles.BackgroundColor.ShouldBe("yellow");
    }

    [Fact]
    public void CssClass_WithColorProperties_ShouldResolveCorrectly()
    {
        // Arrange
        const string html = """
            <html>
            <head>
                <style>
                    .highlight { color: blue; background-color: #ffff00; }
                </style>
            </head>
            <body>
                <p class="highlight">Styled text</p>
            </body>
            </html>
            """;

        // Act
        var document = _parser.Parse(html);

        // Assert
        var paragraph = document.Children.Single();
        paragraph.Styles.Color.ShouldBe("blue");
        paragraph.Styles.BackgroundColor.ShouldBe("#ffff00");
    }
}
