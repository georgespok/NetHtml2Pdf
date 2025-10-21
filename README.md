# NetHtml2Pdf

A .NET library for converting HTML content to PDF using QuestPDF with extensible architecture.

## Overview

NetHtml2Pdf is a modern, extensible HTML-to-PDF conversion library that supports various HTML elements including tables, paragraphs, line breaks, divisions, and sections. The library is designed to ensure maintainability, testability, and extensibility. The goal is to create a pure .NET cross-platform library that runs on Windows and Linux without requiring GDI+ dependencies.

## Defaults
- Font: Inter (bundled) for deterministic rendering
- CSS subset:
  - Inline and class-based: color, font-size, font-weight, font-style, text-align, margin, padding, width/height (blocks only), background-color, border width/color (per-side supported), **display** (block, inline-block, none, flex [preview])
  - Precedence: multiple classes merge in attribute order (later wins) → inline overrides class-derived values
  - Unsupported properties are ignored with structured warnings
- Unsupported elements: ignored, inner text preserved
- Pagination: opt-in auto-pagination (Phase 3) or manual pages via builder
- Style preprocessing: a preprocessing pass computes per-node styles (inheritance, class merge, inline override), resolves percentages to absolute points, and applies vertical margin collapsing semantics.

## Quick Start

### 1. Install the Package

```bash
dotnet add package NetHtml2Pdf
```

### 2. Basic Usage (auto-pagination pipeline)

```csharp
using NetHtml2Pdf;

var html = @"
    <style>
      .city { color: white; margin: 20px; padding: 20px; }
      .inline-box { display: inline-block; width: 150px; background-color: lightblue; margin: 10px; padding: 15px; }
      .hidden { display: none; }
    </style>
    <section>
        <h1>My Document</h1>
        <div class='city' style='color: yellow; background-color: #333; border: 2px solid #444; width: 300; height: 120;'>
            <p>This is a sample document.</p>
        </div>
        
        <!-- CSS Display Examples -->
        <div style='display: block; background-color: lightgreen; padding: 15px; margin: 10px;'>
            Block element - starts new line
        </div>
        
        <div class='inline-box'>Inline-block 1</div>
        <div class='inline-box'>Inline-block 2</div>
        <div class='inline-box'>Inline-block 3</div>
        
        <div class='hidden'>This content is hidden</div>
        
        <div style='display: block; background-color: lightcoral; padding: 15px; margin: 10px;'>
            Another block element
        </div>
    </section>
    
    <table>
        <thead>
            <tr><th>Name</th><th>Age</th></tr>
        </thead>
        <tbody>
            <tr><td style='padding: 8px; border: 1px solid black;'>Alice</td><td>30</td></tr>
            <tr><td style='padding: 8px; border: 1px solid black; background-color: #eee;'>Bob</td><td>25</td></tr>
        </tbody>
    </table>
";

var builder = new PdfBuilder();
var pdfBytes = builder
    .EnablePagination()
    .EnableQuestPdfAdapter()
    .SetHeader("<div style='text-align:right;font-size:12px;'>My Company</div>")
    .SetFooter("<div style='text-align:center;font-size:10px;'>Confidential</div>")
    .AddPage(html)            // Add DOM fragments; auto-pagination will split them
    .Build();

await File.WriteAllBytesAsync("output.pdf", pdfBytes);
```

#### Styling notes
- Supported inline/class CSS in header/footer is the same minimal subset as body:
  - text-align, padding, margin, color, font-size, font-weight/font-style, background-color, border width/color, **display** (block, inline-block, none)
- Height adapts to content; keep header/footer concise for predictable layout.
- **CSS Display Support**:
  - `display: block` - Elements start on new lines and take full width
  - `display: inline-block` - Elements flow side-by-side when space allows, wrap as whole units
  - `display: none` - Elements are completely hidden and occupy no space
  - Unsupported display values (grid, etc.) emit warnings and fallback to HTML semantic defaults

Example (auto-pagination with header and footer):

```csharp
var pdf = new PdfBuilder()
    .EnablePagination()
    .EnableQuestPdfAdapter()
    .SetHeader("<div style='text-align: right; padding: 8px; color: #333; font-size: 12px;'>My Company</div>")
    .SetFooter("<div style='text-align: center; padding: 6px; color: #666; font-size: 10px;'><em>Confidential</em></div>")
    .AddPage(html)
    .Build();
```

### Console Samples

- `src/NetHtml2Pdf.TestConsole/samples/headings.html`
- `src/NetHtml2Pdf.TestConsole/samples/table.html`
- `src/NetHtml2Pdf.Test/samples/display-block.html` - CSS display: block examples
- `src/NetHtml2Pdf.Test/samples/display-inline-block.html` - CSS display: inline-block examples
- `src/NetHtml2Pdf.Test/samples/display-none.html` - CSS display: none examples
- `src/NetHtml2Pdf.Test/samples/display-mixed.html` - Mixed display types
- `src/NetHtml2Pdf.Test/samples/display-spacing.html` - Spacing interactions
- `src/NetHtml2Pdf.Test/samples/display-unsupported.html` - Unsupported values and warnings

Run them with the console app:

```bash
cd src/NetHtml2Pdf.TestConsole
dotnet run samples/headings.html
dotnet run samples/table.html
```

## Features

### CSS Display & Pagination Support
NetHtml2Pdf supports CSS `display` properties and an opt-in pagination pipeline for enhanced layout control:

- **`display: block`** - Elements start on new lines and take full available width
- **`display: inline-block`** - Elements flow side-by-side when space allows, wrap as whole units
- **`display: none`** - Elements are completely hidden and occupy no space
- **Margin Collapsing** - Proper CSS margin collapsing between adjacent block elements
- **Structured Warnings** - Unsupported display values emit warnings and fallback to HTML semantic defaults
- **Spacing Interactions** - Correct handling of margins, padding, and borders with different display types

### Supported HTML Elements
- Tables with headers, rows, and cells
- Paragraphs, headings (h1-h6), divisions, and sections
- Line breaks and text formatting
- Lists (ordered and unordered)
- Images and other inline elements

### CSS Properties
- **Text**: color, font-size, font-weight, font-style, text-align
- **Layout**: margin, padding, width/height (blocks only), **display** (block, inline-block, none, flex [preview])
- **Visual**: background-color, border width/color (per-side supported)
- **Precedence**: Multiple classes merge in attribute order (later wins) → inline overrides class-derived values

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
dotnet run ./samples/example.html
```

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




