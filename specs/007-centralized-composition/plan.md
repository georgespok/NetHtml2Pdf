# Implementation Plan: Centralized Composition and Transient Services

**Branch**: `007-centralized-composition` | **Date**: 2025-10-24 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/007-centralized-composition/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command.

## Summary

Refactor the NetHtml2Pdf rendering pipeline to centralize object construction in a single composition root (`RendererComposition`), making all classes pure with explicit dependencies and enabling test-time service overrides without DI frameworks, while preserving all existing public APIs.

## Technical Context

**Language/Version**: C# .NET 8.0  
**Primary Dependencies**: AngleSharp (HTML parsing), QuestPDF (PDF generation), Microsoft.Extensions.Logging  
**Storage**: N/A (in-memory processing)  
**Testing**: xUnit, NSubstitute (mocking), .NET test framework  
**Target Platform**: .NET 8.0+ (cross-platform)  
**Project Type**: Single library project with test projects  
**Performance Goals**: Not a priority at this stage; focus on design clarity and maintainability  
**Constraints**: Preserve public API compatibility; maintain existing test coverage; no DI container introduction; no breaking changes to existing constructors  
**Scale/Scope**: Refactor existing ~15 core classes; add 2-3 new internal composition classes; preserve all public constructors

## Constitution Check (Post-Design)

*Re-evaluated after Phase 1 design completion.*

✅ **Layering and Public API**: Design maintains existing public API; internal composition changes properly encapsulated.

✅ **Dependencies and Portability**: No new dependencies introduced; existing managed dependencies preserved.

✅ **Test Strategy and Coverage Discipline**: Design enables better testability with explicit dependencies and service overrides; maintains observable behavior testing.

✅ **Input Validation and Structured Diagnostics**: Existing validation patterns preserved; no changes to error handling.

✅ **Clarity Over Cleverness**: Design improves clarity by removing hidden defaults and making dependencies explicit.

**GATE STATUS**: ✅ PASS - All constitution principles satisfied after design phase.

## Project Structure

### Documentation (this feature)

```
specs/007-centralized-composition/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md       # Phase 1 output (/speckit.plan command)
├── contracts/          # Phase 1 output (/speckit.plan command)
└── tasks.md            # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```
src/
├── NetHtml2Pdf/                    # Main library
│   ├── Core/                      # Core domain objects
│   ├── Layout/                    # Layout engine components
│   │   ├── Contexts/             # Formatting contexts
│   │   ├── Diagnostics/          # Layout diagnostics
│   │   ├── Display/              # Display classification
│   │   ├── Engines/              # Layout engines
│   │   ├── FormattingContexts/   # Specific formatting contexts
│   │   ├── Model/                # Layout models
│   │   └── Pagination/           # Pagination services
│   ├── Parser/                    # HTML/CSS parsing
│   ├── Renderer/                  # PDF rendering
│   │   ├── Adapters/             # Rendering adapters
│   │   ├── Inline/               # Inline rendering
│   │   ├── Interfaces/           # Renderer interfaces
│   │   └── Spacing/              # Spacing logic
│   ├── Fonts/                     # Font resources
│   ├── PdfBuilder.cs              # Main public API
│   ├── PdfRendererFactory.cs      # Factory (to be refactored)
│   └── NetHtml2Pdf.csproj
├── NetHtml2Pdf.Test/              # Unit tests
├── NetHtml2Pdf.TestConsole/       # Console test app
└── NetHtml2Pdf.TestAzureFunction/ # Azure Function test app
```

**Structure Decision**: Single library project with comprehensive test coverage. The refactor adds internal composition classes (`RendererComposition`, `RendererServices`) without changing the existing structure.

## Complexity Tracking

*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |

