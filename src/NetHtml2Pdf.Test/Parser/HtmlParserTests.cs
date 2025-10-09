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

    [Theory]
    [InlineData("ul", DocumentNodeType.UnorderedList)]
    [InlineData("ol", DocumentNodeType.OrderedList)]
    public void ListElements_ShouldParseToCorrectNodeType(string tag, DocumentNodeType expectedType)
    {
        // Arrange
        var html = $"<{tag}><li>Item</li></{tag}>";

        // Act
        var document = _parser.Parse(html);

        // Assert
        var list = document.Children.Single();
        list.NodeType.ShouldBe(expectedType);
        list.Children.Count.ShouldBe(1);
        list.Children[0].NodeType.ShouldBe(DocumentNodeType.ListItem);
    }

    [Fact]
    public void ListItem_ShouldParseTextContent()
    {
        // Arrange
        const string html = "<ul><li>First item</li><li>Second item</li></ul>";

        // Act
        var document = _parser.Parse(html);

        // Assert
        var list = document.Children.Single();
        list.Children.Count.ShouldBe(2);
        
        var firstItem = list.Children[0];
        firstItem.NodeType.ShouldBe(DocumentNodeType.ListItem);
        firstItem.Children.Single().TextContent.ShouldBe("First item");
        
        var secondItem = list.Children[1];
        secondItem.NodeType.ShouldBe(DocumentNodeType.ListItem);
        secondItem.Children.Single().TextContent.ShouldBe("Second item");
    }

    [Fact]
    public void NestedLists_ShouldParseHierarchy()
    {
        // Arrange
        const string html = """
            <ul>
                <li>Parent 1
                    <ul>
                        <li>Child 1.1</li>
                        <li>Child 1.2</li>
                    </ul>
                </li>
                <li>Parent 2</li>
            </ul>
            """;

        // Act
        var document = _parser.Parse(html);

        // Assert
        var outerList = document.Children.Single();
        outerList.NodeType.ShouldBe(DocumentNodeType.UnorderedList);
        outerList.Children.Count.ShouldBe(2);
        
        var firstParent = outerList.Children[0];
        firstParent.NodeType.ShouldBe(DocumentNodeType.ListItem);
        
        var nestedList = firstParent.Children.FirstOrDefault(c => c.NodeType == DocumentNodeType.UnorderedList);
        nestedList.ShouldNotBeNull();
        nestedList.Children.Count.ShouldBe(2);
        nestedList.Children[0].Children.Single().TextContent.ShouldBe("Child 1.1");
        nestedList.Children[1].Children.Single().TextContent.ShouldBe("Child 1.2");
    }

    [Fact]
    public void ListWithInlineStyles_ShouldApplyStylesToListItems()
    {
        // Arrange
        const string html = """
            <ul style="margin-left: 20px;">
                <li style="font-weight: bold;">Bold item</li>
                <li>Normal item</li>
            </ul>
            """;

        // Act
        var document = _parser.Parse(html);

        // Assert
        var list = document.Children.Single();
        list.Styles.Margin.Left.ShouldBe(20);
        
        var boldItem = list.Children[0];
        boldItem.Styles.Bold.ShouldBeTrue();
        
        var normalItem = list.Children[1];
        normalItem.Styles.Bold.ShouldBeFalse();
    }

    [Theory]
    [InlineData("div", DocumentNodeType.Div)]
    [InlineData("section", DocumentNodeType.Section)]
    public void StructuralContainers_ShouldParseToCorrectNodeType(string tag, DocumentNodeType expectedType)
    {
        // Arrange
        var html = $"<{tag}><p>Content</p></{tag}>";

        // Act
        var document = _parser.Parse(html);

        // Assert
        var container = document.Children.Single();
        container.NodeType.ShouldBe(expectedType);
        container.Children.Count.ShouldBe(1);
        container.Children[0].NodeType.ShouldBe(DocumentNodeType.Paragraph);
    }

    [Fact]
    public void NestedStructuralContainers_ShouldParseHierarchy()
    {
        // Arrange
        const string html = """
            <section>
                <div>
                    <p>Nested content</p>
                </div>
            </section>
            """;

        // Act
        var document = _parser.Parse(html);

        // Assert
        var section = document.Children.Single();
        section.NodeType.ShouldBe(DocumentNodeType.Section);
        
        var div = section.Children.Single();
        div.NodeType.ShouldBe(DocumentNodeType.Div);
        
        var paragraph = div.Children.Single();
        paragraph.NodeType.ShouldBe(DocumentNodeType.Paragraph);
        paragraph.Children.Single().TextContent.ShouldBe("Nested content");
    }

    [Fact]
    public void StructuralContainerWithStyles_ShouldApplyPaddingAndMargin()
    {
        // Arrange
        const string html = """
            <div style="margin: 10px; padding: 20px;">
                <p>Styled content</p>
            </div>
            """;

        // Act
        var document = _parser.Parse(html);

        // Assert
        var container = document.Children.Single();
        container.Styles.Margin.Top.ShouldBe(10);
        container.Styles.Margin.Right.ShouldBe(10);
        container.Styles.Margin.Bottom.ShouldBe(10);
        container.Styles.Margin.Left.ShouldBe(10);
        container.Styles.Padding.Top.ShouldBe(20);
        container.Styles.Padding.Right.ShouldBe(20);
        container.Styles.Padding.Bottom.ShouldBe(20);
        container.Styles.Padding.Left.ShouldBe(20);
    }

    [Fact]
    public void MultipleContainersAtSameLevel_ShouldParseAllChildren()
    {
        // Arrange
        const string html = """
            <body>
                <div>First</div>
                <section>Second</section>
                <div>Third</div>
            </body>
            """;

        // Act
        var document = _parser.Parse(html);

        // Assert
        document.Children.Count.ShouldBe(3);
        document.Children[0].NodeType.ShouldBe(DocumentNodeType.Div);
        document.Children[1].NodeType.ShouldBe(DocumentNodeType.Section);
        document.Children[2].NodeType.ShouldBe(DocumentNodeType.Div);
    }
}
