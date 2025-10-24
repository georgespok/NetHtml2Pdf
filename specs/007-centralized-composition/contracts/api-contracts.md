# API Contracts: Centralized Composition and Transient Services

**Feature**: 007-centralized-composition  
**Date**: 2025-10-24

## Internal Composition API

### RendererComposition
```csharp
internal static class RendererComposition
{
    /// <summary>
    /// Creates a fully wired PDF renderer with the complete pipeline.
    /// </summary>
    /// <param name="options">Configuration options for the renderer</param>
    /// <param name="services">Optional service overrides (primarily for testing)</param>
    /// <returns>A configured PDF renderer instance</returns>
    public static IPdfRenderer CreateRenderer(RendererOptions options, RendererServices? services = null);
    
    /// <summary>
    /// Creates the internal dependencies that PdfBuilder needs.
    /// </summary>
    /// <param name="options">Configuration options</param>
    /// <param name="services">Optional service overrides</param>
    /// <returns>Tuple containing (IHtmlParser, IPdfRendererFactory)</returns>
    public static (IHtmlParser parser, IPdfRendererFactory rendererFactory) CreatePdfBuilderDependencies(
        RendererOptions options, 
        RendererServices? services = null);
}
```

### RendererServices
```csharp
internal class RendererServices
{
    public PaginationService? PaginationService { get; }
    public ILogger? Logger { get; }
    public DisplayClassifier? DisplayClassifier { get; }
    public InlineFlowLayoutEngine? InlineFlowLayoutEngine { get; }
    public FormattingContextFactory? FormattingContextFactory { get; }
    public LayoutEngine? LayoutEngine { get; }
    public RendererAdapter? RendererAdapter { get; }
    public BlockComposer? BlockComposer { get; }
    
    /// <summary>
    /// Creates a RendererServices instance for testing with no overrides.
    /// </summary>
    public static RendererServices ForTests();
    
    /// <summary>
    /// Creates a new RendererServices instance with the specified overrides.
    /// </summary>
    public RendererServices With(
        PaginationService? pagination = null,
        ILogger? logger = null,
        DisplayClassifier? displayClassifier = null,
        InlineFlowLayoutEngine? inlineFlowLayoutEngine = null,
        FormattingContextFactory? formattingContextFactory = null,
        LayoutEngine? layoutEngine = null,
        RendererAdapter? rendererAdapter = null,
        BlockComposer? blockComposer = null);
}
```

## Public API (Unchanged)

### PdfRendererFactory
```csharp
public class PdfRendererFactory : IPdfRendererFactory
{
    /// <summary>
    /// Creates a PDF renderer with the specified options.
    /// </summary>
    /// <param name="options">Configuration options for the renderer</param>
    /// <returns>A configured PDF renderer instance</returns>
    public IPdfRenderer Create(RendererOptions options);
}
```

### PdfBuilder (Public API Unchanged)
```csharp
public class PdfBuilder : IPdfBuilder
{
    /// <summary>
    /// Creates a PDF builder with default options and logger.
    /// </summary>
    public PdfBuilder();
    
    /// <summary>
    /// Creates a PDF builder with a custom logger.
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public PdfBuilder(ILogger logger);
    
    /// <summary>
    /// Creates a PDF builder with custom options and logger.
    /// </summary>
    /// <param name="rendererOptions">Configuration options</param>
    /// <param name="logger">Logger instance</param>
    public PdfBuilder(RendererOptions rendererOptions, ILogger logger);
    
    // All existing public methods unchanged
    public IPdfBuilder Reset();
    public IPdfBuilder SetHeader(string html);
    public IPdfBuilder SetFooter(string html);
    public IPdfBuilder AddPage(string htmlContent);
    public byte[] Build(ConverterOptions? options = null);
    // ... other existing methods
}
```

## Pipeline Component Contracts (Refactored Constructors)

### DisplayClassifier
```csharp
public class DisplayClassifier
{
    public DisplayClassifier();
}
```

### InlineFlowLayoutEngine
```csharp
public class InlineFlowLayoutEngine
{
    public InlineFlowLayoutEngine(DisplayClassifier displayClassifier);
}
```

### FormattingContextFactory
```csharp
public class FormattingContextFactory
{
    public FormattingContextFactory(RendererOptions options);
}
```

### LayoutEngine
```csharp
public class LayoutEngine
{
    public LayoutEngine(FormattingContextFactory factory);
}
```

### PaginationService
```csharp
public class PaginationService
{
    public PaginationService(RendererOptions options);
}
```

### RendererAdapter
```csharp
public class RendererAdapter
{
    public RendererAdapter(RendererOptions options);
}
```

### BlockComposer
```csharp
public class BlockComposer
{
    public BlockComposer(RendererOptions options);
}
```

### PdfRenderer
```csharp
public class PdfRenderer : IPdfRenderer
{
    public PdfRenderer(
        BlockComposer blockComposer,
        RendererAdapter rendererAdapter,
        LayoutEngine layoutEngine,
        PaginationService paginationService,
        RendererOptions options);
        
    // All existing public methods unchanged
    public byte[] Render(IEnumerable<DocumentNode> documentNodes, DocumentNode? header = null, DocumentNode? footer = null);
}
```

## Usage Examples

### Production Usage
```csharp
// PdfBuilder continues to work as before (unchanged public API)
var builder = new PdfBuilder();
var pdfBytes = builder.AddPage("<html>...</html>").Build();

// Or with custom options and logger
var builder = new PdfBuilder(options, logger);
var pdfBytes = builder.AddPage("<html>...</html>").Build();
```

### Test Usage
```csharp
// Override specific services for testing
var fakePagination = new FakePaginationService();
var services = RendererServices.ForTests().With(pagination: fakePagination);
var renderer = RendererComposition.CreateRenderer(options, services);
```

## Breaking Changes

- **None**: All public APIs remain unchanged

## Backward Compatibility

- **Public API**: `PdfRendererFactory.Create()` maintains same signature and behavior
- **PdfBuilder constructors**: All existing constructors preserved
- **PdfBuilder public methods**: All existing methods unchanged
- **Internal changes**: Composition root and service overrides are internal implementation details
