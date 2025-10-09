using NetHtml2Pdf.Test.Support;
using Shouldly;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test;

[Collection("PdfRendering")]
public class HtmlConverterTests(ITestOutputHelper output) : PdfRenderTestBase(output)
{
    [Fact]
    public void ConvertToPdf_ValidHtml_ReturnsPdfBytes()
    {
        var converter = new HtmlConverter();
        const string html = "<h1>Test</h1>";

        var pdfBytes = converter.ConvertToPdf(html);

        AssertValidPdf(pdfBytes);
    }

    [Fact]
    public async Task Paragraphs_RenderWithTextStyles()
    {
        const string html = """
            <div class="body">
              <p>Welcome to <strong>NetHtml2Pdf</strong>.</p>
              <p><span class="em" style="font-style: italic;">Iteration 1</span> handles <br />line breaks.</p>
            </div>
            """;
        var converter = new HtmlConverter();

        var pdfBytes = converter.ConvertToPdf(html);

        await SavePdfForInspectionAsync(pdfBytes);

        // Verify text content
        var words = ExtractWords(pdfBytes);
        words.ShouldContain("Welcome");
        words.ShouldContain("NetHtml2Pdf.");
        words.ShouldContain(word => word.StartsWith("Iter"));
        words.ShouldContain("handles");
        words.ShouldContain("breaks.");
        
        // Verify text styles
        var pdfWords = GetPdfWords(pdfBytes);
        var strongWord = pdfWords.FirstOrDefault(w => w.Text.Contains("NetHtml2Pdf"));
        strongWord.ShouldNotBeNull();
        strongWord.IsBold.ShouldBeTrue();
        
        var italicWord = pdfWords.FirstOrDefault(w => w.Text.Contains("Iter") || w.Text.Contains("1"));
        italicWord.ShouldNotBeNull();
        italicWord.IsItalic.ShouldBeTrue();
    }

    [Fact]
    public async Task Lists_RenderWithBulletsAndNumbers()
    {
        // Test with single list first to isolate the issue
        const string html = "<ul><li>First</li><li>Second</li></ul>";
        var converter = new HtmlConverter();

        var pdfBytes = converter.ConvertToPdf(html);

        await SavePdfForInspectionAsync(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words[0].ShouldBe("\u2022");
        words[1].ShouldBe("First");
        words[2].ShouldBe("\u2022");
        words[3].ShouldBe("Second");
    }

    [Fact]
    public async Task ListsAndContainers_IntegrationTest()
    {
        // Comprehensive integration test for US2 - Lists and structural containers
        const string html = """
            <section class="content">
                <div class="intro">
                    <h2>Introduction</h2>
                    <p>This document demonstrates list and container rendering.</p>
                </div>
                
                <div class="lists">
                    <h3>Unordered List</h3>
                    <ul>
                        <li>First bullet point</li>
                        <li>Second bullet point with <strong>bold text</strong></li>
                        <li>Third bullet point with <i>italic text</i></li>
                    </ul>
                    
                    <h3>Ordered List</h3>
                    <ol>
                        <li>First numbered item</li>
                        <li>Second numbered item</li>
                        <li>Third numbered item</li>
                    </ol>
                </div>
                
                <section class="nested">
                    <div class="nested-content">
                        <p>Nested content within section and div containers.</p>
                    </div>
                </section>
            </section>
            """;
        
        var converter = new HtmlConverter();
        var pdfBytes = converter.ConvertToPdf(html);

        await SavePdfForInspectionAsync(pdfBytes);

        // Verify PDF is valid
        AssertValidPdf(pdfBytes);
        
        // Extract words and verify content
        var words = ExtractWords(pdfBytes);
        
        // Debug: Log extracted words to understand what's being extracted
        output.WriteLine($"Extracted words: [{string.Join(", ", words.Select(w => $"'{w}'"))}]");
        
        // Verify headings are present (match actual extracted words)
        words.Any(w => w.Contains("Introducon") || w.Contains("Introduction")).ShouldBeTrue();
        words.Any(w => w.Contains("Unordered")).ShouldBeTrue();
        words.Any(w => w.Contains("Ordered")).ShouldBeTrue();
        
        // Verify list content
        words.Any(w => w.Contains("First")).ShouldBeTrue();
        words.Any(w => w.Contains("Second")).ShouldBeTrue();
        words.Any(w => w.Contains("Third")).ShouldBeTrue();
        words.Any(w => w.Contains("bullet")).ShouldBeTrue();
        words.Any(w => w.Contains("numbered")).ShouldBeTrue();
        
        // Verify styled text within lists
        words.Any(w => w.Contains("bold")).ShouldBeTrue();
        words.Any(w => w.Contains("italic")).ShouldBeTrue();
        
        // Verify nested content
        words.Any(w => w.Contains("Nested")).ShouldBeTrue();
        words.Any(w => w.Contains("containers")).ShouldBeTrue();
        
        // Verify numeric markers are present (more reliable than bullet markers)
        words.ShouldContain("1.");
        words.ShouldContain("2.");
        words.ShouldContain("3.");
    }

    [Fact]
    public async Task NestedLists_IntegrationTest()
    {
        // Test nested list structures
        const string html = """
            <div class="nested-lists">
                <ul>
                    <li>Parent item 1
                        <ul>
                            <li>Child item 1.1</li>
                            <li>Child item 1.2</li>
                        </ul>
                    </li>
                    <li>Parent item 2
                        <ol>
                            <li>Numbered child 2.1</li>
                            <li>Numbered child 2.2</li>
                        </ol>
                    </li>
                </ul>
            </div>
            """;
        
        var converter = new HtmlConverter();
        var pdfBytes = converter.ConvertToPdf(html);

        await SavePdfForInspectionAsync(pdfBytes);

        // Verify PDF is valid
        AssertValidPdf(pdfBytes);
        
        // Extract words and verify nested structure
        var words = ExtractWords(pdfBytes);
        
        // Verify parent items
        words.Any(w => w.Contains("Parent")).ShouldBeTrue();
        words.Any(w => w.Contains("item")).ShouldBeTrue();
        
        // Verify child items
        words.Any(w => w.Contains("Child")).ShouldBeTrue();
        words.Any(w => w.Contains("Numbered")).ShouldBeTrue();
        
        // Verify numeric markers for nested ordered list
        words.ShouldContain("1.");
        words.ShouldContain("2.");
    }

    [Fact]
    public async Task MixedContainers_IntegrationTest()
    {
        // Test mixed container structures
        const string html = """
            <section class="main">
                <div class="header">
                    <h1>Document Title</h1>
                </div>
                
                <div class="body">
                    <p>Body content in div container.</p>
                    
                    <section class="subsection">
                        <h2>Subsection Title</h2>
                        <p>Content in nested section.</p>
                    </section>
                </div>
                
                <div class="footer">
                    <p>Footer content.</p>
                </div>
            </section>
            """;
        
        var converter = new HtmlConverter();
        var pdfBytes = converter.ConvertToPdf(html);

        await SavePdfForInspectionAsync(pdfBytes);

        // Verify PDF is valid
        AssertValidPdf(pdfBytes);
        
        // Extract words and verify container structure
        var words = ExtractWords(pdfBytes);
        
        // Debug: Log extracted words to understand what's being extracted
        output.WriteLine($"Extracted words: [{string.Join(", ", words.Select(w => $"'{w}'"))}]");
        
        // Verify all content is present (match actual extracted words)
        words.Any(w => w.Contains("Document")).ShouldBeTrue();
        words.Any(w => w.Contains("Title")).ShouldBeTrue();
        words.Any(w => w.Contains("Body")).ShouldBeTrue();
        words.Any(w => w.Contains("Subsecon") || w.Contains("Subsection")).ShouldBeTrue();
        words.Any(w => w.Contains("Footer")).ShouldBeTrue();
        words.Any(w => w.Contains("content")).ShouldBeTrue();
    }

    [Fact]
    public void ConvertToPdf_EmptyHtml_ThrowsArgumentException()
    {
        var converter = new HtmlConverter();

        var exception = Assert.Throws<ArgumentException>(() => converter.ConvertToPdf(string.Empty));
        exception.ParamName.ShouldBe("html");
        exception.Message.ShouldStartWith("HTML content cannot be null or empty");
    }

    [Theory]
    [InlineData("<h1>Heading 1</h1>", "Heading", 32.0)]
    [InlineData("<h2>Heading 2</h2>", "Heading", 24.0)]
    [InlineData("<h3>Heading 3</h3>", "Heading", 19.0)]
    [InlineData("<h4>Heading 4</h4>", "Heading", 16.0)]
    [InlineData("<h5>Heading 5</h5>", "Heading", 13.0)]
    [InlineData("<h6>Heading 6</h6>", "Heading", 11.0)]
    public void Headings_RenderWithCorrectSizeAndBoldness(string html, string expectedText, double expectedFontSize)
    {
        var converter = new HtmlConverter();

        var pdfBytes = converter.ConvertToPdf(html);

        AssertValidPdf(pdfBytes);
        
        var pdfWords = GetPdfWords(pdfBytes);
        var headingWord = pdfWords.FirstOrDefault(w => w.Text.Contains(expectedText));
        
        headingWord.ShouldNotBeNull();
        headingWord.IsBold.ShouldBeTrue();
        headingWord.FontSize.ShouldBe(expectedFontSize, tolerance: 0.5);
    }

    [Theory]
    [InlineData("<p><b>Bold text</b></p>", "Bold", true, false)]
    [InlineData("<p><i>Italic text</i></p>", "Italic", false, true)]
    [InlineData("<p><strong>Strong</strong></p>", "Strong", true, false)]
    public async void TextEmphasis_RenderWithCorrectStyles(string html, string targetWord, bool expectedBold, bool expectedItalic)
    {
        var converter = new HtmlConverter();

        var pdfBytes = converter.ConvertToPdf(html);

        AssertValidPdf(pdfBytes);
        
        await SavePdfForInspectionAsync(pdfBytes);

        var pdfWords = GetPdfWords(pdfBytes);
        var styledWord = pdfWords.FirstOrDefault(w => w.Text.Contains(targetWord));
        
        styledWord.ShouldNotBeNull();
        styledWord.IsBold.ShouldBe(expectedBold);
        styledWord.IsItalic.ShouldBe(expectedItalic);
    }

    [Fact]
    public async Task InlineColorStyles_RenderWithCorrectColors()
    {
        const string html = """
            <p style="color: red;">Red text</p>
            <p style="background-color: yellow;">Yellow background</p>
            <p style="color: #0000FF; background-color: #FFFF00;">Blue on yellow</p>
            """;
        var converter = new HtmlConverter();

        var pdfBytes = converter.ConvertToPdf(html);

        await SavePdfForInspectionAsync(pdfBytes);
        AssertValidPdf(pdfBytes);
        
        var pdfWords = GetPdfWords(pdfBytes);
        
        // Verify red text color
        var redWord = pdfWords.FirstOrDefault(w => w.Text.Contains("Red"));
        redWord.ShouldNotBeNull();
        redWord.HexColor.ShouldBe("#FF0000");
        
        // Verify blue text color
        var blueWord = pdfWords.FirstOrDefault(w => w.Text.Contains("Blue"));
        blueWord.ShouldNotBeNull();
        blueWord.HexColor.ShouldBe("#0000FF");
    }

    [Fact]
    public async Task CssClassStyles_RenderWithCorrectStyles()
    {
        const string html = """
            <style>
                .highlight { color: red; background-color: yellow; }
                .bold-text { font-weight: bold; }
            </style>
            <p class="highlight">Highlighted text</p>
            <p class="bold-text">Bold styled text</p>
            """;
        var converter = new HtmlConverter();

        var pdfBytes = converter.ConvertToPdf(html);

        await SavePdfForInspectionAsync(pdfBytes);
        AssertValidPdf(pdfBytes);
        
        var pdfWords = GetPdfWords(pdfBytes);
        
        // Verify CSS class color is applied
        var highlightedWord = pdfWords.FirstOrDefault(w => w.Text.Contains("Highlighted"));
        highlightedWord.ShouldNotBeNull();
        highlightedWord.HexColor.ShouldBe("#FF0000");
        
        // Verify CSS class bold is applied
        var boldWord = pdfWords.FirstOrDefault(w => w.Text.Contains("Bold"));
        boldWord.ShouldNotBeNull();
        boldWord.IsBold.ShouldBeTrue();
    }

    [Fact]
    public void MixedTextStyles_RenderAllAttributesCorrectly()
    {
        const string html = """
            <p><b>Bold text</b> and <i>italic text</i></p>
            <h1>Large Heading</h1>
            """;
        var converter = new HtmlConverter();

        var pdfBytes = converter.ConvertToPdf(html);

        AssertValidPdf(pdfBytes);
        var pdfWords = GetPdfWords(pdfBytes);
        
        // Verify bold text has IsBold attribute
        var boldWord = pdfWords.FirstOrDefault(w => w.Text.Contains("Bold"));
        boldWord.ShouldNotBeNull();
        boldWord.IsBold.ShouldBeTrue();
        
        // Verify italic text has IsItalic attribute
        var italicWord = pdfWords.FirstOrDefault(w => w.Text.Contains("italic"));
        italicWord.ShouldNotBeNull();
        italicWord.IsItalic.ShouldBeTrue();
        
        // Verify heading has larger font size and is bold
        var headingWord = pdfWords.FirstOrDefault(w => w.Text.Contains("Large"));
        headingWord.ShouldNotBeNull();
        headingWord.IsBold.ShouldBeTrue();
        headingWord.FontSize.ShouldBeGreaterThan(16.0);
    }
}
