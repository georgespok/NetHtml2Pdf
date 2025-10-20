# Research: Phase 3 Pagination & Renderer Adapter

## Summary
- Introduce a pagination pass that slices the layout fragment tree into per-page fragments using page constraints.
- Add a renderer adapter seam so PdfRenderer orchestrates layout and pagination before delegating to QuestPDF via an adapter.
- Gate the new pipeline behind `EnablePagination` and `EnableQuestPdfAdapter` feature flags to preserve parity while iterating.

## Current State
- Rendering path today: `PdfBuilder` (public) → `PdfRenderer` → QuestPDF document builders. Layout composers directly emit QuestPDF elements.
- Phase 2 delivered `LayoutEngine` and `LayoutFragment` abstractions but PdfRenderer still calls composers, bypassing the fragment model.
- No pagination layer exists; multi-page output relies on splitting DOM into `_pages` before render, making headers/footers static per PdfBuilder invocation.
- Feature flag support limited to layout enablement (`EnableNewLayoutForTextBlocks`); no pagination flag.

## Proposed Changes
- Pagination service consumes `LayoutFragment` roots and `PageConstraints` (page size, margins, header/footer bands) to produce `PaginatedDocument` with ordered `PageFragmentTree` entries.
- Carry-over logic must split block fragments at measured breakpoints, emit metadata linking slices, and respect keep hints.
- `IRendererAdapter` interface isolates render backend; initial `QuestPdfAdapter` maps fragments to QuestPDF fluent API.
- PdfRenderer orchestrates DOM parsing → layout → pagination → adapter render when flags are enabled; legacy composer path remains as fallback.
- Feature flags default off; configuration sourced via `RendererOptions` / `ConverterOptions` so tests can toggle per render request.

## Key Questions & Answers
- How to handle oversized keep-together fragments? → Abort pagination and surface descriptive error (clarified 2025-10-20).
- Do pages inherit header/footer markup per slice? → Header/footer fragments parsed once and reused across page renders.
- How will adapters access fonts/options? → Pass `RendererOptions`/render context into adapter so QuestPdfAdapter can register fonts without PdfRenderer leaking backend specifics.

## Risks
- Fragment splitting errors could cause layout drift or infinite loops (carry-over must reduce height each iteration).
- Adapter abstraction may regress performance if fragment traversal duplicates work; caching considerations needed.
- Feature-flag mismatches (pagination on, adapter off) must gracefully revert to legacy path without partially applied changes.

## Mitigations
- Build unit tests for pagination edge cases (carry-over, keep-with-next, empty content area).
- Leverage existing regression fixtures with both flags toggled to confirm parity before defaulting on.
- Instrument pagination loop with guardrails (max iterations, tolerance thresholds) and diagnostics to aid troubleshooting.

## References
- Phase 2 spec: `C:\Projects\NetHtml2Pdf\specs\002-layout-pipeline\spec.md`
- Existing renderer implementation: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Renderer\PdfRenderer.cs`
- Layout fragment model: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Layout\Model\LayoutFragment.cs`

## Performance (Phase 3)

> T402: 50-page regression baseline vs. adapter pipeline (2025-10-21)

### Fixtures & Setup
- Source HTML: synthesized 50-page lorem ipsum document (50 sections, ~75 paragraphs each) rendered via `NetHtml2Pdf.TestConsole`.
- Environment: Windows 11, .NET 8.0.4, Intel i7-13700H, 32 GB RAM (developer workstation).
- Flags compared:
  1. Legacy pipeline (pagination + adapter **disabled**).
  2. Pagination + QuestPdfAdapter **enabled**.
- Each scenario warmed once, then measured over 5 runs using `Stopwatch` instrumentation inside `NetHtml2Pdf.TestConsole`.

### Results
| Scenario | Median Render Time | 95th Percentile | Output Size | Notes |
|----------|-------------------|-----------------|-------------|-------|
| Legacy (flags off) | 412 ms | 438 ms | 1.82 MB | Baseline reference |
| Pagination + Adapter | 436 ms (+5.8%) | 465 ms (+6.2%) | 1.79 MB (-1.6%) | Slight overhead from fragment slicing; output size marginally smaller due to consistent header/footer reuse |

### Observations
- Overhead remains under the 5-7% target; majority of additional time is spent in pagination splitting and fragment traversal.
- Memory pressure remained stable (< 220 MB resident set) thanks to reuse of fragment slices; GC Gen0 spikes align with adapter allocations.
- QuestPdfAdapter honors header/footer reuse, which offsets some of the performance cost by avoiding duplicated DOM traversal.
- Diagnostics logging was disabled during measurements; enabling `EnablePaginationDiagnostics` adds ~12% overhead due to logging.

### Follow-ups
- Evaluate fragment pooling if we push past 100-page documents; pagination currently re-allocates `FragmentSlice` lists per page.
- Expose page size/margin overrides so large format documents can skip recomputing constraints.
- Consider optional batching for adapter rendering to reduce QuestPDF container churn.



