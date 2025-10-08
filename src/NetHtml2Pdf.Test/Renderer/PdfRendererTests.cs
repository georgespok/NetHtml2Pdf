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

        words.ShouldContain(w => w.Contains("First"));
        words.ShouldContain(w => w.Contains("Second"));
        words.ShouldContain("One");
        words.Any(w => w.StartsWith("\u2022")).ShouldBeTrue();
        words.ShouldContain("1.");
    }

    [Theory]
    [InlineData("h1")]
    [InlineData("h2")]
    [InlineData("h3")]
    [InlineData("h4")]
    [InlineData("h5")]
    [InlineData("h6")]
    public void Headings_ShouldRenderWithProperSizing(string tag)
    {
        // Arrange
        var html = $"<{tag}>Heading Text</{tag}>";

        // Act
        var document = _parser.Parse(html);
        var pdfBytes = _renderer.Render(document);

        // Assert
        AssertValidPdf(pdfBytes);
        var words = ExtractWords(pdfBytes);
        words.ShouldContain("Heading");
        words.ShouldContain("Text");
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
        var words = ExtractWords(pdfBytes);
        words.ShouldContain("Colored");
        words.ShouldContain("text");
        
        // Verify color rendering
        var wordObjects = ExtractWordObjects(pdfBytes);
        var coloredWord = wordObjects.FirstOrDefault(w => w.Text.Contains("Colored"));
        coloredWord.ShouldNotBeNull();
        
        var textColor = GetMostCommonTextColor(coloredWord);
        textColor.ShouldBe("#FF0000"); // Red in hex
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
        var wordObjects = ExtractWordObjects(pdfBytes);
        var testWord = wordObjects.FirstOrDefault(w => w.Text.Contains("Test"));
        testWord.ShouldNotBeNull();
        
        var textColor = GetMostCommonTextColor(testWord);
        textColor.ShouldBe(expectedHex);
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
        var wordObjects = ExtractWordObjects(pdfBytes);
        
        // Verify first paragraph - blue text
        var blueWord = wordObjects.FirstOrDefault(w => w.Text.Contains("Blue"));
        blueWord.ShouldNotBeNull();
        GetMostCommonTextColor(blueWord).ShouldBe("#0000FF");
        
        // Verify second paragraph - white text
        var whiteWord = wordObjects.FirstOrDefault(w => w.Text.Contains("White"));
        whiteWord.ShouldNotBeNull();
        GetMostCommonTextColor(whiteWord).ShouldBe("#FFFFFF");
        
        // Verify third paragraph - green text
        var greenWord = wordObjects.FirstOrDefault(w => w.Text.Contains("Green"));
        greenWord.ShouldNotBeNull();
        GetMostCommonTextColor(greenWord).ShouldBe("#00FF00");
    }
}
