# NetHtml2Pdf

A .NET library for converting HTML content to PDF using QuestPDF with extensible architecture.

## Overview

NetHtml2Pdf is a modern, extensible HTML-to-PDF conversion library that supports various HTML elements including tables, paragraphs, line breaks, divisions, and sections. The library is designed to ensure maintainability, testability, and extensibility. The goal is to create a pure .NET cross-platform library that runs on Windows and Linux without requiring GDI+ dependencies.

## Defaults
- Page size: Letter (8.5×11 in)
- Margins: 1 inch on all sides
- Font: Inter (bundled) for deterministic rendering
- CSS: minimal inline subset (e.g., color, background-color, border width, padding/margin, font-size, font-weight); unsupported properties are ignored
- Unsupported elements: ignored, inner text preserved
- Pagination: explicit via builder (no auto-pagination)

## Quick Start

### 1. Install the Package

```bash
dotnet add package NetHtml2Pdf
```

### 2. Basic Usage (single page)

```csharp
using NetHtml2Pdf;

var html = @"
    <section>
        <h1>My Document</h1>
        <p>This is a sample document.</p>
    </section>
    
    <table>
        <thead>
            <tr><th>Name</th><th>Age</th></tr>
        </thead>
        <tbody>
            <tr><td>Alice</td><td>30</td></tr>
            <tr><td>Bob</td><td>25</td></tr>
        </tbody>
    </table>
";

var converter = new HtmlConverter();
var pdfBytes = await converter.ConvertToPdfBytes(html);
await File.WriteAllBytesAsync("output.pdf", pdfBytes);
```

### 3. Explicit Pagination (multi-page)

```csharp
using NetHtml2Pdf;

var builder = new PdfDocumentBuilder();
builder.AddPdfPage("<section><h1>Page 1</h1><p>Hello</p></section>");
builder.AddPdfPage("<section><h1>Page 2</h1><p>World</p></section>");

var pdf = await builder.RenderAsync();
await File.WriteAllBytesAsync("multi-page.pdf", pdf);
```

### 4. Headers and Footers

Render a header and footer on every page using HTML fragments (same supported subset as body):

```csharp
using NetHtml2Pdf;

var builder = new PdfDocumentBuilder();
builder.SetHeaderHtml("<div><strong>HeaderText</strong></div>");
builder.SetFooterHtml("<div><em>FooterText</em></div>");

builder.AddPdfPage("<section><h1>Page 1</h1><p>Alpha</p></section>");
builder.AddPdfPage("<section><h1>Page 2</h1><p>Beta</p></section>");

var pdf = await builder.RenderAsync();
await File.WriteAllBytesAsync("with-header-footer.pdf", pdf);
```

#### Styling notes
- Supported inline CSS in header/footer is the same minimal subset as body:
  - text-align, padding, margin, color, font-size, font-weight/font-style
- Use <strong>/<em> for bold/italic; unsupported properties (e.g., display, position) are ignored.
- Height adapts to content; keep header/footer concise for predictable layout.

Example (right-aligned header, centered footer):

```csharp
builder.SetHeaderHtml("<div style='text-align: right; padding: 8px; color: #333; font-size: 12px;'>My Company</div>");
builder.SetFooterHtml("<div style='text-align: center; padding: 6px; color: #666; font-size: 10px;'><em>Confidential</em></div>");
```

### Console Samples

- `src/NetHtml2Pdf.TestConsole/samples/headings.html`
- `src/NetHtml2Pdf.TestConsole/samples/table.html`

Run them with the console app:

```bash
cd src/NetHtml2Pdf.TestConsole
dotnet run samples/headings.html
dotnet run samples/table.html
```

## Dependencies

- **QuestPDF**: PDF generation library
- **AngleSharp**: HTML parsing library
- **.NET 8.0**: Target framework

## Building and Testing

```bash
# Clone the repository
git clone <repository-url>
cd NetHtml2Pdf

# Restore dependencies
dotnet restore

# Build all projects
dotnet build

# Run tests
dotnet test

# Run console application
cd src/NetHtml2Pdf.TestConsole
dotnet run example.html
```

### Testing Strategy

- Most unit tests live in `src/NetHtml2Pdf.Renderer.Test` (rendering scope) and `src/NetHtml2Pdf.Parser.Test` (parsing scope).
- `src/NetHtml2Pdf.Test` contains a small number of integration tests across parsing + rendering.
- Tests use xUnit + Shouldly; PDF assertions use PdfPig for text extraction and measurement.

### Minimal CI / Local verification

Run a lightweight CI script that builds, tests, and optionally packs the NuGet:

```bash
powershell -ExecutionPolicy Bypass -File .\build\ci.ps1
# Or include packing
powershell -ExecutionPolicy Bypass -File .\build\ci.ps1 -Pack
```

Artifacts are written to the `nuget/` folder when packing.

### Versioning & Releases

- Semantic Versioning (MAJOR.MINOR.PATCH)
- Deterministic builds with SourceLink embedded
- Pack locally:

```bash
powershell -ExecutionPolicy Bypass -File .\build\build-nuget.ps1 -Configuration Release
```

- Publish to NuGet (requires API key):

```bash
powershell -ExecutionPolicy Bypass -File .\build\publish-nuget.ps1 -ApiKey <NUGET_API_KEY>
```

#### Setting versions

- `VersionPrefix` is defined in `src/NetHtml2Pdf/NetHtml2Pdf.csproj` (e.g., `0.1.0`).
- Use `VersionSuffix` during packing for pre-releases:

```bash
powershell -ExecutionPolicy Bypass -File .\build\build-nuget.ps1 -Configuration Release -VersionSuffix beta.1
```

This produces `NetHtml2Pdf.<VersionPrefix>-beta.1.nupkg` in `nuget/`.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes following good coding practices
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.