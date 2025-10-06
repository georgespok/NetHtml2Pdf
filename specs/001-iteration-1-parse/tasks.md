# Tasks: Iteration 1 - Core HTML parsing & rendering

**Input**: Design documents from C:\Projects\Html2Pdf\specs\001-iteration-1-parse
**Prerequisites**: plan.md (required), research.md, data-model.md, contracts/
**Generated**: 2025-01-27

## Execution Flow (main)
```
1. Load plan.md from feature directory.
   -> Extract: tech stack (C# 12, .NET 8.0), libraries (QuestPDF, AngleSharp), layered architecture.
2. Load design documents.
   -> data-model.md: DocumentNode, PdfRenderSnapshot entities → Core layer tasks.
   -> contracts/: core-paragraphs, fallback-unsupported-tag, table-borders-alignment → contract tests.
   -> research.md: Technology decisions → setup and validation tasks.
3. Generate tasks honoring constitution (TDD approach):
   -> Tests FIRST before implementation (red-green-refactor cycle).
   -> Maintain layer boundaries (Core → Parser → Renderer → Facade).
   -> Comprehensive test coverage for classes with business logic and complex behavior.
4. Apply dependency rules.
   -> Tests precede implementation within the SAME task via red/green loop.
   -> Sequential ordering enforced when files overlap.
5. Number tasks sequentially (T001, T002, ...).
6. Validate readiness after each task (tests passing, docs updated when required).
```

## Phase 1: Setup & Infrastructure

### Project Initialization
- [X] T001 Restore solution dependencies and verify managed-only constraint (`dotnet restore C:/Projects/Html2Pdf/src/NetHtml2Pdf.sln`).
- [X] T002 Configure test project package versions to match src (QuestPDF, AngleSharp, xUnit, Shouldly, PdfPig) via `src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj`.
- [X] T003 Configure logging dependencies for structured warning logs in `src/NetHtml2Pdf/NetHtml2Pdf.csproj`.
- [X] T004 Run managed dependency audit (`pwsh -c 'dotnet list C:/Projects/Html2Pdf/src/NetHtml2Pdf/NetHtml2Pdf.csproj package --include-transitive'`) and document results in `specs/001-iteration-1-parse/research.md`.

### Core Entity Foundation
- [X] T005 [US1] Add DocumentNode entity tests and implementation (`src/NetHtml2Pdf.Test/Core/DocumentNodeTests.cs`, `src/NetHtml2Pdf/Core/DocumentNode.cs`).
  - Write failing tests for DocumentNode entity covering NodeType, TextContent, Styles, Children → implement entity until green.
- [X] T006 [US1] Add PdfRenderSnapshot entity tests and implementation (`src/NetHtml2Pdf.Test/Core/PdfRenderSnapshotTests.cs`, `src/NetHtml2Pdf/Core/PdfRenderSnapshot.cs`).
  - Write failing tests for PdfRenderSnapshot entity covering RenderDuration, Platform, Warnings, OutputSize, Timestamp, FallbackElements, InputHtmlSize, ElementCount, SupportedElementCount, FallbackElementCount, CssPropertyCount, MemoryUsage, IsCrossPlatformValidated, ValidationTimestamp, ValidationResult → implement entity until green.
- [X] T007 [US1] Add DocumentNodeType enum tests and implementation (`src/NetHtml2Pdf.Test/Core/DocumentNodeTypeTests.cs`, `src/NetHtml2Pdf/Core/DocumentNodeType.cs`).
  - Write failing tests for all supported HTML element types including headings (Heading1-Heading6) → implement enum until green.
- [X] T008 [US1] Add CssStyleMap and CssStyleSource tests and implementation (`src/NetHtml2Pdf.Test/Core/CssStyleMapTests.cs`, `src/NetHtml2Pdf/Core/CssStyleMap.cs`, `src/NetHtml2Pdf/Core/CssStyleSource.cs`).
  - Write failing tests for CSS property mapping and source tracking → implement until green.

### Logging Infrastructure
- [ ] T009 [US1] Add structured logging interface tests and implementation (`src/NetHtml2Pdf.Test/Core/ILoggerTests.cs`, `src/NetHtml2Pdf/Core/ILogger.cs`).
  - Write failing tests for warning log emission → implement structured logging interface until green.
- [ ] T010 [US1] Add logger implementation tests and implementation (`src/NetHtml2Pdf.Test/Core/LoggerTests.cs`, `src/NetHtml2Pdf/Core/Logger.cs`).
  - Write failing tests for logger implementation → implement until green.
- [ ] T010A [US1] Add structured warning log format validation tests and implementation (`src/NetHtml2Pdf.Test/Core/StructuredWarningLogTests.cs`, `src/NetHtml2Pdf/Core/StructuredWarningLog.cs`).
  - Write failing tests for warning log format: `{Timestamp} [WARNING] {Component}: {Message}` → implement until green.
- [ ] T010B [US1] Add warning log severity level tests and implementation (`src/NetHtml2Pdf.Test/Core/WarningSeverityTests.cs`, `src/NetHtml2Pdf/Core/WarningSeverity.cs`).
  - Write failing tests for WARNING severity level validation → implement until green.

## Phase 2: Foundational Prerequisites

### HTML Parser Foundation
- [ ] T011 [US1] Add HtmlParser interface tests and implementation (`src/NetHtml2Pdf.Test/Parser/IHtmlParserTests.cs`, `src/NetHtml2Pdf/Parser/IHtmlParser.cs`).
  - Write failing tests for HTML parsing interface → implement until green.
- [ ] T012 [US1] Add HtmlParser implementation tests and implementation (`src/NetHtml2Pdf.Test/Parser/HtmlParserTests.cs`, `src/NetHtml2Pdf/Parser/HtmlParser.cs`).
  - Write failing tests for AngleSharp integration and DOM node mapping → implement until green.

### PDF Renderer Foundation
- [ ] T013 [US1] Add PdfRenderer interface tests and implementation (`src/NetHtml2Pdf.Test/Renderer/IPdfRendererTests.cs`, `src/NetHtml2Pdf/Renderer/IPdfRenderer.cs`).
  - Write failing tests for PDF rendering interface → implement until green.
- [ ] T014 [US1] Add PdfRenderer implementation tests and implementation (`src/NetHtml2Pdf.Test/Renderer/PdfRendererTests.cs`, `src/NetHtml2Pdf/Renderer/PdfRenderer.cs`).
  - Write failing tests for QuestPDF integration and document composition → implement until green.

### Public API Foundation
- [ ] T015 [US1] Add HtmlConverter interface tests and implementation (`src/NetHtml2Pdf.Test/IHtmlConverterTests.cs`, `src/NetHtml2Pdf/IHtmlConverter.cs`).
  - Write failing tests for public API interface → implement until green.
- [ ] T016 [US1] Add HtmlConverter implementation tests and implementation (`src/NetHtml2Pdf.Test/HtmlConverterTests.cs`, `src/NetHtml2Pdf/HtmlConverter.cs`).
  - Write failing tests for facade orchestration → implement until green.

## Phase 3: User Story 1 - Core HTML Element Parsing & Rendering

**Story Goal**: Parse and render basic HTML elements (div, p, span, strong, b, i, br, h1, h2, h3, h4, h5, h6) with inline styles and CSS classes.

**Independent Test Criteria**: 
- All supported HTML elements parse correctly into HtmlFragment entities
- CSS properties (font-weight, font-style, text-decoration, line-height, color, background-color, margin, padding) apply correctly
- PDF output renders consistently with expected text formatting and spacing
- Contract tests pass for core-paragraphs.md scenario
- TDD approach followed (tests written first, then implementation)

### Contract Tests
- [ ] T017 [US1] Add core-paragraphs contract test (`src/NetHtml2Pdf.Test/Contracts/CoreParagraphsContractTests.cs`).
  - Write failing contract test based on `contracts/core-paragraphs.md` → implement until green.

### Parser Implementation
- [ ] T018 [US1] Add parser coverage for basic HTML elements (`src/NetHtml2Pdf.Test/Parser/HtmlParserTests.cs`, `src/NetHtml2Pdf/Parser/HtmlParser.cs`).
  - Write failing tests for basic HTML elements parsing (see FR-001 in spec.md for complete element list: div, p, span, strong, b, i, br, h1, h2, h3, h4, h5, h6) → implement until green.
- [ ] T019 [US1] Add CSS style parsing tests and implementation (`src/NetHtml2Pdf.Test/Parser/CssStyleParserTests.cs`, `src/NetHtml2Pdf/Parser/CssStyleParser.cs`).
  - Write failing tests for supported CSS styles → implement until green.

### Renderer Implementation
- [ ] T020 [US1] Add renderer coverage for basic HTML elements (`src/NetHtml2Pdf.Test/Renderer/PdfRendererTests.cs`, `src/NetHtml2Pdf/Renderer/PdfRenderer.cs`).
  - Write failing tests for QuestPDF paragraph and heading rendering → implement until green.
- [ ] T021 [US1] Add CSS styling renderer tests and implementation (`src/NetHtml2Pdf.Test/Renderer/CssStyleRendererTests.cs`, `src/NetHtml2Pdf/Renderer/CssStyleRenderer.cs`).
  - Write failing tests for CSS style application → implement until green.

### Integration
- [ ] T022 [US1] Add HtmlConverter integration for basic elements (`src/NetHtml2Pdf.Test/HtmlConverterTests.cs`, `src/NetHtml2Pdf/HtmlConverter.cs`).
  - Write failing integration test for end-to-end HTML-to-PDF conversion → implement until green.

**Checkpoint**: US1 complete - Basic HTML elements including headings render correctly with CSS styling.

## Phase 4: User Story 2 - List & Section Support

**Story Goal**: Parse and render list elements (ul, ol, li) and structural containers (div, section).

**Independent Test Criteria**:
- Unordered and ordered lists render with proper bullets/numbers
- List items maintain proper indentation and spacing
- Section and div containers provide structural layout
- Nested lists render correctly
- TDD approach followed (tests written first, then implementation)

### Parser Implementation
- [ ] T023 [US2] Add parser coverage for list elements (`src/NetHtml2Pdf.Test/Parser/HtmlParserTests.cs`, `src/NetHtml2Pdf/Parser/HtmlParser.cs`).
  - Write failing tests for list elements parsing (see FR-001 in spec.md for complete element list: ul, li, ol) → implement until green.
- [ ] T024 [US2] Add parser coverage for structural containers (`src/NetHtml2Pdf.Test/Parser/HtmlParserTests.cs`, `src/NetHtml2Pdf/Parser/HtmlParser.cs`).
  - Write failing tests for structural containers parsing (see FR-001 in spec.md for complete element list: div, section) → implement until green.

### Renderer Implementation
- [ ] T025 [US2] Add renderer coverage for list elements (`src/NetHtml2Pdf.Test/Renderer/PdfRendererTests.cs`, `src/NetHtml2Pdf/Renderer/PdfRenderer.cs`).
  - Write failing tests for QuestPDF list rendering → implement until green.
- [ ] T026 [US2] Add renderer coverage for structural containers (`src/NetHtml2Pdf.Test/Renderer/PdfRendererTests.cs`, `src/NetHtml2Pdf/Renderer/PdfRenderer.cs`).
  - Write failing tests for container layout → implement until green.

### Integration
- [ ] T027 [US2] Add HtmlConverter integration for lists and containers (`src/NetHtml2Pdf.Test/HtmlConverterTests.cs`, `src/NetHtml2Pdf/HtmlConverter.cs`).
  - Write failing integration test for list rendering → implement until green.

**Checkpoint**: US2 complete - Lists and structural containers render correctly.

## Phase 5: User Story 3 - Table Support with Borders & Alignment

**Story Goal**: Parse and render table elements (table, thead, tbody, tr, th, td) with border styling and cell alignment.

**Independent Test Criteria**:
- Tables render with proper borders and cell spacing
- Header cells (th) render with appropriate styling
- Cell alignment (text-align, vertical-align) works correctly
- Border properties (border, border-collapse, border-width, border-style, border-color) apply correctly
- Contract tests pass for table-borders-alignment.md scenario
- TDD approach followed (tests written first, then implementation)

### Contract Tests
- [ ] T028 [US3] Add table-borders-alignment contract test (`src/NetHtml2Pdf.Test/Contracts/TableBordersAlignmentContractTests.cs`).
  - Write failing contract test based on `contracts/table-borders-alignment.md` → implement until green.

### Parser Implementation
- [ ] T029 [US3] Add parser coverage for table elements (`src/NetHtml2Pdf.Test/Parser/HtmlParserTests.cs`, `src/NetHtml2Pdf/Parser/HtmlParser.cs`).
  - Write failing tests for table elements parsing (see FR-001 in spec.md for complete element list: table, thead, tbody, tr, th, td) → implement until green.
- [ ] T030 [US3] Add table CSS style parsing tests and implementation (`src/NetHtml2Pdf.Test/Parser/TableCssStyleParserTests.cs`, `src/NetHtml2Pdf/Parser/TableCssStyleParser.cs`).
  - Write failing tests for table-specific CSS styles → implement until green.

### Renderer Implementation
- [ ] T031 [US3] Add renderer coverage for table elements (`src/NetHtml2Pdf.Test/Renderer/PdfRendererTests.cs`, `src/NetHtml2Pdf/Renderer/PdfRenderer.cs`).
  - Write failing tests for QuestPDF table rendering → implement until green.
- [ ] T032 [US3] Add table styling renderer tests and implementation (`src/NetHtml2Pdf.Test/Renderer/TableStyleRendererTests.cs`, `src/NetHtml2Pdf/Renderer/TableStyleRenderer.cs`).
  - Write failing tests for table border and alignment style rendering → implement until green.

### Integration
- [ ] T033 [US3] Add HtmlConverter integration for tables (`src/NetHtml2Pdf.Test/HtmlConverterTests.cs`, `src/NetHtml2Pdf/HtmlConverter.cs`).
  - Write failing integration test for table rendering → implement until green.

**Checkpoint**: US3 complete - Tables render correctly with borders and alignment.

## Phase 6: User Story 4 - Fallback Support for Unsupported Elements

**Story Goal**: Handle unsupported HTML elements gracefully with fallback rendering and warning logs.

**Independent Test Criteria**:
- Unsupported elements (e.g., video, audio) are processed by fallback renderer
- Warning logs are emitted for unsupported elements
- Document structure is preserved despite unsupported elements
- Contract tests pass for fallback-unsupported-tag.md scenario
- TDD approach followed (tests written first, then implementation)

### Contract Tests
- [ ] T034 [US4] Add fallback-unsupported-tag contract test (`src/NetHtml2Pdf.Test/Contracts/FallbackUnsupportedTagContractTests.cs`).
  - Write failing contract test based on `contracts/fallback-unsupported-tag.md` → implement until green.

### Parser Implementation
- [ ] T035 [US4] Add parser coverage for unsupported elements (`src/NetHtml2Pdf.Test/Parser/HtmlParserTests.cs`, `src/NetHtml2Pdf/Parser/HtmlParser.cs`).
  - Write failing tests for fallback node creation → implement until green.

### Renderer Implementation
- [ ] T036 [US4] Add renderer coverage for fallback elements (`src/NetHtml2Pdf.Test/Renderer/PdfRendererTests.cs`, `src/NetHtml2Pdf/Renderer/PdfRenderer.cs`).
  - Write failing tests for fallback placeholder rendering → implement until green.
- [ ] T037 [US4] Add fallback warning logging tests and implementation (`src/NetHtml2Pdf.Test/Renderer/FallbackLoggingTests.cs`, `src/NetHtml2Pdf/Renderer/PdfRenderer.cs`).
  - Write failing tests for warning log emission → implement until green.

### Integration
- [ ] T038 [US4] Add HtmlConverter integration for fallback behavior (`src/NetHtml2Pdf.Test/HtmlConverterTests.cs`, `src/NetHtml2Pdf/HtmlConverter.cs`).
  - Write failing integration test for fallback handling → implement until green.

**Checkpoint**: US4 complete - Unsupported elements handled gracefully with warnings.

## Phase 7: User Story 5 - Performance Timing & Integration Testing

**Story Goal**: Capture render timing data for monitoring and data collection, and validate end-to-end functionality.

**Independent Test Criteria**:
- Render timing data is captured and logged
- Performance data is recorded for future benchmarking
- Integration test console validates end-to-end functionality
- TDD approach followed (tests written first, then implementation)

### Performance Implementation
- [ ] T039 [US5] Add render timing capture tests and implementation (`src/NetHtml2Pdf.Test/Performance/RenderTimingTests.cs`, `src/NetHtml2Pdf/HtmlConverter.cs`).
  - Write failing tests for timing data capture → implement until green.
- [ ] T040 [US5] Add performance logging tests and implementation (`src/NetHtml2Pdf.Test/Performance/PerformanceLoggingTests.cs`, `src/NetHtml2Pdf/Core/PdfRenderSnapshot.cs`).
  - Write failing tests for performance data logging → implement until green.

### Integration Testing
- [ ] T041 [US5] Add integration test console execution (`src/NetHtml2Pdf.Test/Integration/IntegrationConsoleTests.cs`).
  - Write failing test for integration console execution → implement until green.

**Checkpoint**: US5 complete - Performance timing captured.

## Phase 8: Polish & Cross-Cutting Concerns

### Documentation & Formatting
- [ ] T045 Update README with Iteration 1 capabilities (`src/NetHtml2Pdf/README.md`).
- [ ] T046 Update changelog with Iteration 1 features (`CHANGELOG.md`).
- [ ] T047 Run code formatting (`dotnet format C:/Projects/Html2Pdf/src/NetHtml2Pdf.sln`).

### Final Validation
- [ ] T042 Execute full regression test suite (`dotnet test C:/Projects/Html2Pdf/src/NetHtml2Pdf.sln --filter Iteration1`).
- [ ] T043 Generate final documentation (`specs/001-iteration-1-parse/research.md`).

## Dependencies

### Phase Dependencies
- Phase 1 (Setup) must complete before any other phase
- Phase 2 (Foundational) must complete before Phase 3 (US1)
- Phase 3 (US1) must complete before Phase 4 (US2)
- Phase 4 (US2) must complete before Phase 5 (US3)
- Phase 5 (US3) must complete before Phase 6 (US4)
- Phase 6 (US4) must complete before Phase 7 (US5)
- Phase 7 (US5) must complete before Phase 8 (Polish)

### Task Dependencies
- T005-T008 can run in parallel (different files, no dependencies)
- T009-T010B must run sequentially (logging interface before implementation)
- T011-T016 must run sequentially (foundational layers build on each other)
- T017-T022 must run sequentially (US1 implementation builds incrementally)
- T023-T027 must run sequentially (US2 implementation builds incrementally)
- T028-T033 must run sequential (US3 implementation builds incrementally)
- T034-T038 must run sequentially (US4 implementation builds incrementally)
- T039-T041 must run sequentially (US5 implementation builds incrementally)
- T045-T047 can run in parallel (different files, no dependencies)

## Parallel Execution Examples

### Phase 1 Parallel Opportunities
```
# After T001 completes, these can run in parallel:
T002, T003, T004 [P] - Different project files
T005, T006, T007, T008 [P] - Different entity files
T009, T010, T010A, T010B [P] - Different logging files
```

### Phase 2 Sequential Requirements
```
# Must run sequentially due to layer dependencies:
T011 → T012 (Parser interface before implementation)
T013 → T014 (Renderer interface before implementation)
T015 → T016 (Converter interface before implementation)
```

### Phase 3+ Sequential Requirements
```
# Each user story phase must run sequentially:
US1: T017 → T018 → T019 → T020 → T021 → T022
US2: T023 → T024 → T025 → T026 → T027
US3: T028 → T029 → T030 → T031 → T032 → T033
US4: T034 → T035 → T036 → T037 → T038
US5: T039 → T040 → T041
```

## Implementation Strategy

### MVP Scope (Phase 3 - US1)
The minimum viable product includes:
- Basic HTML element parsing (div, p, span, strong, b, i, br, h1, h2, h3, h4, h5, h6)
- CSS property support (typography, spacing, color)
- PDF rendering with QuestPDF
- Public HtmlConverter API
- Contract test validation

### Incremental Delivery
Each user story phase delivers independently testable functionality:
- US1: Core HTML elements including headings with basic styling
- US2: List and structural container support
- US3: Table support with borders and alignment
- US4: Graceful fallback handling
- US5: Performance timing and integration testing

### Quality Gates
- All tests must pass before advancing to next phase
- Comprehensive test coverage for classes with business logic and complex behavior
- Code formatting must be applied before completion

## Validation Checklist

- [ ] Each contract scenario (core-paragraphs, table-borders-alignment, fallback-unsupported-tag) covered by test+implementation pair
- [ ] Core/Parser/Renderer/Facade updates occur in the same task that adds the relevant test
- [ ] Managed dependency audit recorded in research.md before closing iteration
- [ ] Documentation and formatting updated before closing iteration
- [ ] All user stories independently testable and complete
- [ ] TDD approach followed throughout (tests first, then implementation)
- [ ] Constitution compliance maintained throughout implementation