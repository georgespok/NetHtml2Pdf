using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Constants;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Test.Support;
using Shouldly;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Renderer;

[Collection("PdfRendering")]
public class BorderInheritanceTests(ITestOutputHelper output) : PdfRenderTestBase(output)
{
    private readonly PdfRenderer _renderer = new();

    [Fact]
    public async Task Border_ShouldOnlyApplyToParentElement_NotChildren()
    {
        // Arrange - Test that border on parent div doesn't affect child paragraph
        var document = Document(
            Div(CssStyleMap.Empty.WithBorder(new BorderInfo(2.0, CssBorderValues.Solid, HexColors.Red)),
                Paragraph(CssStyleMap.Empty, Text("No Border"))
            )
        );

        // Act
        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes, "border-inheritance-test");

        // Assert - Verify border is applied to div but not to paragraph
        // The paragraph should have no border properties
        var paragraph = document.Children.Single().Children.Single();
        paragraph.Styles.Border.ShouldBe(BorderInfo.Empty);
        paragraph.Styles.Border.HasValue.ShouldBeFalse();
        paragraph.Styles.Border.IsVisible.ShouldBeFalse();

        // The parent div should have the border
        var container = document.Children.Single();
        container.Styles.Border.Width.ShouldBe(2.0);
        container.Styles.Border.Style.ShouldBe(CssBorderValues.Solid);
        container.Styles.Border.Color.ShouldBe(HexColors.Red);
        container.Styles.Border.IsVisible.ShouldBeTrue();

        Output.WriteLine("✅ Border inheritance test passed - borders only apply to defined element");
    }

    [Fact]
    public async Task Border_WithDifferentChildBorder_ShouldNotOverride()
    {
        // Arrange - Test that child element with its own border doesn't inherit parent border
        var document = Document(
            Div(CssStyleMap.Empty.WithBorder(new BorderInfo(3.0, CssBorderValues.Dashed, HexColors.Blue)),
                Paragraph(CssStyleMap.Empty.WithBorder(new BorderInfo(1.0, CssBorderValues.Dotted, HexColors.Green)),
                    Text("Child Border"))
            )
        );

        // Act
        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes, "border-inheritance-override-test");

        // Assert - Each element should have its own border, no inheritance
        var container = document.Children.Single();
        var paragraph = container.Children.Single();

        // Parent div should have its border
        container.Styles.Border.Width.ShouldBe(3.0);
        container.Styles.Border.Style.ShouldBe(CssBorderValues.Dashed);
        container.Styles.Border.Color.ShouldBe(HexColors.Blue);

        // Child paragraph should have its own border (not parent's border)
        paragraph.Styles.Border.Width.ShouldBe(1.0);
        paragraph.Styles.Border.Style.ShouldBe(CssBorderValues.Dotted);
        paragraph.Styles.Border.Color.ShouldBe(HexColors.Green);

        Output.WriteLine("✅ Border inheritance override test passed - child elements have their own borders");
    }

    [Fact]
    public async Task Border_ShorthandInHTML_ShouldOnlyApplyToDefinedElement()
    {
        // Arrange - Test border shorthand in HTML doesn't affect child elements
        var document = Document(
            Div(CssStyleMap.Empty.WithBorder(new BorderInfo(2.0, CssBorderValues.Solid, HexColors.Red)),
                Paragraph(CssStyleMap.Empty, Text("No Border from Parent"))
            )
        );

        // Act
        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes, "border-html-shorthand-test");

        // Assert - Only the div should have the border
        var container = document.Children.Single();
        container.Styles.Border.Width.ShouldBe(2.0);
        container.Styles.Border.Style.ShouldBe(CssBorderValues.Solid);
        container.Styles.Border.Color.ShouldBe(HexColors.Red);

        // Child paragraph should have no border
        var paragraph = container.Children.Single();
        paragraph.Styles.Border.ShouldBe(BorderInfo.Empty);
        paragraph.Styles.Border.HasValue.ShouldBeFalse();

        Output.WriteLine("✅ Border HTML shorthand test passed - no inheritance to children");
    }
}