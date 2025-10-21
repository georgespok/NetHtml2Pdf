# Tasks: Phase 4 – Extended Formatting Context Coverage

**Input**: Design documents from /specs/006-extend-layout-pipeline/
**Prerequisites**: plan.md (required), spec.md (user stories), research.md, data-model.md, contracts/formatting-contexts.md, quickstart.md

**Tests**: All tests MUST follow incremental TDD (Red-Green-Refactor), focus on observable behaviour (rendered fragments, pagination results, diagnostics), and MUST NOT use reflection APIs (Activator.CreateInstance, Type.GetType, MethodInfo.Invoke).

**Organization**: Tasks grouped by project phase and user story. Tests precede implementation.

## Format: [ID] [P?] [Story] Description
- **[P]**: Task can run in parallel (different files, no shared dependencies)
- **[Story]**: User story label (US1, US2, US3) or GLOBAL for cross-cutting work
- Include exact file paths for implementation

---

## Phase 1: Setup (Shared Infrastructure)

- [X] T001 [GLOBAL] Verify baseline build/tests (dotnet build & dotnet test) to ensure a clean starting point. Command: dotnet build NetHtml2Pdf.sln && dotnet test NetHtml2Pdf.sln
- [X] T002 [GLOBAL] Document new feature flag plan in docs/roadmap.md (create section referencing Phase 4 flags) so stakeholders understand rollout order.

---

## Phase 2: Foundational (Blocking Prerequisites)

- [X] T003 [GLOBAL] Add new feature flags (EnableInlineBlockContext, EnableTableContext, EnableTableBorderCollapse, EnableFlexContext) to src/NetHtml2Pdf/Renderer/RendererOptions.cs and expose fluent toggles in src/NetHtml2Pdf/PdfBuilder.cs.
- [X] T004 [GLOBAL] Update src/NetHtml2Pdf/Renderer/PdfRenderer.cs to propagate formatting-context flags into the layout pipeline (constructor parameters, renderer context creation).
- [X] T005 [GLOBAL] Introduce FormattingContextOptions helper (src/NetHtml2Pdf/Layout/FormattingContexts/FormattingContextOptions.cs) and extend the formatting-context factory (src/NetHtml2Pdf/Layout/FormattingContexts/FormattingContextFactory.cs) to read flag state (no new contexts yet).

---

## Phase 3: Contracts & Shared Tests

- [X] T006 [P] [GLOBAL] Author contract tests covering inline-block, table, and flex behaviours per contracts/formatting-contexts.md in src/NetHtml2Pdf.Test/Layout/FormattingContexts/FormattingContextsContractTests.cs (tests must fail initially).

---

## Phase 4: User Story 1 - Inline-Block Layout Support (Priority: P1)

### Tests

 - [X] T007 [P] [US1] Add failing unit tests validating inline-block fragment sizing/baseline, diagnostics, and overflow handling (wrapping/pagination without duplicate fragments) in `src/NetHtml2Pdf.Test/Layout/FormattingContexts/InlineBlockFormattingContextTests.cs`
- [X] T008 [US1] Extend src/NetHtml2Pdf.Test/Renderer/PdfRendererAdapterTests.cs with a failing scenario proving inline-block fragments route through the adapter when the flag is enabled (and legacy path when disabled).

### Implementation

- [X] T009 [US1] Implement `InlineBlockFormattingContext` in `src/NetHtml2Pdf/Layout/FormattingContexts/InlineBlockFormattingContext.cs`, covering sizing/baseline logic and overflow resolution (wrap or paginate without duplicating fragments) so the new tests pass.
- [X] T010 [US1] Wire inline-block detection into FormattingContextFactory / layout engine (src/NetHtml2Pdf/Layout/FormattingContexts/FormattingContextFactory.cs, src/NetHtml2Pdf/Layout/Engines/LayoutEngine.cs) respecting flag state.
- [X] T011 [US1] Emit structured diagnostics (FormattingContext.InlineBlock events) via src/NetHtml2Pdf/Layout/Diagnostics/FormattingContextDiagnostics.cs ensuring logs appear only when diagnostics flag is on.

---

## Phase 5: User Story 2 - Table Formatting Context (Priority: P2)

### Tests

- [X] T012 [P] [US2] Add failing multi-page table tests verifying header repetition, column width stability, caption anchoring, and border downgrade warnings in src/NetHtml2Pdf.Test/Layout/FormattingContexts/TableFormattingContextTests.cs.
- [X] T013 [US2] Create pagination contract test asserting header/footer and caption behaviour in src/NetHtml2Pdf.Test/Layout/FormattingContexts/TablePaginationContractTests.cs (tests first, must fail).

### Implementation

- [X] T014 [US2] Implement TableFormattingContext with section/row fragments in src/NetHtml2Pdf/Layout/FormattingContexts/TableFormattingContext.cs.
- [X] T015 [US2] Update pagination to repeat headers/footers and manage carry-over rows (src/NetHtml2Pdf/Layout/Pagination/PaginationService.cs, associated DTOs).
- [X] T016 [US2] Implement downgrade diagnostics for unsupported border collapse in src/NetHtml2Pdf/Layout/Diagnostics/TableDiagnostics.cs and ensure tests capture warning payloads.

---

## Phase 6: User Story 3 - Flex Layout Preview (Priority: P3)

### Tests

- [X] T017 [P] [US3] Add failing flex container tests covering direction/justification and downgrade warnings (flex-wrap: wrap, flex-basis:auto) in src/NetHtml2Pdf.Test/Layout/FormattingContexts/FlexFormattingContextTests.cs

### Implementation

- [X] T018 [US3] Implement preview FlexFormattingContext in src/NetHtml2Pdf/Layout/FormattingContexts/FlexFormattingContext.cs, enforcing `nowrap` handling and emitting downgrade diagnostics.
- [X] T019 [US3] Integrate flex selection into layout engine (src/NetHtml2Pdf/Layout/FormattingContexts/FormattingContextFactory.cs) and ensure diagnostics flow to RendererContext.Logger.

---

## Phase 7: Integration & Regression

- [X] T020 [GLOBAL] Update feature flag combination tests in src/NetHtml2Pdf.Test/Renderer/FeatureFlagCombinationTests.cs to cover new flags and ensure invalid combos throw (tests first).
- [X] T021 [GLOBAL] Add regression fixtures comparing legacy vs flagged pipeline output for representative documents in src/NetHtml2Pdf.Test/Renderer/Regression/FormattingContextRegressionTests.cs, and assert layout deviation metrics stay below the <2% threshold defined in the spec (e.g., geometry assertions or screenshot diffs).
- [X] T022 [GLOBAL] Update Quickstart (specs/006-extend-layout-pipeline/quickstart.md) and README section to reflect final flag usage and diagnostics commands.
- [X] T026 [GLOBAL] Instrument and analyse flex flag telemetry to verify an ≥80% reduction in legacy composer usage for flagged flex containers, capturing data via the existing telemetry pipeline (or documenting the required script) and publishing results alongside tests.

---

## Phase 8: Polish & Cross-Cutting

- [X] T023 [P] [GLOBAL] Run 50-page performance benchmark script (scripts/benchmarks/RenderBench.ps1) capturing before/after metrics; document results in specs/006-extend-layout-pipeline/research.md.
- [ ] T024 [P] [GLOBAL] Conduct code cleanup (formatting via dotnet format) and ensure no reflection APIs exist in new test files.
- [ ] T025 [GLOBAL] Final end-to-end validation: run dotnet test NetHtml2Pdf.sln with flags on/off and archive logs/artefacts under specs/006-extend-layout-pipeline/artifacts/.

---

## Dependencies & Execution Order

1. **Setup** (T001-T002) must complete before foundational work.
2. **Foundational** (T003-T005) blocks all stories; no story tasks should start until these are done.
3. For each user story (US1, US2, US3):
   - Write tests first (e.g., T007/T008 before T009-T011).
   - Implementation tasks rely on tests and foundational wiring.
4. Integration/regression (T020-T022, T026) depends on all story implementations.
5. Polish tasks (T023-T025) run after integration succeeds.

### Parallel Opportunities
- [P] tasks can execute simultaneously once prerequisites satisified. Examples:
  - After foundational phase, run tests in parallel:
    `bash
    speckit task run T006 &
    speckit task run T007 &
    speckit task run T012
    `
  - During polish, execute:
    `bash
    speckit task run T023 &
    speckit task run T024
    `

---

## Implementation Strategy

1. Complete Setup and Foundational phases sequentially.
2. Deliver US1 (inline-block) end-to-end (tests → implementation → diagnostics).
3. Deliver US2 (table) and US3 (flex) similarly, respecting test-first workflow.
4. Execute integration/regression tasks to ensure pipeline parity, documentation alignment, telemetry goals, and logging quality.
5. Finish with performance validation and documentation polish.

## Notes

- Ensure every new test fails before implementing corresponding functionality.
- Use behaviour assertions (fragment properties, pagination results, log payloads) rather than reflection/internal inspection.
- Maintain feature flag defaults (false) until parity validated.
- Update tasks.md with actual completion status as work progresses.
