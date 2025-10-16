---
description: "Executable tasks for Phase 1 refactor: DisplayClassifier, WrapWithSpacing, InlineFlowLayoutEngine (behavior parity)"
---

# Tasks: Layout Pipeline Architecture and Migration (Phase 1)

**Input**: C:\Projects\NetHtml2Pdf\specs\002-layout-pipeline\spec.md  
**Plan**: C:\Projects\NetHtml2Pdf\specs\002-layout-pipeline\plan.md  
**Prerequisites**: research.md, data-model.md, contracts\phase1-seams.md

## Format: `[ID] [P?] [Story] Description`
- `[P]` means can run in parallel (different files, no dependencies)
- `[Story]` is user story label from the spec (US1 = Phase 1 seams)

## Phase 1: Extract Seams (Refactor Only) — TDD Order

### Setup (Baseline)
- [X] T001 [P] [US1] Run baseline tests to capture current behavior parity  
  Command: `dotnet test C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\NetHtml2Pdf.Test.csproj`

### Contract Tests (from contracts/phase1-seams.md)
- [X] T002 [P] [US1] Add contract tests for DisplayClassifier policies  
  File: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\Renderer\DisplayClassifierContractTests.cs`  
  Notes: Explicit CSS wins; ListItem=Block; table sub-elements via Table context; warn once for unknown types; warn once per unsupported display value.
- [X] T003 [P] [US1] Add contract tests for WrapWithSpacing order and cumulative behavior  
  File: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\Renderer\WrapWithSpacingContractTests.cs`  
  Notes: Assert margin(parent) → border(element) → padding(element); nested containers cumulative.
- [X] T004 [P] [US1] Add contract tests for InlineFlowLayoutEngine traversal/inheritance  
  File: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\Renderer\InlineFlowEngineContractTests.cs`  
  Notes: text/span/strong/bold/italic/br/inline-block; style inheritance; line breaks.

### Unit Tests (write before implementation)
- [X] T005 [P] [US1] Create unit tests for DisplayClassifier classifications  
  File: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\Renderer\DisplayClassifierTests.cs`
- [X] T006 [P] [US1] Create unit tests for WrapWithSpacing ordering  
  File: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\Renderer\WrapWithSpacingTests.cs`
- [X] T007 [P] [US1] Create unit tests for InlineFlowLayoutEngine behavior  
  File: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\Renderer\InlineFlowLayoutEngineTests.cs`

### Implementation (entities/services)
- [X] T008 [P] [US1] Implement DisplayClassifier (internal interface + impl)  
  Files:
  - `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Layout\Display\IDisplayClassifier.cs`
  - `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Layout\Display\DisplayClassifier.cs`  
  Notes: Apply clarifications FR-006..FR-008; uses ILogger for structured logging; add optional global trace flag.
- [X] T009 [P] [US1] Implement WrapWithSpacing helper (DRY spacing)  
  File: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Renderer\Spacing\WrapWithSpacing.cs`  
  Notes: Delegate to existing BlockSpacingApplier; order = margin→border→padding; preserve current visuals.
- [X] T010 [P] [US1] Implement InlineFlowLayoutEngine (internal)  
  File: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Renderer\Inline\InlineFlowLayoutEngine.cs`  
  Notes: Move traversal/validation from InlineComposer; no behavior change.

### Wiring (sequential - shared files)
- [X] T011 [US1] Wire DisplayClassifier into BlockComposer  
  File: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Renderer\BlockComposer.cs`  
  Notes: Replace ad hoc display checks; map: None→skip, Block→block path, Inline→inline path, InlineBlock→current simplified path.
- [X] T012 [US1] Wire DisplayClassifier into InlineComposer  
  File: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Renderer\InlineComposer.cs`
- [X] T013 [US1] Replace spacing sequences with WrapWithSpacing in BlockComposer  
  File: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Renderer\BlockComposer.cs`
- [X] T014 [US1] Delegate InlineComposer to InlineFlowLayoutEngine  
  File: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Renderer\InlineComposer.cs`

### Integration (user story parity)
- [X] T015 [P] [US1] Parity integration test for typical HTML samples (no output diffs)  
  File: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\Renderer\Phase1ParityTests.cs`  
  Notes: Use existing samples under `src\NetHtml2Pdf.TestConsole\samples\*.html` where feasible; assert text extraction parity.

### Polish & Cross-Cutting
- [X] T016 [P] Update docs references (link to docs\architecture-layout-pipeline.md and docs\migration-plan.md)  
  Files: `C:\Projects\NetHtml2Pdf\specs\002-layout-pipeline\plan.md`, docs files if needed
- [X] T017 [P] Ensure CI runs on Windows/Linux; run full suite  
  Command: `dotnet test C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\NetHtml2Pdf.Test.csproj`
- [X] T018 [P] Add global flag for classifier trace logging (default off) and a smoke test enabling it  
  Files: `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Renderer\RendererOptions.cs`, `C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\Renderer\DisplayClassifierTraceLoggingTests.cs`

## Dependencies & Execution Order
- Setup (T001) first to confirm baseline.
- Contracts/Unit tests (T002–T007) BEFORE implementation (T008–T010) [TDD].
- Wiring (T011–T014) AFTER implementations complete.
- Integration parity (T015) AFTER wiring.
- Polish (T016–T018) last.

## Parallel Opportunities
- [P] tests can be authored in parallel when touching different files:
  - T002, T003, T004 (contracts) can run together.
  - T005, T006, T007 (unit tests) can run together.
- [P] implementations on separate files can proceed in parallel:
  - T008, T009, T010 can run together.
- Wiring (T011–T014) must be sequential due to shared files.

### Parallel Example: Run contract tests together
```
# In separate terminals or job steps
# Contract tests
code C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\Renderer\DisplayClassifierContractTests.cs
code C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\Renderer\WrapWithSpacingContractTests.cs
code C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\Renderer\InlineFlowEngineContractTests.cs

# Execute
dotnet test C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf.Test\NetHtml2Pdf.Test.csproj -v minimal
```

### Parallel Example: Implement entities concurrently
```
# DisplayClassifier
code C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Layout\Display\DisplayClassifier.cs
# WrapWithSpacing
code C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Renderer\Spacing\WrapWithSpacing.cs
# InlineFlowLayoutEngine
code C:\Projects\NetHtml2Pdf\src\NetHtml2Pdf\Renderer\Inline\InlineFlowLayoutEngine.cs
```

## Notes
- All new types are internal; no public API changes.
- Behavior parity is mandatory; rely on existing tests + new contract/unit tests to validate.
- Follow coding and testing standards in `docs/coding-standards.md` and `docs/testing-guidelines.md`.
