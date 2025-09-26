# Tasks: Cross-platform HTML-to-PDF Library

**Input**: Design documents from `/specs/001-build-re-usable/`
**Prerequisites**: plan.md (present), research.md, data-model.md, quickstart.md

## Execution Flow (main)
```
1. Load plan.md from feature directory
   → Extract tech stack: .NET 8, AngleSharp, QuestPDF, Inter font
2. Load data-model.md: entities and options
3. Load research.md: decisions and constraints
4. Load quickstart.md: scenarios
5. Generate tasks by category and order
6. Return: SUCCESS (tasks ready for execution)
```

## Format: `[ID] [P?] Description`
- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

## Phase 3.1: Setup
- [x] T001 Configure Inter font asset copy and registration in `src/NetHtml2Pdf/Fonts/` and ensure it’s bundled
- [x] T002 Wire default `DocumentOptions` (Letter, 1" margins, Inter) in `src/NetHtml2Pdf/HtmlConverter.cs`
- [x] T003 [P] Add PdfPig test helper to `src/NetHtml2Pdf.Test/` for PDF text extraction (GetWords with NearestNeighbourWordExtractor)

## Phase 3.2: Tests First (TDD) ⚠️ MUST COMPLETE BEFORE 3.3
- [x] T004 [P] Unit: paragraphs/headings/br render text in order (use PdfPig) in `src/NetHtml2Pdf.Test/HtmlConverterTests.cs`
- [x] T005 [P] Unit: table renders headers and rows preserving cell text in `src/NetHtml2Pdf.Test/HtmlConverterTests.cs`
- [x] T006 [P] Unit: unsupported elements ignored; inner text preserved in `src/NetHtml2Pdf.Test/HtmlConverterTests.cs`
- [x] T007 [P] Unit: inline CSS subset applied; unsupported CSS ignored in `src/NetHtml2Pdf.Test/HtmlConverterTests.cs`
- [x] T008 Determinism: same input/options yields identical text layout across runs in `src/NetHtml2Pdf.Test/HtmlConverterTests.cs`
- [x] T009 Error handling: empty input fails with clear message in `src/NetHtml2Pdf.Test/HtmlConverterTests.cs`

## Phase 3.3: Core Implementation (ONLY after tests are failing)
- [x] T010 Implement AngleSharp-based parser mapping to intermediate nodes in `src/NetHtml2Pdf.Parser/`
- [x] T011 Implement core model nodes and styles in `src/NetHtml2Pdf.Core/`
- [x] T012 Implement DocumentModelMapper and PdfRenderer in `src/NetHtml2Pdf.Renderer/`
- [x] T013 Implement explicit pagination API: `PdfDocumentBuilder.AddPdfPage(string html)` and `RenderAsync()` in `src/NetHtml2Pdf/`
- [x] T014 Ensure defaults (Letter, 1" margins, Inter) and CSS subset handling are applied in the rendering path (PdfRenderer/Paragraph mapping) and exposed via the public API where applicable.

## Phase 3.4: Integration
- [x] T015 Integration: verify end-to-end quickstart scenario in `src/NetHtml2Pdf.TestConsole/`
- [x] T016 Add determinism smoke test using fixed input and compare extracted words order in `src/NetHtml2Pdf.Test/`

## Phase 3.5: Polish
- [x] T017 [P] Performance test: 3–5 page doc converts < 2s in `src/NetHtml2Pdf.Test/`
- [x] T018 [P] Documentation: update `README.md` with explicit pagination and defaults
- [x] T019 [P] CI: ensure analyzers warn-as-error for `src/*` and tests run PdfPig
- [x] T020 [P] Examples: add table and headings samples to `src/NetHtml2Pdf.TestConsole/`

## Dependencies
- T004–T009 before T010–T014 (TDD)
- T010 blocks T012–T014
- T012 blocks T015

## Parallel Example
```
# Launch T004–T007 together:
Task: "Unit: paragraphs/headings/br render text in order (use PdfPig) in src/NetHtml2Pdf.Test/HtmlConverterTests.cs"
Task: "Unit: table renders headers and rows preserving cell text in src/NetHtml2Pdf.Test/HtmlConverterTests.cs"
Task: "Unit: unsupported elements ignored; inner text preserved in src/NetHtml2Pdf.Test/HtmlConverterTests.cs"
Task: "Unit: inline CSS subset applied; unsupported CSS ignored in src/NetHtml2Pdf.Test/HtmlConverterTests.cs"
```
