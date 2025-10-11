# Contract: CSS Border Shorthand Property Parsing

**Feature**: CSS Shorthand Properties (Border)  
**User Story**: US-CSS-SHORTHAND  
**Acceptance Criteria**: AC-002a.5 through AC-002a.8  
**Date**: 2025-10-09

## Purpose

Define the input/output contract for CSS `border` shorthand property parsing, ensuring the parser correctly extracts width, style, and color components in any order according to standard CSS specifications.

---

## Input Contract

### All Components (Standard Order)
```html
<div style="border: 1px solid black">Content</div>
```

**Expected Expansion**:
- `border-width: 1px`
- `border-style: solid`
- `border-color: black`

**AC**: AC-002a.5

### All Components (Alternate Order)
```html
<div style="border: solid 2px red">Content</div>
<div style="border: #000 1px dashed">Content</div>
```

**Expected Expansion** (order-independent):
- `border-width: 2px` or `border-width: 1px`
- `border-style: solid` or `border-style: dashed`
- `border-color: red` or `border-color: #000`

**AC**: AC-002a.5

---

## Width Component

### Keyword Values
```html
<div style="border: thin solid">Content</div>
<div style="border: medium solid">Content</div>
<div style="border: thick solid">Content</div>
```

**Expected Values**:
- `thin`: 1px equivalent
- `medium`: 3px equivalent
- `thick`: 5px equivalent

**AC**: AC-002a.6

### Length Units
```html
<div style="border: 5px solid">Content</div>
<div style="border: 2pt solid">Content</div>
<div style="border: 1em solid">Content</div>
```

**Supported Units**: px, pt, em (all valid CSS length units)

**AC**: AC-002a.6

---

## Style Component

### Style Keywords
```html
<div style="border: 1px solid">Content</div>
<div style="border: 1px dashed">Content</div>
<div style="border: 1px dotted">Content</div>
<div style="border: 1px none">Content</div>
<div style="border: 1px hidden">Content</div>
```

**Supported Styles**: solid, dashed, dotted, none, hidden

**AC**: AC-002a.7

---

## Color Component

### Named Colors
```html
<div style="border: 1px solid red">Content</div>
<div style="border: 1px solid blue">Content</div>
<div style="border: 1px solid black">Content</div>
```

**Expected**: Standard CSS color names

**AC**: AC-002a.8

### Hex Colors
```html
<div style="border: 1px solid #FF0000">Content</div>
<div style="border: 1px solid #000">Content</div>
<div style="border: 1px solid #123ABC">Content</div>
```

**Expected**: 3-digit or 6-digit hex color codes

**AC**: AC-002a.8

### RGB Colors
```html
<div style="border: 1px solid rgb(255, 0, 0)">Content</div>
<div style="border: 1px solid rgb(0, 128, 255)">Content</div>
```

**Expected**: RGB functional notation

**AC**: AC-002a.8

---

## Partial Specifications

### Width + Style Only
```html
<div style="border: 2px solid">Content</div>
```

**Expected**:
- `border-width: 2px`
- `border-style: solid`
- `border-color`: (default/inherited)

### Style + Color Only
```html
<div style="border: dashed red">Content</div>
```

**Expected**:
- `border-width`: (default/inherited)
- `border-style: dashed`
- `border-color: red`

### Single Component
```html
<div style="border: solid">Content</div>
```

**Expected**:
- `border-style: solid`
- Other components: (default/inherited)

---

## Cascade Behavior

### Shorthand After Longhand
```html
<div style="border-width: 5px; border: 1px solid black">Content</div>
```

**Expected Result**: All border properties = shorthand values (width=1px, style=solid, color=black)

**AC**: AC-002a.9

### Longhand After Shorthand
```html
<div style="border: 1px solid black; border-width: 5px">Content</div>
```

**Expected Result**:
- `border-width: 5px` (longhand overrides)
- `border-style: solid`
- `border-color: black`

**AC**: AC-002a.10

---

## Invalid Values

### Invalid Syntax
```html
<div style="border: 99px rainbow magic">Content</div>
<div style="border: invalid">Content</div>
<div style="border: 10px 20px 30px">Content</div>
```

**Expected Behavior**:
1. Entire declaration rejected
2. Structured warning log emitted
3. Fall back to default or inherited border values
4. Rendering continues without exception

**AC**: AC-002a.11, AC-002a.12, AC-002a.13

---

## Parser Implementation Contract

### CssStyleUpdater.ParseBorderShorthand(string value)

**Input**: Raw border value string (e.g., "1px solid red")

**Output**:
- Success: Dictionary<string, string> with extracted components
- Failure: null (triggers warning and fallback)

**Example**:
```csharp
// Input: "solid 2px red"
// Output:
{
    { "border-width", "2px" },
    { "border-style", "solid" },
    { "border-color", "red" }
}

// Input: "1px dashed"
// Output:
{
    { "border-width", "1px" },
    { "border-style", "dashed" }
    // border-color: omitted (use default/inherited)
}

// Input: "invalid rainbow"
// Output: null
// Side effect: Structured warning logged
```

**Parsing Algorithm**:
1. Tokenize value by whitespace
2. For each token:
   - If matches length or keyword (thin/medium/thick) → width
   - If matches style keyword (solid/dashed/etc.) → style
   - If matches color (name/hex/rgb) → color
3. If unrecognized token found → return null
4. Return extracted components

---

## Test Cases

### Unit Tests (CssStyleUpdaterTests.cs)
1. `ParseBorderShorthand_AllComponents_StandardOrder_ParsesCorrectly`
2. `ParseBorderShorthand_AllComponents_AlternateOrder_ParsesCorrectly`
3. `ParseBorderShorthand_WidthKeywords_ParsesCorrectly`
4. `ParseBorderShorthand_LengthUnits_ParsesCorrectly`
5. `ParseBorderShorthand_StyleKeywords_ParsesCorrectly`
6. `ParseBorderShorthand_NamedColors_ParsesCorrectly`
7. `ParseBorderShorthand_HexColors_ParsesCorrectly`
8. `ParseBorderShorthand_RgbColors_ParsesCorrectly`
9. `ParseBorderShorthand_PartialComponents_ParsesCorrectly`
10. `ParseBorderShorthand_InvalidValue_ReturnsNull`
11. `Apply_BorderShorthandAfterLonghand_ShorthandWins`
12. `Apply_BorderLonghandAfterShorthand_LonghandWins`

### Integration Tests (HtmlParserTests.cs)
1. `Border_AllComponentsShorthand_ParsesCorrectly`
2. `Border_AlternateOrderShorthand_ParsesCorrectly`
3. `Border_PartialShorthand_ParsesCorrectly`
4. `Border_InvalidShorthand_EmitsWarningAndFallsBack`

---

## Success Criteria

✅ All border component combinations parse correctly  
✅ Parser handles any component order  
✅ Width keywords (thin/medium/thick) and length units supported  
✅ Style keywords (solid/dashed/dotted/none/hidden) supported  
✅ Color formats (named/hex/rgb) supported  
✅ Shorthand/longhand cascade follows source order precedence  
✅ Invalid values rejected with structured warnings  
✅ All unit and integration tests pass

---

**Contract Status**: Ready for TDD implementation

