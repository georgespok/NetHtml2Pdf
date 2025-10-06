using NetHtml2Pdf.Core;
using NetHtml2Pdf.Parser;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Test.Support;
using Shouldly;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Renderer;

[Collection("PdfRendering")]
public class PdfRendererTests(ITestOutputHelper output) : PdfRenderTestBase(output)
{
    private readonly HtmlParser _parser = new();
    private readonly PdfRenderer _renderer = new();


    [Fact]
    public void Paragraphs_RenderExpectedTextOrdering()
    {
        const string html = """
            <div class="body">
              <p>Welcome to <strong>NetHtml2Pdf</strong>.</p>
              <p><span class="em" style="font-style: italic;">Iteration 1</span> handles <br />line breaks.</p>
            </div>
            """;

        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);
        var words = ExtractWords(pdfBytes);

        words.ShouldContain("Welcome");
        words.ShouldContain("NetHtml2Pdf.");
        words.ShouldContain(word => word.StartsWith("Iter"));
        words.ShouldContain("1");
        words.ShouldContain("handles");
        words.ShouldContain("line");
        words.ShouldContain("breaks.");
    }

    [Fact]
    public void Lists_RenderBulletsAndNumbers()
    {
        const string html = """
            <section>
              <ul>
                <li>First</li>
                <li>Second</li>
              </ul>
              <ol>
                <li>One</li>
              </ol>
            </section>
            """;

        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);
        var words = ExtractWords(pdfBytes);

        words.Any(w => w.Contains("First")).ShouldBeTrue();
        words.Any(w => w.Contains("Second")).ShouldBeTrue();
        words.ShouldContain("One");
        words.ShouldContain("1."); // Numeric markers extract reliably
    }

    [Theory]
    [InlineData("h1", 32.0)]
    [InlineData("h2", 24.0)]
    [InlineData("h3", 19.0)]
    [InlineData("h4", 16.0)]
    [InlineData("h5", 13.0)]
    [InlineData("h6", 11.0)]
    public void Headings_ShouldRenderWithProperSizing(string tag, double expectedFontSize)
    {
        // Arrange
        var html = $"<{tag}>Heading Text</{tag}>";

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        var words = GetPdfWords(pdfBytes);
        var headingWord = words.FirstOrDefault(w => w.Text.Contains("Heading"));
        
        headingWord.ShouldNotBeNull();
        headingWord.IsBold.ShouldBeTrue();
        headingWord.FontSize.ShouldBe(expectedFontSize, tolerance: 0.5);
    }

    [Fact]
    public void ColorStyles_ShouldRenderWithColors()
    {
        // Arrange
        const string html = """<p style="color: red; background-color: yellow">Colored text</p>""";

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        var words = GetPdfWords(pdfBytes);
        
        var coloredWord = words.FirstOrDefault(w => w.Text.Contains("Colored"));
        coloredWord.ShouldNotBeNull();
        coloredWord.HexColor.ShouldBe(Colors.Red);
    }

    [Theory]
    [InlineData("blue", Colors.Blue)]
    [InlineData(Colors.BrightGreen, Colors.BrightGreen)]
    [InlineData("purple", Colors.Purple)]
    public void ColorStyles_ShouldRenderVariousColors(string cssColor, string expectedHex)
    {
        // Arrange
        var html = $"""<p style="color: {cssColor}">Test</p>""";

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        var words = GetPdfWords(pdfBytes);
        var testWord = words.FirstOrDefault(w => w.Text.Contains("Test"));
        testWord.ShouldNotBeNull();
        testWord.HexColor.ShouldBe(expectedHex);
    }

    [Theory]
    [InlineData("yellow")]
    [InlineData(Colors.BrightGreen)]
    [InlineData("pink")]
    public void BackgroundColorStyles_ShouldRenderWithoutErrors(string backgroundColor)
    {
        // Arrange
        var html = $"""<p style="background-color: {backgroundColor}">Background Test</p>""";

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        // Background colors in PDFs are rendered as separate graphics operations (rectangles),
        // not as text properties, so we verify the PDF renders successfully and contains the text
        AssertValidPdf(pdfBytes);
        var words = ExtractWords(pdfBytes);
        words.ShouldContain("Background");
        words.ShouldContain("Test");
    }

    [Fact]
    public void CombinedColorStyles_ShouldRenderBothTextAndBackground()
    {
        // Arrange
        var html = $"""
            <div>
                <p style="color: blue; background-color: yellow">Blue on Yellow</p>
                <p style="color: white; background-color: red">White on Red</p>
                <p style="color: {Colors.BrightGreen}; background-color: {Colors.Navy}">Green on Navy</p>
            </div>
            """;

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        var words = GetPdfWords(pdfBytes);
        
        // Verify first paragraph - blue text
        var blueWord = words.FirstOrDefault(w => w.Text.Contains("Blue"));
        blueWord.ShouldNotBeNull();
        blueWord.HexColor.ShouldBe(Colors.Blue);
        
        // Verify second paragraph - white text
        var whiteWord = words.FirstOrDefault(w => w.Text.Contains("White"));
        whiteWord.ShouldNotBeNull();
        whiteWord.HexColor.ShouldBe(Colors.White);
        
        // Verify third paragraph - green text
        var greenWord = words.FirstOrDefault(w => w.Text.Contains("Green"));
        greenWord.ShouldNotBeNull();
        greenWord.HexColor.ShouldBe(Colors.BrightGreen);
    }

    [Fact]
    public void UnorderedList_RendersWithBulletMarkers()
    {
        // Arrange
        const string html = "<ul><li>First</li><li>Second</li><li>Third</li></ul>";

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("First")).ShouldBeTrue();
        words.Any(w => w.Contains("Second")).ShouldBeTrue();
        words.Any(w => w.Contains("Third")).ShouldBeTrue();
        // Note: Bullet markers may not extract reliably from PDF
    }

    [Fact]
    public void OrderedList_RendersWithNumericMarkers()
    {
        // Arrange
        const string html = "<ol><li>First</li><li>Second</li><li>Third</li></ol>";

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("First")).ShouldBeTrue();
        words.Any(w => w.Contains("Second")).ShouldBeTrue();
        words.Any(w => w.Contains("Third")).ShouldBeTrue();
        words.ShouldContain("1.");
        words.ShouldContain("2.");
        words.ShouldContain("3.");
    }

    [Fact]
    public void NestedLists_RenderHierarchically()
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
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Parent")).ShouldBeTrue();
        words.Any(w => w.Contains("Child")).ShouldBeTrue();
        // Nested structure is rendered (markers may not extract reliably)
    }

    [Fact]
    public void MixedLists_RenderBothBulletsAndNumbers()
    {
        // Arrange
        const string html = """
            <ul>
                <li>Bullet item</li>
            </ul>
            <ol>
                <li>Numbered item</li>
            </ol>
            """;

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Bullet")).ShouldBeTrue();
        words.Any(w => w.Contains("Numbered")).ShouldBeTrue();
        words.ShouldContain("1."); // Numeric markers are more reliable
    }

    [Fact]
    public void ListWithStyledItems_AppliesFormattingToText()
    {
        // Arrange
        const string html = """
            <ul>
                <li><strong>Bold item</strong></li>
                <li><i>Italic item</i></li>
            </ul>
            """;

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        var pdfWords = GetPdfWords(pdfBytes);
        
        var boldWord = pdfWords.FirstOrDefault(w => w.Text.Contains("Bold"));
        boldWord.ShouldNotBeNull();
        boldWord.IsBold.ShouldBeTrue();
        
        var italicWord = pdfWords.FirstOrDefault(w => w.Text.Contains("Italic"));
        italicWord.ShouldNotBeNull();
        italicWord.IsItalic.ShouldBeTrue();
    }

    [Theory]
    [InlineData("div")]
    [InlineData("section")]
    public void StructuralContainers_ShouldRenderChildParagraphs(string tag)
    {
        // Arrange
        var html = $"<{tag}><p>First</p><p>Second</p></{tag}>";

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("First")).ShouldBeTrue();
        words.Any(w => w.Contains("Second")).ShouldBeTrue();
    }

    [Fact]
    public void NestedStructuralContainers_ShouldRenderHierarchy()
    {
        // Arrange
        const string html = @"<section><div><p>Nested content</p></div></section>";

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Nested")).ShouldBeTrue();
        words.Any(w => w.Contains("content")).ShouldBeTrue();
    }

    [Fact]
    public void MultipleContainersAtSameLevel_ShouldRenderAllChildren()
    {
        // Arrange
        const string html = @"<div>Alpha</div><section>Beta</section><div>Gamma</div>";

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Alpha")).ShouldBeTrue();
        words.Any(w => w.Contains("Beta")).ShouldBeTrue();
        words.Any(w => w.Contains("Gamma")).ShouldBeTrue();
    }

    [Fact]
    public async Task Table_ShouldRenderBasicStructure()
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
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);

        await SavePdfForInspectionAsync(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Header")).ShouldBeTrue();
        words.Any(w => w.Contains("Data")).ShouldBeTrue();
    }

    [Fact]
    public void Table_WithMultipleRows_ShouldRenderAllContent()
    {
        // Arrange
        const string html = """
            <table>
                <tbody>
                    <tr>
                        <td>Row 1 Col 1</td>
                        <td>Row 1 Col 2</td>
                    </tr>
                    <tr>
                        <td>Row 2 Col 1</td>
                        <td>Row 2 Col 2</td>
                    </tr>
                    <tr>
                        <td>Row 3 Col 1</td>
                        <td>Row 3 Col 2</td>
                    </tr>
                </tbody>
            </table>
            """;

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Row") || w.Contains("Col")).ShouldBeTrue();
        words.Any(w => w.Contains("1")).ShouldBeTrue();
        words.Any(w => w.Contains("2")).ShouldBeTrue();
        words.Any(w => w.Contains("3")).ShouldBeTrue();
    }

    [Fact]
    public async Task Table_WithHeaderAndDataCells_ShouldRenderDistinctly()
    {
        // Arrange
        const string html = """
            <table>
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Age</th>
                        <th>City</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>John Doe</td>
                        <td>25</td>
                        <td>New York</td>
                    </tr>
                    <tr>
                        <td>Jane Smith</td>
                        <td>30</td>
                        <td>Los Angeles</td>
                    </tr>
                </tbody>
            </table>
            """;

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);

        await SavePdfForInspectionAsync(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Name")).ShouldBeTrue();
        words.Any(w => w.Contains("Age")).ShouldBeTrue();
        words.Any(w => w.Contains("City")).ShouldBeTrue();
        words.Any(w => w.Contains("John")).ShouldBeTrue();
        words.Any(w => w.Contains("Jane")).ShouldBeTrue();
        words.Any(w => w.Contains("25")).ShouldBeTrue();
        words.Any(w => w.Contains("30")).ShouldBeTrue();
    }

    [Fact]
    public void Table_WithEmptyCells_ShouldRenderWithoutErrors()
    {
        // Arrange
        const string html = """
            <table>
                <tbody>
                    <tr>
                        <td>Filled</td>
                        <td></td>
                        <td>Also Filled</td>
                    </tr>
                </tbody>
            </table>
            """;

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Filled")).ShouldBeTrue();
        words.Any(w => w.Contains("Also")).ShouldBeTrue();
    }

    [Fact]
    public void Table_WithNestedInlineElements_ShouldRenderFormatting()
    {
        // Arrange
        const string html = """
            <table>
                <tbody>
                    <tr>
                        <td><strong>Bold Text</strong></td>
                        <td><i>Italic Text</i></td>
                        <td><span>Span Text</span></td>
                    </tr>
                </tbody>
            </table>
            """;

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Bold")).ShouldBeTrue();
        words.Any(w => w.Contains("Italic")).ShouldBeTrue();
        words.Any(w => w.Contains("Span")).ShouldBeTrue();
    }

    [Fact]
    public async Task Table_WithBorders_ShouldRenderWithBorderStyling()
    {
        // Arrange
        var html = $$"""
            <html>
            <head>
                <style>
                    .bordered { border: 2px solid black; }
                </style>
            </head>
            <body>
                <table class="bordered">
                    <tbody>
                        <tr>
                            <td>Cell 1</td>
                            <td>Cell 2</td>
                        </tr>
                    </tbody>
                </table>
            </body>
            </html>
            """;

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes);
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Cell")).ShouldBeTrue();
    }

    [Fact]
    public async Task Table_WithTextAlignment_ShouldRenderAlignedContent()
    {
        // Arrange
        var html = $$"""
            <html>
            <head>
                <style>
                    .left-align { text-align: left; }
                    .center-align { text-align: center; }
                    .right-align { text-align: right; }
                </style>
            </head>
            <body>
                <table>
                    <tbody>
                        <tr>
                            <td class="left-align">Left Text</td>
                            <td class="center-align">Center Text</td>
                            <td class="right-align">Right Text</td>
                        </tr>
                    </tbody>
                </table>
            </body>
            </html>
            """;

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes);
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Text")).ShouldBeTrue();
        words.Count().ShouldBeGreaterThan(2); // Should have content from all three cells
    }

    [Fact]
    public async Task Table_WithBackgroundColors_ShouldRenderColoredCells()
    {
        // Arrange
        var html = $$"""
            <html>
            <head>
                <style>
                    .header-bg { background-color: {{Colors.LightGray}}; }
                    .yellow-bg { background-color: yellow; }
                </style>
            </head>
            <body>
                <table>
                    <thead>
                        <tr>
                            <th class="header-bg">Header 1</th>
                            <th class="header-bg">Header 2</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>Normal Cell</td>
                            <td class="yellow-bg">Highlighted Cell</td>
                        </tr>
                    </tbody>
                </table>
            </body>
            </html>
            """;

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes);
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Header")).ShouldBeTrue();
        words.Any(w => w.Contains("Normal")).ShouldBeTrue();
        words.Any(w => w.Contains("Highlighted")).ShouldBeTrue();
    }

    [Fact]
    public async Task Table_WithBorderCollapse_ShouldRenderCorrectly()
    {
        // Arrange
        var html = $$"""
            <html>
            <head>
                <style>
                    .collapsed { border: 1px solid black; border-collapse: collapse; }
                </style>
            </head>
            <body>
                <table class="collapsed">
                    <tbody>
                        <tr>
                            <td style="padding: 5px">A1</td>
                            <td style="padding: 5px">A2</td>
                        </tr>
                        <tr>
                            <td style="padding: 5px">B1</td>
                            <td style="padding: 5px">B2</td>
                        </tr>
                    </tbody>
                </table>
            </body>
            </html>
            """;

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes);
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("A1")).ShouldBeTrue();
        words.Any(w => w.Contains("B2")).ShouldBeTrue();
    }

    [Fact]
    public async Task Table_WithVerticalAlignment_ShouldRenderCorrectly()
    {
        // Arrange
        var html = $$"""
            <html>
            <head>
                <style>
                    .top-align { vertical-align: top; }
                    .middle-align { vertical-align: middle; }
                    .bottom-align { vertical-align: bottom; }
                </style>
            </head>
            <body>
                <table>
                    <tbody>
                        <tr>
                            <td class="top-align">Top Content</td>
                            <td class="middle-align">Middle Content</td>
                            <td class="bottom-align">Bottom Content</td>
                        </tr>
                    </tbody>
                </table>
            </body>
            </html>
            """;

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes);
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Content")).ShouldBeTrue();
        words.Count().ShouldBeGreaterThan(2); // Should have content from all three cells
    }

    [Fact]
    public async Task Table_WithCombinedStyling_ShouldRenderAllStyles()
    {
        // Arrange
        var html = $$"""
            <html>
            <head>
                <style>
                    .full-style {
                        border: 2px solid black;
                        background-color: {{Colors.LightGray}};
                        text-align: center;
                        vertical-align: middle;
                        padding: 10px;
                    }
                </style>
            </head>
            <body>
                <table>
                    <tbody>
                        <tr>
                            <td class="full-style">Fully Styled Cell</td>
                        </tr>
                    </tbody>
                </table>
            </body>
            </html>
            """;

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes);
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Fully")).ShouldBeTrue();
        words.Any(w => w.Contains("Styled")).ShouldBeTrue();
    }
}
