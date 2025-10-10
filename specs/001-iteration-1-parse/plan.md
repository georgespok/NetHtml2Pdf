# Implementation Plan: Iteration 1 - Core HTML parsing & rendering with Scope Additions

**Branch**: `001-iteration-1-parse` | **Date**: 2025-10-09 | **Spec**: C:/Projects/Html2Pdf/specs/001-iteration-1-parse/spec.md
**Input**: Feature specification from `C:/Projects/Html2Pdf/specs/001-iteration-1-parse/spec.md`

**Note**: This plan extends the existing Iteration 1 implementation with four scope additions: CSS margin/border shorthand properties, multi-page PDF generation, and PdfBuilder API refactoring.

## Summary

Extend Iteration 1 HTML-to-PDF conversion with: (1) CSS shorthand property support for `margin` (1-4 value patterns) and `border` (width/style/color components), (2) multi-page PDF generation through fluent builder pattern API, (3) global headers/footers with dynamic height adjustment, and (4) complete API refactoring from `HtmlConverter` to `PdfBuilder` with `IPdfBuilder` interface. All features maintain managed-only dependencies (QuestPDF, AngleSharp) for cross-platform compatibility. Breaking changes include removal of `IHtmlConverter`/`HtmlConverter` and introduction of `Reset()`, `SetHeader()`, `SetFooter()`, `AddPage()`, and `Build()` methods with fluent chaining support.

## Technical Context

**Language/Version**: C# 12 / .NET 8  
**Primary Dependencies**: QuestPDF 2024.x (PDF generation), AngleSharp 1.x (HTML parsing)  
**Storage**: In-memory document graphs and streams (no persistent storage)  
**Testing**: xUnit 2.6 with Shouldly assertions, PdfPig for PDF content verification, coverlet for coverage  
**Target Platform**: Windows and Linux (cross-platform via managed-only dependencies)  
**Project Type**: Single library project (`NetHtml2Pdf`) with test project (`NetHtml2Pdf.Test`)  
**Performance Goals**: No explicit targets for Iteration 1; measurement-focused (capture render timing via PdfRenderSnapshot)  
**Constraints**: Managed-only dependencies (no GDI+/native libraries), 30-second timeout for documents ≤10MB, 500MB memory limit  
**Scale/Scope**: Iteration 1 adds 4 features to existing codebase: CSS shorthand parsing (margin/border), multi-page PDF generation, headers/footers, PdfBuilder API refactoring  
**Cross-Platform**: Ensured through managed-only dependencies; issues addressed reactively as they arise  
**Breaking Changes**: Complete removal of `IHtmlConverter`/`HtmlConverter`; all tests and integrations must migrate to `IPdfBuilder`/`PdfBuilder`  
**Supported Markup**: 22 HTML elements + 15+ CSS properties (expanding with margin/border shorthands)  
**CSS Shorthand Scope**: Standard 1-4 value syntax for margin; width/style/color components for border (no CSS variables/calc() in Iteration 1)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### I. Test-Driven Delivery ✅ PASS
- **Status**: Compliant
- **Evidence**: FR-006 mandates red-green-refactor cycle: write ONE failing test → implement minimal code to pass → refactor → repeat. Only ONE test failing at a time. Test concrete implementations only - do NOT test interfaces themselves. All acceptance criteria (95+) are testable and measurable. Spec requires Theory/InlineData consolidation, helper methods, and test methods <15 lines.
- **Action**: Follow classic TDD cycle for all scope additions: write one test → make it green → write next test. Test PdfBuilder, HtmlParser, CssStyleUpdater implementations - not IPdfBuilder, IHtmlParser, or other interfaces. Interfaces are contracts validated through implementation tests.

### II. Managed Cross-Platform Compatibility ✅ PASS
- **Status**: Compliant
- **Evidence**: FR-004 maintains managed-only dependencies constraint. QuestPDF and AngleSharp are managed .NET libraries. No GDI+/native libraries introduced. Nullable reference types enabled.
- **Action**: Verify dependency audit post-implementation: `dotnet list package --include-transitive`.

### III. Layered Rendering Architecture ✅ PASS
- **Status**: Compliant with API layer change
- **Evidence**: Scope additions respect existing layers: Core (CssStyleMap extended for shorthand properties), Parser (CssStyleUpdater handles shorthand parsing), Renderer (no changes needed for CSS shorthands; QuestPDF handles multi-page), Facade (PdfBuilder replaces HtmlConverter).
- **Action**: PdfBuilder refactoring changes facade layer only; Core/Parser/Renderer layers remain architecturally separated.

### IV. HTML & CSS Contract Clarity ✅ PASS
- **Status**: Compliant
- **Evidence**: Spec documents new CSS shorthand properties in FR-002a with complete acceptance criteria (AC-002a.1 through AC-002a.13). Edge cases defined for invalid shorthand values (reject entire declaration, emit warning, fall back to defaults).
- **Action**: Generate contracts in Phase 1 for margin shorthand (1-4 values), border shorthand (component variations), and PdfBuilder API usage patterns.

### V. Extensible Experience Stewardship ⚠️ NEEDS JUSTIFICATION
- **Status**: Requires justification for breaking changes
- **Evidence**: Scope additions include breaking API change (IHtmlConverter → IPdfBuilder) within same iteration as original Iteration 1 work. This deviates from "preserve backward compatibility" principle.
- **Justification**: Breaking change is intentional and acceptable because: (1) Iteration 1 is still in-progress on feature branch `001-iteration-1-parse`, (2) PdfBuilder better reflects builder pattern semantics before public release, (3) All existing tests will be updated in same iteration, (4) No public consumers exist yet (pre-release state).
- **Action**: Document migration path in quickstart.md. Mark AC-003.10 and AC-003.11 as breaking change validation.

### VI. Architecture Planning & Validation ✅ PASS
- **Status**: Compliant
- **Evidence**: This `/speckit.plan` execution documents technical context, validates constitution compliance, and generates research/data-model/contracts before implementation begins.
- **Action**: Complete Phase 0 research and Phase 1 design before any code changes.

**GATE DECISION**: ✅ **PASS WITH JUSTIFICATION**  
All principles satisfied. Principle V deviation (breaking change) is justified because iteration is in-progress on feature branch with no public consumers.

## Project Structure

### Documentation (this feature)

```
specs/[###-feature]/
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
├── NetHtml2Pdf/                        # Main library project
│   ├── Core/                           # Data models and core entities
│   │   ├── BoxSpacing.cs
│   │   ├── ConverterOptions.cs
│   │   ├── CssStyleMap.cs             # Extended for margin/border shorthand
│   │   ├── CssStyleSource.cs
│   │   ├── DocumentNode.cs
│   │   ├── DocumentNodeType.cs
│   │   ├── FontStyle.cs
│   │   ├── PdfRenderSnapshot.cs
│   │   └── TextDecorationStyle.cs
│   ├── Parser/                         # HTML→DocumentNode conversion
│   │   ├── CssClassStyleExtractor.cs
│   │   ├── CssDeclarationParser.cs
│   │   ├── CssStyleResolver.cs
│   │   ├── CssStyleUpdater.cs         # Extended for shorthand parsing
│   │   ├── HtmlNodeConverter.cs
│   │   ├── HtmlParser.cs
│   │   └── Interfaces/
│   ├── Renderer/                       # DocumentNode→PDF rendering
│   │   ├── BlockComposer.cs
│   │   ├── BlockSpacingApplier.cs
│   │   ├── InlineComposer.cs
│   │   ├── InlineStyleState.cs
│   │   ├── ListComposer.cs
│   │   ├── PdfRenderer.cs             # QuestPDF multi-page support
│   │   ├── PdfRendererFactory.cs
│   │   ├── RendererOptions.cs
│   │   ├── TableComposer.cs
│   │   └── Interfaces/
│   ├── DependencyInjection/
│   │   └── ServiceCollectionExtensions.cs  # Updated: AddPdfBuilder()
│   ├── Fonts/
│   │   └── Inter-Regular.ttf
│   ├── IPdfBuilder.cs                 # NEW: Public interface
│   ├── PdfBuilder.cs                  # NEW: Replaces HtmlConverter
│   └── NetHtml2Pdf.csproj
│
├── NetHtml2Pdf.Test/                  # Test project
│   ├── Core/
│   │   ├── CssStyleMapTests.cs        # Extended for shorthand properties
│   │   ├── DocumentNodeTypeTests.cs
│   │   └── PdfRenderSnapshotTests.cs
│   ├── Parser/
│   │   ├── CssStyleUpdaterTests.cs    # Extended for shorthand parsing
│   │   ├── HtmlParserTests.cs
│   │   └── [other parser tests]
│   ├── Renderer/
│   │   ├── PdfRendererTests.cs        # Extended for multi-page tests
│   │   └── [other renderer tests]
│   ├── DependencyInjection/
│   │   └── DependencyInjectionTests.cs  # Updated for AddPdfBuilder()
│   ├── PdfBuilderTests.cs             # NEW: Integration tests (renamed from HtmlConverterTests)
│   ├── Support/
│   │   ├── PdfRenderTestBase.cs
│   │   └── PdfWordParser.cs
│   └── NetHtml2Pdf.Test.csproj
│
└── NetHtml2Pdf.sln
```

**Structure Decision**: Single library project with layered architecture (Core/Parser/Renderer/Facade). Scope additions extend existing layers: Core (CSS model), Parser (shorthand parsing), Renderer (multi-page support), Facade (PdfBuilder API). Test project mirrors source structure with unit tests, integration tests, and contract validation tests. Breaking change requires renaming `HtmlConverterTests.cs` to `PdfBuilderTests.cs` and updating all test instantiations.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Breaking API change (Constitution V) | PdfBuilder better reflects builder pattern semantics; improves API clarity before public release | Keeping HtmlConverter would create confusion with multi-page API (AddPage/Build methods don't semantically fit "Converter" class); renaming now avoids future breaking change after public release |

---

## Phase 0: Research & Decision Documentation

**Status**: Complete (leveraging existing knowledge from clarification session)

### Research Findings

#### 1. CSS Shorthand Property Parsing

**Decision**: Implement custom shorthand parsers in `CssStyleUpdater` using standard CSS syntax rules

**Rationale**:
- **Margin Shorthand**: CSS spec defines clear 1-4 value patterns (all sides, vertical/horizontal, top/horizontal/bottom, TRBL)
- **Border Shorthand**: CSS allows width/style/color in any order; parser must handle all combinations
- **Source Order Precedence**: Standard CSS cascade behavior—later declaration wins regardless of shorthand/longhand

**Alternatives Considered**:
- **AngleSharp CSS parser**: Would add complexity and potential version conflicts; overkill for limited property set
- **Regex-based parsing**: Fragile for edge cases and hard to maintain; dedicated parser methods are more testable

**Implementation Approach**:
- Add `ParseMarginShorthand(string value)` method to `CssStyleUpdater` that splits on whitespace and maps to margin-top/right/bottom/left
- Add `ParseBorderShorthand(string value)` method that tokenizes and identifies width/style/color components
- Invalid shorthands rejected entirely (AC-002a.11) with structured warning logs
- Update `CssStyleMap` to track shorthand vs longhand source for cascade resolution

#### 2. Multi-Page PDF Generation with QuestPDF

**Decision**: Use QuestPDF's `Document.Create()` API with `Page()` method for multi-page support and `Header()`/`Footer()` sections for repeated content

**Rationale**:
- QuestPDF natively supports multi-page documents through fluent API: `Document.Create(container => container.Page(...).Page(...).Page(...))`
- Headers/footers defined once and automatically repeated across all pages
- Dynamic height supported through QuestPDF's layout engine (no manual height calculations needed)

**Alternatives Considered**:
- **Manual page break insertion**: Would require complex content flow logic and break detection; QuestPDF handles automatically
- **Separate PDF merge**: Would generate multiple single-page PDFs and merge; inefficient and loses QuestPDF's layout benefits

**Implementation Approach**:
- Store pages as `List<string>` (HTML content) in `PdfBuilder` state
- Store header/footer as `string?` fields
- In `Build()` method, create QuestPDF `Document` with:
  - `document.Header(header => RenderHtmlToQuestPdf(headerHtml))`
  - `document.Footer(footer => RenderHtmlToQuestPdf(footerHtml))`
  - For each page: `document.Page(page => RenderHtmlToQuestPdf(pageHtml))`
- QuestPDF's layout engine automatically adjusts content area based on header/footer height

#### 3. Builder Pattern API Design

**Decision**: Implement fluent builder pattern with `IPdfBuilder` interface and `PdfBuilder` class using immutable-style state management

**Rationale**:
- **Fluent Interface**: All methods return `IPdfBuilder` for method chaining (industry-standard builder pattern)
- **Reset() Method**: Enables builder reuse without creating new instances; critical for DI scenarios (singleton lifetime)
- **Explicit Build()**: Clear separation between accumulation phase and finalization; prevents accidental PDF generation
- **State Encapsulation**: Internal `List<string> _pages`, `string? _header`, `string? _footer` hidden from consumers

**Alternatives Considered**:
- **Immutable Builder**: Would create new builder instance on each method call; memory overhead and GC pressure for large documents
- **Separate Page class**: Would add unnecessary abstraction; HTML strings sufficient for current scope

**Implementation Approach**:
- `IPdfBuilder` interface with 5 methods: `Reset()`, `SetHeader(string)`, `SetFooter(string)`, `AddPage(string)`, `Build(ConverterOptions?)`
- `PdfBuilder` class maintains mutable state internally but exposes immutable interface semantics
- DI registration as `Transient` (new instance per request) or `Scoped` (one instance per scope)—**needs clarification in data-model.md**
- Validation in each method (null checks, empty checks) with clear exception messages

#### 4. Breaking Change Migration Strategy

**Decision**: Provide clear migration path in `quickstart.md` with before/after code examples

**Rationale**:
- Breaking change is justified (Iteration 1 in-progress, no public consumers), but migration must be straightforward
- Single-page usage becomes: `new PdfBuilder().AddPage(html).Build()` (simple transformation)
- Multi-page usage: `builder.AddPage(page1).AddPage(page2).Build()` (natural extension)

**Migration Steps**:
1. Rename `HtmlConverter` instantiations to `PdfBuilder`
2. Replace `ConvertToPdf(html)` with `AddPage(html).Build()`
3. Update DI registration from `AddHtml2Pdf()` to `AddPdfBuilder()`
4. Update test file: `HtmlConverterTests.cs` → `PdfBuilderTests.cs`

---

## Phase 1: Design Artifacts

**Status**: ✅ Complete

### Progress Tracking

| Artifact | Status | Location |
|----------|--------|----------|
| research.md | ✅ Complete | Phase 0 (embedded in plan.md) |
| data-model.md | ✅ Complete | C:/Projects/Html2Pdf/specs/001-iteration-1-parse/data-model.md |
| contracts/css-margin-shorthand.md | ✅ Complete | C:/Projects/Html2Pdf/specs/001-iteration-1-parse/contracts/css-margin-shorthand.md |
| contracts/css-border-shorthand.md | ✅ Complete | C:/Projects/Html2Pdf/specs/001-iteration-1-parse/contracts/css-border-shorthand.md |
| contracts/pdfbuilder-api.md | ✅ Complete | C:/Projects/Html2Pdf/specs/001-iteration-1-parse/contracts/pdfbuilder-api.md |
| quickstart.md | ✅ Complete | C:/Projects/Html2Pdf/specs/001-iteration-1-parse/quickstart.md (updated with scope additions) |

---

## Constitution Re-Check (Post-Design)

All six constitution principles remain compliant after Phase 1 design:

✅ **I. Test-Driven Delivery**: Contracts define test cases for TDD approach  
✅ **II. Managed Cross-Platform Compatibility**: No new dependencies; QuestPDF/AngleSharp remain  
✅ **III. Layered Rendering Architecture**: Design respects Core/Parser/Renderer/Facade separation  
✅ **IV. HTML & CSS Contract Clarity**: 3 new contracts generated with complete specifications  
✅ **V. Extensible Experience Stewardship**: Breaking change justified (in-progress iteration)  
✅ **VI. Architecture Planning & Validation**: Phase 0 & 1 complete before implementation

**Final Gate Decision**: ✅ **PASS** - Ready for Phase 2 (Task Generation)

---

## Phase 2: Task Generation

**Status**: ✅ Complete

**Generated**: `C:/Projects/Html2Pdf/specs/001-iteration-1-parse/tasks-scope-additions.md`

**Task Summary**:
- **Total Tasks**: 53 tasks across 5 phases
- **User Story 1** (PdfBuilder API): 18 tasks (T004-T021) - FOUNDATIONAL
- **User Story 2** (CSS Shorthands): 15 tasks (T022-T036) - INDEPENDENT
- **User Story 3** (Multi-Page PDF): 9 tasks (T037-T045) - DEPENDS ON US1
- **Integration & Polish**: 8 tasks (T046-T053) - Final validation
- **Parallel Opportunities**: 15 tasks marked [P]

**Implementation Tasks**:
1. ✅ CSS margin shorthand parsing (T022-T032, T035)
2. ✅ CSS border shorthand parsing (T027-T030, T033-T034, T036)
3. ✅ PdfBuilder API implementation (T004-T016)
4. ✅ Breaking change migration (T017-T021)
5. ✅ Multi-page PDF generation (T037-T045)
6. ✅ Headers/footers rendering (T038-T041, T042-T043)
7. ✅ Contract validation tests (T046-T048)

**Critical Path**: T001-T003 → T004-T021 (US1) → [T022-T036 (US2) || T037-T045 (US3)] → T046-T053

---

**Planning Phase Complete**: All design artifacts and task breakdown generated. Constitution validated. Ready for TDD implementation starting with T001.

**Next Steps**:
1. Execute tasks in order: T001 → T053
2. Follow TDD approach: Write tests FIRST (red), implement (green), refactor
3. Mark tasks complete in tasks-scope-additions.md as work progresses
4. Validate checkpoints after each user story phase
