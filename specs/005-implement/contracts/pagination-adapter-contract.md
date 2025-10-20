# Contract: Pagination Service & Renderer Adapter

## PaginationService

### Signature
```csharp
PaginatedDocument Paginate(
    IReadOnlyList<LayoutFragment> roots,
    PageConstraints constraints,
    PaginationOptions options,
    ILogger? logger = null);
```

### Preconditions
- `roots` MUST NOT be null or empty.
- All fragments MUST have absolute measurements (Phase 2 requirement).
- `constraints.ContentHeight` MUST be greater than zero.

### Postconditions
- Returns `PaginatedDocument` with `Pages.Count >= 1`.
- Total ordered fragments (considering slice chaining) equals input fragment count.
- `PaginatedDocument.Warnings` populated for non-blocking issues; fatal issues throw `PaginationException`.
- Throws `PaginationException` with code `KeepTogetherOverflow` when a keep-together fragment exceeds `constraints.ContentHeight`.

### Diagnostics
- Emits structured log `Pagination.PageCreated` with `{pageNumber, remainingContent}` at `LogLevel.Debug` when diagnostics enabled.
- Emits structured log `Pagination.FragmentSplit` with `{nodePath, pageNumber, splitHeight}` for each carry-over event.

## IRendererAdapter

### Interface
```csharp
public interface IRendererAdapter
{
    void BeginDocument(PaginatedDocument document, RendererContext context);
    void Render(PageFragmentTree page, RendererContext context);
    byte[] EndDocument(RendererContext context);
}
```

### Contracts
- `BeginDocument` MUST be called exactly once before any `Render` invocation.
- `Render` MUST be called sequentially in ascending `PageNumber`.
- `EndDocument` MUST flush and return final PDF bytes; MUST NOT be called before at least one `Render`.
- Implementations MUST honor `FragmentSlice.SliceKind` to avoid double-rendering.

### QuestPdfAdapter Expectations
- Registers fonts provided via `RendererContext.RendererOptions.FontPath`.
- Translates `FragmentSlice` trees into QuestPDF containers within a single page context.
- Applies `CarryOverMetadata` to add top spacing for continuation fragments (future enhancement flag-guarded).
- Throws `RendererAdapterException` when encountering unsupported fragment kinds while logging `QuestPdfAdapter.UnsupportedFragment`.

## Feature Flags

| Flag | Consumer | Behavior |
|------|----------|----------|
| `EnablePagination` | `PdfRenderer` | When false, bypass pagination and adapters, executing legacy composer pipeline. |
| `EnableQuestPdfAdapter` | `PdfRenderer` | When true with pagination enabled, instantiate adapter via factory; when false, legacy renderer path executes even if pagination ran. |

### Configuration Contract
- Flags default to `false`.
- Flags available on `RendererOptions` and flow from `PdfBuilder.Build` to `PdfRenderer`.
- Combination validation:
  - If `EnableQuestPdfAdapter` true but `EnablePagination` false → throw `InvalidOperationException` with message "QuestPdfAdapter requires pagination".
  - If `EnablePagination` true but `EnableQuestPdfAdapter` false → paginate but use legacy renderer for parity testing.

## Error Codes

| Code | Thrown By | Description |
|------|-----------|-------------|
| `KeepTogetherOverflow` | `PaginationService` | Keep-together fragment exceeds available content height (clarified 2025-10-20). |
| `UnsupportedFragmentKind` | `QuestPdfAdapter` | Encountered fragment type without mapping; adapter should log and throw. |
| `InvalidFlagCombination` | `PdfRenderer` | Feature flags set to incompatible combination (see configuration contract). |

## Test Hooks
- Provide `IRendererAdapterFactory` abstraction returning adapters to ease unit testing (inject mock adapter).
- Pagination unit tests verify:
  - Single fragment across multiple pages produces correct slice chain.
  - Keep-with-next deferment logic.
  - Header/footer band reservation.
