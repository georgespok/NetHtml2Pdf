# Quickstart: Iteration 1 - Core HTML parsing & rendering

**Branch**: 001-iteration-1-parse | **Date**: 2025-01-27 | **Phase**: 1 Complete

## Overview

This quickstart guide provides step-by-step instructions for validating the Iteration 1 HTML parsing and rendering functionality. It covers local development setup, testing procedures, and cross-platform validation.

## Prerequisites

- .NET 8.0 SDK installed
- Windows or Linux development environment
- Git repository cloned locally
- PowerShell (Windows) or Bash (Linux) terminal

## Local Development Setup

### 1. Restore Dependencies

```bash
# Navigate to project root
cd C:/Projects/Html2Pdf

# Restore solution dependencies
dotnet restore src/NetHtml2Pdf.sln
```

### 2. Build Solution

```bash
# Build the solution
dotnet build src/NetHtml2Pdf.sln

# Verify no build errors
echo "Build completed successfully"
```

### 3. Run Unit Tests

```bash
# Run all tests
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj

# Run specific test categories
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter Category=Core
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter Category=Parser
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter Category=Renderer
```

## Contract Testing

### 1. Core Paragraphs Contract

```bash
# Run core paragraphs contract tests
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter Contract=CoreParagraphs

# Expected output: All tests pass, PDF generated with proper paragraph formatting
```

**Validation Checklist**:
- [ ] Paragraphs render with proper spacing
- [ ] Bold and italic text render correctly
- [ ] CSS classes apply background colors
- [ ] Nested elements maintain structure

### 2. Table Borders and Alignment Contract

```bash
# Run table contract tests
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter Contract=TableBordersAlignment

# Expected output: All tests pass, PDF generated with proper table formatting
```

**Validation Checklist**:
- [ ] Table borders render correctly
- [ ] Cell alignment (left, center, right) works
- [ ] Vertical alignment (top, middle, bottom) works
- [ ] Background colors apply to cells
- [ ] Border collapse behavior works

### 3. Fallback Unsupported Tag Contract

```bash
# Run fallback contract tests
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter Contract=FallbackUnsupportedTag

# Expected output: All tests pass, PDF generated with fallback text
```

**Validation Checklist**:
- [ ] Unsupported elements render as plain text
- [ ] Warning logs generated for each fallback
- [ ] Document structure preserved
- [ ] Supported elements render normally

## Integration Testing

### 1. Run Integration Test Console

```bash
# Navigate to test console project
cd src/NetHtml2Pdf.TestConsole

# Run integration test console
dotnet run --scenario ultimate-html

# Expected output: PDF generated successfully with comprehensive HTML content
```

### 2. Validate Integration Output

**Check generated PDF**:
- [ ] PDF file created in output directory
- [ ] All supported HTML elements render correctly
- [ ] CSS styling applied properly
- [ ] Tables render with borders and alignment
- [ ] Fallback elements render as plain text

## Cross-Platform Validation

### Windows Validation

```powershell
# Run regression test suite on Windows
dotnet test src/NetHtml2Pdf.sln --filter Iteration1

# Run integration test console on Windows
dotnet run --project src/NetHtml2Pdf.TestConsole/NetHtml2Pdf.TestConsole.csproj --scenario ultimate-html

# Capture Windows output
$windowsOutput = Get-ChildItem -Path "output" -Filter "*.pdf" | Select-Object -First 1
Write-Host "Windows PDF: $($windowsOutput.FullName)"
```

### Linux Validation

```bash
# Run regression test suite on Linux
dotnet test src/NetHtml2Pdf.sln --filter Iteration1

# Run integration test console on Linux
dotnet run --project src/NetHtml2Pdf.TestConsole/NetHtml2Pdf.TestConsole.csproj --scenario ultimate-html

# Capture Linux output
linux_output=$(find output -name "*.pdf" | head -1)
echo "Linux PDF: $linux_output"
```

### Cross-Platform Comparison

```bash
# Compare PDF outputs (if both platforms available)
# Note: Visual/functional equivalence expected, not byte-level identity

# Check file sizes are similar
echo "Windows PDF size: $(stat -c%s windows_output.pdf) bytes"
echo "Linux PDF size: $(stat -c%s linux_output.pdf) bytes"

# Visual comparison criteria:
# - Content should be identical
# - Layout should be identical
# - Styling should be identical
# - Metadata differences are acceptable
```

## Performance Validation

### 1. Timing Validation

```bash
# Run performance tests
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter Category=Performance

# Expected results:
# - Parse time: < 200ms for typical documents
# - Render time: < 800ms for typical documents
# - Total time: < 1.5 seconds end-to-end
```

### 2. Memory Validation

```bash
# Monitor memory usage during testing
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter Category=Performance --logger "console;verbosity=detailed"

# Expected results:
# - Memory usage: < 20MB for typical documents
# - Peak memory: < 40MB during processing
# - No memory leaks detected
```

## Dependency Validation

### 1. Managed Dependencies Audit

```bash
# Run dependency audit
dotnet list src/NetHtml2Pdf/NetHtml2Pdf.csproj package --include-transitive

# Expected results:
# - All dependencies are managed .NET libraries
# - No GDI+ or native rendering libraries
# - QuestPDF and AngleSharp are primary dependencies
```

### 2. Cross-Platform Build Validation

```bash
# Verify solution builds on both platforms
dotnet build src/NetHtml2Pdf.sln --configuration Release

# Expected results:
# - Build succeeds without errors
# - No platform-specific dependencies
# - All assemblies generated successfully
```

## Troubleshooting

### Common Issues

**Build Errors**:
- Ensure .NET 8.0 SDK is installed
- Run `dotnet restore` before building
- Check for missing package references

**Test Failures**:
- Verify all dependencies are restored
- Check test data files are present
- Ensure output directories are writable

**Cross-Platform Differences**:
- Accept metadata differences (creation dates, producer strings)
- Focus on visual/functional equivalence
- Document any significant differences

### Debug Mode

```bash
# Run tests in debug mode for detailed output
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --configuration Debug --logger "console;verbosity=detailed"

# Run integration console in debug mode
dotnet run --project src/NetHtml2Pdf.TestConsole/NetHtml2Pdf.TestConsole.csproj --configuration Debug --scenario ultimate-html
```

## Validation Checklist

### Pre-Release Validation

- [ ] All unit tests pass
- [ ] All contract tests pass
- [ ] Integration test console runs successfully
- [ ] Cross-platform validation completed
- [ ] Performance requirements met
- [ ] Memory usage within limits
- [ ] Dependency audit passed
- [ ] No build warnings or errors

### Documentation Validation

- [ ] Research.md completed
- [ ] Data model documented
- [ ] Contracts defined
- [ ] Quickstart guide validated
- [ ] All clarifications resolved

## Next Steps

After completing this quickstart validation:

1. **Phase 2**: Run `/speckit.tasks` to generate implementation tasks
2. **Phase 3**: Execute tasks in order (T001 → T050)
3. **Phase 4**: Complete implementation and testing
4. **Phase 5**: Final validation and release

## Scope Additions (2025-10-09)

### PdfBuilder API Usage

#### Basic Usage (Single Page)

```csharp
using NetHtml2Pdf;

// Create builder instance
var builder = new PdfBuilder();

// Generate single-page PDF
var html = "<h1>Hello World</h1><p>This is a test.</p>";
var pdfBytes = builder.AddPage(html).Build();

// Save to file
File.WriteAllBytes("output.pdf", pdfBytes);
```

#### Multi-Page PDF

```csharp
using NetHtml2Pdf;

var builder = new PdfBuilder();

// Add multiple pages
var pdf = builder
    .AddPage("<h1>Page 1</h1><p>First page content</p>")
    .AddPage("<h1>Page 2</h1><p>Second page content</p>")
    .AddPage("<h1>Page 3</h1><p>Third page content</p>")
    .Build();

File.WriteAllBytes("multi-page.pdf", pdf);
```

#### Multi-Page with Headers and Footers

```csharp
using NetHtml2Pdf;

var builder = new PdfBuilder();

// Build PDF with headers and footers
var pdf = builder
    .SetHeader("<h1 style='text-align: center'>Document Header</h1>")
    .AddPage("<h2>Introduction</h2><p>Welcome to the document.</p>")
    .AddPage("<h2>Chapter 1</h2><p>First chapter content.</p>")
    .AddPage("<h2>Chapter 2</h2><p>Second chapter content.</p>")
    .SetFooter("<p style='text-align: center'>© 2025 Company Name</p>")
    .Build();

File.WriteAllBytes("with-headers-footers.pdf", pdf);
```

#### Builder Reuse with Reset

```csharp
using NetHtml2Pdf;

var builder = new PdfBuilder();

// First PDF
var pdf1 = builder
    .AddPage("<p>Document 1</p>")
    .Build();

// Second PDF (reuse builder)
var pdf2 = builder
    .Reset()                          // Clear previous state
    .AddPage("<p>Document 2</p>")
    .Build();

// Third PDF with headers
var pdf3 = builder
    .Reset()
    .SetHeader("<h1>Report Header</h1>")
    .AddPage("<p>Report content</p>")
    .Build();
```

#### CSS Shorthand Properties

```csharp
using NetHtml2Pdf;

var builder = new PdfBuilder();

// Margin shorthand (1-4 values)
var html = """
    <div style="margin: 10px">All sides: 10px</div>
    <div style="margin: 10px 20px">Vertical: 10px, Horizontal: 20px</div>
    <div style="margin: 10px 20px 30px">Top: 10px, Horizontal: 20px, Bottom: 30px</div>
    <div style="margin: 10px 20px 30px 40px">Top: 10px, Right: 20px, Bottom: 30px, Left: 40px</div>
    
    <div style="border: 1px solid black">Border shorthand</div>
    <div style="border: 2px dashed red">Dashed red border</div>
    <div style="border: solid #000 3px">Any order supported</div>
    """;

var pdf = builder.AddPage(html).Build();
```

#### Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using NetHtml2Pdf;

// Configure services
var services = new ServiceCollection();
services.AddPdfBuilder();             // NEW: Register PdfBuilder

var serviceProvider = services.BuildServiceProvider();

// Resolve builder from DI
var builder = serviceProvider.GetRequiredService<IPdfBuilder>();

// Use builder
var pdf = builder
    .AddPage("<p>Injected builder</p>")
    .Build();
```

### Migration Guide (Breaking Changes)

#### API Changes

**OLD (HtmlConverter)**:
```csharp
// Legacy API (NO LONGER AVAILABLE)
var converter = new HtmlConverter();
var pdf = converter.ConvertToPdf("<p>Content</p>");
```

**NEW (PdfBuilder)**:
```csharp
// New API
var builder = new PdfBuilder();
var pdf = builder.AddPage("<p>Content</p>").Build();
```

#### DI Registration Changes

**OLD (AddHtml2Pdf)**:
```csharp
// Legacy registration (DEPRECATED)
services.AddHtml2Pdf();
```

**NEW (AddPdfBuilder)**:
```csharp
// New registration
services.AddPdfBuilder();
```

#### Interface Changes

**OLD (IHtmlConverter)**:
```csharp
// Legacy interface (REMOVED)
IHtmlConverter converter;
byte[] pdf = converter.ConvertToPdf(html);
```

**NEW (IPdfBuilder)**:
```csharp
// New interface
IPdfBuilder builder;
byte[] pdf = builder.AddPage(html).Build();
```

#### Test File Migration

1. **Rename test file**: `HtmlConverterTests.cs` → `PdfBuilderTests.cs`

2. **Update test instantiations**:
```csharp
// OLD
var converter = new HtmlConverter();
var pdf = converter.ConvertToPdf(html);

// NEW
var builder = new PdfBuilder();
var pdf = builder.AddPage(html).Build();
```

3. **Update DI tests**:
```csharp
// OLD
services.AddHtml2Pdf();
var converter = serviceProvider.GetService<IHtmlConverter>();

// NEW
services.AddPdfBuilder();
var builder = serviceProvider.GetService<IPdfBuilder>();
```

### Testing Scope Additions

#### CSS Shorthand Tests

```bash
# Run CSS shorthand tests
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter "FullyQualifiedName~CssStyleUpdater&FullyQualifiedName~Shorthand"

# Run margin shorthand integration tests
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter "FullyQualifiedName~Margin"

# Run border shorthand integration tests
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter "FullyQualifiedName~Border"
```

#### PdfBuilder API Tests

```bash
# Run PdfBuilder unit tests
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter "FullyQualifiedName~PdfBuilderTests"

# Run multi-page tests
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter "FullyQualifiedName~MultiPage"

# Run header/footer tests
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter "FullyQualifiedName~Header|FullyQualifiedName~Footer"
```

#### Integration Tests

```bash
# Run all scope addition integration tests
dotnet test src/NetHtml2Pdf.Test/NetHtml2Pdf.Test.csproj --filter "FullyQualifiedName~PdfBuilder"

# Verify all tests pass (including updated tests)
dotnet test src/NetHtml2Pdf.sln
```

### Validation Checklist (Scope Additions)

#### CSS Shorthand Validation
- [ ] Margin shorthand 1-value pattern works
- [ ] Margin shorthand 2-value pattern works
- [ ] Margin shorthand 3-value pattern works
- [ ] Margin shorthand 4-value pattern works
- [ ] Border shorthand with all components works
- [ ] Border shorthand with partial components works
- [ ] Invalid shorthand values emit warnings
- [ ] Shorthand/longhand cascade follows source order

#### PdfBuilder API Validation
- [ ] Single-page PDF generation works
- [ ] Multi-page PDF generation works
- [ ] Headers render on all pages
- [ ] Footers render on all pages
- [ ] Reset() clears all state
- [ ] All methods support fluent chaining
- [ ] Build() without pages throws exception
- [ ] Null/empty validation works

#### Migration Validation
- [ ] All test files updated (HtmlConverterTests → PdfBuilderTests)
- [ ] All HtmlConverter instantiations replaced
- [ ] All ConvertToPdf calls replaced
- [ ] DI registration updated (AddHtml2Pdf → AddPdfBuilder)
- [ ] All tests pass after migration
- [ ] No legacy API references remain

---

## Support

For issues or questions:
- Check the research.md for technology decisions
- Review contracts for expected behavior
- Consult the data model for entity specifications
- Refer to the constitution for development standards
- Review pdfbuilder-api.md contract for API details
- Check css-margin-shorthand.md and css-border-shorthand.md for CSS parsing rules

---

**Quickstart Complete**: All validation procedures (including scope additions) documented and ready for execution.