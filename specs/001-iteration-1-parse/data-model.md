# Data Model: Iteration 1 - Core HTML parsing & rendering

**Branch**: 001-iteration-1-parse | **Date**: 2025-01-27 | **Phase**: 1 Complete

## Core Entities

### DocumentNode

Represents a normalized HTML element optimized for PDF rendering. This is the primary entity used throughout the parsing and rendering pipeline.

**Properties**:
- `NodeType` (DocumentNodeType): The type of HTML element (div, p, span, etc.)
- `TextContent` (string?): The text content of the element (for text nodes)
- `Styles` (CssStyleMap): Resolved CSS styles applied to the element
- `Children` (IReadOnlyList<DocumentNode>): Child elements

**Key Features**:
- **Normalized representation**: Focuses on rendering semantics, not HTML fidelity
- **Resolved styles**: Uses `CssStyleMap` with resolved CSS properties
- **Text merging**: Automatically merges adjacent text nodes
- **Clean tree structure**: Simple parent-child relationships

**Validation Rules**:
- `NodeType` must be a valid `DocumentNodeType` enum value
- `TextContent` can be null for non-text nodes
- `Styles` cannot be null (defaults to `CssStyleMap.Empty`)
- `Children` cannot contain null elements

### PdfRenderSnapshot

Captures output metadata including platform parity status, warnings for unsupported markup, and references to regression fixtures.

**Properties**:
- `RenderDuration` (TimeSpan): Total time taken for HTML to PDF conversion
- `Platform` (string): Platform where rendering occurred (Windows/Linux)
- `Warnings` (List<string>): Warning messages generated during rendering
- `OutputSize` (long): Size of generated PDF in bytes
- `Timestamp` (DateTime): When the rendering occurred
- `FallbackElements` (List<string>): List of elements processed by fallback renderer
- `InputHtmlSize` (int): Size of input HTML in characters
- `ElementCount` (int): Total number of HTML elements processed
- `SupportedElementCount` (int): Number of supported elements processed
- `FallbackElementCount` (int): Number of fallback elements processed
- `CssPropertyCount` (int): Number of CSS properties processed
- `MemoryUsage` (long): Memory usage during rendering in bytes
- `IsCrossPlatformValidated` (bool): Whether cross-platform validation passed
- `ValidationTimestamp` (DateTime?): When cross-platform validation occurred
- `ValidationResult` (string?): Cross-platform validation results

**Validation Rules**:
- `RenderDuration` must be non-negative
- `Platform` must be "Windows" or "Linux"
- `Warnings` cannot contain null strings
- `OutputSize` must be positive
- `Timestamp` must be valid DateTime
- `FallbackElements` cannot contain null strings
- `InputHtmlSize` must be non-negative
- `ElementCount` must equal `SupportedElementCount + FallbackElementCount`
- `MemoryUsage` must be non-negative
- `ValidationTimestamp` must be valid if `IsCrossPlatformValidated` is true

### DocumentNodeType

Enumeration of supported HTML element types for Iteration 1.

**Values**:
- `Div`: Block container element
- `Section`: Semantic section element
- `Paragraph`: Text paragraph element
- `Span`: Inline text element
- `Strong`: Bold text emphasis
- `Bold`: Bold text emphasis (alternative)
- `Italic`: Italic text emphasis
- `Break`: Line break element
- `UnorderedList`: Bulleted list container
- `OrderedList`: Numbered list container
- `ListItem`: List item element
- `Table`: Table container element
- `TableHead`: Table header section
- `TableBody`: Table body section
- `TableRow`: Table row element
- `TableHeader`: Table header cell
- `TableData`: Table data cell
- `Heading1`: Level 1 heading
- `Heading2`: Level 2 heading
- `Heading3`: Level 3 heading
- `Heading4`: Level 4 heading
- `Heading5`: Level 5 heading
- `Heading6`: Level 6 heading

**Validation Rules**:
- Must contain exactly 22 values (FR-001)
- Each value must correspond to a supported HTML element
- Values must be unique and non-null

### CssStyleMap

Maps CSS property names to their resolved values with source tracking.

**Properties**:
- `Properties` (Dictionary<string, string>): CSS property name to value mapping
- `Source` (CssStyleSource): Source of the CSS styling
- `FontStyle` (FontStyle?): Resolved font style if applicable
- `TextDecoration` (TextDecorationStyle?): Resolved text decoration if applicable

**Validation Rules**:
- `Properties` keys must be supported CSS properties (FR-002, FR-009)
- `Properties` values must be valid CSS values
- `Source` must be a valid `CssStyleSource` enum value
- `FontStyle` must be valid if specified
- `TextDecoration` must be valid if specified

### CssStyleSource

Enumeration of CSS style sources for cascade resolution.

**Values**:
- `Inline`: CSS properties defined inline on the element
- `Class`: CSS properties from CSS class definitions
- `Inherited`: CSS properties inherited from parent elements
- `Default`: Default CSS property values

**Validation Rules**:
- Must contain exactly 4 values
- Values must be unique and non-null
- Values must represent valid CSS cascade sources

## Entity Relationships

### DocumentNode Relationships
- **Parent-Child**: `DocumentNode` can have multiple `Children` and one implicit parent
- **Style Mapping**: `DocumentNode` references `CssStyleMap` for resolved styles
- **Node Type**: `DocumentNode` has one `DocumentNodeType`

### PdfRenderSnapshot Relationships
- **Rendering Result**: `PdfRenderSnapshot` captures metadata for a complete rendering operation
- **Warning Tracking**: `PdfRenderSnapshot` contains warnings generated during rendering
- **Fallback Tracking**: `PdfRenderSnapshot` tracks elements processed by fallback renderer

### CssStyleMap Relationships
- **Style Source**: `CssStyleMap` has one `CssStyleSource`
- **Font Style**: `CssStyleMap` can reference `FontStyle` enum
- **Text Decoration**: `CssStyleMap` can reference `TextDecorationStyle` enum

## Data Flow

### HTML Parsing Flow
1. Input HTML → AngleSharp parsing → `DocumentNode` creation
2. CSS extraction → `CssStyleMap` creation with `CssStyleSource`
3. Style resolution → Cascade application → Final `CssStyleMap`

### PDF Rendering Flow
1. `DocumentNode` → QuestPDF document structure
2. `CssStyleMap` → QuestPDF styling properties
3. Rendering → PDF generation → `PdfRenderSnapshot` creation

### Cross-Platform Validation Flow
1. Windows rendering → `PdfRenderSnapshot` creation
2. Linux rendering → `PdfRenderSnapshot` creation
3. Comparison → Validation result → `PdfRenderSnapshot` update

## Validation Constraints

### Business Rules
- All HTML elements must be parsed into `DocumentNode` entities
- CSS properties must be resolved according to cascade rules
- Fallback elements must be marked with appropriate `DocumentNodeType`
- Cross-platform validation must be performed before release
- Performance timing must be captured for all rendering operations

### Data Integrity
- Parent-child relationships must be consistent
- CSS property values must be valid for their types
- Enum values must be valid and non-null
- Timestamps must be valid DateTime values
- Counts must be non-negative and consistent

### Performance Constraints
- Memory usage must not exceed 500MB
- Rendering time must not exceed 30 seconds
- Document size must not exceed 10MB
- All operations must complete within defined limits

---

## Scope Addition Entities (2025-10-09)

### IPdfBuilder (Interface)

Public interface for building multi-page PDFs with fluent API pattern.

**Methods**:
- `Reset()` → `IPdfBuilder`: Clears all accumulated state (pages, header, footer) and returns builder to initial state
- `SetHeader(string html)` → `IPdfBuilder`: Sets global header HTML applied to all pages
- `SetFooter(string html)` → `IPdfBuilder`: Sets global footer HTML applied to all pages
- `AddPage(string htmlContent)` → `IPdfBuilder`: Adds a page with specified HTML content
- `Build(ConverterOptions? options = null)` → `byte[]`: Finalizes and builds the PDF, returning byte array

**Validation Rules**:
- All methods must return `IPdfBuilder` for fluent chaining
- `AddPage()` must throw `ArgumentNullException` for null input
- `AddPage()` must throw `ArgumentException` for empty/whitespace input
- `SetHeader()`/`SetFooter()` must throw `ArgumentException` for null/empty input
- `Build()` must throw `InvalidOperationException` if no pages added (message: "At least one page must be added before building PDF")
- `Build()` timeout: 30 seconds for documents ≤10MB (FR-003)

**State Management**:
- Implementation maintains internal mutable state
- Interface exposes immutable semantics (fluent returns)
- Multiple `Build()` calls produce independent PDFs with accumulated state at time of call
- `Reset()` enables builder reuse without creating new instances

### PdfBuilder (Class)

Concrete implementation of `IPdfBuilder` interface. Replaces legacy `HtmlConverter` class (breaking change).

**Internal State**:
- `_pages` (List<string>): Accumulated HTML content for each page
- `_header` (string?): Global header HTML (null if not set)
- `_footer` (string?): Global footer HTML (null if not set)
- `_parser` (IHtmlParser): HTML parser instance
- `_rendererFactory` (IPdfRendererFactory): PDF renderer factory
- `_defaultOptions` (IOptions<RendererOptions>): Default renderer options

**Constructor Dependencies**:
```csharp
public PdfBuilder(
    IHtmlParser htmlParser,
    IPdfRendererFactory rendererFactory,
    IOptions<RendererOptions> rendererOptions)
```

**DI Lifetime**: **Transient** (recommended)
- **Rationale**: Each consumer gets independent builder instance with isolated state
- **Alternative**: Scoped (one builder per request/scope) if request-scoped isolation needed
- **Not Recommended**: Singleton (shared state across all consumers would require locking/Reset() calls)

**Behavior**:
- `Reset()`: Clears `_pages`, sets `_header` and `_footer` to null, returns `this`
- `SetHeader()`/`SetFooter()`: Validates input, stores in field, last call wins, returns `this`
- `AddPage()`: Validates input, adds to `_pages` list, returns `this`
- `Build()`: 
  1. Validates ≥1 page exists
  2. For each page HTML: `_parser.Parse()` → `DocumentNode`
  3. Parse header/footer HTML if present
  4. Create QuestPDF Document with `document.Header()`, `document.Footer()`, `document.Page()` per accumulated page
  5. Call `_rendererFactory.Create().Render()` → byte array
  6. Return PDF bytes

**Breaking Change Impact**:
- Removes: `IHtmlConverter`, `HtmlConverter`, `ConvertToPdf(string html)` method
- Adds: `IPdfBuilder`, `PdfBuilder`, fluent builder methods
- Migration: `converter.ConvertToPdf(html)` → `builder.AddPage(html).Build()`
- Test files: Rename `HtmlConverterTests.cs` → `PdfBuilderTests.cs`
- DI: `services.AddHtml2Pdf()` deprecated → `services.AddPdfBuilder()`

### CssStyleMap (Extended)

Updated to support CSS shorthand properties for margin and border.

**New Handling**:
- **Margin Shorthand**: `margin: 10px` → expands to margin-top/right/bottom/left properties
- **Border Shorthand**: `border: 1px solid black` → expands to border-width/style/color properties
- **Cascade Rules**: Source order precedence (later declaration wins) applies to shorthand/longhand conflicts
- **Invalid Values**: Rejected entirely, emit warning, fall back to defaults/inherited values

**No New Properties Required**:
- Shorthand values are parsed and expanded to existing longhand properties (margin-top, border-width, etc.)
- `CssStyleMap` already has all necessary properties from FR-002 and FR-008

**Parser Extensions**:
- `CssStyleUpdater.ParseMarginShorthand(string value)`: New method for 1-4 value pattern parsing
- `CssStyleUpdater.ParseBorderShorthand(string value)`: New method for width/style/color component extraction

### ConverterOptions (No Changes)

Existing class remains unchanged. Used as optional parameter to `Build()` method.

**Current Properties**:
- `FontPath` (string): Custom font path (empty string = use default)

**Future Extensibility**:
- Could add page size, orientation, margins in future iterations
- Current scope: no new properties needed for multi-page/headers/footers

---

## Updated Entity Relationships

### PdfBuilder Relationships
- **Uses**: `IHtmlParser` for HTML→DocumentNode conversion
- **Uses**: `IPdfRendererFactory` for PDF generation
- **Configures**: `RendererOptions` for rendering settings
- **Produces**: Byte array (PDF document)
- **State**: Maintains `List<string>` pages and optional header/footer strings

### IPdfBuilder Usage Pattern
```csharp
// Single-page
var pdf = builder.AddPage(html).Build();

// Multi-page with headers/footers
var pdf = builder
    .Reset()                              // Optional: clear previous state
    .SetHeader("<h1>Header</h1>")         // Optional: global header
    .AddPage("<p>Page 1</p>")             // Required: ≥1 page
    .AddPage("<p>Page 2</p>")             // Optional: additional pages
    .SetFooter("<p>Footer</p>")           // Optional: global footer
    .Build(options);                      // Finalize and get PDF bytes
```

### DI Registration
```csharp
// New extension method
services.AddPdfBuilder();                 // Registers IPdfBuilder → PdfBuilder (Transient)

// Legacy (deprecated)
services.AddHtml2Pdf();                   // No longer available (breaking change)
```

---

## Updated Data Flow

### Multi-Page PDF Generation Flow
1. Consumer calls `builder.AddPage(html)` × N times
2. Optional: `builder.SetHeader(headerHtml)` and `builder.SetFooter(footerHtml)`
3. Consumer calls `builder.Build(options)`
4. PdfBuilder validates ≥1 page exists
5. For each page: `_parser.Parse(html)` → `DocumentNode`
6. Header/footer HTML parsed similarly
7. QuestPDF `Document.Create()` with:
   - `document.Header()` for header content
   - `document.Footer()` for footer content
   - `document.Page()` for each accumulated page
8. `IPdfRenderer.Render()` generates PDF bytes
9. Return PDF byte array to consumer

### CSS Shorthand Parsing Flow
1. CSS declaration encountered (e.g., `margin: 10px 20px`)
2. `CssStyleUpdater` identifies shorthand property
3. Call appropriate parser: `ParseMarginShorthand()` or `ParseBorderShorthand()`
4. Parser validates syntax and extracts values
5. If valid: expand to longhand properties and apply to `CssStyleMap`
6. If invalid: reject entire declaration, emit structured warning, fall back to defaults
7. Continue processing with resolved longhand properties

---

**Data Model Complete**: All entities (including scope additions) defined with properties, relationships, validation rules, and data flows. Ready for contract generation.