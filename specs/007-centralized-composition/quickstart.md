# Quickstart: Centralized Composition and Transient Services

**Feature**: 007-centralized-composition  
**Date**: 2025-10-24

## Overview

This refactor centralizes object construction in a single composition root, making all classes pure with explicit dependencies and enabling test-time service overrides.

## Key Changes

1. **Composition Root**: `RendererComposition` wires the entire pipeline
2. **Pure Constructors**: All classes accept only explicit dependencies
3. **Test Overrides**: `RendererServices` enables selective service replacement
4. **Public API**: `PdfRendererFactory` delegates to composition root

## Production Usage

### Basic Usage (Unchanged)
```csharp
// Existing public API continues to work

var builder = new PdfBuilder(options, logger);
builder.AddPage("<html>...</html>");
var pdfBytes = builder.Build();
```

### Internal Composition (New)
```csharp
// Direct composition root usage (internal)
var renderer = RendererComposition.CreateRenderer(options);

// PdfBuilder uses composition root internally for dependency creation
var builder = new PdfBuilder(); // Uses composition root internally
```

## Test Usage

### Service Overrides
```csharp
// Override specific services for testing
var fakePagination = new FakePaginationService();
var services = RendererServices.ForTests().With(pagination: fakePagination);
var renderer = RendererComposition.CreateRenderer(options, services);

// Verify interactions with fake
fakePagination.AssertCalled();
```

### Partial Overrides
```csharp
// Override multiple services
var services = RendererServices.ForTests()
    .With(pagination: fakePagination, logger: mockLogger);
var renderer = RendererComposition.CreateRenderer(options, services);
```

### Complete Override
```csharp
// Override all services
var services = RendererServices.ForTests()
    .With(
        pagination: fakePagination,
        logger: mockLogger,
        displayClassifier: fakeDisplayClassifier
    );
var renderer = RendererComposition.CreateRenderer(options, services);
```

## Migration Guide

### For Library Consumers
- **No changes required**: Public API remains unchanged
- **Performance**: No impact on rendering performance
- **Behavior**: Identical output and functionality

### For Test Authors
- **New capabilities**: Can now override specific services
- **Existing tests**: Continue to work without changes
- **Enhanced testing**: Better isolation and mocking capabilities

### For Internal Code
- **Constructor changes**: All constructors now require explicit dependencies
- **No defaults**: Remove all "create defaults" logic from constructors
- **Composition**: Use `RendererComposition` for object creation
- **PdfBuilder**: Uses composition root internally for dependency creation

## Implementation Steps

1. **Create `RendererComposition`**: Internal static factory
2. **Create `RendererServices`**: Service override container
3. **Refactor constructors**: Remove defaulting logic
4. **Update `PdfRendererFactory`**: Delegate to composition root
5. **Refactor `PdfBuilder`**: Use composition root internally for dependency creation
6. **Add tests**: Verify service overrides work correctly

## Benefits

- **Testability**: Easy service replacement for testing
- **Maintainability**: Clear dependency relationships
- **Thread Safety**: Transient instances prevent shared state
- **Clarity**: No hidden defaults or service lookups
- **Flexibility**: Selective service overrides without DI framework

## Troubleshooting

### Common Issues

**Q: My existing tests are failing after the refactor**
A: Ensure you're using the public API (`PdfRendererFactory`) or update to use explicit dependencies in constructors.

**Q: How do I override a service in tests?**
A: Use `RendererServices.ForTests().With(serviceName: fakeService)` and pass to `RendererComposition.CreateRenderer()`.

**Q: Can I still use the old constructors?**
A: No, all constructors now require explicit dependencies. Use `RendererComposition` or the public factory methods.

### Performance Considerations

- **Memory**: Slight increase due to transient instances
- **CPU**: Negligible overhead from composition root
- **Concurrency**: Improved thread safety
- **Overall**: No significant performance impact
