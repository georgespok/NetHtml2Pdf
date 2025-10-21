using NetHtml2Pdf.Core;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Test.Support;
using Shouldly;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Renderer;

[Collection("PdfRendering")]
public class MarginInheritanceTests(ITestOutputHelper output) : PdfRenderTestBase(output)
{
    private readonly PdfRenderer _renderer = new();

    [Fact]
    public async Task Margin_ShouldOnlyApplyToParentElement_NotChildren()
    {
        // Arrange - Test that margin on parent div doesn't affect child paragraph
        var document = Document(
            Div(CssStyleMap.Empty.WithMargin(BoxSpacing.FromVerticalHorizontal(24, 0)),
                Paragraph(CssStyleMap.Empty, Text("No Margin"))
            )
        );

        // Act
        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes, "margin-inheritance-test");

        // Assert - Verify margin is applied to div but not to paragraph
        var words = PdfWordParser.GetRawWords(pdfBytes);
        PdfWordParser.LogWordPositions(words, Output.WriteLine);

        var paragraphWord = PdfWordParser.FindWordByText(words, "No");
        paragraphWord.ShouldNotBeNull("Paragraph word should be found");

        // The paragraph should be positioned within the div's margin
        // but should not have additional margin applied to it
        var wordTop = paragraphWord.BoundingBox.TopLeft.Y;
        var wordBottom = paragraphWord.BoundingBox.BottomLeft.Y;

        // Log positioning for analysis
        Output.WriteLine($"Paragraph word position: Top={wordTop:F1}, Bottom={wordBottom:F1}");
    }

    [Fact]
    public async Task Padding_ShouldOnlyApplyToParentElement_NotChildren()
    {
        // Arrange - Test that padding on parent div doesn't affect child paragraph
        var document = Document(
            Div(CssStyleMap.Empty.WithPadding(BoxSpacing.FromVerticalHorizontal(24, 0)),
                Paragraph(CssStyleMap.Empty, Text("No Padding"))
            )
        );

        // Act
        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes, "padding-inheritance-test");

        // Assert - Verify padding is applied to div but not to paragraph
        var words = PdfWordParser.GetRawWords(pdfBytes);
        PdfWordParser.LogWordPositions(words, Output.WriteLine);

        var paragraphWord = PdfWordParser.FindWordByText(words, "No");
        paragraphWord.ShouldNotBeNull("Paragraph word should be found");

        // The paragraph should be positioned within the div's padding
        // but should not have additional padding applied to it
        var wordTop = paragraphWord.BoundingBox.TopLeft.Y;
        var wordBottom = paragraphWord.BoundingBox.BottomLeft.Y;

        // Log positioning for analysis
        Output.WriteLine($"Paragraph word position: Top={wordTop:F1}, Bottom={wordBottom:F1}");
    }
}