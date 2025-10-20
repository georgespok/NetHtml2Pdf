# Quickstart: Pagination & Renderer Adapter (Phase 3)

## Prerequisites
- .NET 8 SDK installed.
- QuestPDF license set to Community (default) or configured before running samples.
- Existing regression fixtures cloned locally (`src/NetHtml2Pdf.TestConsole/samples`).

## Build & Test
```pwsh
dotnet restore NetHtml2Pdf.sln
dotnet build NetHtml2Pdf.sln
dotnet test NetHtml2Pdf.sln
```

## Enable the New Pipeline
Pagination and the renderer adapter are disabled by default. Toggle both flags when building the document (or via `RendererOptions` if you construct `PdfBuilder` manually).

```csharp
var builder = new PdfBuilder(logger: logger)
    .EnablePagination()
    .EnableQuestPdfAdapter()
    .EnablePaginationDiagnostics(false) // optional
    .SetHeader("<header>My header</header>")
    .SetFooter("<footer>Page {{pageNumber}}</footer>");

builder.AddPage(File.ReadAllText("samples/example.html"));

var pdf = builder.Build();
File.WriteAllBytes("output.pdf", pdf);
```

## Diagnostic Flags
- `RendererOptions.EnableLayoutDiagnostics`: emits layout fragment telemetry (Phase 2).
- `RendererOptions.EnablePaginationDiagnostics` (new): dumps pagination spans and carry-over metadata to logs.

Enable verbose logging to inspect pagination decisions:
```csharp
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});
var logger = loggerFactory.CreateLogger<PdfBuilder>();
```

## Validation Checklist
1. Run regression fixtures with legacy mode (flags off) and new pipeline (flags on). Compare PDF byte outputs or use visual diff tooling.
2. Exercise long content sample to confirm carry-over fragments split across pages.
3. Trigger a `keep-together` overflow to verify descriptive pagination exception.
4. Swap QuestPdfAdapter for a test adapter implementing `IRendererAdapter` to validate extensibility.

## Rollback
- Reset both feature flags to `false` to revert to the legacy composer path without code changes.
- Keep pagination and adapter assemblies internal until production parity validated.
