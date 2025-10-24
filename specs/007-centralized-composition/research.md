# Research: Centralized Composition and Transient Services

**Feature**: 007-centralized-composition  
**Date**: 2025-10-24  
**Status**: Complete

## Research Summary

All technical decisions are clear from existing codebase analysis. No additional research required.

## Technical Decisions

### Decision: Composition Root Pattern
**Rationale**: Centralizes object graph construction in a single location, eliminating hidden defaults and service lookups from individual classes. Enables better testability and maintainability.

**Alternatives considered**: 
- DI Container (rejected: adds complexity, not needed for this scope)
- Factory per service (rejected: scattered construction logic)
- Service Locator (rejected: violates dependency inversion)

### Decision: Transient Lifetime by Default
**Rationale**: Ensures thread safety and eliminates shared mutable state. Each render operation gets fresh instances, preventing cross-contamination between concurrent renders.

**Alternatives considered**:
- Singleton services (rejected: potential thread safety issues)
- Scoped lifetime (rejected: adds complexity without benefit)

### Decision: RendererServices for Test Overrides
**Rationale**: Enables selective service replacement in tests without requiring DI framework. Uses `InternalsVisibleTo` to maintain encapsulation while providing test flexibility.

**Alternatives considered**:
- Constructor injection (rejected: breaks existing API)
- Service locator pattern (rejected: violates dependency inversion)
- Reflection-based overrides (rejected: violates constitution)

### Decision: Preserve Public API Compatibility
**Rationale**: Maintains backward compatibility while enabling internal refactoring. `PdfRendererFactory` becomes a thin wrapper around the composition root.

**Alternatives considered**:
- Breaking API changes (rejected: violates backward compatibility requirement)
- New factory methods (rejected: adds API surface without benefit)

## Implementation Approach

1. **Create `RendererComposition`**: Internal static factory that wires the entire pipeline
2. **Create `RendererServices`**: Internal container for test-time service overrides
3. **Refactor constructors**: Remove defaulting logic, require explicit dependencies
4. **Update `PdfRendererFactory`**: Delegate to composition root
5. **Add tests**: Verify service overrides work correctly

## Dependencies Analysis

- **AngleSharp**: HTML parsing - no changes needed
- **QuestPDF**: PDF generation - no changes needed  
- **Microsoft.Extensions.Logging**: Logging - may be provided as singleton via `RendererServices`
- **xUnit/NSubstitute**: Testing - enhanced with service override capabilities

## Performance Considerations

- **Memory**: Transient instances may increase memory usage slightly, but improves thread safety
- **CPU**: Minimal overhead from composition root; object creation cost is negligible
- **Concurrency**: Improved thread safety eliminates potential race conditions

## Risk Assessment

- **Low Risk**: Refactor preserves existing behavior and API
- **Mitigation**: Comprehensive test coverage ensures no regressions
- **Rollback**: Changes are internal; can revert without API impact
