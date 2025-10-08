using NetHtml2Pdf.Core;
using Shouldly;

namespace NetHtml2Pdf.Test.Core;

public class CssStyleMapTests
{
   
    

    [Fact]
    public void CssStyleMap_ComplexScenario_ShouldWorkCorrectly()
    {
        // Arrange
        var baseMap = CssStyleMap.Empty
            .WithFontStyle(FontStyle.Italic)
            .WithBold()
            .WithTextDecoration(TextDecorationStyle.Underline)
            .WithLineHeight(1.5)
            .WithMarginTop(10)
            .WithMarginRight(20)
            .WithPaddingTop(5)
            .WithPaddingLeft(15);

        // Act & Assert
        baseMap.FontStyle.ShouldBe(FontStyle.Italic);
        baseMap.FontStyleSet.ShouldBeTrue();
        baseMap.Bold.ShouldBeTrue();
        baseMap.BoldSet.ShouldBeTrue();
        baseMap.TextDecoration.ShouldBe(TextDecorationStyle.Underline);
        baseMap.TextDecorationSet.ShouldBeTrue();
        baseMap.LineHeight.ShouldBe(1.5);
        baseMap.Margin.Top.ShouldBe(10);
        baseMap.Margin.Right.ShouldBe(20);
        baseMap.Margin.Bottom.ShouldBeNull();
        baseMap.Margin.Left.ShouldBeNull();
        baseMap.Padding.Top.ShouldBe(5);
        baseMap.Padding.Right.ShouldBeNull();
        baseMap.Padding.Bottom.ShouldBeNull();
        baseMap.Padding.Left.ShouldBe(15);
    }

    [Fact]
    public void CssStyleMap_MergeComplexScenarios_ShouldWorkCorrectly()
    {
        // Arrange
        var map1 = CssStyleMap.Empty
            .WithFontStyle(FontStyle.Italic)
            .WithBold()
            .WithMarginTop(10)
            .WithPaddingLeft(15);

        var map2 = CssStyleMap.Empty
            .WithTextDecoration(TextDecorationStyle.Underline)
            .WithLineHeight(1.5)
            .WithMarginRight(20)
            .WithPaddingTop(5);

        // Act
        var mergedMap = map1.Merge(map2);

        // Assert
        mergedMap.FontStyle.ShouldBe(FontStyle.Italic);
        mergedMap.FontStyleSet.ShouldBeTrue();
        mergedMap.Bold.ShouldBeTrue();
        mergedMap.BoldSet.ShouldBeTrue();
        mergedMap.TextDecoration.ShouldBe(TextDecorationStyle.Underline);
        mergedMap.TextDecorationSet.ShouldBeTrue();
        mergedMap.LineHeight.ShouldBe(1.5);
        mergedMap.Margin.Top.ShouldBe(10);
        mergedMap.Margin.Right.ShouldBe(20);
        mergedMap.Margin.Bottom.ShouldBeNull();
        mergedMap.Margin.Left.ShouldBeNull();
        mergedMap.Padding.Top.ShouldBe(5);
        mergedMap.Padding.Right.ShouldBeNull();
        mergedMap.Padding.Bottom.ShouldBeNull();
        mergedMap.Padding.Left.ShouldBe(15);
    }

}
