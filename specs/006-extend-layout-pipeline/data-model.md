# Data Model: Extended Formatting Contexts

## Overview
Phase 4 introduces three formatting contexts layered on top of the Phase 3 pagination pipeline:

1. **InlineBlockFormattingContext** – bridges inline and block layout rules for inline-block elements.
2. **TableFormattingContext** – handles table headers, body rows, footers, and pagination metadata.
3. **FlexFormattingContext (Preview)** – provides limited one-dimensional flex behaviour with explicit guardrails.

All contexts emit `LayoutFragment` trees compatible with `PaginationService` and downstream adapters.

## Entities & Relationships

### InlineBlockFormattingContext
- **Inputs**: `LayoutBox` representing an inline-block element, computed styles (width, height, margins), child fragments.
- **Outputs**: `LayoutFragment` with explicit baseline, intrinsic width/height, and child fragments (if the element wraps block children).
- **Relationships**: Consumes the inline formatting context for nested inline content; exposes fragments to pagination without bypassing block contexts.

### TableFormattingContext
- **Inputs**: `LayoutBox` tree structured as table -> sections (`thead`, `tbody`, `tfoot`) -> rows -> cells.
- **Outputs**: `LayoutFragment` hierarchy with row groups, cell fragments, per-row height metadata, and pagination hints.
- **Additional Data**:
  - `TableSectionFragment`: aggregates `RowFragment`s with header/footer flags.
  - `RowFragment`: stores cell fragments, intrinsic heights, and carry-over pointers.
  - `TableDiagnostics`: records column widths, border model (separate vs collapse), and downgrade notes.
- **Relationships**: Collaborates with pagination to repeat headers and handle carry-over of partial rows.

### FlexFormattingContext (Preview)
- **Inputs**: `LayoutBox` annotated with `display:flex` plus properties `flex-direction`, `justify-content`, `align-items`, and `gap`.
- **Constraints**: `flex-wrap` forced to `nowrap`; unsupported properties (e.g., `flex-basis:auto`, `align-content`) trigger warnings and fallback.
- **Outputs**: `LayoutFragment` for the container with children annotated with flex offsets and sizes.
- **Relationships**: Emits downgrade diagnostics consumed by `RendererContext.Logger` when flags enabled.

## Supporting Metadata
- **FormattingContextOptions**: Derived from `RendererOptions` feature flags; prevents accidental activation when flags are disabled.
- **Diagnostics Records**: Each context emits structured log payloads (`FormattingContextDiagnostics`) with node paths, downgraded features, and sizing decisions.

## Flag Matrix

| Feature Flag | Context Enabled | Notes |
|--------------|----------------|-------|
| `EnableInlineBlockContext` | InlineBlockFormattingContext | Defaults to `false`; parity gate |
| `EnableTableContext` | TableFormattingContext | Defaults to `false`; requires pagination flag |
| `EnableTableBorderCollapse` | Advanced table collapse | Optional follow-up flag; Phase 4 treats collapse as separate |
| `EnableFlexContext` | FlexFormattingContext (preview) | Defaults to `false`; warns for unsupported features |

## Data Flow Summary
1. `RendererOptions` flags propagate through `PdfBuilder` -> `PdfRenderer` -> layout engine.
2. Layout engine selects appropriate formatting context based on computed display value and flag state.
3. Formatting context produces fragments with diagnostics metadata.
4. `PaginationService` slices fragments into `PaginatedDocument` respecting headers/footers, flex baseline, etc.
5. `IRendererAdapter` consumes paginated fragments; diagnostics flag controls structured logging output.

## Validation & Testing Considerations
- Unit tests should exercise fragment geometry, diagnostics payloads, and flag gating for each context.
- Integration tests validate pagination outcomes (header repetition, inline-block sizing, flex alignment) via rendered PDF byte comparisons or structural assertions.
- Performance tests run with the 50-page fixture pre/post enabling contexts to ensure +/-5% delta.


