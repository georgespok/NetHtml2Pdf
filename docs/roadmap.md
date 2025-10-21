# Product Roadmap

This roadmap highlights the upcoming work on the NetHtml2Pdf layout pipeline.

## Phase 4 – Extended Formatting Context Coverage (2025 Q4)

Feature flags remain the primary rollout control. Each flag stays **default-off** until the listed gate criteria are met.

| Flag | Default | Gate Criteria | Rollout Activities | Observability |
|------|---------|---------------|--------------------|---------------|
| `EnableInlineBlockContext` | Off | Inline-block geometry tests pass; adapter parity verified (T007–T011) | Enable in internal staging, gather feedback from inline-heavy documents | Regression fixtures (T021), diagnostics logs (`FormattingContext.InlineBlock`) |
| `EnableTableContext` | Off | Table pagination passes headers/captions tests; border downgrade diagnostics validated (T012–T016) | Roll out to selected partner templates; monitor pagination feedback | Multi-page table suite, `TableContext.HeaderRepeated` diagnostics |
| `EnableTableBorderCollapse` | Off | Advanced border-collapse proof-of-concept completed in follow-up phase | Document limitations; provide opt-in guidance | Structured downgrade logs, contract tests |
| `EnableFlexContext` | Off (Preview) | Flex preview tests and telemetry show ≥80% reduction in legacy usage (T017–T019, T026) | Preview rollout to pilot teams; capture telemetry before wider enablement | Telemetry script (T026), `FlexContext.Downgrade` warnings |

### Rollout Steps
1. Finish Phase 1–3 setup tasks to ensure flag plumbing is in place (T001–T006).
2. Deliver story-specific work (T007–T019) and verify regression coverage (T020–T022).
3. Capture flex telemetry (T026) and performance benchmarks (T023) before promoting flags.
4. Update documentation (T022) once enablement decisions are made.
5. Flip defaults only after parity and telemetry gates are satisfied, following constitution workflow.

### Communication Plan
- Weekly status updates in the pipeline channel summarise flag readiness.
- Publish enablement guidance in `docs/quickstart.md` (maintained via T022).
- Capture decisions and dates in release notes when defaults change.

