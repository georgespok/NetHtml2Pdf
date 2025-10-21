using Microsoft.Extensions.Logging;
using Moq;
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
    [InlineData(CssProperties.FontStyle, CssFontValues.Italic)]
    [InlineData(CssProperties.FontStyle, CssFontValues.Normal)]
    public void Apply_ShouldUpdateFontStyleProperty(string propertyName, string value)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, value));

        // Assert
        var expectedStyle = value == CssFontValues.Italic ? FontStyle.Italic : FontStyle.Normal;
        styles.FontStyle.ShouldBe(expectedStyle);
    }

    [Theory]
    [InlineData(CssProperties.TextDecoration, CssFontValues.Underline)]
    [InlineData(CssProperties.TextDecoration, CssFontValues.Normal)]
    public void Apply_ShouldUpdateTextDecorationProperty(string propertyName, string value)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, value));

        // Assert
        var expectedDecoration =
            value == CssFontValues.Underline ? TextDecorationStyle.Underline : TextDecorationStyle.None;
        styles.TextDecoration.ShouldBe(expectedDecoration);
    }

    [Theory]
    [InlineData(CssProperties.Color, "red", HexColors.Red)]
    [InlineData(CssProperties.BackgroundColor, "yellow", HexColors.Yellow)]
    [InlineData(CssProperties.LineHeight, "1.5", null)]
    public void Apply_ShouldUpdateColorAndLineHeightProperties(string propertyName, string value,
        string? expectedNormalizedValue)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, value));

        // Assert
        if (propertyName == CssProperties.Color)
            styles.Color.ShouldBe(expectedNormalizedValue);
        else if (propertyName == CssProperties.BackgroundColor)
            styles.BackgroundColor.ShouldBe(expectedNormalizedValue);
        else if (propertyName == CssProperties.LineHeight)
            styles.LineHeight.ShouldBe(1.5);
    }

    [Theory]
    [InlineData(CssProperties.MarginTop, 10)]
    [InlineData(CssProperties.MarginRight, 20)]
    [InlineData(CssProperties.MarginBottom, 30)]
    [InlineData(CssProperties.MarginLeft, 40)]
    public void Apply_ShouldUpdateIndividualMarginProperties(string propertyName, double expectedValue)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, $"{expectedValue}px"));

        // Assert
        if (propertyName == CssProperties.MarginTop)
            styles.Margin.Top.ShouldBe(expectedValue);
        else if (propertyName == CssProperties.MarginRight)
            styles.Margin.Right.ShouldBe(expectedValue);
        else if (propertyName == CssProperties.MarginBottom)
            styles.Margin.Bottom.ShouldBe(expectedValue);
        else if (propertyName == CssProperties.MarginLeft)
            styles.Margin.Left.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData(CssProperties.PaddingTop, 5)]
    [InlineData(CssProperties.PaddingRight, 10)]
    [InlineData(CssProperties.PaddingBottom, 15)]
    [InlineData(CssProperties.PaddingLeft, 20)]
    public void Apply_ShouldUpdateIndividualPaddingProperties(string propertyName, double expectedValue)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, $"{expectedValue}px"));

        // Assert
        if (propertyName == CssProperties.PaddingTop)
            styles.Padding.Top.ShouldBe(expectedValue);
        else if (propertyName == CssProperties.PaddingRight)
            styles.Padding.Right.ShouldBe(expectedValue);
        else if (propertyName == CssProperties.PaddingBottom)
            styles.Padding.Bottom.ShouldBe(expectedValue);
        else if (propertyName == CssProperties.PaddingLeft)
            styles.Padding.Left.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData("border-radius", "5px")]
    [InlineData("box-shadow", "2px 2px 4px rgba(0,0,0,0.5)")]
    [InlineData("transform", "rotate(45deg)")]
    [InlineData("animation", "fadeIn 1s ease-in-out")]
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
    [InlineData(CssProperties.TextAlign, CssAlignmentValues.Center)]
    [InlineData(CssProperties.TextAlign, CssAlignmentValues.Left)]
    [InlineData(CssProperties.TextAlign, CssAlignmentValues.Right)]
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
    [InlineData(CssProperties.VerticalAlign, CssAlignmentValues.Top)]
    [InlineData(CssProperties.VerticalAlign, CssAlignmentValues.Middle)]
    [InlineData(CssProperties.VerticalAlign, CssAlignmentValues.Bottom)]
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
    [InlineData(CssProperties.Border, "1px solid black", 1.0, CssBorderValues.Solid, HexColors.Black)]
    [InlineData(CssProperties.Border, "2px dashed red", 2.0, CssBorderValues.Dashed, HexColors.Red)]
    [InlineData(CssProperties.Border, "3px dotted blue", 3.0, CssBorderValues.Dotted, HexColors.Blue)]
    public void Apply_ShouldUpdateBorderProperty(string propertyName, string value, double expectedWidth,
        string expectedStyle, string expectedColor)
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
    [InlineData(CssProperties.BorderCollapse, CssTableValues.Collapse)]
    [InlineData(CssProperties.BorderCollapse, CssTableValues.Separate)]
    public void Apply_ShouldUpdateBorderCollapseProperty(string propertyName, string value)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, value));

        // Assert
        styles.BorderCollapse.ShouldBe(value);
    }

    [Theory]
    [InlineData(CssProperties.Display, CssDisplayValues.Block)]
    [InlineData(CssProperties.Display, CssDisplayValues.InlineBlock)]
    [InlineData(CssProperties.Display, CssDisplayValues.None)]
    public void Apply_ShouldUpdateDisplayProperty(string propertyName, string value)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, value));

        // Assert
        styles.DisplaySet.ShouldBeTrue();
        if (value == CssDisplayValues.Block)
            styles.Display.ShouldBe(CssDisplay.Block);
        else if (value == CssDisplayValues.InlineBlock)
            styles.Display.ShouldBe(CssDisplay.InlineBlock);
        else if (value == CssDisplayValues.None)
            styles.Display.ShouldBe(CssDisplay.None);
    }

    [Theory]
    [InlineData(CssProperties.Display, "flex")]
    [InlineData(CssProperties.Display, "grid")]
    [InlineData(CssProperties.Display, "inline")]
    [InlineData(CssProperties.Display, "table")]
    [InlineData(CssProperties.Display, "invalid")]
    public void Apply_ShouldFallbackUnsupportedDisplayValues(string propertyName, string value)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, value));

        // Assert - unsupported display values should fallback to Default
        styles.Display.ShouldBe(CssDisplay.Default);
        styles.DisplaySet.ShouldBeTrue(); // Still set since we processed the display property
    }

    [Fact]
    public void Apply_ShouldUpdateMultipleTableProperties()
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(CssProperties.Border, "2px solid black"));
        styles = _updater.UpdateStyles(styles,
            new CssDeclaration(CssProperties.BorderCollapse, CssTableValues.Collapse));
        styles = _updater.UpdateStyles(styles, new CssDeclaration(CssProperties.TextAlign, CssAlignmentValues.Center));
        styles = _updater.UpdateStyles(styles,
            new CssDeclaration(CssProperties.VerticalAlign, CssAlignmentValues.Middle));

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
    public void ParseMarginShorthand_ValidValues_ExpandsCorrectly(string marginValue, double expectedTop,
        double expectedRight, double expectedBottom, double expectedLeft)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(CssProperties.Margin, marginValue));

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
        styles = _updater.UpdateStyles(styles, new CssDeclaration(CssProperties.Margin, "10px invalid"));

        // Assert - invalid values should result in empty spacing (entire declaration rejected)
        // According to CSS contract: invalid values should fall back to default/inherited values
        styles.Margin.Top.ShouldBeNull(); // fallback to null (default)
        styles.Margin.Right.ShouldBeNull(); // fallback to null (default)
        styles.Margin.Bottom.ShouldBeNull(); // fallback to null (default)
        styles.Margin.Left.ShouldBeNull(); // fallback to null (default)
    }

    [Theory]
    [InlineData("1px solid red", 1.0, CssBorderValues.Solid, HexColors.Red)] // Standard order: width, style, color
    [InlineData("solid 2px #000", 2.0, CssBorderValues.Solid, HexColors.Black)] // Alternate order: style, width, color
    [InlineData("thick dashed blue", 5.0, CssBorderValues.Dashed,
        HexColors.Blue)] // Width keywords: thick, medium, thin
    [InlineData("medium dotted green", 3.0, CssBorderValues.Dotted, HexColors.Green)] // Additional keyword test
    [InlineData("thin solid black", 1.0, CssBorderValues.Solid, HexColors.Black)] // Additional keyword test
    public void ParseBorderShorthand_ValidValues_ParsesCorrectly(string borderValue, double expectedWidth,
        string expectedStyle, string expectedColor)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(CssProperties.Border, borderValue));

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
    public void ParseBorderShorthand_PartialComponents_ParsesCorrectly(string borderValue, double? expectedWidth,
        string? expectedStyle, string? expectedColor)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(CssProperties.Border, borderValue));

        // Assert - partial border shorthand should parse available components
        styles.Border.Width.ShouldBe(expectedWidth);
        styles.Border.Style.ShouldBe(expectedStyle);
        styles.Border.Color.ShouldBe(expectedColor);
    }

    [Theory]
    [InlineData("none", null, "none", null)] // Explicit none
    [InlineData("hidden", null, "hidden", null)] // Hidden border
    public void ParseBorderShorthand_NonVisibleStyles_ParsesCorrectly(string borderValue, double? expectedWidth,
        string? expectedStyle, string? expectedColor)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(CssProperties.Border, borderValue));

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
        styles = _updater.UpdateStyles(styles, new CssDeclaration(CssProperties.Border, "99px rainbow magic"));

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
        styles = _updater.UpdateStyles(styles, new CssDeclaration(CssProperties.Border, "1px solid black"));

        // Assert - Shorthand should override all longhand values (AC-002a.9)
        styles.Border.Width.ShouldBe(1.0); // Shorthand overrides longhand
        styles.Border.Style.ShouldBe(CssBorderValues.Solid); // Shorthand overrides longhand
        styles.Border.Color.ShouldBe(HexColors.Black); // Shorthand overrides longhand
        styles.Border.IsVisible.ShouldBeTrue();
    }

    [Fact]
    public void Apply_BorderLonghandAfterShorthand_ShorthandPreserved()
    {
        // Arrange - Test cascade behavior: longhand after shorthand (current implementation behavior)
        // Note: Individual border properties (border-width, border-style, border-color) are not yet implemented
        var styles = CssStyleMap.Empty;

        // Act - Apply shorthand first, then attempt longhand (which will be ignored)
        styles = _updater.UpdateStyles(styles, new CssDeclaration(CssProperties.Border, "1px solid black"));
        styles = _updater.UpdateStyles(styles, new CssDeclaration("border-width", "5px"));

        // Assert - Current implementation preserves shorthand values since individual properties not supported
        styles.Border.Width.ShouldBe(1.0); // Shorthand value preserved (longhand ignored)
        styles.Border.Style.ShouldBe(CssBorderValues.Solid); // Shorthand value preserved
        styles.Border.Color.ShouldBe(HexColors.Black); // Shorthand value preserved
        styles.Border.IsVisible.ShouldBeTrue();

        // TODO: Implement individual border properties (border-width, border-style, border-color)
        // to support proper CSS cascade behavior per AC-002a.10
    }

    [Fact]
    public void Merge_DisplayPrecedence_InlineStyleOverridesClass()
    {
        // Arrange - Test CSS precedence: inline style overrides class-derived values
        var classStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block);
        var inlineStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.None);

        // Act - Merge inline styles (higher precedence) with class styles
        var merged = classStyles.Merge(inlineStyles);

        // Assert - Inline style should override class style
        merged.Display.ShouldBe(CssDisplay.None); // inline style wins
        merged.DisplaySet.ShouldBeTrue();
    }

    [Fact]
    public void Merge_DisplayPrecedence_ClassStyleOverridesParent()
    {
        // Arrange - Test CSS precedence: class style overrides parent default
        var parentStyles = CssStyleMap.Empty; // Default display
        var classStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock);

        // Act - Merge class styles with parent styles
        var merged = parentStyles.Merge(classStyles);

        // Assert - Class style should override parent default
        merged.Display.ShouldBe(CssDisplay.InlineBlock); // class style wins
        merged.DisplaySet.ShouldBeTrue();
    }

    [Fact]
    public void Merge_DisplayPrecedence_ParentStylePreservedWhenChildNotSet()
    {
        // Arrange - Test CSS precedence: parent style preserved when child doesn't set display
        var parentStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block);
        var childStyles = CssStyleMap.Empty; // No display set

        // Act - Merge child styles (no display) with parent styles
        var merged = parentStyles.Merge(childStyles);

        // Assert - Parent style should be preserved
        merged.Display.ShouldBe(CssDisplay.Block); // parent style preserved
        merged.DisplaySet.ShouldBeTrue(); // parent had it set
    }

    [Fact]
    public void Merge_DisplayPrecedence_MultipleClassesLaterWins()
    {
        // Arrange - Test CSS precedence: multiple classes merge in attribute order (later wins)
        var firstClass = CssStyleMap.Empty.WithDisplay(CssDisplay.Block);
        var secondClass = CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock);

        // Act - Merge second class (later in attribute order) with first class
        var merged = firstClass.Merge(secondClass);

        // Assert - Later class should win
        merged.Display.ShouldBe(CssDisplay.InlineBlock); // second class wins
        merged.DisplaySet.ShouldBeTrue();
    }

    #region Warning Tests for Unsupported Display Values

    [Fact]
    public void UpdateStyles_WithUnsupportedDisplayValue_ShouldLogWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var styles = CssStyleMap.Empty;
        var declaration = new CssDeclaration(CssProperties.Display, "flex");

        // Act
        var updatedStyles = _updater.UpdateStyles(styles, declaration, mockLogger.Object);

        // Assert
        updatedStyles.Display.ShouldBe(CssDisplay.Default);
        updatedStyles.DisplaySet.ShouldBeTrue();

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) =>
                    o.ToString()!.Contains("Unsupported CSS display value 'flex' encountered")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("grid")]
    [InlineData("inline")]
    [InlineData("table")]
    [InlineData("table-cell")]
    [InlineData("table-row")]
    [InlineData("flex")]
    [InlineData("inline-flex")]
    [InlineData("contents")]
    [InlineData("list-item")]
    [InlineData("run-in")]
    [InlineData("compact")]
    [InlineData("marker")]
    [InlineData("invalid")]
    [InlineData("unknown")]
    public void UpdateStyles_WithVariousUnsupportedDisplayValues_ShouldLogWarning(string displayValue)
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var styles = CssStyleMap.Empty;
        var declaration = new CssDeclaration(CssProperties.Display, displayValue);

        // Act
        var updatedStyles = _updater.UpdateStyles(styles, declaration, mockLogger.Object);

        // Assert
        updatedStyles.Display.ShouldBe(CssDisplay.Default);
        updatedStyles.DisplaySet.ShouldBeTrue();

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) =>
                    o.ToString()!.Contains($"Unsupported CSS display value '{displayValue}' encountered")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("default")]
    [InlineData("initial")]
    [InlineData("inherit")]
    [InlineData("unset")]
    public void UpdateStyles_WithCssKeywords_ShouldNotLogWarning(string displayValue)
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var styles = CssStyleMap.Empty;
        var declaration = new CssDeclaration(CssProperties.Display, displayValue);

        // Act
        var updatedStyles = _updater.UpdateStyles(styles, declaration, mockLogger.Object);

        // Assert
        updatedStyles.Display.ShouldBe(CssDisplay.Default);
        updatedStyles.DisplaySet.ShouldBeTrue();

        // Should not log warning for CSS keywords
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Theory]
    [InlineData(CssDisplayValues.Block)]
    [InlineData(CssDisplayValues.InlineBlock)]
    [InlineData(CssDisplayValues.None)]
    public void UpdateStyles_WithSupportedDisplayValues_ShouldNotLogWarning(string displayValue)
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var styles = CssStyleMap.Empty;
        var declaration = new CssDeclaration(CssProperties.Display, displayValue);

        // Act
        var updatedStyles = _updater.UpdateStyles(styles, declaration, mockLogger.Object);

        // Assert
        updatedStyles.DisplaySet.ShouldBeTrue();

        // Should not log warning for supported values
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void UpdateStyles_WithNullLogger_ShouldNotThrow()
    {
        // Arrange
        var styles = CssStyleMap.Empty;
        var declaration = new CssDeclaration(CssProperties.Display, "flex");

        // Act & Assert
        // Should not throw an exception when logger is null
        var updatedStyles = _updater.UpdateStyles(styles, declaration);
        updatedStyles.Display.ShouldBe(CssDisplay.Default);
        updatedStyles.DisplaySet.ShouldBeTrue();
    }

    [Fact]
    public void UpdateStyles_WithEmptyDisplayValue_ShouldNotLogWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var styles = CssStyleMap.Empty;
        var declaration = new CssDeclaration(CssProperties.Display, "");

        // Act
        var updatedStyles = _updater.UpdateStyles(styles, declaration, mockLogger.Object);

        // Assert
        updatedStyles.Display.ShouldBe(CssDisplay.Default);
        updatedStyles.DisplaySet.ShouldBeTrue();

        // Should not log warning for empty values
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void UpdateStyles_WithWhitespaceOnlyDisplayValue_ShouldNotLogWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var styles = CssStyleMap.Empty;
        var declaration = new CssDeclaration(CssProperties.Display, "   ");

        // Act
        var updatedStyles = _updater.UpdateStyles(styles, declaration, mockLogger.Object);

        // Assert
        updatedStyles.Display.ShouldBe(CssDisplay.Default);
        updatedStyles.DisplaySet.ShouldBeTrue();

        // Should not log warning for whitespace-only values
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public void UpdateStyles_WithCaseInsensitiveUnsupportedDisplayValue_ShouldLogWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var styles = CssStyleMap.Empty;
        var declaration = new CssDeclaration(CssProperties.Display, "FLEX"); // uppercase

        // Act
        var updatedStyles = _updater.UpdateStyles(styles, declaration, mockLogger.Object);

        // Assert
        updatedStyles.Display.ShouldBe(CssDisplay.Default);
        updatedStyles.DisplaySet.ShouldBeTrue();

        // Should log warning for unsupported values regardless of case
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) =>
                    o.ToString()!.Contains("Unsupported CSS display value 'flex' encountered")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void UpdateStyles_MultipleUnsupportedDisplayValues_ShouldLogMultipleWarnings()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var styles = CssStyleMap.Empty;

        // Act - Apply multiple unsupported display values
        styles = _updater.UpdateStyles(styles, new CssDeclaration(CssProperties.Display, "flex"), mockLogger.Object);
        styles = _updater.UpdateStyles(styles, new CssDeclaration(CssProperties.Display, "grid"), mockLogger.Object);
        styles = _updater.UpdateStyles(styles, new CssDeclaration(CssProperties.Display, "table"), mockLogger.Object);

        // Assert
        styles.Display.ShouldBe(CssDisplay.Default);
        styles.DisplaySet.ShouldBeTrue();

        // Should log warning for each unsupported value
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(3));
    }

    #endregion
}