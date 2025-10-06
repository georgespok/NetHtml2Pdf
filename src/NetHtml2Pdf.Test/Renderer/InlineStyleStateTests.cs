using NetHtml2Pdf.Core;
using NetHtml2Pdf.Renderer;
using Shouldly;

namespace NetHtml2Pdf.Test.Renderer;

public class InlineStyleStateTests
{
    [Fact]
    public void ApplyCss_WhenCssSpecifiesBold_ReturnsBoldState()
    {
        var css = CssStyleMap.Empty.WithBold(true);

        var result = InlineStyleState.Empty.ApplyCss(css);

        result.Bold.ShouldBeTrue();
        result.Italic.ShouldBeFalse();
        result.Underline.ShouldBeFalse();
    }

    [Fact]
    public void ApplyCss_MergesLineHeight()
    {
        var css = CssStyleMap.Empty.WithLineHeight(1.5);

        var result = InlineStyleState.Empty.ApplyCss(css);

        result.LineHeight.ShouldBe(1.5);
    }

    [Fact]
    public void WithBold_SetsBoldToTrue()
    {
        var state = InlineStyleState.Empty;

        var result = state.WithBold();

        result.Bold.ShouldBeTrue();
        result.Italic.ShouldBe(state.Italic);
        result.Underline.ShouldBe(state.Underline);
        result.LineHeight.ShouldBe(state.LineHeight);
    }
}
