# Feature Specification: Phase 4 – Extended Formatting Context Coverage

**Feature Branch**: `006-extend-layout-pipeline`  
**Created**: 2025-10-21  
**Status**: Draft  
## Clarifications
### Session 2025-10-21
- Q: For the initial FlexFormattingContext preview, how should containers with `flex-wrap: wrap` be handled? -> A: Do not support wrapping; treat `flex-wrap` as `nowrap` and render a single line
- Q: When `EnableTableContext` is true but the advanced border-collapse flag is off, how should tables with `border-collapse: collapse` behave? -> A: Render via TableFormattingContext but treat `border-collapse` as `separate` (no collapse)
## User Scenarios & Testing *(mandatory)*

### User Story 1 - Inline-Block Layout Support (Priority: P1)

As a document author using inline-block elements, I can enable the inline-block formatting flag and have those elements measured and positioned by the layout engine so that pagination and adapters render them consistently without falling back to legacy composers.

**Why this priority**: Inline-block is a prerequisite for many modern layouts (badges, cards, icon/text combos); missing support blocks several partner documents from using the new pipeline.

**Independent Test**: Render a fixture containing inline, inline-block, and block siblings with the flag toggled. With the flag on, geometry assertions confirm layout engine output matches expectations and pagination/adapters render correctly without hitting legacy code paths.

**Acceptance Scenarios**:

1. **Given** a document containing inline-block elements and the inline-block flag enabled, **When** PdfRenderer runs through the adapter pipeline, **Then** LayoutEngine routes those nodes through `InlineBlockFormattingContext` and the produced fragments render with correct width/height and baseline alignment.
2. **Given** the same document with the flag disabled, **When** PdfRenderer executes, **Then** those elements fall back to the legacy composer path with unchanged output, ensuring backward compatibility.

---

### User Story 2 - Table Formatting Context (Priority: P2)

As a user rendering tabular data, I can opt into a table formatting context that converts tables into layout fragments (including header/footer rows) so pagination, carry-over, and adapters treat tables consistently while advanced behaviors remain gated by feature flags.

**Why this priority**: Tables are ubiquitous in generated PDFs; without proper context handling, pagination of tables remains unreliable, limiting Phase 3 adoption.

**Independent Test**: Execute regression fixtures containing tables with the table context flag toggled. With the flag on, geometry and pagination tests validate row splitting, header retention, and column widths. With the flag off, output matches the legacy composer.

**Acceptance Scenarios**:

1. **Given** a table with header and body rows and the table context flag enabled, **When** pagination runs, **Then** the header rows repeat on new pages and column widths stay consistent across fragments.
2. **Given** a table with `border-collapse` disabled and the border-collapse flag off, **When** rendering completes, **Then** the adapter pipeline matches legacy spacing, and enabling the flag applies the new border handling rules.

---

### User Story 3 - Flex Layout Preview (Priority: P3)

As a developer experimenting with modern layouts, I can enable a flex formatting flag to render simple one-dimensional flex containers via the layout engine, while unsupported constructs gracefully fall back to legacy rendering.

**Why this priority**: Flex support unblocks incremental migration of more sophisticated layouts; keeping it behind a flag lets us collect feedback without destabilizing production.

**Independent Test**: Build targeted fixtures for row/column flex containers with basic justify/align rules. With the flex flag on, geometry assertions verify item positioning; with the flag off, output matches the existing composer behavior.

**Acceptance Scenarios**:

1. **Given** a flex container with `display:flex` and the flex flag enabled, **When** LayoutEngine processes the DOM, **Then** items are positioned according to flex direction and justification rules captured in fragment metadata.
2. **Given** a flex container using unsupported properties (e.g., `flex-basis:auto`) while the flag is enabled, **When** LayoutEngine encounters the node, **Then** it emits a warning and safely falls back to legacy rendering without throwing.

---

### Edge Cases

- What happens when inline-block elements overflow the available inline space? Layout engine must wrap or paginate without duplicating fragments.
- How does the system handle tables spanning more than two pages with mixed row heights and captions? Ensure carry-over metadata stays accurate, captions anchor correctly, and when `border-collapse` is requested but disabled, the layout treats it as `separate` while logging the downgrade.
- How do flex items behave when the sum of preferred widths exceeds container width while flex flag is enabled? Items should shrink per flex rules or fall back with diagnostics, and wrapping (`flex-wrap`) remains unsupported (treated as `nowrap`).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide `InlineBlockFormattingContext` to measure and arrange inline-block elements when `EnableInlineBlockContext` is true.
- **FR-002**: System MUST preserve existing inline layout behavior when `EnableInlineBlockContext` is false.
- **FR-003**: System MUST introduce `TableFormattingContext` that handles table header/footer repetition, column sizing, and pagination when `EnableTableContext` is true.
- **FR-004**: System MUST guard advanced table features (e.g., `border-collapse`, column groups) behind dedicated flags and, when those flags are off, default to treating `border-collapse` as `separate` while emitting diagnostics as needed.
- **FR-005**: System MUST implement a preview `FlexFormattingContext` for one-dimensional flex containers when `EnableFlexContext` is true, emitting diagnostics, degrading safely for unsupported constructs, and treating `flex-wrap` as `nowrap` (wrapping not supported in preview).
- **FR-006**: System MUST emit structured diagnostics for each new formatting context when the relevant diagnostics flag is active.
- **FR-007**: System MUST ensure adapters consume the new fragment metadata without requiring QuestPDF-specific logic in formatting contexts.

### Key Entities *(include if feature involves data)*

- **InlineBlockFormattingContext**: Applies inline-level sizing rules while respecting block-level box metrics; outputs fragments with baseline metadata for pagination/adapters.
- **TableFormattingContext**: Converts DOM tables into fragment trees capturing row groups, headers, and cells, including carry-over hints for pagination.
- **FlexFormattingContext**: Computes item positions for simple flex containers, attaching alignment metadata for adapters.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: With inline-block flag enabled, regression fixtures containing inline-block elements render via the adapter pipeline with <2% layout deviation compared to baseline screenshots.
- **SC-002**: Table fixtures paginate with header repetition and column alignment when `EnableTableContext` is true, achieving 100% pass rate in updated table regression suite.
- **SC-003**: Flex preview flag reduces legacy composer usage for flagged flex containers by at least 80% in telemetry within the first release cycle.
- **SC-004**: No increase (>5%) in PDF generation time for migrated fixtures when new context flags are enabled.









