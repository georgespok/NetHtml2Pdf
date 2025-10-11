# Contract: CSS Margin Shorthand Property Parsing

**Feature**: CSS Shorthand Properties (Margin)  
**User Story**: US-CSS-SHORTHAND  
**Acceptance Criteria**: AC-002a.1 through AC-002a.4  
**Date**: 2025-10-09

## Purpose

Define the input/output contract for CSS `margin` shorthand property parsing, ensuring the parser correctly expands 1-4 value patterns into individual margin properties according to standard CSS specifications.

---

## Input Contract

### 1-Value Pattern
```html
<div style="margin: 10px">Content</div>
```

**Expected Expansion**:
- `margin-top: 10px`
- `margin-right: 10px`
- `margin-bottom: 10px`
- `margin-left: 10px`

**AC**: AC-002a.1

### 2-Value Pattern
```html
<div style="margin: 10px 20px">Content</div>
```

**Expected Expansion**:
- `margin-top: 10px` (vertical)
- `margin-right: 20px` (horizontal)
- `margin-bottom: 10px` (vertical)
- `margin-left: 20px` (horizontal)

**AC**: AC-002a.2

### 3-Value Pattern
```html
<div style="margin: 10px 20px 30px">Content</div>
```

**Expected Expansion**:
- `margin-top: 10px`
- `margin-right: 20px` (horizontal)
- `margin-bottom: 30px`
- `margin-left: 20px` (horizontal)

**AC**: AC-002a.3

### 4-Value Pattern
```html
<div style="margin: 10px 20px 30px 40px">Content</div>
```

**Expected Expansion**:
- `margin-top: 10px`
- `margin-right: 20px`
- `margin-bottom: 30px`
- `margin-left: 40px`

**AC**: AC-002a.4

---

## Cascade Behavior

### Shorthand After Longhand
```html
<div style="margin-top: 20px; margin: 10px">Content</div>
```

**Expected Result**: All margins = 10px (shorthand overrides earlier longhand)

**AC**: AC-002a.9

### Longhand After Shorthand
```html
<div style="margin: 10px; margin-top: 20px">Content</div>
```

**Expected Result**: 
- `margin-top: 20px` (longhand overrides)
- `margin-right: 10px`
- `margin-bottom: 10px`
- `margin-left: 10px`

**AC**: AC-002a.10

---

## Invalid Values

### Invalid Syntax
```html
<div style="margin: 10px invalid 20px">Content</div>
```

**Expected Behavior**:
1. Entire declaration rejected
2. Structured warning log emitted with property name and invalid value
3. Fall back to default or inherited margin values
4. Rendering continues without exception

**AC**: AC-002a.11, AC-002a.12, AC-002a.13

### Examples of Invalid Values
- `margin: abc` (non-numeric)
- `margin: 10px 20px 30px 40px 50px` (too many values)
- `margin: 10px invalid` (invalid token)
- `margin: ` (empty value)

---

## Parser Implementation Contract

### CssStyleUpdater.ParseMarginShorthand(string value)

**Input**: Raw margin value string (e.g., "10px 20px")

**Output**: 
- Success: Dictionary<string, string> with expanded properties
- Failure: null (triggers warning and fallback)

**Example**:
```csharp
// Input: "10px 20px"
// Output:
{
    { "margin-top", "10px" },
    { "margin-right", "20px" },
    { "margin-bottom", "10px" },
    { "margin-left", "20px" }
}

// Input: "invalid"
// Output: null
// Side effect: Structured warning logged
```

---

## Test Cases

### Unit Tests (CssStyleUpdaterTests.cs)
1. `ParseMarginShorthand_OneValue_ExpandsToAllSides`
2. `ParseMarginShorthand_TwoValues_ExpandsVerticalHorizontal`
3. `ParseMarginShorthand_ThreeValues_ExpandsTopHorizontalBottom`
4. `ParseMarginShorthand_FourValues_ExpandsTopRightBottomLeft`
5. `ParseMarginShorthand_InvalidValue_ReturnsNull`
6. `ParseMarginShorthand_EmptyValue_ReturnsNull`
7. `Apply_MarginShorthandAfterLonghand_ShorthandWins`
8. `Apply_MarginLonghandAfterShorthand_LonghandWins`

### Integration Tests (HtmlParserTests.cs)
1. `Margin_OneValueShorthand_ParsesCorrectly`
2. `Margin_TwoValueShorthand_ParsesCorrectly`
3. `Margin_ThreeValueShorthand_ParsesCorrectly`
4. `Margin_FourValueShorthand_ParsesCorrectly`
5. `Margin_InvalidShorthand_EmitsWarningAndFallsBack`

---

## Success Criteria

✅ All 4 margin value patterns parse correctly  
✅ Shorthand/longhand cascade follows source order precedence  
✅ Invalid values rejected with structured warnings  
✅ Rendering continues despite invalid shorthands  
✅ All unit and integration tests pass

---

**Contract Status**: Ready for TDD implementation

