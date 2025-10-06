using NetHtml2Pdf.Test.Support;
using Shouldly;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test;

[Collection("PdfRendering")]
public class HtmlConverterTests(ITestOutputHelper output) : PdfRenderingTestBase(output)
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
    public async Task Iteration1_Paragraphs_RenderConsistentPdf()
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

        var words = PdfParser.ExtractWords(pdfBytes);
        words.ShouldContain("Welcome");
        words.ShouldContain("NetHtml2Pdf.");
        words.ShouldContain(word => word.StartsWith("Iter"));
        words.ShouldContain("1");
        words.ShouldContain("handles");
        words.ShouldContain("line");
        words.ShouldContain("breaks.");
    }

    [Fact]
    public async Task Iteration1_Lists_RenderConsistentPdf()
    {
        const string html = """
            <section class="content">
              <ul>
                <li>First</li>
                <li>Second</li>
              </ul>
              <ol>
                <li>One</li>
              </ol>
            </section>
            """;
        var converter = new HtmlConverter();

        var pdfBytes = converter.ConvertToPdf(html);

        await SavePdfForInspectionAsync(pdfBytes);

        var words = PdfParser.ExtractWords(pdfBytes);
        words.ShouldContain("First");
        words.ShouldContain("Second");
        words.ShouldContain("One");
        words.Any(w => w.StartsWith("\u2022")).ShouldBeTrue();
        words.ShouldContain("1.");
    }

    [Fact]
    public void ConvertToPdf_EmptyHtml_ThrowsArgumentException()
    {
        var converter = new HtmlConverter();

        var exception = Assert.Throws<ArgumentException>(() => converter.ConvertToPdf(string.Empty));
        exception.ParamName.ShouldBe("html");
        exception.Message.ShouldStartWith("HTML content cannot be null or empty");
    }
}
