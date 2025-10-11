# Contract: PdfBuilder API - Fluent Builder Pattern

**Feature**: Multi-Page PDF Generation with PdfBuilder  
**User Story**: US-MULTI-PAGE  
**Acceptance Criteria**: AC-003.1 through AC-003.12, AC-011.1 through AC-011.16  
**Date**: 2025-10-09

## Purpose

Define the input/output contract for the new `IPdfBuilder` interface and `PdfBuilder` class, replacing the legacy `IHtmlConverter`/`HtmlConverter` API with a fluent builder pattern supporting multi-page PDFs, headers, and footers.

---

## Interface Definition

```csharp
public interface IPdfBuilder
{
    IPdfBuilder Reset();
    IPdfBuilder SetHeader(string html);
    IPdfBuilder SetFooter(string html);
    IPdfBuilder AddPage(string htmlContent);
    byte[] Build(ConverterOptions? options = null);
}
```

**AC**: AC-003.1, AC-003.3

---

## Usage Patterns

### Single-Page PDF (Migration from HtmlConverter)
```csharp
// OLD (HtmlConverter)
var converter = new HtmlConverter();
var pdf = converter.ConvertToPdf("<p>Hello World</p>");

// NEW (PdfBuilder)
var builder = new PdfBuilder();
var pdf = builder.AddPage("<p>Hello World</p>").Build();
```

**AC**: AC-003.12 (migration)

### Multi-Page PDF
```csharp
var builder = new PdfBuilder();
var pdf = builder
    .AddPage("<h1>Page 1</h1><p>Content</p>")
    .AddPage("<h1>Page 2</h1><p>More content</p>")
    .AddPage("<h1>Page 3</h1><p>Final content</p>")
    .Build();
```

**AC**: AC-011.8 (multiple AddPage calls produce corresponding number of pages)

### Multi-Page with Headers and Footers
```csharp
var builder = new PdfBuilder();
var pdf = builder
    .Reset()                                              // Optional: clear state
    .SetHeader("<h1>Document Header</h1>")                // Global header
    .AddPage("<h2>Page 1</h2><p>Content</p>")            // Page 1
    .AddPage("<h2>Page 2</h2><p>Content</p>")            // Page 2
    .SetFooter("<p>© 2025 Company Name</p>")             // Global footer
    .Build();
```

**AC**: AC-011.5 (fluent call sequence), AC-011.6 (headers at top), AC-011.7 (footers at bottom)

### Builder Reuse with Reset
```csharp
var builder = new PdfBuilder();

// First PDF
var pdf1 = builder
    .AddPage("<p>Document 1</p>")
    .Build();

// Second PDF (reuse builder)
var pdf2 = builder
    .Reset()                                              // Clear previous state
    .AddPage("<p>Document 2</p>")
    .Build();
```

**AC**: AC-003.8 (Reset clears all state), AC-011.15 (Reset returns IPdfBuilder)

---

## Method Contracts

### Reset()

**Signature**: `IPdfBuilder Reset()`

**Behavior**:
- Clears all accumulated pages (`_pages` list)
- Sets `_header` to null
- Sets `_footer` to null
- Returns `this` for fluent chaining

**Exceptions**: None

**AC**: AC-003.8, AC-011.15

**Example**:
```csharp
builder.Reset();  // Builder is now in initial state
```

---

### SetHeader(string html)

**Signature**: `IPdfBuilder SetHeader(string html)`

**Behavior**:
- Validates `html` parameter (not null, not empty/whitespace)
- Stores `html` in `_header` field
- Last call wins (replaces previous header)
- Returns `this` for fluent chaining

**Exceptions**:
- `ArgumentNullException`: if `html` is null
- `ArgumentException`: if `html` is empty or whitespace

**AC**: AC-011.2 (defines header content), AC-011.6 (header at top of each page)

**Example**:
```csharp
builder.SetHeader("<h1>My Header</h1>");
builder.SetHeader("<h1>Updated Header</h1>");  // Replaces previous
```

---

### SetFooter(string html)

**Signature**: `IPdfBuilder SetFooter(string html)`

**Behavior**:
- Validates `html` parameter (not null, not empty/whitespace)
- Stores `html` in `_footer` field
- Last call wins (replaces previous footer)
- Returns `this` for fluent chaining

**Exceptions**:
- `ArgumentNullException`: if `html` is null
- `ArgumentException`: if `html` is empty or whitespace

**AC**: AC-011.3 (defines footer content), AC-011.7 (footer at bottom of each page)

**Example**:
```csharp
builder.SetFooter("<p>Page Footer</p>");
builder.SetFooter("<p>Updated Footer</p>");  // Replaces previous
```

---

### AddPage(string htmlContent)

**Signature**: `IPdfBuilder AddPage(string htmlContent)`

**Behavior**:
- Validates `htmlContent` parameter (not null, not empty/whitespace)
- Adds `htmlContent` to `_pages` list
- Does NOT replace previous pages (accumulates)
- Returns `this` for fluent chaining

**Exceptions**:
- `ArgumentNullException`: if `htmlContent` is null
- `ArgumentException`: if `htmlContent` is empty or whitespace

**AC**: AC-003.4, AC-003.5, AC-011.1 (accumulates pages)

**Example**:
```csharp
builder.AddPage("<p>Page 1</p>")
       .AddPage("<p>Page 2</p>")
       .AddPage("<p>Page 3</p>");
// _pages list now contains 3 entries
```

---

### Build(ConverterOptions? options = null)

**Signature**: `byte[] Build(ConverterOptions? options = null)`

**Behavior**:
1. Validates ≥1 page exists in `_pages` list
2. Parse each page HTML → `DocumentNode` via `IHtmlParser`
3. Parse header HTML → `DocumentNode` (if `_header` is not null)
4. Parse footer HTML → `DocumentNode` (if `_footer` is not null)
5. Create QuestPDF `Document` with:
   - `document.Header()` section if header present
   - `document.Footer()` section if footer present
   - `document.Page()` for each accumulated page
6. Render via `IPdfRenderer.Render()` → byte array
7. Return PDF bytes

**Exceptions**:
- `InvalidOperationException`: if no pages added (message: "At least one page must be added before building PDF")
- `TimeoutException`: if rendering exceeds 30 seconds for documents ≤10MB
- `OutOfMemoryException`: if memory usage exceeds 500MB

**Timeout**: 30 seconds

**AC**: AC-003.6, AC-003.7, AC-011.4, AC-011.13, AC-011.14

**Example**:
```csharp
// Valid
var pdf = builder.AddPage("<p>Content</p>").Build();

// Invalid - throws InvalidOperationException
var pdf = new PdfBuilder().Build();  // No pages added!

// With options
var pdf = builder.AddPage("<p>Content</p>").Build(new ConverterOptions { FontPath = "custom.ttf" });
```

---

## State Management

### Internal State
```csharp
private readonly List<string> _pages;
private string? _header;
private string? _footer;
```

**Rules**:
- `_pages` accumulates with each `AddPage()` call
- `_header` and `_footer` replaced with each `SetHeader()`/`SetFooter()` call
- `Reset()` clears all state
- State persists across multiple `Build()` calls

**AC**: AC-011.1, AC-011.2, AC-011.3, AC-011.15

---

## Header & Footer Rendering

### Dynamic Height
- Headers/footers expand to fit content (no fixed height)
- Page content area adjusts automatically based on header/footer height
- No overlap between header/footer and page content

**AC**: AC-011.10, AC-011.11, AC-011.12

### Content Parsing
- Header/footer HTML uses same parsing as page content
- All 22 supported HTML elements available
- All CSS properties supported

**AC**: AC-011.9

### Global Application
- Headers appear at top of every page
- Footers appear at bottom of every page
- Same header/footer content repeated across all pages

**AC**: AC-011.6, AC-011.7

---

## Dependency Injection

### Registration
```csharp
// New extension method
services.AddPdfBuilder();

// Implementation (ServiceCollectionExtensions.cs)
public static IServiceCollection AddPdfBuilder(this IServiceCollection services)
{
    services.TryAddTransient<IPdfBuilder, PdfBuilder>();
    // Register dependencies...
    return services;
}
```

**Lifetime**: `Transient` (new instance per resolution)

**Rationale**: Each consumer gets independent builder with isolated state

**AC**: AC-003.9

### Legacy Deprecation
```csharp
// Deprecated (breaking change)
services.AddHtml2Pdf();  // No longer available

// Migration guidance in exception/documentation
[Obsolete("AddHtml2Pdf is deprecated. Use AddPdfBuilder() instead.")]
```

**AC**: AC-003.10, AC-003.11

---

## Breaking Changes

### Removed
- `IHtmlConverter` interface
- `HtmlConverter` class
- `ConvertToPdf(string html)` method
- `AddHtml2Pdf()` extension method

### Added
- `IPdfBuilder` interface
- `PdfBuilder` class
- `Reset()`, `SetHeader()`, `SetFooter()`, `AddPage()`, `Build()` methods
- `AddPdfBuilder()` extension method

### Migration Steps
1. Replace `IHtmlConverter` references with `IPdfBuilder`
2. Replace `HtmlConverter` instantiations with `PdfBuilder`
3. Replace `ConvertToPdf(html)` with `AddPage(html).Build()`
4. Update DI registration from `AddHtml2Pdf()` to `AddPdfBuilder()`
5. Rename test file: `HtmlConverterTests.cs` → `PdfBuilderTests.cs`
6. Update all test methods to use new API

**AC**: AC-003.10, AC-003.12

---

## Test Cases

### Unit Tests (PdfBuilderTests.cs)
1. `AddPage_ValidHtml_AccumulatesPage`
2. `AddPage_NullHtml_ThrowsArgumentNullException`
3. `AddPage_EmptyHtml_ThrowsArgumentException`
4. `SetHeader_ValidHtml_StoresHeader`
5. `SetHeader_CalledTwice_LastCallWins`
6. `SetFooter_ValidHtml_StoresFooter`
7. `SetFooter_CalledTwice_LastCallWins`
8. `Build_WithPages_ReturnsValidPdf`
9. `Build_WithoutPages_ThrowsInvalidOperationException`
10. `Build_WithHeaderAndFooter_RendersOnAllPages`
11. `Reset_ClearsAllState_ReturnsBuilder`
12. `FluentChaining_AllMethods_ReturnsIPdfBuilder`

### Integration Tests (PdfBuilderTests.cs)
1. `PdfBuilder_SinglePage_GeneratesValidPdf`
2. `PdfBuilder_MultiPage_GeneratesCorrectNumberOfPages`
3. `PdfBuilder_WithHeader_AppearsOnAllPages`
4. `PdfBuilder_WithFooter_AppearsOnAllPages`
5. `PdfBuilder_WithHeaderAndFooter_RendersCorrectly`
6. `PdfBuilder_Reset_AllowsBuilderReuse`
7. `PdfBuilder_MultipleBuildCalls_ProducesIndependentPdfs`

---

## Success Criteria

✅ IPdfBuilder interface defines all required methods  
✅ PdfBuilder implements IPdfBuilder correctly  
✅ All methods return IPdfBuilder for fluent chaining  
✅ Validation enforces non-null, non-empty inputs  
✅ Build() requires ≥1 page or throws exception  
✅ Reset() clears all state and enables reuse  
✅ Headers/footers render dynamically on all pages  
✅ DI registration works with Transient lifetime  
✅ Legacy API completely removed  
✅ All tests updated and passing  

---

**Contract Status**: Ready for TDD implementation

