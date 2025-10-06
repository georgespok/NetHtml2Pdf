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

**Data Model Complete**: All entities defined with properties, relationships, and validation rules. Ready for contract generation.