# Implementation Plan: [FEATURE]

**Branch**: `002-layout-pipeline` | **Date**: 2025-10-16 | **Spec**: C:\\Projects\\NetHtml2Pdf\\specs\\002-layout-pipeline\\spec.md
**Input**: Feature specification from `/specs/002-layout-pipeline/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command.

## Summary

Phase 1 introduces clean seams while preserving behavior:
- Centralize display classification (DisplayClassifier)
- DRY spacing application (WrapWithSpacing)
- Extract inline flow into an internal engine (InlineFlowLayoutEngine)

Reference docs: [Architecture Overview](../../docs/architecture-layout-pipeline.md) and [Migration Plan](../../docs/migration-plan.md) capture the long-term layout pipeline design and rollout sequencing that this plan aligns with.

No new features, pagination, or layout fragments in Phase 1. Public API
(`PdfBuilder`) remains unchanged. Later phases will introduce layout model,
pagination, and a renderer adapter.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: .NET 8  
**Primary Dependencies**: QuestPDF (render backend), AngleSharp (HTML/CSS parse)  
**Storage**: N/A  
**Testing**: xUnit; unit + renderer parity tests  
**Target Platform**: Windows and Linux  
**Project Type**: Single library + tests  
**Performance Goals**: Behavior parity; no >5% regressions  
**Constraints**: Respect constitutional layering and dependency policies  
**Scale/Scope**: Library-level feature; no external services

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
specs/002-layout-pipeline/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```
src/
└─ NetHtml2Pdf/
   ├─ Core/
   │  ├─ DocumentNode.cs
   │  ├─ CssStyleMap.cs
   │  ├─ ConverterOptions.cs
   │  ├─ PdfRenderSnapshot.cs
   │  ├─ BoxSpacing.cs
   │  ├─ BorderInfo.cs
   │  ├─ Enums/
   │  └─ Constants/
   ├─ Parser/
   │  ├─ HtmlParser.cs
   │  ├─ HtmlNodeConverter.cs
   │  ├─ CssStyleResolver.cs
   │  ├─ CssDeclarationParser.cs
   │  └─ CssStyleUpdater.cs
   ├─ Renderer/
   │  ├─ PdfRenderer.cs
   │  ├─ PdfRendererFactory.cs
   │  ├─ BlockComposer.cs
   │  ├─ InlineComposer.cs
   │  ├─ ListComposer.cs
   │  ├─ TableComposer.cs
   │  ├─ BlockSpacingApplier.cs
   │  ├─ RenderingHelpers.cs
   │  ├─ InlineStyleState.cs
   │  └─ Interfaces/
   │     ├─ IBlockComposer.cs
   │     ├─ IInlineComposer.cs
   │     ├─ IBlockSpacingApplier.cs
   │     ├─ IListComposer.cs
   │     └─ ITableComposer.cs
   ├─ Fonts/
   └─ Properties/

src/
└─ NetHtml2Pdf.Test/
   ├─ Renderer/
   │  ├─ BlockComposerTests.cs
   │  ├─ InlineComposerTests.cs
   │  ├─ BlockBorderRenderingTests.cs
   │  ├─ ListComposerTests.cs
   │  └─ PdfRendererTests.cs
   ├─ Parser/
   │  ├─ HtmlParserTests.cs
   │  ├─ HtmlNodeConverterTests.cs
   │  ├─ CssDeclarationParserTests.cs
   │  ├─ CssStyleResolverTests.cs
   │  └─ CssStyleUpdaterTests.cs
   └─ Support/
      └─ *.cs

# Phase 1 (new files to add)
src/NetHtml2Pdf/Renderer/Spacing/WrapWithSpacing.cs          # helper (DRY spacing)
src/NetHtml2Pdf/Renderer/Inline/InlineFlowLayoutEngine.cs    # internal inline flow service
src/NetHtml2Pdf/Layout/Display/DisplayClassifier.cs          # internal classifier (prep for layout layer)
src/NetHtml2Pdf/Layout/Display/IDisplayClassifier.cs         # interface (internal)
```

**Structure Decision**: Single project (library + tests).
- Existing code concentrated in `src/NetHtml2Pdf/Core`, `Parser`, and `Renderer`.
- Phase 1 touches `Renderer/*` only, adding two internal helpers under
  `Renderer/Spacing` and `Renderer/Inline`.
- DisplayClassifier is placed under `src/NetHtml2Pdf/Layout/Display` to prepare
  the future layout layer while remaining `internal` and referenced only by
  composers in Phase 1.

## Complexity Tracking

*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| - | - | - |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |




