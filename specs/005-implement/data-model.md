# Data Model: Pagination & Renderer Adapter

## Overview
Pagination produces immutable value objects describing how measured layout fragments map onto physical pages. Rendering adapters consume these objects to translate into backend-specific draw calls.

## Entities

### PaginatedDocument
| Field | Type | Description |
|-------|------|-------------|
| `Pages` | `IReadOnlyList<PageFragmentTree>` | Ordered page slices from pagination pass. |
| `PageConstraints` | `PageConstraints` | Constraints used to paginate (width, height, bands). |
| `Warnings` | `IReadOnlyList<PaginationWarning>` | Non-fatal issues encountered (e.g., deferred fragments). |

**Identity**: Hash of `PageConstraints` + source document fingerprint (node path + version).  
**Lifecycle**: Produced once by pagination; consumed (read-only) by renderer adapters.  
**Relationships**: Owns `PageFragmentTree` instances; referenced by diagnostics tooling.

### PageFragmentTree
| Field | Type | Description |
|-------|------|-------------|
| `PageNumber` | `int` | 1-based page index. |
| `ContentBounds` | `Rectangle` | Content box (x, y, width, height) within the physical page. |
| `Fragments` | `IReadOnlyList<FragmentSlice>` | Root-level fragment slices placed on this page. |
| `CarryLink` | `CarryPageLink?` | Link metadata describing fragments continuing to next/prev pages. |

**Identity**: composite (`PaginatedDocument`, `PageNumber`).  
**Lifecycle**: Created during pagination; immutable afterwards.

### FragmentSlice
| Field | Type | Description |
|-------|------|-------------|
| `SourceFragment` | `LayoutFragment` | Reference to the original measured fragment. |
| `SliceBounds` | `Rectangle` | Bounding box on the page (relative to content origin). |
| `Children` | `IReadOnlyList<FragmentSlice>` | Nested slices for hierarchical fragments. |
| `SliceKind` | `FragmentSliceKind` | Full, Start, Continuation, or End of a multi-page fragment. |
| `IsBreakAllowed` | `bool` | Indicates whether further splitting is permitted (after keep-with-next evaluation). |

**Identity**: `SourceFragment.NodePath` + `SliceKind` + `PageNumber`.  
**Lifecycle**: Created during pagination; shared references maintain pointer back to original fragment for adapter reuse.

### CarryPageLink
| Field | Type | Description |
|-------|------|-------------|
| `ContinuesToPage` | `int?` | Next page index if fragment continues. |
| `ContinuesFromPage` | `int?` | Prior page index if fragment started earlier. |
| `RemainingBlockSize` | `float` | Remaining block size to lay out on next page. |

Used for diagnostics and adapter hints (e.g., apply top padding for continuation markers).

### PageConstraints
| Field | Type | Description |
|-------|------|-------------|
| `PageWidth` | `float` | Full page width (points). |
| `PageHeight` | `float` | Full page height (points). |
| `Margin` | `Thickness` | Uniform or per-edge margin values. |
| `HeaderBand` | `float` | Reserved height for header fragments. |
| `FooterBand` | `float` | Reserved height for footer fragments. |
| `ContentHeight` | `float` | Derived `PageHeight - margins - header - footer`. |

**Lifecycle**: Constructed before pagination from `RendererOptions` / `ConverterOptions`. Immutable.

### PaginationWarning
| Field | Type | Description |
|-------|------|-------------|
| `Code` | `PaginationWarningCode` | Enum (e.g., `KeepTogetherOverflow`, `UnsupportedFragment`). |
| `Message` | `string` | User-facing description. |
| `NodePath` | `string?` | Optional link to problem fragment. |

Stored on `PaginatedDocument.Warnings`.

### IRendererAdapter
| Member | Signature | Notes |
|--------|-----------|-------|
| `void Render(PageFragmentTree page, RendererContext context)` | Renders a single page slice into backend document primitives. |
| `void BeginDocument(PaginatedDocument document, RendererContext context)` | Optional setup (fonts, metadata). |
| `void EndDocument(RendererContext context)` | Optional teardown/flushing. |

**Implementations**: `QuestPdfAdapter` (initial). Future adapters must support multi-page iteration and respect carry-over hints.

### RendererContext
| Field | Type | Description |
|-------|------|-------------|
| `RendererOptions` | `RendererOptions` | Provides font path, feature flags, diagnostics toggles. |
| `ILogger?` | `ILogger?` | Optional logging sink for diagnostics. |
| `Header` | `LayoutFragment?` | Header fragment tree (reused each page). |
| `Footer` | `LayoutFragment?` | Footer fragment tree (reused each page). |

**Lifecycle**: Created by PdfRenderer at run start, passed to adapter methods.

## Relationships
- `PdfRenderer` orchestrates layout (`LayoutEngine.Layout`), pagination (`PaginationService`), and adapter (implementing `IRendererAdapter`).
- `PaginationService` uses `PageConstraints` plus `LayoutFragment` roots to produce `PaginatedDocument`.
- `QuestPdfAdapter` iterates `PaginatedDocument.Pages`, building QuestPDF containers per `FragmentSlice`.
- Feature flags influence the orchestrator: when disabled, adapters and pagination are bypassed and legacy composers execute directly.

## State Transitions
1. DOM parsed into `DocumentNode`.
2. `LayoutEngine` returns `LayoutFragment[]`.
3. `PaginationService.Paginate` → `PaginatedDocument`.
4. PdfRenderer loops over pages, calling `IRendererAdapter.Render` per page.
5. Adapter flushes backend document (`EndDocument`) and returns `byte[]`.

Error scenario: Pagination detects overflow keep-together fragment → throws `PaginationException` and aborts; no `PaginatedDocument` emitted.

## Scale Considerations
- `PaginatedDocument` must handle dozens of pages without significant allocations. Use pooling or reuse `FragmentSlice` lists where possible.
- `FragmentSlice` tree depth mirrors layout tree; ensure adapter traversals are streaming to prevent stack overflow.
