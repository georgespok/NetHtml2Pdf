# Implementation Plan: Phase 2 - Layout Model, Rendering Parity Intact

**Branch**: `003-rendering-parity` | **Date**: 2025-10-17 | **Spec**: C:\Projects\NetHtml2Pdf\specs\003-rendering-parity\spec.md
**Input**: Feature specification from `/specs/003-rendering-parity/spec.md`

## Summary

Phase 2 adds a minimal layout layer for paragraphs, headings, and span-level inline text while keeping QuestPDF as the renderer. The new flow builds `LayoutBox` trees, measures them through block and inline formatting contexts, and feeds the resulting fragments back into the existing composers under the `EnableNewLayoutForTextBlocks` flag (default off). Tables, lists, inline-blocks, and floats remain on legacy composers for parity. Diagnostics (structured logs + optional fragment dumps) allow us to compare new and old pipelines before wider rollout.

## Technical Context

**Language/Version**: .NET 8  
**Primary Dependencies**: QuestPDF (render backend), AngleSharp (HTML/CSS parse)  
**Storage**: N/A  
**Testing**: xUnit; unit, contract, and renderer parity tests  
**Target Platform**: Windows and Linux  
**Project Type**: Single library + tests  
**Performance Goals**: Preserve current rendering time within ±5% for text-only documents  
**Constraints**: Maintain constitutional layering; keep new layout types internal; preserve legacy outputs when flag is off  
**Scale/Scope**: Library-level refactor; no external services  
**Additional Input**: (no supplementary implementation arguments provided)

## Constitution Check

- Respect layered architecture: new layout layer stays internal and is accessed via `ILayoutEngine`.
- Public API remains stable; `ConverterOptions` flag is the only surfaced toggle.
- No new native dependencies; continue using QuestPDF and AngleSharp.
- Input validation remains explicit (flag defaults to false, explicit exceptions for unsupported contexts where applicable).
- Structured diagnostics extended with fragment JSON logging when opt-in tracing is enabled.
- Testing approach stays TDD-first for new contexts and parity verification.
- Documentation (spec, plan, research, contracts, quickstart, tasks) will be updated as artifacts are produced.

Gate status: ✅ All constitutional gates satisfied for planning; re-validate after Phase 1 deliverables.

## Project Structure

### Documentation (this feature)

```
specs/003-rendering-parity/
��� plan.md              # This file (implementation plan)
��� research.md          # Phase 0 output
��� data-model.md        # Phase 1 output
��� quickstart.md        # Phase 1 output
��� contracts/           # Phase 1 output (phase2-layout-model.md)
��� tasks.md             # Phase 2 output (execution tracker)
```

### Source Code (repository root)

```
src/
�� NetHtml2Pdf/
   �� Layout/
   �  �� Model/
   �  �  �� LayoutBox.cs
   �  �  �� LayoutConstraints.cs
   �  �  �� LayoutFragment.cs
   �  �� Contexts/
   �     �� BlockFormattingContext.cs
   �     �� InlineFormattingContext.cs
   �  �� Engines/
   �     �� LayoutEngine.cs (implements ILayoutEngine)
   �  �� Interfaces/
   �     �� ILayoutEngine.cs
   �� Renderer/
   �  �� BlockComposer.cs            # updated to consume fragments under flag
   �  �� InlineComposer.cs           # minor wiring for inline context delegation
   �  �� PdfRenderer.cs              # ensures flag propagation
   �� Core/
      �� ConverterOptions.cs         # adds EnableNewLayoutForTextBlocks

src/
�� NetHtml2Pdf.Test/
   �� Layout/
   �  �� BlockFormattingContextTests.cs
   �  �� InlineFormattingContextTests.cs
   �  �� LayoutEngineIntegrationTests.cs
   �� Renderer/
      �� BlockComposerFragmentParityTests.cs

docs/
�� architecture-layout-pipeline.md   # reference only (update if needed)
�� migration-plan.md                 # reference only
```

**Structure Decision**: Single library + tests. New layout-layer types live under `src/NetHtml2Pdf/Layout` with subfolders for `Model`, `Contexts`, and `Engines`. Tests mirror layout namespace under `src/NetHtml2Pdf.Test/Layout`. Existing renderer files are updated in place with feature-flagged wiring.

## Execution Plan

### Phase 0 – Research & Alignment
- **Goals**: Confirm guardrails, catalog existing composer behavior for paragraphs/headings/spans, and finalize diagnostics strategy.
- **Key Activities**:
  - Review current Block/Inline composer behavior with flag off to document baseline spacing and inline decisions.
  - Inventory logging hooks and define fragment JSON schema aligned with diagnostics requirements.
  - Capture rollout risks (parity regressions, performance, diagnostics noise) and mitigation strategies.
- **Outputs**: `research.md` summarizing findings, risks, and mitigation plan.
- **Gate**: Reaffirm constitutional compliance before moving to design.

### Phase 1 – Layout Model Design & Contracts
- **Goals**: Formalize layout primitives, formatting contexts, and contract/spec coverage before implementation.
- **Key Activities**:
  - Define `LayoutBox`, `LayoutConstraints`, `LayoutFragment` attributes, relationships, and lifecycle.
  - Draft contracts covering Block/Inline formatting contexts, ILayoutEngine orchestration, diagnostics payloads, and feature-flag wiring.
  - Expand quickstart to guide enabling the feature flag and interpreting diagnostics.
- **Outputs**: `data-model.md`, `contracts/phase2-layout-model.md`, `quickstart.md`.
- **Gate**: Contracts + data model reviewed against spec requirements and constitution before coding begins.

### Phase 2 – Implementation & Test Execution
- **Goals**: Build the new layout layer, integrate with BlockComposer, and achieve parity behind the feature flag.
- **Key Activities**:
  - Implement layout model types and contexts with unit tests (TDD) per contracts.
  - Implement `ILayoutEngine` facade, register it with DI/factory, and add feature flag to `ConverterOptions`.
  - Update BlockComposer (and InlineComposer as needed) to call layout engine when flag enabled for supported nodes.
  - Emit structured fragment diagnostics, including DOM node paths, respecting opt-in tracing.
  - Author parity and performance guard tests to validate success criteria.
- **Outputs**: Code changes in `src/` and tests in `src/NetHtml2Pdf.Test/` per structure above; `tasks.md` capturing the full execution backlog with progress markers.
- **Gate**: All tasks in Phase 2 marked complete with green test suite and parity verification.

### Phase 3 – Validation & Rollout Readiness
- **Goals**: Confirm readiness for broader rollout and document follow-up work (pagination, additional contexts).
- **Key Activities**:
  - Review diagnostics output for traceability and ensure no excessive logging when disabled.
  - Validate performance target (≤5% regression) using representative documents.
  - Compile rollout notes and flag configuration guidance for documentation/README updates.
- **Outputs**: Updates to research/plan with observations, readiness checklist in `tasks.md` polish section.
- **Gate**: Success criteria from spec satisfied; outstanding items documented for Phase 3 migration.

## Progress Tracking

| Phase | Status | Notes |
|-------|--------|-------|
| Phase 0 – Research | Not Started | Pending constitution re-check after artifact drafted |
| Phase 1 – Design & Contracts | Not Started | Awaiting Phase 0 completion |
| Phase 2 – Implementation | Not Started | Blocked on contracts approval |
| Phase 3 – Validation | Not Started | Runs after implementation parity achieved |

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| - | - | - |
