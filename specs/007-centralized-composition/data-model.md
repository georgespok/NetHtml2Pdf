# Data Model: Centralized Composition and Transient Services

**Feature**: 007-centralized-composition  
**Date**: 2025-10-24

## Core Entities

### RendererComposition
**Purpose**: Internal static factory responsible for constructing the complete rendering pipeline

**Fields**:
- Static methods only (no instance state)

**Methods**:
- `CreateRenderer(RendererOptions options, RendererServices? services = null) : IPdfRenderer`
- `CreatePdfBuilderDependencies(RendererOptions options, RendererServices? services = null) : (IHtmlParser, IPdfRendererFactory)`

**Relationships**:
- Creates all pipeline components
- Owns all default creation logic
- Depends on `RendererOptions` for configuration
- Uses `RendererServices` for test overrides

### RendererServices
**Purpose**: Internal container for selective service overrides (primarily for testing)

**Fields**:
- `PaginationService? PaginationService { get; }`
- `ILogger? Logger { get; }`
- `DisplayClassifier? DisplayClassifier { get; }`
- `InlineFlowLayoutEngine? InlineFlowLayoutEngine { get; }`
- `FormattingContextFactory? FormattingContextFactory { get; }`
- `LayoutEngine? LayoutEngine { get; }`
- `RendererAdapter? RendererAdapter { get; }`
- `BlockComposer? BlockComposer { get; }`

**Methods**:
- `static RendererServices ForTests() : RendererServices`
- `RendererServices With(PaginationService? pagination = null, ILogger? logger = null, ...) : RendererServices`

**Relationships**:
- Used by `RendererComposition` for service overrides
- Created by tests for selective mocking
- All fields nullable to allow partial overrides

### PdfBuilder (Refactored)
**Purpose**: Main public API for PDF generation (existing class, refactored)

**Fields**:
- `IHtmlParser _parser` (explicit dependency)
- `IPdfRendererFactory _rendererFactory` (explicit dependency)
- `RendererOptions _rendererOptions` (explicit dependency)
- `ILogger _logger` (explicit dependency)
- `List<string> _pages` (internal state)
- `List<string> _fallbackElements` (internal state)
- `string? _header` (internal state)
- `string? _footer` (internal state)

**Methods**:
- `PdfBuilder()` (default constructor - preserved)
- `PdfBuilder(ILogger logger)` (logger constructor - preserved)
- `PdfBuilder(RendererOptions rendererOptions, ILogger logger)` (options constructor - preserved)
- All existing public methods unchanged

**Relationships**:
- Uses `RendererComposition.CreatePdfBuilderDependencies()` internally to get dependencies
- Depends on `IHtmlParser` and `IPdfRendererFactory`
- No longer creates its own dependencies

### PdfRendererFactory (Refactored)
**Purpose**: Public factory for creating renderers (existing class, refactored)

**Fields**:
- No instance fields (becomes stateless)

**Methods**:
- `Create(RendererOptions options) : IPdfRenderer` (delegates to `RendererComposition`)

**Relationships**:
- Delegates to `RendererComposition.CreateRenderer`
- Maintains public API compatibility

## Pipeline Components (Refactored)

All existing pipeline components maintain their current interfaces but have constructors refactored to accept only explicit dependencies:

### DisplayClassifier
- Constructor: `DisplayClassifier()` (no dependencies)

### InlineFlowLayoutEngine  
- Constructor: `InlineFlowLayoutEngine(DisplayClassifier displayClassifier)`

### FormattingContextFactory
- Constructor: `FormattingContextFactory(RendererOptions options)`

### LayoutEngine
- Constructor: `LayoutEngine(FormattingContextFactory factory)`

### PaginationService
- Constructor: `PaginationService(RendererOptions options)`

### RendererAdapter
- Constructor: `RendererAdapter(RendererOptions options)`

### BlockComposer
- Constructor: `BlockComposer(RendererOptions options)`

### PdfRenderer
- Constructor: `PdfRenderer(BlockComposer blockComposer, RendererAdapter rendererAdapter, LayoutEngine layoutEngine, PaginationService paginationService, RendererOptions options)`

## Validation Rules

1. **RendererComposition**: Must create all required dependencies; cannot have null dependencies
2. **RendererServices**: All override fields are optional; partial overrides allowed
3. **PdfBuilder**: All constructor parameters must be non-null
4. **Pipeline Components**: All constructor parameters must be non-null

## State Transitions

### RendererComposition
- Static factory, no state transitions

### RendererServices  
- Immutable after creation
- `With()` methods return new instances

### PdfBuilder
- State transitions unchanged from current implementation
- `Reset()` clears internal state
- `AddPage()`, `SetHeader()`, `SetFooter()` modify internal state

## Data Flow

1. **Composition**: `RendererComposition.CreateRenderer()` → creates all pipeline components
2. **Rendering**: `PdfBuilder.Build()` → uses `IPdfRenderer` to generate PDF
3. **Testing**: `RendererServices.ForTests().With(...)` → provides overrides for specific services
4. **Public API**: `PdfRendererFactory.Create()` → delegates to composition root
