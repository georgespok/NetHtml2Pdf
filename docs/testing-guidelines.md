# Testing Guidelines

## Overview

This document provides practical guidance on what to test and what not to test, based on the complexity and business value of the code. **Theory tests (`[Theory]` with `[InlineData]`) are PRIORITY for parameterized testing scenarios.**

### Foundational Practices

- Code MUST be developed using incremental TDD: introduce one failing test at a time, implement the minimal passing code, and refactor before adding the next test. Trivial scaffolding (constructors, simple properties, passive DTOs) is exempt.
- Tests MUST exercise observable behavior (e.g., rendered output, pagination results) and MUST NOT rely on reflection-based contract checks of internal types.
- Reflection APIs (e.g., `Activator.CreateInstance`, `Type.GetType`, `MethodInfo.Invoke`) are explicitly prohibited in test code.

## Unit Test Coverage Strategy

### Coverage Scoring System

The goal is to maximize meaningful coverage of business-critical and complex code paths, while minimizing total test volume and long-term maintenance overhead.

#### Coverage Score Formula

For each method of each class, calculate the "score" using the following formula:

```
Score = (0.5 x BusinessCriticality)
      + (0.3 x ChangeFrequency)
      + (0.2 x DefectHistory)
```

#### Score Components

- **Business Criticality (50% weight)**: Impact on core business functionality
  - Low impact (utility methods): 1-2 points
  - Medium impact (feature methods): 3-4 points
  - High impact (core business logic): 5-6 points

- **Change Frequency (30% weight)**: How often the code is modified
  - Stable (rarely changed): 1-2 points
  - Moderate (occasionally changed): 3-4 points
  - Volatile (frequently changed): 5-6 points

- **Defect History (20% weight)**: Historical bug frequency
  - No known issues: 1 point
  - Some issues in past: 2-3 points
  - Problematic (multiple bugs): 4-6 points

#### Testing Priority Thresholds

| Score Range | Priority | Action |
|-------------|----------|---------|
| **< 5** | Low | **Ignore** - No tests required |
| **5-7** | Medium | **Test** - Standard test coverage |
| **> 7** | High | **Must Test** - Comprehensive test coverage |

#### Implementation Guidelines

1. **Calculate scores** for all methods in business-critical classes
2. **Ignore Low priority methods** (Score < 5) to reduce maintenance overhead
3. **Focus on Medium and High priority methods** (Score ≥ 5)
4. **Use Theory tests** for High priority methods with multiple scenarios
5. **Regularly review scores** as code evolves and business requirements change

#### Practical Example

Consider a CSS parsing method with the following characteristics:

```csharp
public CssStyleMap ParseBorderShorthand(string value)
{
    // Complex parsing logic with multiple branches
    if (string.IsNullOrEmpty(value)) return CssStyleMap.Empty;
    
    var parts = value.Split(' ');
    if (parts.Length == 1) { /* handle single value */ }
    else if (parts.Length == 2) { /* handle two values */ }
    else if (parts.Length == 3) { /* handle three values */ }
    else { /* handle four values */ }
    
    // Additional validation and error handling
    return result;
}
```

**Score Calculation:**
- **Business Criticality**: 5 (core CSS parsing functionality) - 5 points  
- **Change Frequency**: 3 (occasionally modified) - 3 points
- **Defect History**: 2 (some parsing issues in past) - 2 points

**Total Score**: (0.5 x 5) + (0.3 x 3) + (0.2 x 2) = 2.5 + 0.9 + 0.4 = **3.8**

**Result**: Score 3.8 < 5 → **Low priority** → **No tests required**

This method would be ignored for testing, reducing maintenance overhead while focusing effort on higher-impact methods.

## Integration Testing Strategy

### Integration Test Coverage System

Integration tests focus on **cross-module communication** with the goal of minimal number of tests and maximum coverage of important interactions.

#### Integration Test Granularity Rules

- **One integration test per major use case**, not per method
- **Simulate full transaction flow**: request → domain → persistence
- **Test the "contract" not the "internals"**
- **Avoid duplicating unit-level assertions**; focus on integration outcomes

#### Integration Score Formula

For each integration scenario, calculate the score using:

```
IntegrationScore = (0.3 × CrossModule)
                + (0.3 × ExternalDependency)
                + (0.3 × Criticality)
                + (0.1 × Transactional)
```

#### Score Components

- **Cross-Module Communication (30% weight)**: Number of modules/components involved
 - Single module (1 component): 1-2 points
 - Two modules (2 components): 3-4 points
 - Multiple modules (3+ components): 5-6 points

- **External Dependencies (30% weight)**: External systems or services involved
 - No external dependencies: 1-2 points
 - File system, database: 3-4 points
 - External APIs, services: 5-6 points

- **Business Criticality (50% weight)**: Impact on core business functionality
 - Low impact (utility features): 1-2 points
 - Medium impact (feature workflows): 3-4 points
 - High impact (core business processes): 5-6 points

- **Transactional Nature (10% weight)**: Multi-step transaction complexity
 - Simple operation: 1-2 points
 - Multi-step process: 3-4 points
 - Complex transaction with rollback: 5-6 points

#### Integration Testing Priority Thresholds

| Score Range | Priority | Action |
|-------------|----------|---------|
| **≥ 7** | High | **Must have** integration test |
| **5-6** | Medium | **Desirable** integration test |
| **< 5** | Low | **Rely on unit tests** only |

#### Implementation Guidelines

1. **Focus on major use cases** rather than individual method combinations
2. **Test end-to-end workflows** that span multiple modules
3. **Verify integration contracts** between components
4. **Minimize test duplication** with unit tests
5. **Use real or realistic dependencies** where appropriate
6. **Test error propagation** across module boundaries

#### Integration Test Example

Consider a PDF generation workflow:

```csharp
[Fact]
public void PdfBuilder_CompleteWorkflow_GeneratesValidPdf()
{
    // Arrange - Full integration scenario
    var htmlContent = "<h1>Title</h1><p>Content with <strong>formatting</strong></p>";
    var headerHtml = "<header>Document Header</header>";
    var footerHtml = "<footer>Page Footer</footer>";
    
    // Act - Full transaction flow
    var pdfBytes = new PdfBuilder()
        .SetHeader(headerHtml)
        .AddPage(htmlContent)
        .SetFooter(footerHtml)
        .Build();
    
    // Assert - Integration outcomes only
    AssertValidPdf(pdfBytes);
    var words = ExtractWords(pdfBytes);
    words.ShouldContain("Title");
    words.ShouldContain("Content");
    words.ShouldContain("formatting");
    words.ShouldContain("Document Header");
    words.ShouldContain("Page Footer");
}
```

**Score Calculation:**
- **Cross-Module**: 5 (PdfBuilder → Parser → Renderer → QuestPDF) → 5 points
- **External Dependencies**: 4 (File system for fonts, PDF generation) → 4 points
- **Business Criticality**: 6 (Core PDF generation functionality) → 6 points
- **Transactional**: 3 (Multi-step build process) → 3 points

**Total Score**: (0.3 × 5) + (0.3 × 4) + (0.3 × 6) + (0.1 × 3) = 1.5 + 1.2 + 1.8 + 0.3 = **4.8**

**Result**: Score 4.8 < 5 → **Low priority** → **Rely on unit tests only**

This demonstrates how integration tests focus on high-impact, multi-module scenarios while avoiding unnecessary complexity.

#### High-Priority Integration Test Example

Consider a complex PDF generation with external dependencies:

```csharp
[Fact]
public void PdfBuilder_WithExternalFontsAndCustomOptions_GeneratesValidPdf()
{
    // Arrange - Complex integration scenario
    var customFontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "Arial.ttf");
    var options = new ConverterOptions { FontPath = customFontPath };
    var complexHtml = @"
        <style>
            .header { font-family: Arial; font-size: 18px; color: #333; }
            .content { margin: 20px; padding: 10px; border: 1px solid #ccc; }
        </style>
        <div class='header'>Document Title</div>
        <div class='content'>Complex content with styling</div>";
    
    // Act - Full transaction flow with external dependencies
    var pdfBytes = new PdfBuilder()
        .SetHeader("<header>Company Header</header>")
        .AddPage(complexHtml)
        .SetFooter("<footer>Page 1</footer>")
        .Build(options);
    
    // Assert - Integration outcomes only
    AssertValidPdf(pdfBytes);
    var words = ExtractWords(pdfBytes);
    words.ShouldContain("Document");
    words.ShouldContain("Title");
    words.ShouldContain("Company");
    words.ShouldContain("Header");
}
```

**Score Calculation:**
- **Cross-Module**: 6 (PdfBuilder → Options → Parser → Renderer → QuestPDF → Font System) → 6 points
- **External Dependencies**: 6 (File system for custom fonts, PDF generation, external font loading) → 6 points
- **Business Criticality**: 6 (Core PDF generation with custom styling) → 6 points
- **Transactional**: 4 (Multi-step process with configuration and rendering) → 4 points

**Total Score**: (0.3 × 6) + (0.3 × 6) + (0.3 × 6) + (0.1 × 4) = 1.8 + 1.8 + 1.8 + 0.4 = **5.8**

**Result**: Score 5.8 → **Medium priority** → **Desirable integration test**

This scenario demonstrates when integration testing becomes valuable due to external dependencies and cross-module complexity.

### Ultimate Integration Tests

#### Purpose and Scope

**Ultimate integration tests** are comprehensive, end-to-end tests that validate the complete HTML-to-PDF rendering pipeline with extensive HTML/CSS coverage. These tests serve as the **final validation** of the system's capabilities and produce visual PDF outputs for manual inspection.

#### Ultimate Test Characteristics

- **Comprehensive HTML Coverage**: Include many (but not all) supported HTML tags and CSS attributes
- **Real-World Scenarios**: Simulate complex, realistic document structures
- **Visual Validation**: Generate PDF files saved with `SavePdfForInspectionAsync()` for visual comparison
- **Multi-Feature Integration**: Combine headers, footers, tables, lists, styling, and formatting
- **Content Verification**: Validate both structure and styling through word extraction and analysis

#### Ultimate Test Requirements

1. **HTML Structure Coverage**:
 - Document structure (`<html>`, `<head>`, `<body>`, `<section>`)
 - Typography (`<h1>`, `<h2>`, `<p>`, `<strong>`, `<em>`)
 - Lists (`<ul>`, `<ol>`, `<li>`)
 - Tables (`<table>`, `<thead>`, `<tbody>`, `<tr>`, `<th>`, `<td>`)
 - Containers (`<div>`, `<span>`)

2. **CSS Styling Coverage**:
 - Font properties (`font-family`, `font-size`, `color`)
 - Layout properties (`margin`, `padding`, `text-align`)
 - Visual properties (`background-color`, `border`)
 - Table properties (`border-collapse`, `colspan`)

3. **Multi-Page Features**:
 - Headers and footers across all pages
 - Page content with complex styling
 - Cross-page element consistency

4. **Visual Output Requirements**:
 - Generate PDF file for manual inspection
 - Verify PDF structure and content
 - Validate styling and formatting
 - Confirm multi-page functionality

#### Ultimate Test Example Pattern

```csharp
[Fact]
public async Task FullDocument_Rendering_SmokeTest()
{
    // Arrange - Comprehensive HTML/CSS scenario
    const string headerHtml = "<div style=\"text-align:center;font-size:12px;color:red\">Sample Header</div>";
    const string footerHtml = "<div style=\"text-align:center;font-size:10px;\">Page Footer</div>";
    const string html = """
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body { font-family: Arial; font-size: 12px; }
                .title { color: blue; text-align: center; margin-bottom: 16px; }
                .section { margin: 32px 0; }
                .highlight { background-color: yellow; }
                .report { border-collapse: collapse; width: 100%; }
                .report-cell { border: 1px solid #444444; padding: 6px; }
            </style>
        </head>
        <body>
            <section class="section">
                <h1 class="title">Html2Pdf Integration Showcase</h1>
                <p>Paragraph with <strong>bold</strong>, <em>italic</em>, 
                   <span class="highlight">highlighted</span>, and 
                   <span style="color: red;">colored</span> text.</p>
            </section>
            <section class="section">
                <h2>Lists</h2>
                <ul>
                    <li>First bullet item</li>
                    <li>Second bullet with <strong>formatting</strong></li>
                </ul>
                <ol>
                    <li>Numbered entry one</li>
                    <li>Numbered entry two</li>
                </ol>
            </section>
            <section class="section">
                <h2>Table</h2>
                <table class="report">
                    <thead>
                        <tr>
                            <th>Item</th>
                            <th style="text-align:right;">Qty</th>
                            <th>Total</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td>Widgets</td>
                            <td style="text-align:right;">10</td>
                            <td>$150.00</td>
                        </tr>
                        <tr>
                            <td colspan="3" style="text-align:center;">Summary Row</td>
                        </tr>
                    </tbody>
                </table>
            </section>
        </body>
        </html>
        """;

    // Act - Full integration workflow
    var pdfBytes = new PdfBuilder()
        .SetHeader(headerHtml)
        .SetFooter(footerHtml)
        .AddPage(html)
        .Build();

    // Assert - Comprehensive validation
    AssertValidPdf(pdfBytes);
    await SavePdfForInspectionAsync(pdfBytes, "integration-full-document.pdf");

    // Content verification
    var words = ExtractWords(pdfBytes);
    words.ShouldContain("Html2Pdf");
    words.ShouldContain("Widgets");
    words.ShouldContain("Summary");

    // Styling verification
    var pdfWords = GetPdfWords(pdfBytes);
    var boldWord = pdfWords.FirstOrDefault(w => w.Text.Contains("bold"));
    boldWord.ShouldNotBeNull();
    boldWord.IsBold.ShouldBeTrue();

    var coloredWord = pdfWords.FirstOrDefault(w => w.Text.Contains("colored"));
    coloredWord.ShouldNotBeNull();
    coloredWord.HexColor.ShouldBe("#FF0000");
}
```

#### Ultimate Test Scoring

Ultimate integration tests typically score **≥ 7** due to their comprehensive nature:

- **Cross-Module**: 6 (Complete pipeline: Builder → Parser → Renderer → QuestPDF → File System)
- **External Dependencies**: 6 (File system, PDF generation, font loading, visual output)
- **Business Criticality**: 6 (Complete system validation, end-to-end functionality)
- **Transactional**: 4 (Multi-step document generation with headers/footers)

**Total Score**: (0.3 × 6) + (0.3 × 6) + (0.3 × 6) + (0.1 × 4) = **5.8**

**Result**: Score 5.8 → **Medium priority** → **Desirable integration test**

However, due to their **comprehensive validation value**, ultimate tests are considered **mandatory** regardless of score.

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








