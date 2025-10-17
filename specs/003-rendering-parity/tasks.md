---
description: "Executable tasks for Phase 2 layout model rollout (text blocks behind feature flag)"
---

# Tasks: Layout Model, Rendering Parity Intact (Phase 2)

**Input**: C:\Projects\NetHtml2Pdf\specs\003-rendering-parity\spec.md  
**Plan**: C:\Projects\NetHtml2Pdf\specs\003-rendering-parity\plan.md  
**Prerequisites**: research.md, data-model.md, contracts\phase2-layout-model.md, quickstart.md

## Format: `[ID] [P?] [Story] Description`
- `[P]` indicates tasks that may run in parallel (touch distinct files).
- `[Story]` corresponds to user stories in the spec (US1 toggle pipeline, US2 diagnostics, US3 legacy fallback).

## Setup
- [X] T201 [P] [US1] Run baseline renderer tests with flag off to capture current parity  
  Command: `dotnet test C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\NetHtml2Pdf.Test.csproj`

## Tests (author before implementation)
- [X] T202 [P] [US1] Create unit tests for layout primitives (`LayoutBox`, `LayoutConstraints`, `LayoutFragment`)  
  Files: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\Layout\LayoutModelTests.cs`
- [X] T203 [P] [US1] Add block formatting context tests covering spacing, ordering, baseline propagation  
  Files: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\Layout\BlockFormattingContextTests.cs`
- [X] T204 [P] [US1] Add inline formatting context tests (wrapping InlineFlowLayoutEngine, DOM path diagnostics)  
  Files: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\Layout\InlineFormattingContextTests.cs`
- [X] T205 [P] [US2] Add layout engine + diagnostics contract tests (flag on/off, JSON payload presence)  
  Files: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\Layout\LayoutEngineContractTests.cs`
- [X] T206 [P] [US3] Extend fallback coverage to assert tables/lists/inline-blocks stay on legacy path when flag on  
  Files: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\Renderer\LegacyFallbackTests.cs`

## Core Implementation (parallel where files differ)
- [X] T207 [P] [US1] Implement layout model types (`LayoutBox`, `LayoutConstraints`, `LayoutFragment`) with diagnostics fields  
  Files: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Layout\Model\*.cs`
- [X] T208 [P] [US1] Implement `BlockFormattingContext` applying spacing plan and producing fragments  
  Files: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Layout\Contexts\BlockFormattingContext.cs`
- [X] T209 [P] [US1] Implement `InlineFormattingContext` leveraging `InlineFlowLayoutEngine`  
  Files: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Layout\Contexts\InlineFormattingContext.cs`
- [X] T210 [P] [US1][US2] Implement `ILayoutEngine` facade and concrete `LayoutEngine` orchestrator  
  Files: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Layout\Engines\LayoutEngine.cs`, `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Layout\Engines\ILayoutEngine.cs`

## Integration (sequential – shared files)
- [X] T211 [US1] Add `EnableNewLayoutForTextBlocks` flag to `ConverterOptions` and propagate through `PdfBuilder`/`PdfRendererFactory`  
  Files: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Core\ConverterOptions.cs`, `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Renderer\PdfRendererFactory.cs`
- [X] T212 [US1] Wire `BlockComposer` to call layout engine when flag enabled for paragraphs/headings/spans  
  Files: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Renderer\BlockComposer.cs`
- [X] T213 [US1] Ensure InlineComposer/BlockComposer share inline context results (no duplicate work)  
  Files: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Renderer\InlineComposer.cs`

## Integration Tests & Diagnostics
- [X] T214 [P] [US1] Add fragment-to-QuestPDF integration test verifying identical output for sample documents  
  Files: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\Renderer\BlockComposerFragmentParityTests.cs`
- [X] T215 [P] [US2] Add diagnostics logging tests for `LayoutEngine.FragmentMeasured` event and fragment JSON payload  
  Files: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\Layout\LayoutDiagnosticsTests.cs`
- [X] T216 [P] [US2] Ensure diagnostics disabled path produces no additional logs/perf overhead (benchmark smoke)  
  Files: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\Layout\LayoutDiagnosticsTests.cs`

## Polish & Cross-Cutting
- [X] T217 [P] [US3] Update docs (`docs\architecture-layout-pipeline.md`, `docs\migration-plan.md`) with new layout flag notes  
  Files: `C:\Projects\NetHtml2Pdf\docs\architecture-layout-pipeline.md`, `C:\Projects\NetHtml2Pdf\docs\migration-plan.md`
- [X] T218 [P] [US1][US2] Run full test suite with flag on/off and capture performance delta (<5%)  
  Command: `dotnet test C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\NetHtml2Pdf.Test.csproj -l "console;verbosity=minimal"`
- [X] T219 [P] [US1] Prepare rollout notes + flag guidance (update README or release notes draft)  
  Files: `C:\Projects\NetHtml2Pdf\docs\migration-plan.md`, `C:\Projects\NetHtml2Pdf\README.md`

## Dependencies & Execution Order
- Complete Setup (T201) before writing new tests to ensure clean baseline.
- Tests (T202–T206) precede corresponding implementation tasks (T207–T210) per TDD.
- Integration tasks (T211–T213) run sequentially due to shared files.
- Diagnostics and parity validation (T214–T216) follow integration.
- Polish tasks (T217–T219) complete documentation and performance verification.

## Parallel Opportunities
- Tests in different files (T202–T205) can be authored concurrently.
- Layout model and contexts (T207–T210) may progress in parallel by separate owners.
- Diagnostics/performance tasks (T214–T216) can run concurrently once integration is stable.

## Notes
- Leave tables/lists/inline-blocks/floats routed through legacy composers regardless of flag (per FR-011).
- Exceptions from the new layout pipeline must bubble to callers (FR-012) – ensure tests cover this behavior.
- Diagnostics logging should remain opt-in; verify logging level respects existing renderer configuration.

