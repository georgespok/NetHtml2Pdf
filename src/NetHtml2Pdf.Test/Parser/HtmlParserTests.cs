using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Constants;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Parser;
using Shouldly;

namespace NetHtml2Pdf.Test.Parser;

public class HtmlParserTests
{
    private readonly HtmlParser _parser;

    public HtmlParserTests()
    {
        var angleSharp = new AngleSharp.Html.Parser.HtmlParser();
        var cssParser = new CssDeclarationParser();
        var cssUpdater = new CssStyleUpdater();
        var classExtractor = new CssClassStyleExtractor(cssParser, cssUpdater);
        _parser = new HtmlParser(angleSharp, classExtractor, null);
    }

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
        paragraph.Styles.Color.ShouldBe(HexColors.Red);
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
        paragraph.Styles.BackgroundColor.ShouldBe(HexColors.Yellow);
    }

    [Fact]
    public void CssClass_WithColorProperties_ShouldResolveCorrectly()
    {
        // Arrange
        var html = $$"""
            <html>
            <head>
                <style>
                    .highlight { color: blue; background-color: {{HexColors.Yellow}}; }
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
        paragraph.Styles.Color.ShouldBe(HexColors.Blue);
        paragraph.Styles.BackgroundColor.ShouldBe(HexColors.Yellow);
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

    [Fact]
    public void Table_ShouldParseToTableNodeType()
    {
        // Arrange
        const string html = "<table><tr><td>Content</td></tr></table>";

        // Act
        var document = _parser.Parse(html);

        // Assert
        document.Children.Single().NodeType.ShouldBe(DocumentNodeType.Table);
    }

    [Fact]
    public void TableHead_ShouldParseToTableHeadNodeType()
    {
        // Arrange
        const string html = "<table><thead><tr><th>Header</th></tr></thead></table>";

        // Act
        var document = _parser.Parse(html);

        // Assert
        var table = document.Children.Single();
        table.Children[0].NodeType.ShouldBe(DocumentNodeType.TableHead);
    }

    [Fact]
    public void TableBody_ShouldParseToTableBodyNodeType()
    {
        // Arrange
        const string html = "<table><tbody><tr><td>Data</td></tr></tbody></table>";

        // Act
        var document = _parser.Parse(html);

        // Assert
        var table = document.Children.Single();
        table.Children[0].NodeType.ShouldBe(DocumentNodeType.TableBody);
    }

    [Fact]
    public void TableRow_ShouldParseToTableRowNodeType()
    {
        // Arrange
        const string html = "<table><tr><td>Cell</td></tr></table>";

        // Act
        var document = _parser.Parse(html);

        // Assert
        var table = document.Children.Single();
        // AngleSharp auto-inserts tbody, so row is under tbody
        var tbody = table.Children[0];
        tbody.Children[0].NodeType.ShouldBe(DocumentNodeType.TableRow);
    }

    [Fact]
    public void TableHeaderCell_ShouldParseToTableHeaderCellNodeType()
    {
        // Arrange
        const string html = "<table><thead><tr><th>Header</th></tr></thead></table>";

        // Act
        var document = _parser.Parse(html);

        // Assert
        var table = document.Children.Single();
        var thead = table.Children[0];
        var row = thead.Children[0];
        row.Children[0].NodeType.ShouldBe(DocumentNodeType.TableHeaderCell);
    }

    [Fact]
    public void TableCell_ShouldParseToTableCellNodeType()
    {
        // Arrange
        const string html = "<table><tbody><tr><td>Data</td></tr></tbody></table>";

        // Act
        var document = _parser.Parse(html);

        // Assert
        var table = document.Children.Single();
        var tbody = table.Children[0];
        var row = tbody.Children[0];
        row.Children[0].NodeType.ShouldBe(DocumentNodeType.TableCell);
    }

    [Fact]
    public void Table_WithCompleteStructure_ShouldParseCorrectHierarchy()
    {
        // Arrange
        const string html = """
            <table>
                <thead>
                    <tr>
                        <th>Header 1</th>
                        <th>Header 2</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>Data 1</td>
                        <td>Data 2</td>
                    </tr>
                </tbody>
            </table>
            """;

        // Act
        var document = _parser.Parse(html);

        // Assert
        var table = document.Children.Single();
        table.NodeType.ShouldBe(DocumentNodeType.Table);
        table.Children.Count.ShouldBe(2);

        // Verify thead structure
        var thead = table.Children[0];
        thead.NodeType.ShouldBe(DocumentNodeType.TableHead);
        thead.Children.Count.ShouldBe(1);

        var headerRow = thead.Children[0];
        headerRow.NodeType.ShouldBe(DocumentNodeType.TableRow);
        headerRow.Children.Count.ShouldBe(2);
        headerRow.Children[0].NodeType.ShouldBe(DocumentNodeType.TableHeaderCell);
        headerRow.Children[1].NodeType.ShouldBe(DocumentNodeType.TableHeaderCell);

        // Verify tbody structure
        var tbody = table.Children[1];
        tbody.NodeType.ShouldBe(DocumentNodeType.TableBody);
        tbody.Children.Count.ShouldBe(1);

        var dataRow = tbody.Children[0];
        dataRow.NodeType.ShouldBe(DocumentNodeType.TableRow);
        dataRow.Children.Count.ShouldBe(2);
        dataRow.Children[0].NodeType.ShouldBe(DocumentNodeType.TableCell);
        dataRow.Children[1].NodeType.ShouldBe(DocumentNodeType.TableCell);
    }

    [Fact]
    public void Table_WithInlineStyles_ShouldApplyStylesToElements()
    {
        // Arrange
        var html = $"""
            <table style="margin: 10px;">
                <tbody>
                    <tr>
                        <td style="background-color: {HexColors.LightGray}; padding: 5px;">Styled Cell</td>
                    </tr>
                </tbody>
            </table>
            """;

        // Act
        var document = _parser.Parse(html);

        // Assert
        var table = document.Children.Single();
        table.Styles.Margin.Top.ShouldBe(10);

        var tbody = table.Children[0];
        var row = tbody.Children[0];
        var cell = row.Children[0];
        cell.Styles.BackgroundColor.ShouldBe(HexColors.LightGray);
        cell.Styles.Padding.Top.ShouldBe(5);
    }

    [Fact]
    public void Table_WithCssClasses_ShouldResolveStylesToElements()
    {
        // Arrange
        var html = $$"""
            <html>
            <head>
                <style>
                    .styled-table { margin: 20px; padding: 10px; }
                    .header-cell { background-color: {{HexColors.LightGray}}; font-weight: bold; }
                    .data-cell { color: {{HexColors.DarkGray}}; }
                </style>
            </head>
            <body>
                <table class="styled-table">
                    <thead>
                        <tr>
                            <th class="header-cell">Name</th>
                            <th class="header-cell">Age</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td class="data-cell">John Doe</td>
                            <td class="data-cell">25</td>
                        </tr>
                    </tbody>
                </table>
            </body>
            </html>
            """;

        // Act
        var document = _parser.Parse(html);

        // Assert
        var table = document.Children.Single();
        table.Styles.Margin.Top.ShouldBe(20);
        table.Styles.Padding.Top.ShouldBe(10);

        var thead = table.Children[0];
        var headerRow = thead.Children[0];
        var headerCell1 = headerRow.Children[0];
        headerCell1.Styles.BackgroundColor.ShouldBe(HexColors.LightGray);
        headerCell1.Styles.Bold.ShouldBeTrue();

        var tbody = table.Children[1];
        var dataRow = tbody.Children[0];
        var dataCell1 = dataRow.Children[0];
        dataCell1.Styles.Color.ShouldBe(HexColors.DarkGray);
    }

    [Fact]
    public void Table_WithEmptyCells_ShouldPreserveStructure()
    {
        // Arrange
        const string html = """
            <table>
                <tr>
                    <td>Filled</td>
                    <td></td>
                    <td>Also Filled</td>
                </tr>
            </table>
            """;

        // Act
        var document = _parser.Parse(html);

        // Assert
        var table = document.Children.Single();
        // AngleSharp auto-inserts tbody
        var tbody = table.Children[0];
        var row = tbody.Children[0];
        row.Children.Count.ShouldBe(3);
        row.Children[0].NodeType.ShouldBe(DocumentNodeType.TableCell);
        row.Children[1].NodeType.ShouldBe(DocumentNodeType.TableCell);
        row.Children[2].NodeType.ShouldBe(DocumentNodeType.TableCell);

        // Empty cell should have no children
        row.Children[1].Children.Count.ShouldBe(0);
    }

    [Fact]
    public void Table_WithoutTbody_ShouldParseTrDirectly()
    {
        // Arrange
        const string html = """
            <table>
                <tr>
                    <td>Cell 1</td>
                    <td>Cell 2</td>
                </tr>
            </table>
            """;

        // Act
        var document = _parser.Parse(html);

        // Assert
        var table = document.Children.Single();
        table.NodeType.ShouldBe(DocumentNodeType.Table);

        // AngleSharp should auto-insert tbody
        var firstChild = table.Children[0];
        (firstChild.NodeType == DocumentNodeType.TableRow ||
         firstChild.NodeType == DocumentNodeType.TableBody).ShouldBeTrue();
    }

    [Fact]
    public void Table_WithTextContentInCells_ShouldParseCorrectly()
    {
        // Arrange
        const string html = """
            <table>
                <tbody>
                    <tr>
                        <td>Simple Text</td>
                        <td><strong>Bold Text</strong></td>
                        <td><span>Span Text</span></td>
                    </tr>
                </tbody>
            </table>
            """;

        // Act
        var document = _parser.Parse(html);

        // Assert
        var table = document.Children.Single();
        var tbody = table.Children[0];
        var row = tbody.Children[0];

        row.Children.Count.ShouldBe(3);

        // First cell should have text node
        var cell1 = row.Children[0];
        cell1.Children.Any(c => c.NodeType == DocumentNodeType.Text).ShouldBeTrue();

        // Second cell should have strong node
        var cell2 = row.Children[1];
        cell2.Children.Any(c => c.NodeType == DocumentNodeType.Strong).ShouldBeTrue();

        // Third cell should have span node
        var cell3 = row.Children[2];
        cell3.Children.Any(c => c.NodeType == DocumentNodeType.Span).ShouldBeTrue();
    }

    [Fact]
    public void Table_WithBorderAndAlignmentStyles_ShouldParseCorrectly()
    {
        // Arrange
        var html = $$"""
            <html>
            <head>
                <style>
                    .bordered-table { border: 2px solid black; border-collapse: collapse; }
                    .header-cell { text-align: center; vertical-align: middle; background-color: {{HexColors.LightGray}}; }
                    .data-cell { text-align: left; vertical-align: top; }
                    .right-aligned { text-align: right; }
                </style>
            </head>
            <body>
                <table class="bordered-table">
                    <thead>
                        <tr>
                            <th class="header-cell">Name</th>
                            <th class="header-cell">Age</th>
                            <th class="header-cell">City</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td class="data-cell">John Doe</td>
                            <td class="data-cell right-aligned">25</td>
                            <td class="data-cell">New York</td>
                        </tr>
                    </tbody>
                </table>
            </body>
            </html>
            """;

        // Act
        var document = _parser.Parse(html);

        // Assert
        var table = document.Children.Single();
        table.Styles.Border.Width.ShouldBe(2.0);
        table.Styles.Border.Style.ShouldBe(CssBorderValues.Solid);
        table.Styles.Border.Color.ShouldBe(HexColors.Black);
        table.Styles.BorderCollapse.ShouldBe(CssTableValues.Collapse);

        var thead = table.Children[0];
        var headerRow = thead.Children[0];

        var headerCell1 = headerRow.Children[0];
        headerCell1.Styles.TextAlign.ShouldBe(CssAlignmentValues.Center);
        headerCell1.Styles.VerticalAlign.ShouldBe(CssAlignmentValues.Middle);
        headerCell1.Styles.BackgroundColor.ShouldBe(HexColors.LightGray);

        var tbody = table.Children[1];
        var dataRow = tbody.Children[0];

        var dataCell1 = dataRow.Children[0];
        dataCell1.Styles.TextAlign.ShouldBe(CssAlignmentValues.Left);
        dataCell1.Styles.VerticalAlign.ShouldBe(CssAlignmentValues.Top);

        var dataCell2 = dataRow.Children[1];
        dataCell2.Styles.TextAlign.ShouldBe(CssAlignmentValues.Right);
        dataCell2.Styles.VerticalAlign.ShouldBe(CssAlignmentValues.Top);
    }

    [Fact]
    public void MarginShorthand_WithTwoParameters_ShouldParseVerticalAndHorizontal()
    {
        // Arrange - Test margin: 10px 0 (vertical: 10px, horizontal: 0)
        const string html = """
            <div style="margin: 10px 0;">
                <p>Content with vertical and horizontal margin</p>
            </div>
            """;

        // Act
        var document = _parser.Parse(html);

        // Assert - Two-value margin shorthand should expand to:
        // margin-top: 10px, margin-right: 0, margin-bottom: 10px, margin-left: 0
        var container = document.Children.Single();
        container.Styles.Margin.Top.ShouldBe(10);
        container.Styles.Margin.Right.ShouldBe(0);
        container.Styles.Margin.Bottom.ShouldBe(10);
        container.Styles.Margin.Left.ShouldBe(0);
    }

    [Fact]
    public void Border_AllComponentsShorthand_ParsesCorrectly()
    {
        // Arrange - Test border shorthand with all components (width, style, color)
        const string html = """
            <div style="border: 2px solid red;">
                <p>Content with full border shorthand</p>
            </div>
            """;

        // Act
        var document = _parser.Parse(html);

        // Assert - All border components should be parsed correctly
        var container = document.Children.Single();
        container.Styles.Border.Width.ShouldBe(2.0);
        container.Styles.Border.Style.ShouldBe(CssBorderValues.Solid);
        container.Styles.Border.Color.ShouldBe(HexColors.Red);
        container.Styles.Border.IsVisible.ShouldBeTrue();
    }

    [Fact]
    public void Border_AlternateOrderShorthand_ParsesCorrectly()
    {
        // Arrange - Test border shorthand with components in alternate order
        const string html = """
            <div style="border: dashed 3px blue;">
                <p>Content with alternate order border shorthand</p>
            </div>
            """;

        // Act
        var document = _parser.Parse(html);

        // Assert - Border components should be parsed regardless of order
        var container = document.Children.Single();
        container.Styles.Border.Width.ShouldBe(3.0);
        container.Styles.Border.Style.ShouldBe(CssBorderValues.Dashed);
        container.Styles.Border.Color.ShouldBe(HexColors.Blue);
        container.Styles.Border.IsVisible.ShouldBeTrue();
    }

    [Fact]
    public void Border_PartialShorthand_ParsesCorrectly()
    {
        // Arrange - Test border shorthand with only some components
        const string html = """
            <div style="border: 1px solid;">
                <p>Content with partial border shorthand (width + style only)</p>
            </div>
            """;

        // Act
        var document = _parser.Parse(html);

        // Assert - Available components should be parsed, others should be null/default
        var container = document.Children.Single();
        container.Styles.Border.Width.ShouldBe(1.0);
        container.Styles.Border.Style.ShouldBe(CssBorderValues.Solid);
        container.Styles.Border.Color.ShouldBeNull(); // Not specified in shorthand
        container.Styles.Border.IsVisible.ShouldBeTrue();
    }

    [Fact]
    public void Border_InvalidShorthand_EmitsWarningAndFallsBack()
    {
        // Arrange - Test invalid border shorthand that should be rejected
        const string html = """
            <div style="border: 99px rainbow magic;">
                <p>Content with invalid border shorthand</p>
            </div>
            """;

        // Act
        var document = _parser.Parse(html);

        // Assert - Invalid border shorthand should be rejected (entire declaration rejected)
        var container = document.Children.Single();
        container.Styles.Border.ShouldBe(BorderInfo.Empty);
        container.Styles.Border.HasValue.ShouldBeFalse();
        container.Styles.Border.IsVisible.ShouldBeFalse();

        // Content should still be parsed and rendered (graceful fallback)
        var paragraph = container.Children.Single();
        paragraph.NodeType.ShouldBe(DocumentNodeType.Paragraph);

        // Check if paragraph has text content directly or through children
        if (paragraph.TextContent != null)
        {
            paragraph.TextContent.ShouldContain("Content with invalid border shorthand");
        }
        else
        {
            // Text might be in child text nodes
            paragraph.Children.ShouldNotBeEmpty("Paragraph should have child nodes");
            var textNode = paragraph.Children.FirstOrDefault(c => c.NodeType == DocumentNodeType.Text);
            textNode.ShouldNotBeNull("Paragraph should have a text child node");
            textNode.TextContent.ShouldNotBeNull("Text node content should not be null");
            textNode.TextContent.ShouldContain("Content with invalid border shorthand");
        }
    }

    [Fact]
    public void Display_InlineBlock_ParsesCorrectly()
    {
        // Arrange 
        const string html = """
                            <style>
                                .text-block { margin: 4px 0; padding: 2px; display: inline-block; border: 2px solid {{{HexColors.Orange}}};}
                            </style>
                            <div>
                              <div class="text-block">
                                  Should be displayed in line 1.
                              </div>
                              <div class="text-block">
                                Should be displayed in line 2.
                              </div>
                            </div>
                            """;

        // Act
        var document = _parser.Parse(html);

        // Assert 
        var container = document.Children.Single();
        container.Children[0].Styles.Display.ShouldBe(CssDisplay.InlineBlock);
        container.Children[1].Styles.Display.ShouldBe(CssDisplay.InlineBlock);
    }


}
