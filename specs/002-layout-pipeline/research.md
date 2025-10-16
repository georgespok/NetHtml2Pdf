# Research: Layout Pipeline (Phase 1)

## Summary
Refactor for clean seams without behavior change: centralize display decisions,
DRY spacing application, and isolate inline flow logic.

## Context
- Current: DOM → composers → QuestPDF; logic scattered across composers.
- Pain: Hard to extend and test; rendering code mixes layout rules.

## Prior Art & Options
- Formatting contexts pattern (block/inline/inline-block; table later).
- Stable intermediate layout model (deferred to Phase 2+).

## Risks (Phase 1)
- Subtle behavior drift in classification or spacing.
- Inline flow extraction could alter ordering.

## Mitigations
- Replace logic incrementally with unit tests and run existing renderer tests.
- Optional global trace flag for classifier decisions.
