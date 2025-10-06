using NetHtml2Pdf.Core;
using NetHtml2Pdf.Parser;
using Shouldly;
using Xunit;

namespace NetHtml2Pdf.Test.Parser;

public class CssStyleUpdaterTests
{
    private readonly CssStyleUpdater _updater = new();

    [Fact]
    public void Apply_ShouldUpdateSupportedProperties()
    {
        var styles = CssStyleMap.Empty;
        styles = _updater.UpdateStyles(styles, new CssDeclaration("font-style", "italic"));
        styles = _updater.UpdateStyles(styles, new CssDeclaration("margin", "8px 4px"));
        styles = _updater.UpdateStyles(styles, new CssDeclaration("padding-left", "12px"));
        styles = _updater.UpdateStyles(styles, new CssDeclaration("text-decoration", "underline"));

        styles.FontStyle.ShouldBe(FontStyle.Italic);
        styles.Margin.Top.ShouldBe(8);
        styles.Margin.Right.ShouldBe(4);
        styles.Margin.Bottom.ShouldBe(8);
        styles.Margin.Left.ShouldBe(4);
        styles.Padding.Left.ShouldBe(12);
        styles.TextDecoration.ShouldBe(TextDecorationStyle.Underline);
    }

    [Fact]
    public void Apply_ShouldIgnoreUnsupportedProperties()
    {
        var styles = CssStyleMap.Empty;
        styles = _updater.UpdateStyles(styles, new CssDeclaration("color", "red"));

        styles.ShouldBeSameAs(CssStyleMap.Empty);
    }
}
