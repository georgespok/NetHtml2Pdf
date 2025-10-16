# Implementation Plan: CSS Display Support (Block, Inline-Block, None)

**Branch**: `001-css-display-support` | **Date**: 2025-10-15 | **Spec**: C:\Projects\NetHtml2Pdf\specs\001-css-display-support\spec.md
**Input**: Feature specification from `/specs/001-css-display-support/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command.

## Summary

Add support for CSS `display` values: `block`, `inline-block`, and `none`.
Parse and map `display` in Parser; represent it in Core style models; apply
layout semantics in Renderer (block breaking, inline flow as atomic boxes, omit
for none). Emit structured warnings for unsupported values and update
contracts/docs/fixtures.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: .NET 8  
**Primary Dependencies**: QuestPDF, AngleSharp  
**Storage**: N/A  
**Testing**: xUnit; see `docs/testing-guidelines.md`  
**Target Platform**: Windows and Linux  
**Project Type**: Single library + tests  
**Performance Goals**: Rendering overhead ≤ 5% vs baseline  
**Constraints**: Managed-only deps; strict layer boundaries; facade-only public API  
**Scale/Scope**: Feature-scoped library changes; no external integrations

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- Layered boundaries respected: Core ↔ Parser ↔ Renderer ↔ Facade (`PdfBuilder`).
- Public surface via `PdfBuilder` only; no leakage of internal types.
- Managed‑only deps: QuestPDF, AngleSharp; no GDI+/native libs.
- Input validation present with specific exceptions and parameter names.
- Structured warnings for unsupported tags/CSS; inner text preserved when sensible.
- Tests written first (fail → implement → refactor) with `[Theory]`/`[InlineData]`.
- Docs/contracts updated: supported tags/CSS and fixtures included.

## Project Structure

### Documentation (this feature)

```
specs/001-css-display-support/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── contracts/           # Phase 1 output
    └── css-display.md
```

### Source Code (repository root)

```
src/NetHtml2Pdf/
├── Core/
│   ├── Enums/
│   │   └── CssDisplay.cs              # new enum: Block, InlineBlock, None, Default
│   └── CssStyleMap.cs                 # add Display property and WithDisplay()
├── Parser/
│   ├── CssDeclarationParser.cs        # parse "display" values
│   └── CssStyleUpdater.cs             # apply precedence and map to CssDisplay
└── Renderer/
    ├── InlineComposer.cs              # inline-block atomic box behavior
    ├── BlockComposer.cs               # block breaking and width rules
    └── PdfRenderer.cs                 # skip nodes with display:none (and descendants)

tests/NetHtml2Pdf.Test/
├── Parser/CssDeclarationParserTests.cs
├── Parser/CssStyleUpdaterTests.cs
├── Renderer/BlockComposerTests.cs
├── Renderer/InlineComposerTests.cs
└── Renderer/PdfRendererTests.cs
```

**Structure Decision**: Single library with layered folders; extend existing files and add `CssDisplay` enum. Tests added under existing suites. Follow `docs/coding-standards.md` and `docs/testing-guidelines.md`. 

## Complexity Tracking

*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |

