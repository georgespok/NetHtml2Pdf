using Shouldly;
using NetHtml2Pdf.Core.Models;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Renderer.Test
{
    public class PdfRendererTests(ITestOutputHelper output) : PdfRendererTestBase(output)
    {
        [Fact]
        public async Task Render_TextRuns_H1LargerThanH2()
        {
            // Build the SAME node shape as the parser would for headings, but only H1 and H2
            var nodes = new List<DocumentNode>();

            var h1 = new ParagraphNode();
            h1.TextRuns.Add(new TextRunNode { Text = "H1", IsBold = true });
            h1.Style.Text.FontSize = 32f;
            h1.Style.Box.MarginTop = 0.67f * 32f;
            h1.Style.Box.MarginBottom = 0.67f * 32f;
            nodes.Add(h1);

            var h2 = new ParagraphNode();
            h2.TextRuns.Add(new TextRunNode { Text = "H2", IsBold = true });
            h2.Style.Text.FontSize = 24f;
            h2.Style.Box.MarginTop = 0.83f * 24f;
            h2.Style.Box.MarginBottom = 0.83f * 24f;
            nodes.Add(h2);

            // Render parsed heading nodes
            var pdf = Renderer.RenderToPdf(nodes);
            await SavePdfForInspectionAsync(pdf);
            
            var words = GetWords(pdf);
            words.Count.ShouldBe(2);

            var fontHeights = words.Select(w => w.BoundingBox.Height).ToList();
            fontHeights[0].ShouldBeGreaterThan(fontHeights[1],
                customMessage: $"H1 height ({fontHeights[0]}) should be greater than H2 height ({fontHeights[1]})");
        }
    }
}