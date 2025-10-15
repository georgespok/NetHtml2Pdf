# Contract: CSS Display

## Supported Values
- `display: block`
- `display: inline-block`
- `display: none`

## Behavior
- Block: starts on new line; full available width by default; margins/padding/border apply; vertical margin collapsing with adjacent blocks.
- Inline-block: participates in inline flow as an atomic box; wraps as whole; width/height, margins, padding, border apply; vertical-align: middle.
- None: node and descendants produce no boxes and no output; layout properties ignored without warnings.

## Unsupported Values
- Examples: `flex`, `grid`, `inline`, `inline-flex`, `table`, etc.
- Handling: Emit structured warning and fallback to HTML element semantic default (e.g., `div`=block, `span`=inline).

## Precedence Rules
1. Multiple classes merge in attribute order (later wins)
2. Inline style overrides class-derived values

## Examples

### Display: Block

#### Basic Block Behavior
```html
<div style="display: block; background-color: lightblue; padding: 10px;">
    Block element 1
</div>
<div style="display: block; background-color: lightgreen; padding: 10px;">
    Block element 2
</div>
```
**Result**: Each div starts on a new line and takes full available width.

#### Block with Width and Margins
```html
<div style="display: block; width: 200px; margin: 20px; background-color: lightcoral; padding: 15px;">
    Block with fixed width and margins
</div>
<div style="display: block; background-color: lightyellow; padding: 10px;">
    Another block element
</div>
```
**Result**: First div has fixed width (200px) plus margins (20px each side), second div takes remaining width.

#### Margin Collapsing Between Blocks
```html
<div style="display: block; margin-bottom: 30px; background-color: lightblue; padding: 10px;">
    Block with bottom margin
</div>
<div style="display: block; margin-top: 20px; background-color: lightgreen; padding: 10px;">
    Block with top margin
</div>
```
**Result**: Margins collapse to the larger value (30px), not 50px total.

#### Margin Collapsing Prevention with Border
```html
<div style="display: block; margin-bottom: 30px; border-bottom: 2px solid black; background-color: lightblue; padding: 10px;">
    Block with border preventing margin collapse
</div>
<div style="display: block; margin-top: 20px; background-color: lightgreen; padding: 10px;">
    Block with top margin
</div>
```
**Result**: Margins do not collapse due to border, total spacing is 50px.

### Display: Inline-Block

#### Basic Inline-Block Behavior
```html
<div style="display: inline-block; width: 100px; background-color: lightblue; padding: 10px;">
    Inline-block 1
</div>
<div style="display: inline-block; width: 100px; background-color: lightgreen; padding: 10px;">
    Inline-block 2
</div>
<div style="display: inline-block; width: 100px; background-color: lightcoral; padding: 10px;">
    Inline-block 3
</div>
```
**Result**: All three divs appear side-by-side on the same line if space allows.

#### Inline-Block Wrapping
```html
<div style="display: inline-block; width: 200px; background-color: lightblue; padding: 10px;">
    Wide inline-block
</div>
<div style="display: inline-block; width: 200px; background-color: lightgreen; padding: 10px;">
    Another wide inline-block
</div>
<div style="display: inline-block; width: 200px; background-color: lightcoral; padding: 10px;">
    Third wide inline-block
</div>
```
**Result**: Elements wrap as whole units when insufficient space remains.

#### Mixed Block and Inline-Block
```html
<div style="display: block; background-color: lightblue; padding: 10px;">
    Block element
</div>
<div style="display: inline-block; width: 150px; background-color: lightgreen; padding: 10px;">
    Inline-block 1
</div>
<div style="display: inline-block; width: 150px; background-color: lightcoral; padding: 10px;">
    Inline-block 2
</div>
<div style="display: block; background-color: lightyellow; padding: 10px;">
    Another block element
</div>
```
**Result**: Block elements start new lines, inline-block elements flow together.

#### Inline-Block with Vertical Alignment
```html
<div style="display: inline-block; width: 100px; height: 60px; background-color: lightblue; padding: 10px;">
    Tall inline-block
</div>
<div style="display: inline-block; width: 100px; height: 30px; background-color: lightgreen; padding: 10px;">
    Short inline-block
</div>
```
**Result**: Elements align to middle by default (vertical-align: middle).

### Display: None

#### Basic None Behavior
```html
<div style="display: block; background-color: lightblue; padding: 10px;">
    Visible block element
</div>
<div style="display: none; background-color: lightgreen; padding: 10px;">
    Hidden block element
</div>
<div style="display: block; background-color: lightcoral; padding: 10px;">
    Another visible block element
</div>
```
**Result**: Middle div is completely hidden, no space occupied.

#### None with Nested Elements
```html
<div style="display: none; background-color: lightblue; padding: 10px;">
    Hidden container
    <div style="display: block; background-color: lightgreen; padding: 10px;">
        Hidden nested block
    </div>
    <div style="display: inline-block; background-color: lightcoral; padding: 10px;">
        Hidden nested inline-block
    </div>
</div>
<div style="display: block; background-color: lightyellow; padding: 10px;">
    Visible element after hidden container
</div>
```
**Result**: Entire container and all nested elements are hidden.

#### None with Layout Properties
```html
<div style="display: none; width: 200px; margin: 50px; padding: 20px; border: 5px solid black;">
    Hidden element with layout properties
</div>
<div style="display: block; background-color: lightblue; padding: 10px;">
    Visible element
</div>
```
**Result**: Layout properties (width, margin, padding, border) are ignored without warnings.

### Unsupported Display Values

#### Unsupported Values with Warnings
```html
<div style="display: flex; background-color: lightblue; padding: 10px;">
    Flex element (unsupported)
</div>
<div style="display: grid; background-color: lightgreen; padding: 10px;">
    Grid element (unsupported)
</div>
<div style="display: table; background-color: lightcoral; padding: 10px;">
    Table element (unsupported)
</div>
```
**Result**: Each element emits a structured warning and falls back to HTML semantic default (div=block).

#### Class-Based Unsupported Values
```html
<style>
.flex-item { display: flex; }
.grid-item { display: grid; }
</style>
<div class="flex-item" style="background-color: lightblue; padding: 10px;">
    Flex via class (unsupported)
</div>
<div class="grid-item" style="background-color: lightgreen; padding: 10px;">
    Grid via class (unsupported)
</div>
```
**Result**: Each element emits a structured warning and falls back to HTML semantic default.

### Precedence Rules

#### Class vs Inline Style Precedence
```html
<style>
.block-class { display: block; }
.inline-block-class { display: inline-block; }
</style>
<div class="block-class" style="display: inline-block; background-color: lightblue; padding: 10px;">
    Inline style overrides class
</div>
```
**Result**: Element displays as inline-block (inline style wins over class).

#### Multiple Classes (Later Wins)
```html
<style>
.first-class { display: block; }
.second-class { display: inline-block; }
</style>
<div class="first-class second-class" style="background-color: lightgreen; padding: 10px;">
    Multiple classes - later wins
</div>
```
**Result**: Element displays as inline-block (second class wins).

### Spacing Interactions

#### Block with Complex Spacing
```html
<div style="display: block; margin: 20px; padding: 15px; border: 3px solid black; background-color: lightblue;">
    Block with margins, padding, and border
</div>
```
**Result**: All spacing properties apply correctly with proper box model calculations.

#### Inline-Block with Spacing
```html
<div style="display: inline-block; width: 150px; margin: 10px; padding: 8px; border: 2px solid red; background-color: lightgreen;">
    Inline-block with spacing
</div>
<div style="display: inline-block; width: 150px; margin: 10px; padding: 8px; border: 2px solid blue; background-color: lightcoral;">
    Another inline-block with spacing
</div>
```
**Result**: Spacing applies correctly, elements flow inline with proper spacing.

#### Nested Elements with Different Display Values
```html
<div style="display: block; margin: 20px; padding: 15px; background-color: lightblue;">
    Block container
    <div style="display: inline-block; width: 100px; margin: 5px; padding: 8px; background-color: lightgreen;">
        Nested inline-block
    </div>
    <div style="display: inline-block; width: 100px; margin: 5px; padding: 8px; background-color: lightcoral;">
        Another nested inline-block
    </div>
    <div style="display: none; background-color: lightyellow; padding: 10px;">
        Hidden nested element
    </div>
</div>
```
**Result**: Container is block, nested inline-blocks flow together, hidden element is omitted.

## Fixtures
- Include samples covering block, inline-block, none (block/inline/nested) with spacing interactions.
