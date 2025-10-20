# Implementation Plan: Phase 3 Pagination & Renderer Adapter

**Branch**: `005-implement` | **Date**: 2025-10-20 | **Spec**: C:\Projects\NetHtml2Pdf\specs\005-implement\spec.md  
**Input**: Feature specification from `/specs/005-implement/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command.

## Summary

- Build a pagination service that consumes Phase 2 layout fragments and produces page-scoped `PageFragmentTree` slices (`research.md`).
- Introduce an `IRendererAdapter` seam (QuestPdfAdapter first) so PdfRenderer orchestrates DOM -> layout -> pagination -> adapter rendering with QuestPDF isolated behind adapters.
- Add feature flags `EnablePagination` and `EnableQuestPdfAdapter` (default false) to preserve behavior parity while migration proceeds.

## Technical Context

**Language/Version**: .NET 8 (C#)  
**Primary Dependencies**: QuestPDF 2025.7.1, AngleSharp 1.3.0, Microsoft.Extensions.Logging.Abstractions 8.0  
**Storage**: N/A (in-memory PDF generation)  
**Testing**: xUnit; incremental TDD (single failing test rule) for pagination, carry-over, and adapter orchestration; behavior-first tests only (no reflection contract tests); renderer regression fixtures for parity  
**Target Platform**: Cross-platform (.NET 8 on Windows/Linux)  
**Project Type**: Single library plus supporting test projects  
**Performance Goals**: Maintain <=5% render time delta vs legacy on 50-page fixtures; pagination loop must terminate within O(page count) iterations  
**Constraints**: Keep PdfBuilder public API stable; add pagination under `Layout/*`; adapters live under `Renderer/*`; detailed logs only when diagnostics flag enabled  
**Scale/Scope**: Library-level change affecting all PDF exports (typical workloads 1-50 pages)  
**Additional Input**: No extra user arguments supplied (`$ARGUMENTS` empty)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Respect documented architectural layering; cross-layer access only via interfaces/adapters.
- Public surface via approved facades/entry points; no leakage of internal types.
- Prefer managed-only dependencies; native/platform deps require an ADR with rollback plan.
- Validate inputs with explicit exceptions and parameter names.
- Emit structured warnings for unsupported features; preserve content when sensible.
- Use TDD where practical; parameterized tests for multi-scenario logic.
- Keep docs/specs/contracts in sync with behavior and supported capabilities.

## Project Structure

### Documentation (this feature)

```
specs/005-implement/
|- plan.md
|- research.md
|- data-model.md
|- quickstart.md
|- contracts/
|  |- pagination-adapter-contract.md
|- tasks.md
```

### Source Code (repository root)

```
src/
|- NetHtml2Pdf/
|  |- Core/
|  |- Layout/
|  |  |- Contexts/
|  |  |- Display/
|  |  |- Engines/
|  |  |- Model/
|  |  |- Pagination/                # new pagination service, entities, diagnostics
|  |- Renderer/
|  |  |- Interfaces/
|  |  |- Pagination/                # orchestration helpers bridging layout to adapters
|  |  |- Adapters/                  # IRendererAdapter, QuestPdfAdapter, factories
|  |  |- BlockComposer.cs           # legacy path retained
|  |- PdfBuilder.cs
|- NetHtml2Pdf.Test/
|  |- Layout/
|  |  |- Pagination/
|  |- Renderer/
|  |  |- Adapters/
|- NetHtml2Pdf.TestConsole/
   |- samples/
```

**Structure Decision**: Single-library solution with supporting test projects. Phase 3 adds `Layout/Pagination` and `Renderer/Adapters` namespaces while maintaining the existing public facade (`PdfBuilder`) and legacy renderer path for fallback.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| - | - | - |

## Progress Tracking

| Phase | Description | Outputs | Status |
|-------|-------------|---------|--------|
| Phase 0 | Research & current-state analysis | `research.md` | Completed |
| Phase 1 | Data model, contracts, quickstart | `data-model.md`, `contracts/pagination-adapter-contract.md`, `quickstart.md` | Completed |
| Phase 2 | Task breakdown | `tasks.md` | Completed |
