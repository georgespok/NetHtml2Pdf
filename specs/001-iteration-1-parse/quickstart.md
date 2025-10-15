# Quickstart: Iteration 1 – Core HTML parsing & rendering

**Branch**: 001-iteration-1-parse | **Date**: 2025-10-14 | **Phase**: 1 Complete

## Purpose

Guide contributors through restoring dependencies, running regression suites, and validating the PdfBuilder façade, CSS shorthand support, and multi-page rendering delivered in Iteration 1.

## Prerequisites

- .NET 8.0 SDK  
- Windows or Linux development environment  
- PowerShell 7+ (or `pwsh` on Linux/macOS)  
- Repository cloned to `C:/Projects/Html2Pdf`

## Setup & Verification

```bash
cd C:/Projects/Html2Pdf
dotnet restore src/NetHtml2Pdf.sln
dotnet build   src/NetHtml2Pdf.sln
dotnet test    src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj
```

All commands must complete without warnings (CI treats build warnings as errors).

## Running Targeted Suites

| Scenario | Command |
|----------|---------|
| PdfBuilder unit suite | `dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter FullyQualifiedName~PdfBuilderTests` |
| Parser CSS shorthand tests | `dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter FullyQualifiedName~CssStyleUpdater` |
| Renderer table contracts | `dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter Contract=TableBordersAlignment` |
| Fallback logging contracts | `dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter Contract=FallbackUnsupportedTag` |
| Ultimate “full document” smoke | `dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter FullyQualifiedName~FullDocument_Rendering_SmokeTest` |

## Manual PdfBuilder Usage

```csharp
using NetHtml2Pdf;

var pdfBytes = new PdfBuilder()
    .SetHeader("<div style='text-align:center;font-size:12px;'>Quarterly Report</div>")
    .AddPage("<h1>Summary</h1><p>Iteration 1</p>")
    .AddPage("<h2>Details</h2><table>...</table>")
    .SetFooter("<div style='text-align:right;font-size:10px;'>Page {{pageNumber}}</div>")
    .Build();

File.WriteAllBytes("output.pdf", pdfBytes);
```

Key behaviors:
- `Reset()` allows builder reuse in scoped DI scenarios.
- `AddPage()` must be called at least once before `Build()`.
- Validation throws `ArgumentNullException` / `ArgumentException` with meaningful parameter names.

## Integration Console (optional)

```
cd C:/Projects/Html2Pdf/src/NetHtml2Pdf.TestConsole
dotnet run --scenario ultimate-html
```

The console saves PDFs to `%TEMP%` and logs the path; inspect output with any PDF viewer.

## Validation Checklists

### CSS Shorthand
- `margin` expands correctly for 1–4 value forms.
- `border` parses width/style/color permutations (including keyword widths).
- Invalid shorthands emit warnings and leave existing values untouched.

### PdfBuilder API
- Fluent chaining (`SetHeader`, `SetFooter`, `AddPage`, `Build`) compiles.
- `Build()` without pages throws `InvalidOperationException`.
- Headers/footers repeat across all pages and shrink page content when tall.

### Fallback & Telemetry
- Unsupported tags render as text and appear in warning logs.  
- `PdfRenderSnapshot` captures render duration, output size, and fallback counts.

## Support Resources

- `specs/001-iteration-1-parse/research.md` – technology rationale.  
- `specs/001-iteration-1-parse/data-model.md` – entity relationships.  
- `specs/001-iteration-1-parse/contracts/` – acceptance scenarios.  
- Constitution (`.specify/memory/constitution.md`) – non-negotiable delivery rules.

**Quickstart complete** – proceed to regression or manual validation as necessary.
