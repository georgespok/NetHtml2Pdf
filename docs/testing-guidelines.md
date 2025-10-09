# Testing Guidelines

## Overview

This document provides practical guidance on what to test and what not to test, based on the complexity and business value of the code.

## Tests as First-Class Code

**Tests MUST be treated with the same quality standards as production code.**

### Clean Code Principles for Tests

- **SOLID**: Apply Single Responsibility, Open/Closed, and other SOLID principles
- **DRY**: Don't Repeat Yourself - eliminate code duplication
- **KISS**: Keep It Simple, Stupid - avoid over-engineering tests
- **Readability**: Tests should be easy to read and understand

### Test Quality Standards

1. **Use Theory and InlineData**: Consolidate similar test scenarios
   ```csharp
   // ✅ Good - consolidated with Theory
   [Theory]
   [InlineData(null, typeof(ArgumentNullException))]
   [InlineData("", typeof(ArgumentException))]
   public void Method_WithInvalidInput_ShouldThrow(string input, Type exceptionType)
   
   // ❌ Bad - repetitive individual tests
   [Fact] public void Method_WithNull_ShouldThrow() { }
   [Fact] public void Method_WithEmpty_ShouldThrow() { }
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
