# Migration Plan: Layout Pipeline Architecture

This plan describes how we will migrate from the current DOM → QuestPDF composers flow to the new layered layout pipeline defined in `docs/architecture-layout-pipeline.md`.

Related: `docs/coding-standards.md`, `docs/testing-guidelines.md`.

## Overview

We will incrementally introduce a layout layer between the DOM and rendering, minimizing risk and keeping behavior stable. The new layer outputs positioned fragments and isolates formatting contexts (Block, Inline, Inline-Block, later Table/Flex/Grid). Rendering becomes a thin adapter.

## Scope & Non‑Goals

- In scope: display classification, box model normalization, inline/paragraph layout, pagination, renderer adapter.
- Out of scope (initially): full CSS coverage, complex tables (border-collapse), flex/grid algorithms, pixel-perfect PDF diffs.

## Guiding Principles

- Keep public surface stable (`PdfBuilder`).
- Prefer pure, immutable data in the layout layer.
- Add features behind flags with safe fallbacks.
- Test geometry and contracts, not QuestPDF internals.
- Follow `docs/coding-standards.md` and `docs/testing-guidelines.md`.

## Phases

### Phase 1 — Extract Seams (No behavior change)
- Objectives
  - Centralize display decisions and spacing application.
  - Isolate inline flow logic behind an internal service.
- Changes (implementation later; documented now)
  - Add `DisplayClassifier` (explicit CSS wins; semantic default otherwise).
  - Add `WrapWithSpacing` to DRY margin/padding/border decisions (still QuestPDF-backed).
  - Introduce `InlineFlowLayoutEngine` and make `InlineComposer` delegate to it.
- Deliverables
  - Internal APIs documented in code comments referencing `architecture-layout-pipeline.md`.
  - Unit tests for display classification and inline flow (behavior preserved).
- Flags
  - None (pure refactor). Optional logging toggle for classifier trace.

### Phase 2 — Layout Model & Engine (Partial adoption)
- Objectives
  - Introduce `LayoutBox`, `LayoutConstraints`, `LayoutFragment`, and `ILayoutEngine`.
  - Implement Block and Inline formatting contexts for paragraphs, headings, spans.
- Changes
  - Build `LayoutBox` from DOM+styles.
  - `ILayoutEngine` selects context per box; returns fragment tree (no pagination yet).
  - Adapt composers to consume fragments for supported elements; tables/lists remain legacy.
- Deliverables
  - Geometry tests for block/inline fragments (positions, sizes, baselines, wraps).
  - Trace dump option for boxes/fragments.
- Flags
  - `EnableNewLayoutForTextBlocks` (default off → on in CI after parity).

### Phase 3 — Pagination & Renderer Adapter
- Objectives
  - Split fragments by `PageConstraints` and render via adapter.
- Changes
  - Pagination step produces per‑page fragment trees.
  - Add `IRendererAdapter` and `QuestPdfAdapter`; `PdfRenderer` orchestrates DOM → Layout → Fragments → Adapter.
  - Remove direct QuestPDF calls from layout contexts.
- Deliverables
  - Pagination tests (carryover fragments, page boundaries). Adapter mapping tests.
- Flags
  - `EnablePagination`, `EnableQuestPdfAdapter` (default off → on after tests).

### Phase 4 — Extend Context Coverage
- Objectives
  - Add `InlineBlockFormattingContext`; migrate inline‑block behavior.
  - Migrate tables into `TableFormattingContext`; add initial border model.
  - Prepare Flex (and Grid later) behind flags.
- Changes
  - Implement contexts incrementally; provide graceful block fallback when disabled/unsupported.
- Deliverables
  - Context‑specific geometry tests; feature-flag integration tests.
- Flags
  - `EnableInlineBlock`, `EnableTableContext`, `EnableTableBorderCollapse`, `EnableFlexContext`.

## Testing Strategy

- Unit: classifier, spacing calculations, inline line building, block flow.
- Layout geometry: assert fragment positions/sizes/baselines/wraps.
- Pagination: verify page splits and residual fragments.
- Adapter: verify fragment → QuestPDF mapping (behavioral assertions; avoid image diffs by default).
- Integration: retain existing end‑to‑end tests; add a small set gated by flags.

## Backward Compatibility & Rollout

- `PdfBuilder` API remains unchanged.
- New behavior is gated; default remains legacy until parity tests pass.
- Staged rollout: enable flags in CI, then default‑on in main; keep opt‑out for at least one release.

## Observability

- Structured logs: node path, display class, chosen context, measured sizes.
- Optional trace files for `LayoutBox` and `LayoutFragment` trees.

## Risk Mitigation

- Feature flags with safe fallbacks.
- Incremental adoption (per element/context).
- Geometry‑level tests to catch regressions early.

## Repository & Ownership

- New code lives under `src/NetHtml2Pdf/Layout/` (model, contexts, engine) and `src/NetHtml2Pdf/Render/Adapter/`.
- Each file includes a short header comment pointing to `docs/architecture-layout-pipeline.md`.

## Timeline (Indicative)

- Phase 1: 1–2 iterations
- Phase 2: 2–3 iterations
- Phase 3: 2 iterations
- Phase 4: iterative, feature by feature

## Acceptance Criteria

- Parity: existing tests pass with new flags off; then with flags on for supported elements.
- Coverage: geometry tests for supported contexts; pagination correctness.
- Docs: architecture + migration kept current and referenced from code.

## References

- Architecture: `docs/architecture-layout-pipeline.md`
- Standards: `docs/coding-standards.md`, `docs/testing-guidelines.md`

