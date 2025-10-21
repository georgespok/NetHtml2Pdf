# Quickstart: Extended Formatting Contexts (Phase 4)

## Prerequisites
- .NET 8 SDK installed.
- Existing NetHtml2Pdf solution restored (`dotnet restore`).
- QuestPDF license configured (Community acceptable for development).

## Build & Test
```powershell
dotnet build NetHtml2Pdf.sln
dotnet test NetHtml2Pdf.sln
```

## Enabling Formatting Contexts
```csharp
var rendererOptions = new RendererOptions
{
    EnablePagination = true,
    EnableQuestPdfAdapter = true,
    EnableInlineBlockContext = true,      // new flag
    EnableTableContext = true,            // new flag
    EnableFlexContext = true,             // preview flag
    EnablePaginationDiagnostics = true,
    EnableLayoutDiagnostics = true,
    FontPath = string.Empty               // use environment fonts in dev
};

var builder = new PdfBuilder(rendererOptions, logger)
    .EnablePagination()
    .EnableQuestPdfAdapter()
    .EnablePaginationDiagnostics();

var pdf = builder
    .AddPage(htmlContent)
    .Build();
```

### Flag Notes
- `EnableInlineBlockContext`: Enables inline-block sizing via the layout engine.
- `EnableTableContext`: Enables table section handling (header/footer repetition). Requires pagination flag.
- `EnableTableBorderCollapse`: Optional future enhancement; Phase 4 treats collapse as separate.
- `EnableFlexContext`: Preview only; enforces `flex-wrap: nowrap` and logs downgrades.
  - Diagnostic event names: `FormattingContext.Flex` (measured), `FlexContext.Downgrade` (fallbacks)

## Diagnostics
- Enable `RendererOptions.EnablePaginationDiagnostics` or `PdfBuilder.EnablePaginationDiagnostics()` to emit structured logs such as `FormattingContext.InlineBlock`, `TableContext.HeaderRepeated`, and `FlexContext.Downgrade`.
- Logs are routed through the `ILogger` passed to `PdfBuilder` or `RendererContext`.

## Validation Checklist
1. Run unit tests covering new contexts (see `NetHtml2Pdf.Test/Layout/FormattingContexts`).
2. Execute regression fixtures with flags off/on to confirm parity.
3. Benchmark using the 50-page lorem fixture to ensure render time delta <=5%.
4. Review logs for downgrade messages when unsupported features are encountered.

## Rollback
- Disable new flags (`false`) to revert to the legacy composer path without code changes.
- Remove new assemblies only after ensuring no consumers depend on the flagged path.


