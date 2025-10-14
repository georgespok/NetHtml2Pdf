# Testing Guidelines

## Overview

This document provides practical guidance on what to test and what not to test, based on the complexity and business value of the code. **Theory tests (`[Theory]` with `[InlineData]`) are PRIORITY for parameterized testing scenarios.**

## Tests as First-Class Code

**Tests MUST be treated with the same quality standards as production code.**

### Clean Code Principles for Tests

- **SOLID**: Apply Single Responsibility, Open/Closed, and other SOLID principles
- **DRY**: Don't Repeat Yourself - eliminate code duplication
- **KISS**: Keep It Simple, Stupid - avoid over-engineering tests
- **Readability**: Tests should be easy to read and understand

### Theory Tests Priority

**Theory tests (`[Theory]` with `[InlineData]`) are PRIORITY for parameterized testing scenarios.** Analyze test classes regularly to ensure optimal organization using xUnit features.

#### When to Use Theory Tests
- ✅ **Use `[Theory]` with `[InlineData]`**: For simple parameterized tests with 2-5 parameters
- ✅ **Use `[Theory]` with `[MemberData]`**: For complex test data or dynamic data generation
- ✅ **Use `[Theory]` with `[ClassData]`**: For reusable test data classes
- ✅ **Consolidate Similar Scenarios**: Multiple test cases that follow the same pattern
- ❌ **Don't Use `[Fact]`**: For scenarios that can be parameterized

#### Theory Test Examples
```csharp
// ✅ PRIORITY - Theory test with InlineData
[Theory]
[InlineData("color", "red", HexColors.Red)]
[InlineData("background-color", "yellow", HexColors.Yellow)]
[InlineData("border", "1px solid black", 1.0, CssBorderValues.Solid, HexColors.Black)]
public void Apply_ShouldUpdateCssProperties(string propertyName, string value, object expectedValue)
{
    // Arrange
    var styles = CssStyleMap.Empty;
    
    // Act
    styles = _updater.UpdateStyles(styles, new CssDeclaration(propertyName, value));
    
    // Assert
    // Assert based on propertyName and expectedValue
}

// ✅ Good - Theory test with MemberData for complex data
[Theory]
[MemberData(nameof(GetBorderTestData))]
public void ParseBorderShorthand_WithVariousInputs_ShouldParseCorrectly(string input, double expectedWidth, string expectedStyle, string expectedColor)
{
    // Test implementation
}

public static IEnumerable<object[]> GetBorderTestData()
{
    yield return new object[] { "1px solid red", 1.0, CssBorderValues.Solid, HexColors.Red };
    yield return new object[] { "thick dashed blue", 5.0, CssBorderValues.Dashed, HexColors.Blue };
    yield return new object[] { "medium dotted green", 3.0, CssBorderValues.Dotted, HexColors.Green };
}

// ❌ Bad - Multiple individual Fact tests
[Fact] public void Apply_WithColorRed_ShouldSetRed() { }
[Fact] public void Apply_WithColorBlue_ShouldSetBlue() { }
[Fact] public void Apply_WithColorGreen_ShouldSetGreen() { }
```

2. **Create Helper Methods**: Eliminate repetitive arrange sections
   ```csharp
   // ✅ Good - helper method
   private MyClass CreateInstance(int value = 10) => new MyClass(value);
   
   // ❌ Bad - repeated setup in every test
   var instance = new MyClass(10);
   ```

3. **Keep Tests Short**: Ideally under 15 lines per test method

4. **Use Test Data Builders**: For complex object creation

5. **Refactor Tests**: Apply the same refactoring standards as production code

## Test Analysis and Refactoring

### Regular Test Analysis
**Analyze test classes regularly to ensure optimal organization using xUnit features.**

#### Analysis Checklist
- [ ] **Identify Duplication**: Look for repeated test patterns that can be consolidated
- [ ] **Find Parameterization Opportunities**: Convert multiple `[Fact]` tests to `[Theory]` tests
- [ ] **Evaluate Test Data**: Consider using `[MemberData]` or `[ClassData]` for complex scenarios
- [ ] **Check Helper Methods**: Ensure repetitive arrange sections are eliminated
- [ ] **Review Test Names**: Ensure descriptive naming following `MethodName_Scenario_ExpectedResult` pattern
- [ ] **Assess Organization**: Group tests by functionality, not implementation details

#### Refactoring Examples
```csharp
// ❌ Before - Multiple Fact tests with duplication
[Fact]
public void ParseMarginShorthand_OneValue_ShouldApplyToAllSides()
{
    var result = ParseMarginShorthand("10px");
    result.Top.ShouldBe(10);
    result.Right.ShouldBe(10);
    result.Bottom.ShouldBe(10);
    result.Left.ShouldBe(10);
}

[Fact]
public void ParseMarginShorthand_TwoValues_ShouldApplyVerticalHorizontal()
{
    var result = ParseMarginShorthand("10px 20px");
    result.Top.ShouldBe(10);
    result.Right.ShouldBe(20);
    result.Bottom.ShouldBe(10);
    result.Left.ShouldBe(20);
}

[Fact]
public void ParseMarginShorthand_ThreeValues_ShouldApplyTopHorizontalBottom()
{
    var result = ParseMarginShorthand("10px 20px 30px");
    result.Top.ShouldBe(10);
    result.Right.ShouldBe(20);
    result.Bottom.ShouldBe(30);
    result.Left.ShouldBe(20);
}

[Fact]
public void ParseMarginShorthand_FourValues_ShouldApplyTopRightBottomLeft()
{
    var result = ParseMarginShorthand("10px 20px 30px 40px");
    result.Top.ShouldBe(10);
    result.Right.ShouldBe(20);
    result.Bottom.ShouldBe(30);
    result.Left.ShouldBe(40);
}

// ✅ After - Consolidated Theory test
[Theory]
[InlineData("10px", 10, 10, 10, 10)]           // One value
[InlineData("10px 20px", 10, 20, 10, 20)]     // Two values
[InlineData("10px 20px 30px", 10, 20, 30, 20)] // Three values
[InlineData("10px 20px 30px 40px", 10, 20, 30, 40)] // Four values
public void ParseMarginShorthand_WithVariousValues_ShouldParseCorrectly(
    string input, double expectedTop, double expectedRight, double expectedBottom, double expectedLeft)
{
    // Arrange
    var styles = CssStyleMap.Empty;
    
    // Act
    styles = _updater.UpdateStyles(styles, new CssDeclaration("margin", input));
    
    // Assert
    styles.Margin.Top.ShouldBe(expectedTop);
    styles.Margin.Right.ShouldBe(expectedRight);
    styles.Margin.Bottom.ShouldBe(expectedBottom);
    styles.Margin.Left.ShouldBe(expectedLeft);
}
```

### xUnit Features for Optimal Testing

#### 1. `[Theory]` with `[InlineData]`
**Best for**: Simple parameterized tests with 2-5 parameters
```csharp
[Theory]
[InlineData("red", HexColors.Red)]
[InlineData("blue", HexColors.Blue)]
[InlineData("green", HexColors.Green)]
public void ConvertColor_WithNamedColors_ShouldReturnHex(string namedColor, string expectedHex)
{
    var result = ColorNormalizer.NormalizeToHex(namedColor);
    result.ShouldBe(expectedHex);
}
```

#### 2. `[Theory]` with `[MemberData]`
**Best for**: Complex test data or dynamic data generation
```csharp
[Theory]
[MemberData(nameof(GetBorderTestCases))]
public void ParseBorderShorthand_WithComplexData_ShouldParseCorrectly(BorderTestCase testCase)
{
    var result = ParseBorderShorthand(testCase.Input);
    result.Width.ShouldBe(testCase.ExpectedWidth);
    result.Style.ShouldBe(testCase.ExpectedStyle);
    result.Color.ShouldBe(testCase.ExpectedColor);
}

public static IEnumerable<object[]> GetBorderTestCases()
{
    yield return new object[] { new BorderTestCase("1px solid red", 1.0, CssBorderValues.Solid, HexColors.Red) };
    yield return new object[] { new BorderTestCase("thick dashed blue", 5.0, CssBorderValues.Dashed, HexColors.Blue) };
    yield return new object[] { new BorderTestCase("medium dotted green", 3.0, CssBorderValues.Dotted, HexColors.Green) };
}
```

#### 3. `[Theory]` with `[ClassData]`
**Best for**: Reusable test data classes
```csharp
[Theory]
[ClassData(typeof(BorderTestData))]
public void ParseBorderShorthand_WithClassData_ShouldParseCorrectly(BorderTestCase testCase)
{
    // Test implementation
}

public class BorderTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { new BorderTestCase("1px solid red", 1.0, CssBorderValues.Solid, HexColors.Red) };
        yield return new object[] { new BorderTestCase("thick dashed blue", 5.0, CssBorderValues.Dashed, HexColors.Blue) };
    }
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

#### 4. `[Fact]` Tests
**Use only for**: Single, unique test scenarios that cannot be parameterized
```csharp
[Fact]
public void ParseHtml_WithComplexNesting_ShouldCreateCorrectStructure()
{
    // This test is unique and cannot be parameterized
    var html = "<div><p><span>Nested content</span></p></div>";
    var result = _parser.Parse(html);
    
    result.Children.ShouldHaveSingleItem();
    result.Children[0].Children.ShouldHaveSingleItem();
    result.Children[0].Children[0].Children.ShouldHaveSingleItem();
}
```

## Test Coverage Priorities

### 🔴 High Priority - MUST Test
Classes and methods with **business logic, algorithms, and complex behavior**:

- **Parsing Logic**: HTML parsing, CSS parsing, validation
- **Rendering Logic**: PDF generation, layout calculations, styling
- **Business Rules**: Validation rules, transformation logic
- **Complex State Management**: State machines, workflow orchestration
- **Integration Points**: External API calls, file I/O operations
- **Error Handling**: Exception scenarios, edge cases
- **Performance-Critical Code**: Algorithms, data processing

**Examples**:
```csharp
// ✅ MUST test - complex parsing logic
public class HtmlParser 
{
    public HtmlFragment ParseHtml(string html) { /* complex logic */ }
    public bool IsValidHtml(string html) { /* validation logic */ }
}

// ✅ MUST test - rendering algorithms
public class PdfRenderer 
{
    public byte[] RenderDocument(HtmlFragment fragment) { /* complex rendering */ }
    public void ApplyStyling(CssStyleMap styles) { /* styling logic */ }
}
```

### 🟡 Medium Priority - SHOULD Test
Classes with **moderate complexity**:

- **Data Transformation**: Converting between formats, mapping data
- **Configuration Classes**: Complex configuration with validation
- **Helper Methods**: Utility methods with multiple branches
- **Integration Adapters**: Wrapper classes around external libraries

**Examples**:
```csharp
// 🟡 SHOULD test - data transformation
public class CssStyleResolver 
{
    public CssStyleMap ResolveStyles(HtmlElement element) { /* mapping logic */ }
}

// 🟡 SHOULD test - configuration validation
public class ConverterOptions 
{
    public void Validate() { /* validation logic */ }
}
```

### 🟢 Low Priority - OPTIONAL Test
**Simple data containers and basic operations**:

- **Data Transfer Objects (DTOs)**: Simple property containers
- **Value Objects**: Immutable objects with simple equality
- **Enums**: Simple enumeration types
- **Basic Property Setters**: Simple getters/setters without logic

**Examples**:
```csharp
// 🟢 OPTIONAL test - simple data container
public class CssStyleMap 
{
    public FontStyle FontStyle { get; }
    public bool Bold { get; }
    public CssStyleMap WithFontStyle(FontStyle style) => new(style, Bold, ...);
}

// 🟢 OPTIONAL test - simple enum
public enum CssStyleSource 
{
    Inline, Class, Inherited, Default
}
```

### ⚪ Exempt - DON'T Test
**Auto-generated or trivial code**:

- **Auto-generated Code**: Compiler-generated properties, partial classes
- **Simple Constructors**: Constructors that only assign parameters to fields
- **Trivial Getters/Setters**: Properties without any logic
- **Pure Data Classes**: Classes with only public properties and no methods

**Examples**:
```csharp
// ⚪ DON'T test - trivial constructor
public class SimpleData 
{
    public string Name { get; }
    public int Value { get; }
    
    public SimpleData(string name, int value) 
    {
        Name = name;  // Just assignment
        Value = value; // Just assignment
    }
}

// ⚪ DON'T test - auto-generated properties
public partial class GeneratedClass 
{
    public string AutoProperty { get; set; } // Auto-generated
}
```

## Practical Examples

### CssStyleMap - Low Priority Testing
```csharp
// This class is mostly data container with simple immutable operations
// Focus testing on the Merge() method (has logic) and complex scenarios
public class CssStyleMap 
{
    // ✅ Test the Merge logic (has business rules)
    [Fact]
    public void Merge_ShouldPrioritizeOtherValues_WhenBothSet() { }
    
    // ✅ Test complex scenarios
    [Fact] 
    public void ComplexScenario_ShouldWorkCorrectly() { }
    
    // 🟢 Optional - simple property setters
    [Fact]
    public void WithFontStyle_ShouldSetFontStyle() { }
}
```

### HtmlParser - High Priority Testing
```csharp
// This class has complex parsing logic - MUST test thoroughly
public class HtmlParser 
{
    // ✅ MUST test - core parsing logic
    [Fact]
    public void ParseHtml_WithComplexNesting_ShouldCreateCorrectStructure() { }
    
    // ✅ MUST test - edge cases
    [Fact]
    public void ParseHtml_WithMalformedHtml_ShouldHandleGracefully() { }
    
    // ✅ MUST test - validation logic
    [Fact]
    public void IsValidHtml_WithInvalidSyntax_ShouldReturnFalse() { }
}
```

## Testing Strategy

### 1. Start with High Priority
Focus testing efforts on classes that contain business logic and complex behavior.

### 2. Use Risk-Based Testing
Test areas that are:
- **Most likely to break** (complex algorithms)
- **Most critical to users** (core functionality)
- **Most frequently changed** (active development areas)

### 3. Test Behavior, Not Implementation
```csharp
// ❌ DON'T test implementation details
[Fact]
public void ShouldCallPrivateMethod() { }

// ✅ DO test behavior
[Fact]
public void ParseHtml_WithValidInput_ShouldReturnParsedFragment() { }
```

### 4. Focus on Edge Cases
Test boundary conditions, error scenarios, and unusual inputs.

## Coverage Metrics

Instead of aiming for 100% line coverage, focus on:

- **Critical Path Coverage**: All business-critical code paths tested
- **Edge Case Coverage**: Boundary conditions and error scenarios
- **Integration Coverage**: Component interactions tested
- **Regression Coverage**: Bug fixes have corresponding tests

## Conclusion

The goal is **effective testing**, not **exhaustive testing**. Focus your testing efforts on code that:
1. Contains business logic
2. Has complex algorithms
3. Handles user input
4. Manages state
5. Integrates with external systems

Simple data containers and property setters don't need extensive testing - they're unlikely to contain bugs and testing them provides minimal value.
