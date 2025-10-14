# Implementation Plan: Iteration 1 - Core HTML parsing & rendering

**Branch**: `001-iteration-1-parse` | **Date**: 2025-10-14 | **Spec**: C:/Projects/Html2Pdf/specs/001-iteration-1-parse/spec.md  
**Input**: Feature specification from `/specs/001-iteration-1-parse/spec.md`

## Summary

Iteration 1 delivers a managed-only HTML → PDF pipeline accessible through the fluent `IPdfBuilder`/`PdfBuilder` facade. AngleSharp normalizes and parses supported HTML into `DocumentNode` trees, `CssStyleUpdater` resolves inline/class/shorthand styles (including `margin`/`border`), and QuestPDF composes single or multi-page documents with optional headers/footers. Ultimate goals include structured warnings for fallbacks, render-time telemetry, and contract-grade regression coverage.

## Technical Context

**Language/Version**: C# 12 on .NET 8.0  
**Primary Dependencies**: QuestPDF (PDF composition), AngleSharp (HTML parsing), Microsoft.Extensions.Logging (structured logs)  
**Storage**: N/A (all processing in-memory)  
**Testing**: xUnit 2.6, Shouldly assertions, PdfPig for PDF inspection, Coverlet for coverage reporting  
**Target Platform**: Windows and Linux (managed-only dependencies)  
**Project Type**: Single .NET class library with companion test project  
**Performance Goals**: Capture render duration and memory via `PdfRenderSnapshot`; no throughput SLA in Iteration 1  
**Constraints**: Managed-only dependencies, nullable reference types enforced, memory guardrails (~500 MB) and 30 s timeout per spec edge cases  
**Scale/Scope**: First iteration supporting 22 HTML elements, core CSS subset, PdfBuilder API, multi-page rendering, fallback logging

_Additional stakeholder input_: none supplied via $ARGUMENTS.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **I. Test-Driven Delivery** – Plan phases enforce TDD: every task begins with failing tests (unit, integration, contracts) before implementation. Regression suites and helpers are mandated. ✅
- **II. Managed Cross-Platform Compatibility** – Dependencies limited to QuestPDF/AngleSharp/Logging; plan includes recurring dependency audits. ✅
- **III. Layered Rendering Architecture** – Responsibilities mapped to Core (entities), Parser (AngleSharp adapters), Renderer (QuestPDF), and Facade (PdfBuilder). All public entry points flow through `IPdfBuilder`. ✅
- **IV. HTML & CSS Contract Clarity** – Contracts directory will define supported elements/CSS and fallback expectations; plan mandates structured warning tests. ✅
- **V. Extensible Experience Stewardship** – Iteration is scoped to core tags, CSS shorthands, PdfBuilder API, and multi-page rendering; each user story produces an independently testable slice. ✅
- **VI. Architecture Planning & Validation** – This plan documents architecture decisions prior to implementation; phases capture research, design, and constitution verification checkpoints. ✅

## Project Structure

### Documentation (this feature)

```
C:/Projects/Html2Pdf/specs/001-iteration-1-parse/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
└── tasks.md
```

### Source Code (repository root)

```
C:/Projects/Html2Pdf/
├── src/
│   ├── NetHtml2Pdf/
│   │   ├── Core/
│   │   ├── Parser/
│   │   ├── Renderer/
│   │   └── DependencyInjection/
│   └── NetHtml2Pdf.Test/
│       ├── Core/
│       ├── Parser/
│       ├── Renderer/
│       ├── Integration/
│       └── Performance/
├── specs/001-iteration-1-parse/...
└── build/, docs/, .specify/, etc.
```

**Structure Decision**: Single library + test project. Core/Parser/Renderer/Facade folders align with constitution Principle III. Tests mirror production namespaces for unit, integration, contract, and performance coverage.

## Complexity Tracking

No constitution violations anticipated; complexity table not required.
