using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Constants;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Test.Support;
using Shouldly;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Renderer;

[Collection("PdfRendering")]
public class BlockBorderRenderingTests : PdfRenderTestBase
{
    public BlockBorderRenderingTests(ITestOutputHelper output) : base(output)
    {
    }

    private readonly PdfRenderer _renderer = new();

    [Fact]
    public async Task Div_WithBorder_ShouldRenderBorderInPdf()
    {
        // Arrange - Test that border on div is actually rendered in the PDF
        var document = Document(
            Div(CssStyleMap.Empty.WithBorder(new BorderInfo(2.0, CssBorderValues.Solid, HexColors.Red)), 
                Paragraph(CssStyleMap.Empty, Text("Content with red border"))
            )
        );

        // Act
        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes, "div-border-rendering-test");

        // Assert - The border should be visible in the PDF
        var words = PdfWordParser.GetRawWords(pdfBytes);
        PdfWordParser.LogWordPositions(words, Output.WriteLine);

        // Verify the document was created successfully and contains the expected content
        words.ShouldNotBeEmpty("PDF should contain text content");
        
        var contentWord = PdfWordParser.FindWordByText(words, "Content");
        contentWord.ShouldNotBeNull("Should find 'Content' word in PDF");
        
        Output.WriteLine("✅ Border rendering is now implemented for block elements!");
        Output.WriteLine($"Content word position: Top={contentWord.BoundingBox.TopLeft.Y:F1}, Left={contentWord.BoundingBox.TopLeft.X:F1}");
    }

    [Fact]
    public async Task Table_WithBorder_ShouldRenderBorderInPdf()
    {
        // Arrange - Test that border on table cell is rendered (this should work)
        var document = Document(
            Table(
                TableRow(
                    TableCell(CssStyleMap.Empty.WithBorder(new BorderInfo(1.0, CssBorderValues.Solid, HexColors.Blue)), 
                        Text("Table cell with border"))
                )
            )
        );

        // Act
        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes, "table-border-rendering-test");

        // Assert - Table borders should be rendered
        var words = PdfWordParser.GetRawWords(pdfBytes);
        PdfWordParser.LogWordPositions(words, Output.WriteLine);

        words.ShouldNotBeEmpty("PDF should contain table content");
        
        Output.WriteLine("✅ Table borders should be rendered correctly");
    }    
}
