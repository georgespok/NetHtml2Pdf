# Coding Standards

## Overview

This document defines the coding standards and best practices for the NetHtml2Pdf project, with a focus on maintainable, clean, and consistent code.

## Testing Standards

### Unit Test Coverage Strategy

The project uses a **coverage scoring system** to maximize meaningful coverage of business-critical and complex code paths while minimizing test volume and maintenance overhead.

#### Coverage Score Formula

For each method, calculate the coverage score using:

```
Score = (0.4 × CyclomaticComplexity)
      + (0.3 × BusinessCriticality)
      + (0.2 × ChangeFrequency)
      + (0.1 × DefectHistory)
```

#### Testing Priorities

- **Score < 5**: Low priority - No tests required
- **Score 5-7**: Medium priority - Standard test coverage
- **Score > 7**: High priority - Comprehensive test coverage (must test)

#### Implementation Rules

1. **Calculate scores** for all methods in business-critical classes
2. **Ignore Low priority methods** (Score < 5) to reduce maintenance overhead
3. **Focus testing effort** on Medium and High priority methods (Score ≥ 5)
4. **Use Theory tests** for High priority methods with multiple scenarios
5. **Regularly review scores** as code evolves

### Test Quality Standards

- **Theory tests PRIORITY**: Use `[Theory]` with `[InlineData]` for parameterized tests
- **Clean test code**: Apply same quality standards as production code
- **Minimal maintenance**: Focus on business-critical paths only
- **Regular analysis**: Review test organization and coverage quarterly

### Integration Testing Standards

#### Integration Test Coverage Strategy

Integration tests focus on **cross-module communication** with minimal number of tests and maximum coverage of important interactions.

#### Integration Score Formula

For each integration scenario, calculate the score using:

```
IntegrationScore = (0.3 × CrossModule)
                + (0.3 × ExternalDependency)
                + (0.3 × Criticality)
                + (0.1 × Transactional)
```

#### Integration Testing Priorities

- **Score ≥ 7**: High priority - Must have integration test
- **Score 5-6**: Medium priority - Desirable integration test
- **Score < 5**: Low priority - Rely on unit tests only

#### Integration Test Rules

1. **One integration test per major use case**, not per method
2. **Simulate full transaction flow**: request → domain → persistence
3. **Test the "contract" not the "internals"**
4. **Avoid duplicating unit-level assertions**; focus on integration outcomes
5. **Focus on major use cases** rather than individual method combinations
6. **Test end-to-end workflows** that span multiple modules
7. **Verify integration contracts** between components
8. **Minimize test duplication** with unit tests

### Ultimate Integration Testing Standards

#### Ultimate Test Requirements

**Ultimate integration tests** are comprehensive, end-to-end tests that validate the complete HTML-to-PDF rendering pipeline with extensive HTML/CSS coverage.

#### Ultimate Test Characteristics

- **Comprehensive HTML Coverage**: Include many (but not all) supported HTML tags and CSS attributes
- **Real-World Scenarios**: Simulate complex, realistic document structures
- **Visual Validation**: Generate PDF files saved with `SavePdfForInspectionAsync()` for visual comparison
- **Multi-Feature Integration**: Combine headers, footers, tables, lists, styling, and formatting
- **Content Verification**: Validate both structure and styling through word extraction and analysis

#### Ultimate Test Implementation Rules

1. **HTML Structure Coverage**: Include document structure, typography, lists, tables, and containers
2. **CSS Styling Coverage**: Include font, layout, visual, and table properties
3. **Multi-Page Features**: Test headers, footers, and cross-page consistency
4. **Visual Output**: Generate PDF files for manual inspection and validation
5. **Comprehensive Validation**: Verify both content and styling through automated checks
6. **Mandatory Status**: Ultimate tests are required regardless of integration score due to their comprehensive validation value

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
