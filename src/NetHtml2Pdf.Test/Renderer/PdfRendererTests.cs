using NetHtml2Pdf.Test.Support;
using Shouldly;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Renderer;

[Collection("PdfRendering")]
public class PdfRendererTests(ITestOutputHelper output) : PdfRenderingTestBase(output)
{
    [Fact]
    public void Paragraphs_RenderExpectedTextOrdering()
    {
        const string html = """
            <div class="body">
              <p>Welcome to <strong>NetHtml2Pdf</strong>.</p>
              <p><span class="em" style="font-style: italic;">Iteration 1</span> handles <br />line breaks.</p>
            </div>
            """;

        var words = RenderWords(html);

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

        var words = RenderWords(html);

        words.ShouldContain("First");
        words.ShouldContain("Second");
        words.ShouldContain("One");
        words.Any(w => w.StartsWith("\u2022")).ShouldBeTrue();
        words.ShouldContain("1.");
    }
}
