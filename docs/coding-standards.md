# Coding Standards

## Overview

This document defines the coding standards and best practices for the NetHtml2Pdf project, with a focus on maintainable, clean, and consistent code.

## Constants and String Management

### Core Principle
**Reusable text strings MUST be defined in the `Core/Constants` subfolder with `NetHtml2Pdf.Core.Constants` namespace where applicable and where text is going to be reused.**

### When to Use Constants

#### ✅ Use Constants For:
- **Reusable Values**: Values used in multiple places across different classes
- **Domain-Specific Standards**: HTML tag names, CSS property names, color values
- **Cross-Cutting Concerns**: Values that span multiple layers/domains
- **Standardized Values**: Industry-standard values like CSS property names

#### ❌ Do NOT Use Constants For:
- **Context-Dependent Messages**: Error messages specific to one class's validation logic
- **Single Use**: Values used only in one place
- **Implementation Details**: File paths, connection strings, etc.
- **Localized Logic**: Business logic specific to one component

### Constants Organization

Constants MUST be organized by domain and purpose into specialized classes:

```csharp
// ✅ Good - organized by domain
namespace NetHtml2Pdf.Core.Constants;

public static class HtmlTagNames
{
    public const string Div = "DIV";
    public const string Paragraph = "P";
    public const string Table = "TABLE";
}

public static class CssProperties
{
    public const string Color = "color";
    public const string Margin = "margin";
    public const string Border = "border";
}

public static class HexColors
{
    public const string Black = "#000000";
    public const string White = "#FFFFFF";
    public const string Red = "#FF0000";
}
```

### Naming Conventions

Follow these established naming conventions:

- **`HtmlTagNames`** - HTML element names (DIV, P, TABLE, etc.)
- **`HtmlAttributes`** - HTML attribute names (class, style, colspan, etc.)
- **`CssProperties`** - CSS property names (color, margin, border, etc.)
- **`Css*Values`** - CSS value constants (CssBorderValues, CssFontValues, etc.)
- **`HexColors`** - Color constants in hex format
- **`CssRegexPatterns`** - Regex patterns used in parsing
- **`ValidationValues`** - Validation thresholds and platform names

### Examples

#### ✅ Good - Using Constants
```csharp
// Using constants for reusable values
public class HtmlNodeConverter
{
    private static readonly Dictionary<string, DocumentNodeType> ElementTypeMap = new()
    {
        [HtmlTagNames.Div] = DocumentNodeType.Div,
        [HtmlTagNames.Paragraph] = DocumentNodeType.Paragraph,
        [HtmlTagNames.Table] = DocumentNodeType.Table
    };
}

public class CssStyleUpdater
{
    public CssStyleMap UpdateStyles(CssStyleMap styles, CssDeclaration declaration)
    {
        return declaration.Name switch
        {
            CssProperties.Color => styles.WithColor(ColorNormalizer.NormalizeToHex(declaration.Value)),
            CssProperties.Margin => styles.WithMargin(ParseBoxSpacing(declaration.Value)),
            CssProperties.Border => styles.WithBorder(ParseBorderShorthand(declaration.Value)),
            _ => styles
        };
    }
}
```

#### ❌ Bad - Hardcoded Strings
```csharp
// ❌ Don't use hardcoded strings for reusable values
public class HtmlNodeConverter
{
    private static readonly Dictionary<string, DocumentNodeType> ElementTypeMap = new()
    {
        ["DIV"] = DocumentNodeType.Div,        // Hardcoded
        ["P"] = DocumentNodeType.Paragraph,    // Hardcoded
        ["TABLE"] = DocumentNodeType.Table     // Hardcoded
    };
}
```

#### ✅ Good - Local Error Messages
```csharp
// ✅ Keep context-dependent messages local
public class PdfRenderSnapshot
{
    private static TimeSpan ValidateRenderDuration(TimeSpan renderDuration)
    {
        if (renderDuration < TimeSpan.Zero)
        {
            throw new ArgumentException("RenderDuration must be non-negative", nameof(renderDuration));
        }
        return renderDuration;
    }
}
```

#### ❌ Bad - Centralized Error Messages
```csharp
// ❌ Don't centralize context-dependent messages
public class PdfRenderSnapshot
{
    private static TimeSpan ValidateRenderDuration(TimeSpan renderDuration)
    {
        if (renderDuration < TimeSpan.Zero)
        {
            throw new ArgumentException(ValidationMessages.RenderDurationMustBeNonNegative, nameof(renderDuration));
        }
        return renderDuration;
    }
}
```

## Code Quality Standards

### Clean Code Principles
- **SOLID**: Apply Single Responsibility, Open/Closed, and other SOLID principles
- **DRY**: Don't Repeat Yourself - eliminate code duplication
- **KISS**: Keep It Simple, Stupid - avoid over-engineering
- **Readability**: Code should be easy to read and understand

### Testing Standards
- **TDD Approach**: Write ONE failing test → implement minimal code to pass → refactor → repeat
- **Theory Tests Priority**: Theory tests (`[Theory]` with `[InlineData]`) are PRIORITY for parameterized testing scenarios
- **Regular Analysis**: Analyze test classes regularly to ensure optimal organization using xUnit features
- **Consolidation**: Consolidate similar test scenarios using `[Theory]` with `[InlineData]`, `[MemberData]`, or `[ClassData]`
- **Helper Methods**: Create helper methods to eliminate repetitive arrange sections
- **Short Tests**: Keep test methods short and focused (ideally under 15 lines)
- **Clean Tests**: Apply the same clean code principles to test code
- **Descriptive Names**: Use descriptive test method names following `MethodName_Scenario_ExpectedResult` pattern
- **Functional Organization**: Organize tests by functionality, not by implementation details

### Example Test Structure
```csharp
// ✅ Good - Theory test with helper methods
public class CssStyleUpdaterTests
{
    private readonly CssStyleUpdater _updater = new();

    [Theory]
    [InlineData("color", "red", HexColors.Red)]
    [InlineData("background-color", "yellow", HexColors.Yellow)]
    public void Apply_ShouldUpdateColorProperties(string propertyName, string value, string? expectedValue)
    {
        // Arrange
        var styles = CssStyleMap.Empty;

        // Act
        styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, value));

        // Assert
        if (propertyName == "color")
            styles.Color.ShouldBe(expectedValue);
        else if (propertyName == "background-color")
            styles.BackgroundColor.ShouldBe(expectedValue);
    }

    private CssStyleMap CreateEmptyStyles() => CssStyleMap.Empty;
}
```

## File Organization

### Constants Structure
```
src/NetHtml2Pdf/Core/Constants/
├── HtmlTagNames.cs          # HTML element names
├── HtmlAttributes.cs        # HTML attribute names
├── CssProperties.cs         # CSS property names
├── CssAlignmentValues.cs    # Alignment values
├── CssBorderValues.cs       # Border values
├── CssColorNames.cs         # Named colors
├── CssFontValues.cs         # Font values
├── CssTableValues.cs        # Table values
├── CssUnits.cs              # CSS units
├── CssRegexPatterns.cs      # Regex patterns
├── HexColors.cs             # Hex color constants
└── ValidationValues.cs      # Validation thresholds
```

### Import Statements
Always import the constants namespace where needed:

```csharp
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Constants;
using NetHtml2Pdf.Core.Enums;
```

## Benefits

### Maintainability
- **Single Source of Truth**: All constants defined in one organized location
- **Easy Updates**: Change a constant once, updates everywhere automatically
- **Clear Intent**: Constants have descriptive names that explain their purpose
- **Reduced Duplication**: No more repeated string literals

### Code Quality
- **Type Safety**: Compile-time validation prevents typos and inconsistencies
- **Better IntelliSense**: IDE can provide better autocomplete and suggestions
- **Consistent Naming**: Uniform patterns across all constant classes
- **Documentation**: XML comments provide clear documentation

### Developer Experience
- **Better Navigation**: Easy to find and use constants via IDE
- **Clear Organization**: Constants grouped logically by domain and purpose
- **Future-Proof**: Easy to add new constants as the application grows
- **Professional Code**: Eliminates magic strings throughout the codebase

## Conclusion

Following these coding standards ensures that the codebase remains maintainable, consistent, and professional. The key is to strike the right balance between eliminating hardcoded strings for truly reusable values while keeping context-dependent strings local to their specific classes.

Remember: **Constants for reusability, local strings for context-dependency.**
