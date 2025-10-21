# Research Notes: Phase 4 Benchmarks

## Benchmark Plan (T023)

- Goal: Validate performance delta between legacy vs flagged layout within <=5% target and capture word-count parity.
- Script: `scripts/benchmarks/RenderBench.ps1`
  - Builds the solution
  - Renders representative documents with flags off/on (placeholder in current version)
  - Produces a JSON report under `scripts/benchmarks/artifacts/bench_<timestamp>.json`

## How to Run

```powershell
pwsh scripts/benchmarks/RenderBench.ps1 -Configuration Debug
```

## Next Steps

- Implement a minimal runner to call the library directly and capture timings and output word counts (reuse PdfWord parsing utilities from tests).
- Store before/after snapshots and compare over time.
- Add CI hook to publish benchmark artifacts for PRs.

# Research: Phase 4 - Extended Formatting Context Coverage

## Objectives
- Extend the Phase 3 pagination + adapter pipeline with additional formatting contexts while keeping behaviour flag-gated.
- Preserve existing PdfRenderer parity (flags off) and enable safe experimentation (flags on).
- Document constraints (no flex-wrap support, table border behaviour) and performance expectations (+/-5% render-time delta).

## Current State Snapshot
- Phase 3 delivered pagination, diagnostics, and QuestPdfAdapter integration behind feature flags.
- Layout engine currently supports block/inline fragments; inline-block, tables, and flex fallback to legacy composers.
- RendererOptions already surfaces boolean flags: EnablePagination, EnableQuestPdfAdapter, plus diagnostics toggles.
- Tests: NetHtml2Pdf.Test covers pagination, renderer adapter, and diagnostics; no coverage yet for inline-block/table/flex contexts.

## Dependencies & Interfaces
- Core assembly: src/NetHtml2Pdf/Layout/* (new contexts will live beside pagination).
- Renderer pipeline: PdfRenderer, RendererOptions, PaginationService, IRendererAdapter.
- Diagnostics: PaginationDiagnostics, logging abstractions, structured warnings.
- Feature flags must flow from PdfBuilder -> RendererOptions -> layout/renderer layers.

## Clarifications Captured
- Flex preview treats `flex-wrap: wrap` as unsupported (`nowrap` behaviour) and should log downgrade diagnostics.
- Table context treats `border-collapse: collapse` as separate unless an advanced flag enables true collapse.

## Constraints & Assumptions
- No new native dependencies; managed-only requirement enforced.
- Incremental TDD required; tests must exercise observable behaviour (rendered fragments, pagination outputs, logs) without reflection APIs.
- Feature flags default *off* to maintain parity until feature parity validated.
- Performance benchmarks must include the 50-page lorem fixture used in previous phases.

## Risks & Open Questions
- **Risk**: Table pagination misalignment with headers/footers. *Mitigation*: design acceptance tests with multi-page fixtures and captions.
- **Risk**: Flex preview misinterpreted as production-ready. *Mitigation*: Quickstart and logging emphasise limitations.
- **Risk**: Performance degradation >5%. *Mitigation*: Profile new contexts and optimise hotspots (e.g., fragment cloning).
- **Open Question**: Need confirmation on additional CSS properties to support in inline-block context (e.g., vertical-align). Capture during design review.

## References
- Phase 3 documentation (pagination & adapter) for layering patterns.
- .specify/memory/constitution.md for governance, testing, and dependency policies.
- Existing unit tests: PaginationServiceTests, PdfRendererAdapterTests for extending coverage pattern.


