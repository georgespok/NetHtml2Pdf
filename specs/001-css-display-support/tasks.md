# Tasks: CSS Display Support (Block, Inline-Block, None)

**Input**: Design documents from `/specs/001-css-display-support/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

## Phase 1: Setup (Shared Infrastructure)

- [x] T001 Ensure solution builds and tests run locally (`dotnet test`) at repo root
- [x] T002 Verify references to `docs/testing-guidelines.md` and `docs/coding-standards.md` in plan.md

## Phase 2: Foundational (Blocking Prerequisites)

- [x] T003 Create enum `CssDisplay` in `src/NetHtml2Pdf/Core/Enums/CssDisplay.cs`
- [x] T004 Extend `CssStyleMap` to include `Display` and `WithDisplay()` in `src/NetHtml2Pdf/Core/CssStyleMap.cs`

## Phase 3: User Story 1 - Honor block and inline-block layout (Priority: P1) 🎯 MVP

**Goal**: Block starts new lines; inline-block flows inline as atomic boxes.
**Independent Test**: Verify line breaks and box dimensions in PDF.

- [x] T005 [P] [US1] Parse `display` property in `src/NetHtml2Pdf/Parser/CssDeclarationParser.cs`
- [x] T006 [P] [US1] Apply display precedence in `src/NetHtml2Pdf/Parser/CssStyleUpdater.cs`
- [x] T007 [US1] Implement block behavior in `src/NetHtml2Pdf/Renderer/BlockComposer.cs`
- [x] T008 [US1] Implement inline-block behavior in `src/NetHtml2Pdf/Renderer/InlineComposer.cs`
- [x] T009 [P] [US1] Add parser tests in `src/NetHtml2Pdf.Test/Parser/CssDeclarationParserTests.cs`
- [x] T010 [P] [US1] Add style updater tests in `src/NetHtml2Pdf.Test/Parser/CssStyleUpdaterTests.cs`
- [x] T011 [US1] Add renderer tests for block/inline-block in `src/NetHtml2Pdf.Test/Renderer/InlineComposerTests.cs`

## Phase 4: User Story 2 - Omit elements with display:none (Priority: P1)

**Goal**: Elements with `display: none` render no boxes and occupy no space.
**Independent Test**: Omitted from output and text extraction.

- [x] T012 [US2] Skip nodes with `display:none` in `src/NetHtml2Pdf/Renderer/PdfRenderer.cs`
- [x] T013 [P] [US2] Add renderer tests for `display:none` in `src/NetHtml2Pdf.Test/Renderer/PdfRendererTests.cs`

## Phase 5: User Story 3 - Warnings and fallbacks for unsupported values (Priority: P2)

**Goal**: Structured warning + fallback to HTML semantic default.
**Independent Test**: Single warning per occurrence; consistent fallback.

- [X] T014 [US3] Emit structured warning for unsupported display in `src/NetHtml2Pdf/Parser/CssStyleUpdater.cs`
- [X] T015 [P] [US3] Add updater tests for unsupported values in `src/NetHtml2Pdf.Test/Parser/CssStyleUpdaterTests.cs`

## Phase 6: User Story 4 - Interactions with width/height/margins (Priority: P3)

**Goal**: Correct sizing and margin interactions for both display modes.
**Independent Test**: Verify computed layout in PDF.

- [X] T016 [US4] Validate block width rules and margin collapsing in `src/NetHtml2Pdf/Renderer/BlockComposer.cs`
- [X] T017 [US4] Validate inline-block sizing/wrapping in `src/NetHtml2Pdf/Renderer/InlineComposer.cs`
- [X] T018 [P] [US4] Add renderer tests for spacing interactions in `src/NetHtml2Pdf.Test/Renderer/BlockComposerTests.cs`

## Phase N: Polish & Cross-Cutting

- [X] T019 [P] Update contracts doc `specs/001-css-display-support/contracts/css-display.md` with examples
- [X] T020 [P] Add fixtures for block/inline-block/none in `src/NetHtml2Pdf.Test/samples/`
- [X] T021 Run `dotnet format` and ensure no linter errors
- [X] T022 Update README highlights if needed

## Dependencies & Execution Order

- Setup → Foundational → US1 (MVP) → US2 → US3 → US4 → Polish
- Parallel opportunities: T005/T006/T009/T010 can run in parallel; renderer tests per story can run in parallel.
