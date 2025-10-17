# Research: Phase 2 Layout Model Migration

## Summary
Phase 2 introduces a layout-first pipeline for text blocks while leaving the renderer (QuestPDF) untouched. We measured current paragraph/heading/span rendering paths to ensure the new layout fragments reproduce identical spacing, baseline, and inline flow decisions. Diagnostics requirements drive fragment metadata (DOM node path, constraints, measured size) to support parity comparisons.

## Baseline Observations
- Legacy BlockComposer inlines paragraph handling with spacing + QuestPDF paragraph creation; inline flow already centralized via Phase 1 `InlineFlowLayoutEngine`.
- Spacing order remains margin (parent) -> border (element) -> padding (element); new layout must reuse existing spacing plan to avoid regressions.
- InlineComposer delegates to InlineFlowLayoutEngine, which already tracks baselines; fragments can consume this output with minimal change.

## Layout Model Considerations
- `LayoutBox` tree mirrors DOM order; we will classify nodes using existing `DisplayClassifier` to stay consistent.
- `LayoutConstraints` must capture inline and block extents plus pagination hints for future phases; for Phase 2 we fix block constraints to page width and capture remaining height for later pagination work.
- `LayoutFragment` requires size (width/height), baseline (for inline), and child fragments. Optional diagnostics payload includes DOM node path and constraint snapshot.

## Diagnostics & Instrumentation
- Structured logging extends the Phase 1 classifier trace: scope `LayoutEngine`, with payload `{ nodePath, context, constraints, fragmentSize }`.
- Fragment tree serialization piggybacks on structured logging as JSON payload for selected nodes (opt-in when diagnostics enabled).
- Performance sampling uses existing renderer benchmark harness; enable flag and compare render times for reference documents (<5% delta target).

## Risks
- **Parity drift**: Differences in margin collapsing or inline baseline calculations could alter output.
- **Performance overhead**: Additional layout objects and diagnostics could slow rendering.
- **Error handling**: Unexpected exceptions in formatting contexts must fail fast without silent fallback.

## Mitigations
- Build unit + contract tests around spacing, fragment sizing, and baseline expectations.
- Keep layout objects lightweight structs/classes; avoid allocations in hot paths where possible.
- Guard diagnostics behind opt-in flags; default logging to minimal level.
- Bubble exceptions to callers (per clarification) so regressions surface during testing rather than in production silently.
