using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Constants;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Parser;
using Shouldly;

namespace NetHtml2Pdf.Test.Parser;

public class CssStyleUpdaterTests
{
    private readonly CssStyleUpdater _updater = new();

    [Theory]
    [InlineData("font-style", "italic")]
    [InlineData("font-style", "normal")]
    public void Apply_ShouldUpdateFontStyleProperty(string propertyName, string value)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, value));

        // Assert
        var expectedStyle = value == "italic" ? FontStyle.Italic : FontStyle.Normal;
        styles.FontStyle.ShouldBe(expectedStyle);
    }

    [Theory]
    [InlineData("text-decoration", "underline")]
    [InlineData("text-decoration", "none")]
    public void Apply_ShouldUpdateTextDecorationProperty(string propertyName, string value)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, value));

        // Assert
        var expectedDecoration = value == "underline" ? TextDecorationStyle.Underline : TextDecorationStyle.None;
        styles.TextDecoration.ShouldBe(expectedDecoration);
    }

    [Theory]
    [InlineData("color", "red", HexColors.Red)]
    [InlineData("background-color", "yellow", HexColors.Yellow)]
    [InlineData("line-height", "1.5", null)]
    public void Apply_ShouldUpdateColorAndLineHeightProperties(string propertyName, string value, string? expectedNormalizedValue)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, value));

        // Assert
        if (propertyName == "color")
            styles.Color.ShouldBe(expectedNormalizedValue);
        else if (propertyName == "background-color")
            styles.BackgroundColor.ShouldBe(expectedNormalizedValue);
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

    [Theory]
    [InlineData("border-radius", "5px")]
    [InlineData("box-shadow", "2px 2px 4px rgba(0,0,0,0.5)")]
    [InlineData("transform", "rotate(45deg)")]
    [InlineData("animation", "fadeIn 1s ease-in-out")]
    [InlineData("display", "flex")]
    public void Apply_ShouldIgnoreUnsupportedProperties(string propertyName, string value)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, value));

        // Assert - unsupported properties should be ignored (styles unchanged)
        styles.ShouldBeSameAs(CssStyleMap.Empty);
    }

    [Theory]
    [InlineData("text-align", CssAlignmentValues.Center)]
    [InlineData("text-align", CssAlignmentValues.Left)]
    [InlineData("text-align", CssAlignmentValues.Right)]
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
    [InlineData("vertical-align", CssAlignmentValues.Top)]
    [InlineData("vertical-align", CssAlignmentValues.Middle)]
    [InlineData("vertical-align", CssAlignmentValues.Bottom)]
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
    [InlineData("border", "1px solid black", 1.0, CssBorderValues.Solid, HexColors.Black)]
    [InlineData("border", "2px dashed red", 2.0, CssBorderValues.Dashed, HexColors.Red)]
    [InlineData("border", "3px dotted blue", 3.0, CssBorderValues.Dotted, HexColors.Blue)]
    public void Apply_ShouldUpdateBorderProperty(string propertyName, string value, double expectedWidth, string expectedStyle, string expectedColor)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, value));

        // Assert - border should be parsed into components
        styles.Border.Width.ShouldBe(expectedWidth);
        styles.Border.Style.ShouldBe(expectedStyle);
        styles.Border.Color.ShouldBe(expectedColor);
    }

    [Theory]
    [InlineData("border-collapse", CssTableValues.Collapse)]
    [InlineData("border-collapse", CssTableValues.Separate)]
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
        styles = _updater.UpdateStyles(styles, new CssDeclaration("border-collapse", CssTableValues.Collapse));
        styles = _updater.UpdateStyles(styles, new CssDeclaration("text-align", CssAlignmentValues.Center));
        styles = _updater.UpdateStyles(styles, new CssDeclaration("vertical-align", CssAlignmentValues.Middle));

        // Assert
        styles.Border.Width.ShouldBe(2.0);
        styles.Border.Style.ShouldBe(CssBorderValues.Solid);
        styles.Border.Color.ShouldBe(HexColors.Black);
        styles.BorderCollapse.ShouldBe(CssTableValues.Collapse);
        styles.TextAlign.ShouldBe(CssAlignmentValues.Center);
        styles.VerticalAlign.ShouldBe(CssAlignmentValues.Middle);
    }

    [Theory]
    [InlineData("10px", 10, 10, 10, 10)] // 1 value: expands to all sides
    [InlineData("10px 20px", 10, 20, 10, 20)] // 2 values: vertical/horizontal
    [InlineData("10px 20px 30px", 10, 20, 30, 20)] // 3 values: top/horizontal/bottom
    [InlineData("10px 20px 30px 40px", 10, 20, 30, 40)] // 4 values: TRBL
    public void ParseMarginShorthand_ValidValues_ExpandsCorrectly(string marginValue, double expectedTop, double expectedRight, double expectedBottom, double expectedLeft)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration("margin", marginValue));

        // Assert - margin values should expand according to CSS shorthand rules
        styles.Margin.Top.ShouldBe(expectedTop);
        styles.Margin.Right.ShouldBe(expectedRight);
        styles.Margin.Bottom.ShouldBe(expectedBottom);
        styles.Margin.Left.ShouldBe(expectedLeft);
    }

    [Fact]
    public void ParseMarginShorthand_InvalidValue_ReturnsNull()
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration("margin", "10px invalid"));

        // Assert - invalid values should result in empty spacing (entire declaration rejected)
        // According to CSS contract: invalid values should fall back to default/inherited values
        styles.Margin.Top.ShouldBeNull();      // fallback to null (default)
        styles.Margin.Right.ShouldBeNull();    // fallback to null (default)
        styles.Margin.Bottom.ShouldBeNull();   // fallback to null (default)
        styles.Margin.Left.ShouldBeNull();     // fallback to null (default)
    }

    [Theory]
    [InlineData("1px solid red", 1.0, CssBorderValues.Solid, HexColors.Red)] // Standard order: width, style, color
    [InlineData("solid 2px #000", 2.0, CssBorderValues.Solid, HexColors.Black)] // Alternate order: style, width, color
    [InlineData("thick dashed blue", 5.0, CssBorderValues.Dashed, HexColors.Blue)] // Width keywords: thick, medium, thin
    [InlineData("medium dotted green", 3.0, CssBorderValues.Dotted, HexColors.Green)] // Additional keyword test
    [InlineData("thin solid black", 1.0, CssBorderValues.Solid, HexColors.Black)] // Additional keyword test
    public void ParseBorderShorthand_ValidValues_ParsesCorrectly(string borderValue, double expectedWidth, string expectedStyle, string expectedColor)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration("border", borderValue));

        // Assert - border shorthand should be parsed into components
        styles.Border.Width.ShouldBe(expectedWidth);
        styles.Border.Style.ShouldBe(expectedStyle);
        styles.Border.Color.ShouldBe(expectedColor);
        styles.Border.IsVisible.ShouldBeTrue();
    }

    [Theory]
    [InlineData("2px solid", 2.0, CssBorderValues.Solid, null)] // Width + Style only
    [InlineData("dashed red", null, CssBorderValues.Dashed, HexColors.Red)] // Style + Color only
    [InlineData("solid", null, CssBorderValues.Solid, null)] // Style only
    [InlineData("5px", 5.0, null, null)] // Width only
    [InlineData("blue", null, null, HexColors.Blue)] // Color only
    public void ParseBorderShorthand_PartialComponents_ParsesCorrectly(string borderValue, double? expectedWidth, string? expectedStyle, string? expectedColor)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration("border", borderValue));

        // Assert - partial border shorthand should parse available components
        styles.Border.Width.ShouldBe(expectedWidth);
        styles.Border.Style.ShouldBe(expectedStyle);
        styles.Border.Color.ShouldBe(expectedColor);
    }

    [Theory]
    [InlineData("none", null, "none", null)] // Explicit none
    [InlineData("hidden", null, "hidden", null)] // Hidden border
    public void ParseBorderShorthand_NonVisibleStyles_ParsesCorrectly(string borderValue, double? expectedWidth, string? expectedStyle, string? expectedColor)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration("border", borderValue));

        // Assert - non-visible border styles should parse but not be visible
        styles.Border.Width.ShouldBe(expectedWidth);
        styles.Border.Style.ShouldBe(expectedStyle);
        styles.Border.Color.ShouldBe(expectedColor);
        styles.Border.IsVisible.ShouldBeFalse();
    }

    [Fact]
    public void ParseBorderShorthand_InvalidValue_ReturnsEmpty()
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration("border", "99px rainbow magic"));

        // Assert - invalid border shorthand should be rejected (entire declaration rejected)
        styles.Border.ShouldBe(BorderInfo.Empty);
        styles.Border.HasValue.ShouldBeFalse();
        styles.Border.IsVisible.ShouldBeFalse();
    }

    [Fact]
    public void Apply_BorderShorthandAfterLonghand_ShorthandWins()
    {
        // Arrange - Test cascade behavior: shorthand after longhand should override all border properties
        var styles = CssStyleMap.Empty;

        // Act - Apply longhand first, then shorthand
        styles = _updater.UpdateStyles(styles, new CssDeclaration("border-width", "5px"));
        styles = _updater.UpdateStyles(styles, new CssDeclaration("border-style", "dashed"));
        styles = _updater.UpdateStyles(styles, new CssDeclaration("border-color", "yellow"));
        styles = _updater.UpdateStyles(styles, new CssDeclaration("border", "1px solid black"));

        // Assert - Shorthand should override all longhand values (AC-002a.9)
        styles.Border.Width.ShouldBe(1.0);      // Shorthand overrides longhand
        styles.Border.Style.ShouldBe(CssBorderValues.Solid);  // Shorthand overrides longhand
        styles.Border.Color.ShouldBe(HexColors.Black);        // Shorthand overrides longhand
        styles.Border.IsVisible.ShouldBeTrue();
    }

    [Fact]
    public void Apply_BorderLonghandAfterShorthand_ShorthandPreserved()
    {
        // Arrange - Test cascade behavior: longhand after shorthand (current implementation behavior)
        // Note: Individual border properties (border-width, border-style, border-color) are not yet implemented
        var styles = CssStyleMap.Empty;

        // Act - Apply shorthand first, then attempt longhand (which will be ignored)
        styles = _updater.UpdateStyles(styles, new CssDeclaration("border", "1px solid black"));
        styles = _updater.UpdateStyles(styles, new CssDeclaration("border-width", "5px"));

        // Assert - Current implementation preserves shorthand values since individual properties not supported
        styles.Border.Width.ShouldBe(1.0);      // Shorthand value preserved (longhand ignored)
        styles.Border.Style.ShouldBe(CssBorderValues.Solid);  // Shorthand value preserved
        styles.Border.Color.ShouldBe(HexColors.Black);        // Shorthand value preserved
        styles.Border.IsVisible.ShouldBeTrue();
        
        // TODO: Implement individual border properties (border-width, border-style, border-color)
        // to support proper CSS cascade behavior per AC-002a.10
    }
}
