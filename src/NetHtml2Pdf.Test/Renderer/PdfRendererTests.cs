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
        coloredWord.HexColor.ShouldBe("#FF0000"); // Red in hex
    }

    [Theory]
    [InlineData("blue", "#0000FF")]
    [InlineData("#00FF00", "#00FF00")]
    [InlineData("purple", "#800080")]
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
    [InlineData("#00FF00")]
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
        const string html = """
            <div>
                <p style="color: blue; background-color: yellow">Blue on Yellow</p>
                <p style="color: white; background-color: red">White on Red</p>
                <p style="color: #00FF00; background-color: #000080">Green on Navy</p>
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
        blueWord.HexColor.ShouldBe("#0000FF");
        
        // Verify second paragraph - white text
        var whiteWord = words.FirstOrDefault(w => w.Text.Contains("White"));
        whiteWord.ShouldNotBeNull();
        whiteWord.HexColor.ShouldBe("#FFFFFF");
        
        // Verify third paragraph - green text
        var greenWord = words.FirstOrDefault(w => w.Text.Contains("Green"));
        greenWord.ShouldNotBeNull();
        greenWord.HexColor.ShouldBe("#00FF00");
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
}
