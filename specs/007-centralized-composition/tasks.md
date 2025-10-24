# Implementation Tasks: Centralized Composition and Transient Services

**Feature**: 007-centralized-composition  
**Branch**: `007-centralized-composition`  
**Date**: 2025-10-24  
**Generated from**: plan.md, spec.md, data-model.md, research.md

## Summary

This task list implements centralized composition for the NetHtml2Pdf rendering pipeline, making all classes pure with explicit dependencies while preserving public API compatibility.

**Total Tasks**: 24  
**User Stories**: 3 (P1, P2, P3)  
**MVP Scope**: User Story 1 (P1) - Create pipeline via single composition root

## Dependencies

**User Story Completion Order**:
1. **User Story 1 (P1)** → **User Story 2 (P2)** → **User Story 3 (P3)**

**Parallel Opportunities**:
- Tasks T005-T008 can run in parallel (different pipeline components)
- Tasks T010-T012 can run in parallel (different service refactors)
- Tasks T018-T020 can run in parallel (different test scenarios)

## Implementation Strategy

**MVP First**: Complete User Story 1 to establish the composition root foundation.  
**Incremental Delivery**: Each user story is independently testable and deployable.

---

## Phase 1: Setup

### Story Goal
Initialize the composition infrastructure and prepare for refactoring.

### Independent Test Criteria
- Composition root can be created and accessed
- All existing tests continue to pass

### Implementation Tasks

- [x] T001 Create RendererServices class in src/NetHtml2Pdf/Renderer/RendererServices.cs
- [x] T002 Create RendererComposition class in src/NetHtml2Pdf/Renderer/RendererComposition.cs
- [x] T003 Add InternalsVisibleTo attribute for test projects in src/NetHtml2Pdf/Properties/AssemblyInfo.cs

---

## Phase 2: Foundational

### Story Goal
Establish the core composition root functionality that all user stories depend on.

### Independent Test Criteria
- Composition root can create renderer instances
- Service overrides work correctly

### Implementation Tasks

- [x] T004 [P] Implement RendererServices.ForTests() method in src/NetHtml2Pdf/Renderer/RendererServices.cs
- [x] T005 [P] Implement RendererServices.With() method in src/NetHtml2Pdf/Renderer/RendererServices.cs
- [x] T006 [P] Implement RendererComposition.CreateRenderer() method in src/NetHtml2Pdf/Renderer/RendererComposition.cs
- [x] T007 [P] Implement RendererComposition.CreatePdfBuilderDependencies() method in src/NetHtml2Pdf/Renderer/RendererComposition.cs

---

## Phase 3: User Story 1 - Create pipeline via single composition root (Priority: P1)

### Story Goal
As a maintainer, I can create a fully wired pipeline via `RendererComposition.CreateRenderer(options)` without any hidden defaults inside classes.

### Independent Test Criteria
- Instantiate the renderer with only `RendererOptions` and render a simple HTML input to PDF
- Verify output is produced without service lookups or defaulting in constructors

### Implementation Tasks

- [x] T008 [US1] Refactor DisplayClassifier constructor in src/NetHtml2Pdf/Layout/Display/DisplayClassifier.cs
- [x] T009 [US1] Refactor InlineFlowLayoutEngine constructor in src/NetHtml2Pdf/Layout/Engines/InlineFlowLayoutEngine.cs
- [x] T010 [US1] Refactor FormattingContextFactory constructor in src/NetHtml2Pdf/Layout/FormattingContexts/FormattingContextFactory.cs
- [x] T011 [US1] Refactor LayoutEngine constructor in src/NetHtml2Pdf/Layout/Engines/LayoutEngine.cs
- [x] T012 [US1] Refactor PaginationService constructor in src/NetHtml2Pdf/Layout/Pagination/PaginationService.cs
- [x] T013 [US1] Refactor RendererAdapter constructor in src/NetHtml2Pdf/Renderer/Adapters/RendererAdapter.cs
- [x] T014 [US1] Refactor BlockComposer constructor in src/NetHtml2Pdf/Renderer/BlockComposer.cs
- [x] T015 [US1] Refactor PdfRenderer constructor in src/NetHtml2Pdf/Renderer/PdfRenderer.cs

---

## Phase 4: User Story 2 - Override a pipeline service in tests (Priority: P2)

### Story Goal
As a tester, I can supply `RendererServices` with a fake `PaginationService` and verify the renderer uses it.

### Independent Test Criteria
- Use `RendererServices.ForTests().With(pagination: fake)` when creating the renderer
- Assert calls and behavior route through the fake

### Implementation Tasks

- [x] T016 [US2] Create test for RendererServices service override using observable behavior in src/NetHtml2Pdf.Test/Renderer/RendererServicesTests.cs
- [x] T017 [US2] Create test for RendererComposition with service overrides using observable behavior in src/NetHtml2Pdf.Test/Renderer/RendererCompositionTests.cs
- [x] T018 [US2] Create test for PaginationService override using observable behavior in src/NetHtml2Pdf.Test/Renderer/PaginationServiceOverrideTests.cs

---

## Phase 5: User Story 3 - Public API remains unchanged (Priority: P3)

### Story Goal
As a consumer, I can continue using `PdfBuilder` exactly as before, with no changes to my existing code.

### Independent Test Criteria
- Use `new PdfBuilder()` and verify it works exactly as before
- All existing functionality preserved

### Implementation Tasks

- [x] T019 [US3] Refactor PdfBuilder constructors to use composition root in src/NetHtml2Pdf/PdfBuilder.cs
- [x] T020 [US3] Refactor PdfRendererFactory to delegate to composition root in src/NetHtml2Pdf/PdfRendererFactory.cs
- [x] T021 [US3] Create test for PdfBuilder backward compatibility using observable behavior in src/NetHtml2Pdf.Test/PdfBuilderCompatibilityTests.cs
- [x] T022 [US3] Create test for PdfRendererFactory delegation using observable behavior in src/NetHtml2Pdf.Test/Renderer/PdfRendererFactoryTests.cs

---

## Phase 6: Polish & Cross-Cutting Concerns

### Story Goal
Ensure all edge cases are handled and the refactor is complete.

### Independent Test Criteria
- All edge cases from spec are handled
- No breaking changes

### Implementation Tasks

- [x] T023 Create test for null RendererServices overrides using observable behavior in src/NetHtml2Pdf.Test/Renderer/EdgeCaseTests.cs
- [x] T024 Create test for logging disabled scenario using observable behavior in src/NetHtml2Pdf.Test/Renderer/LoggingTests.cs

---

## Parallel Execution Examples

### Phase 2 (Foundational)
```bash
# Run in parallel
T004 & T005 & T006 & T007
```

### Phase 3 (User Story 1)
```bash
# Run in parallel
T008 & T009 & T010 & T011 & T012 & T013 & T014 & T015
```

### Phase 4 (User Story 2)
```bash
# Run in parallel
T016 & T017 & T018
```

### Phase 5 (User Story 3)
```bash
# Run in parallel
T019 & T020 & T021 & T022
```

---

## Implementation Notes

- **Transient Lifetime**: All services are created fresh for each render operation
- **Public API**: No breaking changes to existing constructors
- **Test Overrides**: Use `InternalsVisibleTo` to access internal composition classes
- **Performance**: Maintain current rendering performance with minimal overhead
- **Thread Safety**: Transient instances prevent shared mutable state issues

## Validation Checklist

- [ ] All constructors accept only explicit dependencies
- [ ] No "create defaults" logic in constructors
- [ ] Composition root owns all default creation
- [ ] Service overrides work in tests
- [ ] Public API remains unchanged
- [ ] All existing tests pass
- [ ] Performance is maintained
- [ ] Thread safety is improved
