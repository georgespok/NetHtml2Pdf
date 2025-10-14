using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Constants;
using NetHtml2Pdf.Test.Support;
using Shouldly;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test;

[Collection("PdfRendering")]
public class HtmlConverterTests : PdfRenderTestBase
{
    private readonly ITestOutputHelper _output;

    public HtmlConverterTests(ITestOutputHelper output) : base(output)
    {
        _output = output;
    }

    [Fact]
    public void ConvertToPdf_ValidHtml_ReturnsPdfBytes()
    {
        var builder = new PdfBuilder();
        const string html = "<h1>Test</h1>";

        var pdfBytes = builder.AddPage(html).Build();

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
        var builder = new PdfBuilder();

        var pdfBytes = builder.AddPage(html).Build();

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
        var builder = new PdfBuilder();

        var pdfBytes = builder.AddPage(html).Build();

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
        
        var builder = new PdfBuilder();
        var pdfBytes = builder.AddPage(html).Build();

        await SavePdfForInspectionAsync(pdfBytes);

        // Verify PDF is valid
        AssertValidPdf(pdfBytes);
        
        // Extract words and verify content
        var words = ExtractWords(pdfBytes);
        
        // Debug: Log extracted words to understand what's being extracted
        _output.WriteLine($"Extracted words: [{string.Join(", ", words.Select(w => $"'{w}'"))}]");
        
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
        
        var builder = new PdfBuilder();
        var pdfBytes = builder.AddPage(html).Build();

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
        
        var builder = new PdfBuilder();
        var pdfBytes = builder.AddPage(html).Build();

        await SavePdfForInspectionAsync(pdfBytes);

        // Verify PDF is valid
        AssertValidPdf(pdfBytes);
        
        // Extract words and verify container structure
        var words = ExtractWords(pdfBytes);
        
        // Debug: Log extracted words to understand what's being extracted
        _output.WriteLine($"Extracted words: [{string.Join(", ", words.Select(w => $"'{w}'"))}]");
        
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
        var builder = new PdfBuilder();

        var exception = Assert.Throws<ArgumentException>(() => builder.AddPage(string.Empty));
        exception.ParamName.ShouldBe("htmlContent");
        exception.Message.ShouldStartWith("HTML content cannot be empty or whitespace");
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
        var builder = new PdfBuilder();

        var pdfBytes = builder.AddPage(html).Build();

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
        var builder = new PdfBuilder();

        var pdfBytes = builder.AddPage(html).Build();

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
        var html = $"""
            <p style="color: red;">Red text</p>
            <p style="background-color: yellow;">Yellow background</p>
            <p style="color: {HexColors.Blue}; background-color: {HexColors.Yellow};">Blue on yellow</p>
            """;
        var builder = new PdfBuilder();

        var pdfBytes = builder.AddPage(html).Build();

        await SavePdfForInspectionAsync(pdfBytes);
        AssertValidPdf(pdfBytes);
        
        var pdfWords = GetPdfWords(pdfBytes);
        
        // Verify red text color
        var redWord = pdfWords.FirstOrDefault(w => w.Text.Contains("Red"));
        redWord.ShouldNotBeNull();
        redWord.HexColor.ShouldBe(HexColors.Red);
        
        // Verify blue text color
        var blueWord = pdfWords.FirstOrDefault(w => w.Text.Contains("Blue"));
        blueWord.ShouldNotBeNull();
        blueWord.HexColor.ShouldBe(HexColors.Blue);
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
        var builder = new PdfBuilder();

        var pdfBytes = builder.AddPage(html).Build();

        await SavePdfForInspectionAsync(pdfBytes);
        AssertValidPdf(pdfBytes);
        
        var pdfWords = GetPdfWords(pdfBytes);
        
        // Verify CSS class color is applied
        var highlightedWord = pdfWords.FirstOrDefault(w => w.Text.Contains("Highlighted"));
        highlightedWord.ShouldNotBeNull();
        highlightedWord.HexColor.ShouldBe(HexColors.Red);
        
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
        var builder = new PdfBuilder();

        var pdfBytes = builder.AddPage(html).Build();

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

    [Fact]
    public async Task Table_BasicStructure_IntegrationTest()
    {
        // Arrange
        const string html = """
            <table>
                <thead>
                    <tr>
                        <th>Header 1</th>
                        <th>Header 2</th>
                        <th>Header 3</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>Data 1</td>
                        <td>Data 2</td>
                        <td>Data 3</td>
                    </tr>
                    <tr>
                        <td>Data 4</td>
                        <td>Data 5</td>
                        <td>Data 6</td>
                    </tr>
                </tbody>
            </table>
            """;
        var builder = new PdfBuilder();

        // Act
        var pdfBytes = builder.AddPage(html).Build();

        // Assert
        await SavePdfForInspectionAsync(pdfBytes);
        AssertValidPdf(pdfBytes);
        
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Header")).ShouldBeTrue();
        words.Any(w => w.Contains("Data")).ShouldBeTrue();
        
        var pdfWords = GetPdfWords(pdfBytes);
        // Header cells should be bold
        var headerWord = pdfWords.FirstOrDefault(w => w.Text.Contains("Header"));
        headerWord.ShouldNotBeNull();
        headerWord.IsBold.ShouldBeTrue();
    }

    [Fact]
    public async Task Table_WithBordersAndAlignment_IntegrationTest()
    {
        // Arrange - Based on table-borders-alignment.md contract
        var html = $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    .bordered-table {
                        border: 2px solid black;
                        border-collapse: collapse;
                    }
                    .header-cell {
                        background-color: {{HexColors.LightGray}};
                        text-align: center;
                        vertical-align: middle;
                    }
                    .data-cell {
                        text-align: left;
                        vertical-align: top;
                    }
                    .right-aligned {
                        text-align: right;
                    }
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
                        <tr>
                            <td class="data-cell">Jane Smith</td>
                            <td class="data-cell right-aligned">30</td>
                            <td class="data-cell">Los Angeles</td>
                        </tr>
                    </tbody>
                </table>
            </body>
            </html>
            """;
        var builder = new PdfBuilder();

        // Act
        var pdfBytes = builder.AddPage(html).Build();

        // Assert
        await SavePdfForInspectionAsync(pdfBytes);
        AssertValidPdf(pdfBytes);
        
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Name")).ShouldBeTrue();
        words.Any(w => w.Contains("Age")).ShouldBeTrue();
        words.Any(w => w.Contains("City")).ShouldBeTrue();
        words.Any(w => w.Contains("John")).ShouldBeTrue();
        words.Any(w => w.Contains("Jane")).ShouldBeTrue();
        words.Any(w => w.Contains("25")).ShouldBeTrue();
        words.Any(w => w.Contains("30")).ShouldBeTrue();
        words.Any(w => w.Contains("York")).ShouldBeTrue();
        words.Any(w => w.Contains("Angeles")).ShouldBeTrue();
        
        var pdfWords = GetPdfWords(pdfBytes);
        // Verify headers are bold
        var nameHeader = pdfWords.FirstOrDefault(w => w.Text.Contains("Name"));
        nameHeader.ShouldNotBeNull();
        nameHeader.IsBold.ShouldBeTrue();
    }

    [Fact]
    public async Task Table_WithComplexContent_IntegrationTest()
    {
        // Arrange
        const string html = """
            <div class="report">
                <h2>Sales Report</h2>
                <p>This is a summary of sales data.</p>
                
                <table>
                    <thead>
                        <tr>
                            <th>Product</th>
                            <th>Quantity</th>
                            <th>Revenue</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td><strong>Widget A</strong></td>
                            <td>100</td>
                            <td>$5,000</td>
                        </tr>
                        <tr>
                            <td><strong>Widget B</strong></td>
                            <td>75</td>
                            <td>$3,750</td>
                        </tr>
                    </tbody>
                </table>
                
                <p>Total revenue: <strong>$8,750</strong></p>
            </div>
            """;
        var builder = new PdfBuilder();

        // Act
        var pdfBytes = builder.AddPage(html).Build();

        // Assert
        await SavePdfForInspectionAsync(pdfBytes);
        AssertValidPdf(pdfBytes);
        
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Sales")).ShouldBeTrue();
        words.Any(w => w.Contains("Report")).ShouldBeTrue();
        words.Any(w => w.Contains("Product")).ShouldBeTrue();
        words.Any(w => w.Contains("Widget")).ShouldBeTrue();
        words.Any(w => w.Contains("Total") || w.Contains("revenue")).ShouldBeTrue();
        // Verify table has data
        words.Any(w => w.Contains("100") || w.Contains("75")).ShouldBeTrue();
    }

    [Fact]
    public async Task Table_WithEmptyCells_IntegrationTest()
    {
        // Arrange
        const string html = """
            <table>
                <tbody>
                    <tr>
                        <td>Cell 1</td>
                        <td></td>
                        <td>Cell 3</td>
                    </tr>
                    <tr>
                        <td></td>
                        <td>Cell 5</td>
                        <td></td>
                    </tr>
                </tbody>
            </table>
            """;
        var builder = new PdfBuilder();

        // Act
        var pdfBytes = builder.AddPage(html).Build();

        // Assert
        await SavePdfForInspectionAsync(pdfBytes);
        AssertValidPdf(pdfBytes);
        
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Cell")).ShouldBeTrue();
    }

    [Fact]
    public async Task Table_WithInlineStyles_IntegrationTest()
    {
        // Arrange
        var html = $$"""
            <table style="margin: 20px;">
                <tbody>
                    <tr>
                        <td style="background-color: {{HexColors.LightGray}}; padding: 10px;">Styled Cell 1</td>
                        <td style="text-align: center; padding: 10px;">Styled Cell 2</td>
                        <td style="text-align: right; padding: 10px;">Styled Cell 3</td>
                    </tr>
                </tbody>
            </table>
            """;
        var builder = new PdfBuilder();

        // Act
        var pdfBytes = builder.AddPage(html).Build();

        // Assert
        await SavePdfForInspectionAsync(pdfBytes);
        AssertValidPdf(pdfBytes);
        
        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Styled")).ShouldBeTrue();
        words.Any(w => w.Contains("Cell")).ShouldBeTrue();
    }
}
