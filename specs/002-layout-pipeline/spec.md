# Feature Specification: Layout Pipeline Architecture and Migration

**Feature Branch**: `002-layout-pipeline`  
**Created**: 2025-10-16  
**Status**: Draft  
**Input**: User description: "Layout pipeline architecture and migration plan"

## Clarifications

### Session 2025-10-16

- Q: For DisplayClassifier semantic defaults (when CSS display not set), how should list and table sub-elements be classified to preserve current behavior? → A: B (ListItem = Block; table sub-elements classified indirectly via Table context; do not explicitly classify them)

- Q: For unknown/unsupported node types without explicit CSS display, what fallback should be used? → A: C (Warn once per node type, then treat as Block)

- Q: For unsupported CSS display values (e.g., flex, grid) encountered in styles, what fallback and warning policy should we adopt? → A: A (Warn once per value; fallback to semantic default)

- Q: For the optional classifier trace logging, what granularity do you prefer? → A: A (Off by default; enable via single boolean flag - global)

- Q: For spacing wrapper behavior under nested containers, should parent “margin” simulation via padding be cumulative or clamped to avoid excessive stacking? → A: A (Cumulative; preserve current exact behavior)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Extract Seams Without Behavior Change (Priority: P1)

As a developer, I can centralize display classification and spacing logic, and
delegate inline flow to an internal engine so that composers are simpler and
easier to test, with zero change to output.

**Why this priority**: Reduces complexity and risk; foundational for later
layout work without changing behavior.

**Independent Test**: Run existing renderer tests; add unit tests for
DisplayClassifier and InlineFlow engine, verifying parity with current behavior.

**Acceptance Scenarios**:

1. Given existing inputs and styles, When DisplayClassifier runs, Then elements
   are classified identically to current logic (block/inline/inline-block/none).
2. Given inline content trees, When InlineFlow engine composes, Then produced
   text runs and line breaks match current output.

---

### User Story 2 - Introduce Layout Model for Text Blocks (Priority: P2)

As a developer, I can produce `LayoutBox` and `LayoutFragment` for paragraphs,
headings, and spans behind a flag, enabling geometry tests while keeping legacy
paths for other elements.

**Why this priority**: Establishes the intermediate layout model incrementally.

**Independent Test**: With the flag enabled, geometry tests assert positions,
sizes, baselines, and wrap points for text blocks only.

**Acceptance Scenarios**:

1. Given a paragraph with inline content, When laid out under constraints, Then
   fragment positions and widths match expectations and wrap points are stable.
2. Given headings with default sizes, When measured, Then baselines and sizes
   reflect heading levels consistently.

---

### User Story 3 - Pagination and Renderer Adapter (Priority: P3)

As a developer, I can paginate fragment trees by page constraints and render via
a stateless adapter, isolating rendering from layout.

**Why this priority**: Decouples rendering from layout, improving testability
and portability.

**Independent Test**: Pagination tests verify fragment splitting; adapter tests
verify mapping to rendering primitives without pixel diffs.

**Acceptance Scenarios**:

1. Given a long text flow, When applying page height constraints, Then fragments
   split across pages with correct carryover.
2. Given a fragment tree, When rendered via the adapter, Then the expected
   rendering API calls are invoked in the correct order.

---

### Edge Cases

- Unsupported or unknown display values: classifier falls back to semantic
  defaults; optional trace logging available.
- Elements with `display:none`: nodes and descendants are skipped consistently.
- Large margins/padding: spacing wrapper preserves current “margin at parent”
  simulation via container padding.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a centralized display classification service
  that honors explicit CSS and semantic defaults.
- **FR-002**: System MUST provide a spacing wrapper that applies margin (at
  parent), border, and padding (at element) consistent with current behavior.
- **FR-003**: An InlineFlow engine MUST compose inline content with style
  inheritance and line breaks matching current output.
- **FR-004**: System MUST (behind a flag) produce `LayoutBox` and
  `LayoutFragment` for paragraphs, headings, and spans.
- **FR-005**: System MUST (behind a flag) support pagination from fragment flow
  to page-scoped fragment trees and render via a stateless adapter.

- **FR-006**: DisplayClassifier semantic defaults MUST classify `ListItem` as
  Block. Table sub-elements (thead/tbody/row/th/td/section) MUST be handled via
  the Table context and not explicitly classified; they inherit behavior through
  the Table formatter.

- **FR-007**: For unknown/unsupported node types without explicit CSS display,
  the system MUST emit a single warning per node type per run and then treat
  such nodes as Block by default.

- **FR-008**: For unsupported CSS display values (e.g., flex, grid), the system
  MUST emit a single warning per value per run and fallback to the element's
  semantic default display classification.

- **FR-009**: Classifier trace logging MUST be off by default and controlled by
  a single global boolean flag. When enabled, logs include node path, display
  source (explicit vs semantic), and final classification.

- **FR-010**: Spacing wrapper MUST preserve cumulative “margin via parent
  padding” behavior for nested containers (no clamping), matching current
  rendering semantics exactly.

### Key Entities *(include if feature involves data)*

- **LayoutBox**: Layout input derived from DOM + computed styles; input to
  formatting contexts.
- **LayoutFragment**: Positioned, sized output (with baselines and children)
  produced by contexts and consumed by pagination/renderer.
- **DisplayClassifier**: Maps element + styles to a formatting class
  (block/inline/inline-block/none).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: All existing renderer tests pass unchanged after Phase 1
  refactors.
- **SC-002**: New unit tests for classifier and inline engine achieve ≥90%
  branch coverage across their modules.
- **SC-003**: With flags on for text blocks, geometry tests pass for paragraphs
  and headings on CI.
- **SC-004**: Pagination tests demonstrate correct fragment splitting for at
  least three page sizes.
