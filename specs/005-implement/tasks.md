# Tasks: Phase 3 Pagination & Renderer Adapter

**Input**: Design documents from `/specs/005-implement/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Testing Note**: Failing tests MUST exercise observable behavior (pagination results, rendered output) rather than reflection-based contract checks.

## Phase A: Shared Foundations (Setup)

- [X] T001 Add pagination feature flags to `src/NetHtml2Pdf/Core/ConverterOptions.cs`, plumb through `src/NetHtml2Pdf/Renderer/RendererOptions.cs`, and expose fluent toggles in `src/NetHtml2Pdf/PdfBuilder.cs`.
- [X] T002 [P] Scaffold pagination value objects (`PaginatedDocument`, `PageFragmentTree`, `FragmentSlice`, `CarryPageLink`, `PaginationWarning`, `PaginationWarningCode`, `PageConstraints`) under `src/NetHtml2Pdf/Layout/Pagination/` (passive DTOs, no tests).
- [X] T003 Establish pagination diagnostics options (`PaginationOptions`, logging helpers) in `src/NetHtml2Pdf/Layout/Pagination/PaginationOptions.cs` and ensure defaults surface via `RendererOptions`.
- [X] T004 Introduce adapter contracts (`IRendererAdapter`, `RendererContext`, `IRendererAdapterFactory`, placeholder `NullRendererAdapter`) in `src/NetHtml2Pdf/Renderer/Adapters/`.

## Phase B: User Story 1 – Paginated PDF export (Tests then Core)

- [X] T101 [P] Author the first failing pagination behavior test in `src/NetHtml2Pdf.Test/Layout/Pagination/PaginationServiceBehaviorTests.cs` asserting a single-page document paginates to one page (no reflection-based contracts).
- [X] T102 Implement minimal pagination pass (`PaginationService`) in `src/NetHtml2Pdf/Layout/Pagination/PaginationService.cs` to satisfy T101, expanding via incremental TDD for multi-page splits.
- [X] T103 [P] Author failing adapter seam test (`QuestPdfAdapterTests.Render_ShouldProduceQuestPdfDocumentForSinglePage`) in `src/NetHtml2Pdf.Test/Renderer/Adapters/QuestPdfAdapterTests.cs`.
- [X] T104 Implement `QuestPdfAdapter` in `src/NetHtml2Pdf/Renderer/Adapters/QuestPdfAdapter.cs` to satisfy T103, iterating with additional red/green cycles.
- [X] T105 Add failing orchestration test (`PdfRendererAdapterTests.FlagGatesPagination`) in `src/NetHtml2Pdf.Test/Renderer/PdfRendererAdapterTests.cs`, then update `src/NetHtml2Pdf/Renderer/PdfRenderer.cs` to pass while preserving legacy behavior.
- [X] T106 [P] Update docs (`README.md`, `specs/005-implement/quickstart.md`) describing feature flags and usage examples.

## Phase C: User Story 2 – Carry-over Fragment Splitting

- [X] T201 Add failing tests for cross-page fragments, carry-over metadata, and header/footer reservations in `src/NetHtml2Pdf.Test/Layout/Pagination/PaginationServiceTests.cs` (execute sequentially, one red at a time).
- [X] T202 Implement carry-over slicing and `FragmentSliceKind` handling in `src/NetHtml2Pdf/Layout/Pagination/PaginationService.cs` to satisfy T201.
- [X] T203 Introduce failing keep-with-next / keep-together tests then implement enforcement helpers (e.g., `BreakEvaluator`) in `src/NetHtml2Pdf/Layout/Pagination/`.
- [X] T204 Add failing test for keep-together overflow exception and propagate `PaginationException` through `src/NetHtml2Pdf/PdfBuilder.cs` and adapter flow.

## Phase D: User Story 3 – Renderer Adapter Extensibility

- [X] T301 [P] Add adapter factory tests in `src/NetHtml2Pdf.Test/Renderer/Adapters/RendererAdapterFactoryTests.cs` verifying QuestPdfAdapter resolution.
- [X] T302 Implement `RendererAdapterFactory` and integrate with `src/NetHtml2Pdf/Renderer/PdfRendererFactory.cs` following incremental TDD.
- [X] T303 Add spy adapter tests in `src/NetHtml2Pdf.Test/Renderer/PdfRendererAdapterTests.cs` to verify BeginDocument -> Render -> EndDocument ordering when flags enabled.
- [X] T304 Refactor composers and `PdfRenderer` to rely solely on adapters in the flagged path, keeping legacy composers for fallback.
- [X] T305 Add diagnostics coverage tests ensuring pagination/adapters emit structured logs only when diagnostics flags are enabled.

## Phase E: Polish & Cross-Cutting

- [X] T401 [P] Document pagination architecture and adapter interfaces in `docs/` (e.g., `docs/pagination-architecture.md`).
- [X] T402 Run performance regression on 50-page fixture, capture findings in `specs/005-implement/research.md` (append PERF section).
- [X] T403 Validate all flag combinations via automated tests in `src/NetHtml2Pdf.Test/Renderer/FeatureFlagCombinationTests.cs`.

## Parallel Execution Examples

```bash
# Parallel test creation (independent files)
speckit task run T101 & speckit task run T103

# Parallel documentation updates
speckit task run T106 & speckit task run T401
```





