# Feature Specification: CSS Display Support (Block and Inline-Block)

**Feature Branch**: `001-css-display-support`  
**Created**: 2025-10-15  
**Status**: Completed  
**Input**: User description: "Extend css attributes to support \"display: block/inline-block\" that define display behavior (the type of rendering box) of an element"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Honor block and inline-block layout (Priority: P1)

As a report author, when elements specify `display: block` or `display: inline-block`,
their layout behaves accordingly so documents render predictably.

**Why this priority**: Correct layout semantics are foundational for report fidelity.

**Independent Test**: Render sample HTML with mixed `div`, `span`, `p`, `img`, `table`
using `display: block/inline-block` and verify positions, line breaks, and box
dimensions via existing PDF validation helpers.

**Acceptance Scenarios**:

1. **Given** surrounding inline text, **When** an element has `display: block`,
   **Then** it starts on a new line and occupies the available width unless width is specified, with margins/padding/border applied.
2. **Given** inline text, **When** an element has `display: inline-block` with a fixed width,
   **Then** it participates in inline flow as an atomic box, staying on the same line if space allows and wrapping as a whole when space runs out.

---

### User Story 2 - Omit elements with display:none (Priority: P1)

As a report author, when an element specifies `display: none`, it does not
render any box and occupies no space in the layout; its contents are not
rendered.

**Why this priority**: Authors rely on conditionally hiding sections without
affecting layout.

**Independent Test**: Render samples where `div`, `span`, and nested elements
have `display: none` and verify complete omission from the PDF output and text
extraction.

**Acceptance Scenarios**:

1. **Given** a block element with `display: none`, **When** rendered, **Then** it
   produces no output and no layout effect (including its children).
2. **Given** an inline element with `display: none` inside a text run,
   **When** rendered, **Then** it contributes no width/height and no text.

---

### User Story 3 - Warnings and fallbacks for unsupported values (Priority: P2)

As a developer, when `display` has unsupported values (e.g., `grid`, `flex`),
the system emits a structured warning and falls back to a sensible default.

**Why this priority**: Prevents silent failures and ensures predictable output.

**Independent Test**: Render samples using unsupported `display` values and
assert a single structured warning per occurrence and consistent fallback layout.

**Acceptance Scenarios**:

1. **Given** `display: grid`, **When** rendering, **Then** a structured warning is logged and the element uses the default display behavior.

---

### User Story 4 - Interactions with width/height/margins (Priority: P3)

As a report author, `display` semantics interact correctly with width/height and
spacing properties.

**Why this priority**: Common report layouts rely on size and spacing.

**Independent Test**: Render elements with width/height/margin/padding/border
under both display modes and verify computed layout in PDF.

**Acceptance Scenarios**:

1. **Given** `display: inline-block` with `width: 120`, **When** rendered,
   **Then** the box width equals 120 units plus horizontal padding/border and flows inline.
2. **Given** adjacent blocks with vertical margins, **When** rendered,
   **Then** vertical margin collapsing follows documented rules (largest margin wins).

---

### Edge Cases

- Inline style vs class precedence for `display`.
- Multiple classes defining `display` (last one wins).
- `span` with `display: block` (line break enforced) and `div` with `display: inline-block` (inline flow).
- Nested `inline-block` boxes in a line; wrapping as whole boxes.
- `display` specified alongside `width/height: auto` or zero values.
- Elements with default semantics overriding unspecified `display`.
- Ancestor with `display: none` hides all descendants with no residual spacing.
- Conflicting properties with `display: none` (e.g., margins/width/height) have
  no effect and produce no warnings.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST parse the CSS `display` property from inline styles and class declarations.
- **FR-002**: The system MUST support `display: block` semantics: starts on a new line, uses available width by default, applies margin/padding/border, and participates in vertical margin collapsing with adjacent blocks.
- **FR-003**: The system MUST support `display: inline-block` semantics: behaves as an atomic inline-level box that flows within the line, wraps as a whole when insufficient space remains, and applies width/height and spacing.
- **FR-004**: For unsupported `display` values, the system MUST emit a structured warning and ignore the value, falling back to the HTML element's semantic default (e.g., `div`=block, `span`=inline).
- **FR-005**: Style precedence MUST follow: multiple classes merge in attribute order (later wins), and inline style overrides class-derived values.
- **FR-006**: Invalid or unrecognized `display` values MUST NOT throw; they MUST produce a single structured warning per element.
- **FR-007**: Provide fixtures and tests covering `div`, `span`, `p`, `img`, and `table` with `display: block/inline-block` plus spacing interactions.
- **FR-008**: Update supported CSS contracts/documentation to list `display: block` and `display: inline-block` and note unsupported values with fallback behavior.
- **FR-009**: For `display: inline-block`, inline-level vertical alignment MUST default to middle relative to the line box.
- **FR-010**: For `display: inline-block`, top/bottom margins MUST NOT affect the line box height (they affect surrounding spacing but not line sizing).
-- **FR-011**: The system MUST support `display: none` semantics: the element
   generates no box and contributes no layout; its contents are not rendered.
- **FR-012**: If an ancestor has `display: none`, all descendants MUST NOT
  render and MUST NOT produce individual warnings.
- **FR-013**: When `display: none` is set, other layout properties (size,
  margins, padding, border) MUST be ignored without warnings.
- **FR-014**: Provide fixtures and tests covering `display: none` for block,
  inline, and nested elements; update contracts/documentation accordingly.

### Key Entities *(include if feature involves data)

- **Style (conceptual)**: has `Display` attribute with allowed values: `block`, `inline-block`, `default`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All P1 fixtures render with expected line breaks and box dimensions (100% pass).
- **SC-002**: Unsupported `display` values produce exactly one structured warning per element (≥ 95% automated detection across fixtures).
- **SC-003**: Existing rendering tests remain green (0 regressions).
- **SC-004**: Rendering time impact ≤ 5% compared to baseline on sample documents.
- **SC-005**: All `display: none` fixtures produce no visible output or text (100% pass).

## Assumptions

- Only `block` and `inline-block` are added in this iteration; other values remain unsupported and trigger warnings.
- Default element behavior applies when `display` is omitted.
- Existing percentage/absolute sizing and margin collapsing rules remain unchanged.

