# Feature Specification: Phase 2 - Layout Model, Rendering Parity Intact

**Feature Branch**: `003-rendering-parity`  
**Created**: 2025-10-17  
**Status**: Completed  
**Input**: User description: "Goals & Guardrails
- Keep external behavior identical. Only paragraphs, headings, and span-level inline text switch to the new pipeline.
- Introduce a minimal layout layer that can coexist with the legacy composers. All new machinery runs behind a feature flag (EnableNewLayoutForTextBlocks), default-off until parity is proven.
- Preserve QuestPDF as the rendering backend; the change is layout-first, renderer-last."

## Clarifications

### Session 2025-10-17

- Q: What is the intended approach for fragment tree serialization? -> A: Emit fragment trees as JSON payloads to the structured logging pipeline (no files).
- Q: How should the `EnableNewLayoutForTextBlocks` flag be toggled? -> A: Add a boolean property on `ConverterOptions` passed to `PdfBuilder`.
- Q: Which elements remain on the legacy pipeline during Phase 2? -> A: Tables, lists, inline-blocks, and floats stay on the legacy composers.
- Q: What identifier should fragments carry for diagnostics? -> A: Attach the DOM node path string to each fragment.
- Q: What is the expected failover behavior if the new layout errors on supported nodes? -> A: Abort rendering and bubble the exception to the caller.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Toggle new layout pipeline for text blocks (Priority: P1)

When I enable the new layout flag, paragraph- and heading-based content should be laid out by the new model yet render identically to the legacy pipeline.

**Why this priority**: This is the primary behavior change for Phase 2 and provides the proof that the new layout layer can coexist without user-visible regressions.

**Independent Test**: Enable `EnableNewLayoutForTextBlocks` and render sample HTML containing paragraphs, headings, and spans; compare PDF or extracted text output against legacy baseline and assert byte-for-byte parity.

**Acceptance Scenarios**:

1. **Given** the feature flag is enabled for a document with mixed paragraphs and inline spans, **When** the PDF is rendered, **Then** the resulting output matches the baseline produced with the flag disabled.
2. **Given** the feature flag is disabled, **When** the same document is rendered, **Then** the system automatically routes all elements through the legacy composers.

---

### User Story 2 - Observe layout engine diagnostics (Priority: P2)

As a developer I can inspect structured logs and optional fragment dumps that confirm which pipeline handled each node and what sizing decisions were made.

**Why this priority**: Diagnostics are needed to validate parity and root-cause mismatches while the new layout is behind a flag.

**Independent Test**: Enable diagnostic logging, render a document, and verify logs include context name, node path, constraints, and fragment sizes for paragraphs/headings/spans.

**Acceptance Scenarios**:

1. **Given** diagnostics are enabled, **When** the new layout processes a paragraph, **Then** logs include the node path, selected formatting context, input constraints, and measured fragment size.

---

### User Story 3 - Fallback to legacy behavior on unsupported nodes (Priority: P3)

If the new pipeline encounters elements it cannot handle (e.g., tables, lists, inline-block), it should seamlessly defer to the legacy composers without throwing or altering output.

**Why this priority**: Ensures migration safety and limits the blast radius while the layout layer is incomplete.

**Independent Test**: Render a document with tables and lists while the flag is enabled; confirm those structures still route through the legacy implementation and their rendered output matches baseline.

**Acceptance Scenarios**:

1. **Given** the feature flag is enabled and a document contains a table, **When** rendering occurs, **Then** logs show the legacy table composer was used and the output matches the baseline file.

---

### Edge Cases

- How does the layout engine behave when nested spans include mixed inline styles, soft breaks, and whitespace-only nodes? Ensure fragment creation preserves whitespace handling from the legacy pipeline.
- What happens when a paragraph exceeds the available inline width due to long unbreakable text? Confirm the inline context falls back to the same line-breaking logic and produces identical wrap decisions.
- How does the system handle documents that toggle the feature flag mid-tree (e.g., per-section configuration)? Validate the flag is evaluated per render invocation and cannot be partially applied inside a single render.
- What is the behavior when diagnostics are disabled in production? Confirm no additional logging overhead or fragment dumps are produced.
- What happens if fragment measurement fails for a supported node? Ensure the renderer surfaces the exception to the caller so parity regressions are detected during validation.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST introduce `LayoutBox`, `LayoutConstraints`, and `LayoutFragment` abstractions to represent layout inputs, available geometry, and measured output respectively.
- **FR-002**: The system MUST provide `BlockFormattingContext` and `InlineFormattingContext` that consume `LayoutBox` trees and return `LayoutFragment` instances for paragraphs, headings, and spans.
- **FR-003**: The system MUST expose an `ILayoutEngine` facade that orchestrates display classification, formatting context selection, and fragment generation.
- **FR-004**: The system MUST add a feature flag (`EnableNewLayoutForTextBlocks`) that gates use of the new layout engine and defaults to off.
- **FR-005**: The system MUST adapt `BlockComposer` so that, when the flag is enabled and the element is a paragraph/heading/span, it consumes layout fragments and renders them through QuestPDF without altering visual output.
- **FR-006**: The system MUST fall back to the legacy composers for any element or display mode not yet supported by the new layout engine.
- **FR-007**: The system MUST emit structured diagnostics (node path, context choice, constraints, fragment size) when layout tracing is enabled.
- **FR-008**: The system MUST provide automated parity tests that validate flagged-on output matches flagged-off output for representative HTML samples.
- **FR-009**: The system MUST support opt-in fragment tree serialization for developer debugging by emitting fragment trees as JSON payloads through the structured logging pipeline (no on-disk artifacts).
- **FR-010**: The system MUST surface the `EnableNewLayoutForTextBlocks` feature flag as a boolean property on `ConverterOptions`, allowing callers to opt in per invocation, and ensure `PdfBuilder` propagates the setting to the renderer pipeline.
- **FR-011**: Phase 2 MUST leave tables, lists, inline-block elements, and floats on the legacy composers regardless of the feature flag state, ensuring they bypass the new layout engine entirely.
- **FR-012**: If the new layout pipeline encounters an unexpected error while processing a supported text block, it MUST propagate the exception to the caller without attempting automatic fallback.

### Key Entities *(include if feature involves data)*

- **LayoutBox**: Represents a style-resolved node entering layout; includes tag, display classification, computed spacing, children, and references to source DOM nodes.
- **LayoutConstraints**: Captures available space for layout calculation (inline size range, block progression, pagination hints); passed down the formatting contexts.
- **LayoutFragment**: Immutable measurement result consisting of final size, baseline metrics, and child fragment references that can be fed to renderers or paginated later.
- Each `LayoutFragment` MUST include the originating DOM node path string so diagnostics and parity tooling can trace fragments back to their source nodes.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Rendering a suite of paragraph/heading/span documents with the feature flag enabled produces byte-identical PDFs to the legacy pipeline in 100% of monitored cases.
- **SC-002**: Automated regression tests covering block and inline scenarios execute under both flag states with no new failures introduced by the layout engine.
- **SC-003**: Diagnostic logs contain context, constraints, and fragment dimensions for at least 95% of nodes processed by the new layout when tracing is on.
- **SC-004**: Rendering performance for text-only documents regresses by no more than 5% when the feature flag is enabled (measured as average render time across baseline samples).
