# NetHtml2Pdf: Pagination & Renderer Adapter Architecture

> Related reading: `docs/architecture-layout-pipeline.md`, `specs/005-implement/spec.md`, `specs/005-implement/data-model.md`, `specs/005-implement/research.md`

## 1. Goals & Scope

Phase 3 extends the layout pipeline to deliver **page-aware rendering** while keeping QuestPDF isolated behind an adapter seam. The objectives are:

- Slice layout fragments produced in Phase 2 into **per-page trees** driven by `PageConstraints`.
- Surface a **pluggable renderer adapter** (`IRendererAdapter`) so PdfRenderer orchestrates layout → pagination → adapter without referencing QuestPDF directly.
- Preserve parity with the legacy composers via feature flags (`EnablePagination`, `EnableQuestPdfAdapter`) until the new path is production ready.
- Provide diagnostics and error handling consistent with the contract doc (`contracts/pagination-adapter-contract.md`).

This document explains the moving pieces, how they fit together, and how to extend or troubleshoot the pagination/adapter stack.

## 2. High-Level Pipeline

```
PdfBuilder
  └─ PdfRenderer
       ├─ LayoutEngine          (Phase 2)
       ├─ PaginationService     (Phase 3)
       └─ IRendererAdapter      (QuestPdfAdapter in Phase 3)
```

1. **DOM Parsing** – same as Phase 2; produces `DocumentNode` with computed `CssStyleMap`.
2. **Layout Engine** – converts DOM nodes into `LayoutFragment` trees (geometry only).
3. **Pagination Service** – partitions fragments into `PaginatedDocument` → `PageFragmentTree` slices using `PageConstraints`.
4. **Renderer Adapter** – renders each page through an adapter (QuestPDF for now) to produce the final PDF bytes.
5. **Legacy Fallback** – when either flag is off, PdfRenderer bypasses pagination and adapters, delegating to Block/Inline composers directly.

## 3. Core Components

### 3.1 PaginationService (`src/NetHtml2Pdf/Layout/Pagination/PaginationService.cs`)

- **Inputs**
  - `IReadOnlyList<LayoutFragment>` – top-level fragments from the layout engine.
  - `PageConstraints` – width, height, margins, header/footer bands, derived content size.
  - `PaginationOptions` – enables structured diagnostics.
  - `ILogger?` – optional diagnostics sink.
- **Outputs**
  - `PaginatedDocument` containing ordered `PageFragmentTree` instances.
  - `PaginationWarning` list surfaced when non-fatal issues occur (e.g., unsupported fragment types).
- **Responsibilities**
  - Enforce keep-together and keep-with-next metadata.
  - Split fragments crossing page boundaries while preserving geometry (`FragmentSliceKind.Start/Continuation/End`).
  - Maintain carry-over metadata (`CarryPageLink`) so adapters know when a fragment spans pages.
  - Throw `PaginationException` when a keep-together fragment cannot fit (`KeepTogetherOverflow`), returning no document.
- **Diagnostics**
  - `PaginationDiagnostics.LogPageCreated` – emitted when a page is finalized.
  - `PaginationDiagnostics.LogFragmentSplit` – emitted when a fragment is split (only when diagnostics flag is on).

### 3.2 Renderer Adapter Layer (`src/NetHtml2Pdf/Renderer/Adapters/`)

- **IRendererAdapter**
  - `BeginDocument` – initialize adapter state / register fonts.
  - `Render` – render a `PageFragmentTree`.
  - `EndDocument` – flush and return final byte array.
  - Contract: Begin once, render pages in order, end once.
- **RendererAdapterFactory**
  - Selects the adapter based on `RendererOptions`.
  - Throws if `EnableQuestPdfAdapter` is true without pagination (`InvalidOperationException`).
  - Defaults to `NullRendererAdapter` when the adapter pipeline is disabled.
- **QuestPdfAdapter**
  - Materializes fragment trees into QuestPDF constructs.
  - Reuses header/footer fragments across pages.
  - Honors diagnostics flag (logs `QuestPdfAdapter rendered fragment ...` only when `EnablePaginationDiagnostics` is true).
  - Keeps PDF generation encapsulated (no layout logic).
- **NullRendererAdapter**
  - No-op implementation useful for tests or legacy fallback.

### 3.3 PdfRenderer (`src/NetHtml2Pdf/Renderer/PdfRenderer.cs`)

- Decides between **new pipeline** and **legacy composers** based on `RendererOptions`.
- When pipeline is enabled:
  - Uses `LayoutEngine` to gather fragments (only supports nodes covered by Phase 2; falls back to legacy if unsupported).
  - Invokes `PaginationService` with canonical A4 `PageConstraints` (future work: make dynamic).
  - Builds `RendererContext` with options, logger, and header/footer fragments.
  - Streams pages through the adapter.
- Maintains legacy QuestPDF composer path for parity testing (`EnablePagination || EnableQuestPdfAdapter` must both be true to use the adapter).

## 4. Data Structures Refresher

- **`PageConstraints`** – page geometry; ensures content width/height remain non-negative.
- **`PaginatedDocument`**
  - `Pages` – ordered `PageFragmentTree` slices.
  - `PageConstraints` – constraints used for the pass.
  - `Warnings` – non-fatal pagination issues.
- **`PageFragmentTree`**
  - `PageNumber` – 1-based.
  - `ContentBounds` – actual usable area (`ContentWidth`/`ContentHeight`).
  - `Fragments` – list of `FragmentSlice` roots placed on this page.
  - `CarryLink` – continuation metadata between pages.
- **`FragmentSlice`**
  - `SourceFragment` – original `LayoutFragment`.
  - `SliceBounds` – position/size on the page.
  - `SliceKind` – `Full`, `Start`, `Continuation`, `End`.
  - `Children` – nested slices (hierarchical blocks).

See `specs/005-implement/data-model.md` for full tables.

## 5. Feature Flags & Configuration

| Flag | Location | Default | Description |
|------|----------|---------|-------------|
| `EnablePagination` | `RendererOptions` / `PdfBuilder` fluent API | `false` | Enables pagination pass. |
| `EnableQuestPdfAdapter` | `RendererOptions` / `PdfBuilder` | `false` | Uses adapter path; requires pagination flag. |
| `EnablePaginationDiagnostics` | `RendererOptions` / `PdfBuilder` | `false` | Emits structured logs for pagination and adapter. |

**Combination rules**
- Pagination **must** be enabled before adapter; otherwise `InvalidOperationException("QuestPdfAdapter requires pagination.")`.
- When pagination is enabled but adapter is disabled, PdfRenderer paginates but still renders pages through the legacy composers (useful for parity diffing).
- Diagnostics flag is orthogonal; only controls logging.

## 6. Observability

- Pagination logs:
  - `Pagination.PageCreated {pageNumber, remainingContent}`
  - `Pagination.FragmentSplit {nodePath, pageNumber, splitHeight}`
- Adapter logs (QuestPDF):
  - `QuestPdfAdapter rendered fragment at path {NodePath}`
- Logging is suppressed unless `EnablePaginationDiagnostics` is true *and* a logger is supplied in `RendererContext`.
- Tests (`PaginationServiceDiagnosticsTests`, `QuestPdfAdapterTests`) assert that logs appear only when the flag is enabled.

## 7. Error Handling

- `PaginationException` (inherits from `Exception`)
  - Thrown when keep-together fragments overflow the page or other fatal pagination issues occur.
  - Bubbles up through PdfRenderer so builder callers can handle the error.
- `RendererAdapterException` (future extension) – reserved for adapters encountering unsupported fragments.
- All other exceptions fall back to standard .NET behavior (caller receives exception; no silent swallowing).

## 8. Testing Strategy

| Layer | Representative Tests |
|-------|----------------------|
| Pagination | `PaginationServiceBehaviorTests`, `PaginationServiceTests`, new `PaginationServiceDiagnosticsTests` |
| Adapter | `QuestPdfAdapterTests` (behavior + diagnostics gating) |
| Renderer orchestration | `PdfRendererAdapterTests` verifies fragment delivery, ordering, and exception propagation |
| Factory selection | `RendererAdapterFactoryTests` ensures flags select the right adapter |

Testing guidance: Use fragment factories (see unit tests) to create deterministic input; prefer asserting on geometry/carry metadata rather than internal state.

## 9. How to Integrate the New Pipeline

1. Instantiate `PdfBuilder` (or supply `RendererOptions` to downstream callers).
2. Opt-in by calling:
   ```csharp
   var builder = new PdfBuilder(logger)
       .EnablePagination()
       .EnableQuestPdfAdapter()
       .EnablePaginationDiagnostics(false); // optional
   ```
3. Provide HTML pages, header, and footer as before.
4. Handle `PaginationException` to present friendly user messages when content cannot fit within constraints (e.g., update the UI with suggestions).
5. If you need to plug in a different adapter (e.g., custom PDF renderer), implement `IRendererAdapter` and register your factory (future work: DI hook).

### Notes for Consumers

- Fonts: QuestPdfAdapter registers fonts from `RendererOptions.FontPath`. Ensure the file exists or set `UseEnvironmentFonts`.
- Header/Footer reuse: they are parsed once, converted to fragments, and re-rendered on each page.
- Performance: Pagination adds additional passes but remains O(page count). Keep fragment trees small by leveraging layout pooling (future improvement).

## 10. Future Enhancements

- **Dynamic Page Constraints** – allow callers to configure page size/margins via `RendererOptions` instead of hard-coded A4.
- **Multiple Adapters** – expand `RendererAdapterFactory` to accept DI/registration.
- **Column Pagination** – extend `PageConstraints` with column definitions.
- **Diagnostics Export** – optional JSON emitters for visual tooling (flag-guarded).
- **Adapter Contracts** – implement `RendererAdapterException` once real adapters beyond QuestPDF exist.
- **Performance Instrumentation** – capture pagination timings when diagnostics are enabled.

## 11. Quick Reference

- Primary Types
  - Pagination: `PaginationService`, `PageConstraints`, `PaginatedDocument`, `FragmentSlice`, `CarryPageLink`
  - Adapter: `IRendererAdapter`, `RendererAdapterFactory`, `RendererContext`, `QuestPdfAdapter`
  - Orchestrator: `PdfRenderer` (layout + pagination + adapter handshake)
- Key Flags
  - `EnablePagination`, `EnableQuestPdfAdapter`, `EnablePaginationDiagnostics`
- Key Exceptions
  - `PaginationException`
- Key Logs
  - `Pagination.PageCreated`, `Pagination.FragmentSplit`, `QuestPdfAdapter rendered fragment ...`

---

Document path: `docs/pagination-architecture.md`  
Maintainer: Phase 3 Pagination & Renderer Adapter initiative (feature branch `005-implement`)
