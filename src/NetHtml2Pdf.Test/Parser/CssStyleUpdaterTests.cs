using NetHtml2Pdf.Core;
using NetHtml2Pdf.Parser;
using Shouldly;

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

    [Theory]
    [InlineData("color", "red")]
    [InlineData("background-color", "yellow")]
    [InlineData("line-height", "1.5")]
    public void Apply_ShouldUpdateColorAndLineHeightProperties(string propertyName, string value)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, value));

        // Assert
        if (propertyName == "color")
            styles.Color.ShouldBe(value);
        else if (propertyName == "background-color")
            styles.BackgroundColor.ShouldBe(value);
        else if (propertyName == "line-height")
            styles.LineHeight.ShouldBe(1.5);
    }

    [Theory]
    [InlineData("margin-top", 10)]
    [InlineData("margin-right", 20)]
    [InlineData("margin-bottom", 30)]
    [InlineData("margin-left", 40)]
    public void Apply_ShouldUpdateIndividualMarginProperties(string propertyName, double expectedValue)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, $"{expectedValue}px"));

        // Assert
        if (propertyName == "margin-top")
            styles.Margin.Top.ShouldBe(expectedValue);
        else if (propertyName == "margin-right")
            styles.Margin.Right.ShouldBe(expectedValue);
        else if (propertyName == "margin-bottom")
            styles.Margin.Bottom.ShouldBe(expectedValue);
        else if (propertyName == "margin-left")
            styles.Margin.Left.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData("padding-top", 5)]
    [InlineData("padding-right", 10)]
    [InlineData("padding-bottom", 15)]
    [InlineData("padding-left", 20)]
    public void Apply_ShouldUpdateIndividualPaddingProperties(string propertyName, double expectedValue)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, $"{expectedValue}px"));

        // Assert
        if (propertyName == "padding-top")
            styles.Padding.Top.ShouldBe(expectedValue);
        else if (propertyName == "padding-right")
            styles.Padding.Right.ShouldBe(expectedValue);
        else if (propertyName == "padding-bottom")
            styles.Padding.Bottom.ShouldBe(expectedValue);
        else if (propertyName == "padding-left")
            styles.Padding.Left.ShouldBe(expectedValue);
    }

    [Fact]
    public void Apply_ShouldIgnoreUnsupportedProperties()
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration("border-radius", "5px"));

        // Assert
        styles.ShouldBeSameAs(CssStyleMap.Empty);
    }

    [Theory]
    [InlineData("text-align", "center")]
    [InlineData("text-align", "left")]
    [InlineData("text-align", "right")]
    public void Apply_ShouldUpdateTextAlignProperty(string propertyName, string value)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, value));

        // Assert
        styles.TextAlign.ShouldBe(value);
    }

    [Theory]
    [InlineData("vertical-align", "top")]
    [InlineData("vertical-align", "middle")]
    [InlineData("vertical-align", "bottom")]
    public void Apply_ShouldUpdateVerticalAlignProperty(string propertyName, string value)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, value));

        // Assert
        styles.VerticalAlign.ShouldBe(value);
    }

    [Theory]
    [InlineData("border", "1px solid black")]
    [InlineData("border", "2px dashed red")]
    [InlineData("border", "3px dotted blue")]
    public void Apply_ShouldUpdateBorderProperty(string propertyName, string value)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, value));

        // Assert
        styles.Border.ShouldBe(value);
    }

    [Theory]
    [InlineData("border-collapse", "collapse")]
    [InlineData("border-collapse", "separate")]
    public void Apply_ShouldUpdateBorderCollapseProperty(string propertyName, string value)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, value));

        // Assert
        styles.BorderCollapse.ShouldBe(value);
    }

    [Fact]
    public void Apply_ShouldUpdateMultipleTableProperties()
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration("border", "2px solid black"));
        styles = _updater.UpdateStyles(styles, new CssDeclaration("border-collapse", "collapse"));
        styles = _updater.UpdateStyles(styles, new CssDeclaration("text-align", "center"));
        styles = _updater.UpdateStyles(styles, new CssDeclaration("vertical-align", "middle"));

        // Assert
        styles.Border.ShouldBe("2px solid black");
        styles.BorderCollapse.ShouldBe("collapse");
        styles.TextAlign.ShouldBe("center");
        styles.VerticalAlign.ShouldBe("middle");
    }
}
