# Feature Specification: Phase 3 Pagination & Renderer Adapter

**Feature Branch**: `005-implement`  
**Created**: 2025-10-20  
**Status**: Draft  
**Input**: User description: "Implement next step for layout rendering migration. Phase 3 - Pagination & Renderer Adapter. Goal: Move from \"layout-first, renderer-last\" to a paginated fragment pipeline. Layout already produces fragment trees (Phase 2). Now we need to split those fragments into page-sized slices and hand them to QuestPDF through a clean adapter instead of direct composer calls. That unlocks page-aware features (headers/footers, carry-over fragments, future multi-column support) while keeping render surface swappable. What changes in this phase 1. Pagination pass - Take the fragment tree returned by the layout engine and traverse it with PageConstraints (page size, margins, header/footer bands). - Emit per-page fragment trees, tagging each fragment with the page slice it belongs to. Handle carry-over fragments (e.g., long paragraphs that continue on the next page) by splitting them at the measured positions. - Output format: something like PaginatedDocument -> [PageFragmentTree]. 2. Rendering adapter layer - Define IRendererAdapter (e.g., Render(PageFragmentTree)); implement QuestPdfAdapter to translate fragments into QuestPDF calls. - PdfRenderer becomes orchestration only: DOM --> LayoutEngine --> PaginatedFragments --> RendererAdapter - Remove direct QuestPDF calls from layout contexts. They should only produce geometry; adapters do the drawing. 3. Feature flags - EnablePagination - toggles the new pagination step.- EnableQuestPdfAdapter - switches PdfRenderer from the legacy composers to the adapter path. Keep both flags default-off until parity tests pass"

## Clarifications

### Session 2025-10-20

- Q: When a fragment marked `keep-together` still exceeds the available page height, how should the system respond? -> A: Abort rendering and surface a descriptive pagination error.
- Q: Which observability approach should pagination use when the new pipeline runs? -> A: Emit detailed logs only when diagnostics flag is enabled; otherwise warnings/errors only.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Paginated PDF export (Priority: P1)

As a consumer of NetHtml2Pdf, I can enable the pagination and renderer adapter feature flags so that HTML documents render through the paginated fragment pipeline into PDFs without layout code touching QuestPDF directly.

**Why this priority**: This is the minimal viable flow that proves the migration works end to end and keeps existing PDF output available.

**Independent Test**: Execute PdfRenderer against a representative multi-page DOM fixture with both EnablePagination and EnableQuestPdfAdapter set to true, then validate that the produced PDF matches the legacy baseline while the renderer only depends on IRendererAdapter.

**Acceptance Scenarios**:

1. **Given** a DOM that produces layout fragments taller than a single page and both feature flags set to true, **When** PdfRenderer runs, **Then** it produces a PaginatedDocument with sequential PageFragmentTrees and emits the PDF exclusively through the QuestPdfAdapter.
2. **Given** the same DOM and both feature flags set to false, **When** PdfRenderer runs, **Then** it uses the legacy composer path and emits the same PDF as before this phase.

---

### User Story 2 - Carry-over fragment splitting (Priority: P2)

As a user rendering long paragraphs or tables, I want fragments that do not fit in the available page content area to split cleanly across pages so the document remains readable.

**Why this priority**: Without correct splitting the new pipeline would regress readability for real-world documents.

**Independent Test**: Use a layout fixture whose measured fragment height exceeds the page height and verify that pagination yields two PageFragmentTrees with accurate geometry for the remainder fragments.

**Acceptance Scenarios**:

1. **Given** a fragment whose measured height exceeds the available page body height, **When** the pagination pass runs, **Then** it emits a first slice trimmed to the page boundary and a follow-on slice on the next page with carry-over metadata.
2. **Given** inline elements marked as non-breakable (e.g., headers or captions), **When** pagination evaluates breakpoints, **Then** it defers the entire fragment to the next page instead of splitting inside the protected span.

---

### User Story 3 - Renderer adapter extensibility (Priority: P3)

As a platform maintainer, I want PdfRenderer to delegate drawing through an IRendererAdapter so we can plug in QuestPDF now and support alternative renderers later without reworking layout code.

**Why this priority**: This decoupling justifies the migration investment and prevents future vendor lock-in.

**Independent Test**: Replace QuestPdfAdapter with a test double that records rendering calls and confirm PdfRenderer orchestrates layout, pagination, and adapter interactions without referencing QuestPDF composers.

**Acceptance Scenarios**:

1. **Given** a custom test adapter registered when EnableQuestPdfAdapter is true, **When** PdfRenderer renders a document, **Then** all drawing commands flow through IRendererAdapter.Render(PageFragmentTree) invocations.
2. **Given** EnableQuestPdfAdapter is false while EnablePagination is true, **When** PdfRenderer runs, **Then** it skips adapter usage and retains the legacy QuestPDF composer path to avoid partial migrations.

### Edge Cases

- What happens when header and footer bands consume the entire page height, leaving no content area? Pagination must report a validation error before rendering.
- How does the system handle a fragment taller than multiple pages? Pagination must iterate until the fragment is exhausted and prevent infinite loops by reducing the slice height each pass.
- What happens when a layout fragment declares it must stay with its following sibling (e.g., title + paragraph)? Pagination must honor keep-with-next hints and move both fragments to the next page.
- How does the system handle a keep-together fragment that exceeds page height? Pagination must abort and emit a descriptive error instead of overriding the keep constraint.
- How does the system handle disabled feature flags in staging? PdfRenderer must bypass pagination and adapter layers entirely to avoid mixed-mode bugs.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Pagination pass MUST accept the layout fragment tree and PageConstraints (page size, margins, header height, footer height) as input.
- **FR-002**: Pagination pass MUST output a PaginatedDocument containing an ordered collection of PageFragmentTrees with page metadata.
- **FR-003**: Pagination pass MUST split fragments that exceed the available content height, preserving geometry offsets and linking carry-over slices.
- **FR-004**: Pagination pass MUST respect non-breakable fragment hints (keep-together, keep-with-next) when determining page boundaries.
- **FR-004a**: If a keep-together fragment exceeds the available page height, the system MUST abort pagination with a descriptive error rather than force-splitting it.
- **FR-005**: The system MUST expose IRendererAdapter with a Render(PageFragmentTree) contract and route all drawing operations through it when EnableQuestPdfAdapter is true.
- **FR-006**: The system MUST implement QuestPdfAdapter that maps PageFragmentTree nodes to QuestPDF instructions without accessing layout-time contexts.
- **FR-007**: PdfRenderer MUST orchestrate DOM -> LayoutEngine -> Pagination -> RendererAdapter when EnablePagination is true and fall back to the current composer path otherwise.
- **FR-008**: Feature flags EnablePagination and EnableQuestPdfAdapter MUST default to false, be configurable per render request, and support independent toggling.
- **FR-009**: The migration MUST maintain parity with existing regression baselines when both flags are enabled, with any deviation documented as a known issue.
- **FR-010**: Pagination diagnostics MUST emit detailed structured logs only when a diagnostics flag (e.g., EnablePaginationDiagnostics) is enabled; otherwise restrict output to warnings and errors.

### Process Requirements

- **PR-001**: Core pagination, fragment splitting, and renderer adapter behavior MUST be developed using incremental TDD: introduce one failing test at a time, implement the minimal passing code, and refactor before adding the next test. Trivial scaffolding (constructors, simple properties, passive DTOs) is exempt.
- **PR-002**: Tests MUST exercise observable behavior (e.g., rendered output, pagination results) and MUST NOT rely on reflection-based contract checks of internal types. Reflection APIs (e.g., `Activator.CreateInstance`, `Type.GetType`, `MethodInfo.Invoke`) are explicitly prohibited in test code.

### Key Entities *(include if feature involves data)*

- **PaginatedDocument**: Container returned by the pagination pass, including collection of PageFragmentTrees, total page count, and document-level margin constraints.
- **PageFragmentTree**: Tree of layout fragments scoped to a single page, annotated with slice bounds, carry-over markers, and references back to source fragments for reuse.
- **PageConstraints**: Value object describing page width, height, margins, header band, and footer band fed into pagination calculations.
- **IRendererAdapter**: Interface that abstracts rendering of a PageFragmentTree, enabling QuestPdfAdapter or future implementations.
- **QuestPdfAdapter**: Concrete adapter translating PageFragmentTrees into QuestPDF calls while remaining unaware of layout internals.
- **FeatureFlags**: Configuration holder (e.g., EnablePagination, EnableQuestPdfAdapter) injected into PdfRenderer to select the pipeline.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: 100% of existing PDF regression fixtures render with identical page counts and bounding boxes when both feature flags are enabled.
- **SC-002**: Pagination pass processes a 50-page stress document within 5% of the legacy render time, ensuring no major performance regression.
- **SC-003**: QuestPDF direct composer accesses are reduced to zero within layout-related assemblies when EnableQuestPdfAdapter is true.
- **SC-004**: Feature flag toggles are exposed through configuration so QA can run migration parity suites in both legacy and paginated modes without code changes.
- **SC-005**: Commit/test history demonstrates incremental TDD for pagination, carry-over logic, and adapter orchestration, with no concurrent failing tests outside the active red step.

