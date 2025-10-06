# Research: Iteration 1 - Core HTML parsing & rendering

**Branch**: 001-iteration-1-parse | **Date**: 2025-01-27 | **Phase**: 0 Complete

## Technology Decisions

### CSS-to-QuestPDF Mapping Strategy

**Decision**: Use QuestPDF's built-in styling capabilities with custom CSS property mapping
**Rationale**: QuestPDF provides comprehensive styling support including borders, alignment, and typography that aligns with our supported CSS properties
**Alternatives considered**: 
- Custom CSS engine (rejected: too complex for Iteration 1)
- Direct style application without CSS parsing (rejected: doesn't meet FR-002 requirements)

**Implementation approach**:
- Map CSS properties to QuestPDF style properties using `CssStyleMap`
- Support CSS cascade rules (inline > class > inherited > default)
- Handle table-specific properties (border-collapse, cell alignment) through QuestPDF table styling

### Fallback Rendering Strategy

**Decision**: Generic fallback renderer that converts unsupported elements to plain text
**Rationale**: Maintains document structure while providing clear degradation path for unsupported markup
**Alternatives considered**:
- Skip unsupported elements entirely (rejected: loses content)
- Throw exceptions for unsupported elements (rejected: violates FR-005)

**Implementation approach**:
- Process unsupported elements through `FallbackRenderer`
- Convert element content to plain text with preserved structure
- Mark elements with `HasFallback=true` in `HtmlFragment`
- Emit structured warning logs for each fallback event

### Cross-Platform Validation Strategy

**Decision**: Visual/functional equivalence validation with platform-specific tolerance
**Rationale**: Byte-level identical PDFs are unrealistic due to platform differences in font rendering and metadata
**Alternatives considered**:
- Byte-level comparison (rejected: impossible with QuestPDF)
- Manual visual inspection (rejected: not scalable)

**Implementation approach**:
- Run regression test suite on both Windows and Linux
- Execute integration test console on both platforms
- Compare PDF outputs for visual/functional equivalence
- Document platform differences in `research.md`
- Accept tolerance for metadata differences (creation dates, producer strings, font rendering variations)

### Empty Element Handling

**Decision**: Differentiate between empty inline and block elements
**Rationale**: Follows HTML5 rendering model and provides consistent PDF output
**Alternatives considered**:
- Treat all empty elements the same (rejected: violates HTML standards)
- Skip all empty elements (rejected: loses document structure)

**Implementation approach**:
- Empty inline elements: collapse without content, no warning
- Empty block elements: render as empty paragraph with minimal height (4pt)
- Malformed markup: normalize using AngleSharp, fallback if normalization fails

### Performance Monitoring Strategy

**Decision**: Capture timing metrics without performance targets
**Rationale**: Focus on measurement and data collection for future optimization
**Alternatives considered**:
- Set specific performance targets (rejected: premature optimization)
- No performance monitoring (rejected: needed for future iterations)

**Implementation approach**:
- Capture render timing in milliseconds with 3 decimal precision
- Store timing data in `PdfRenderSnapshot` entity
- Log timing data to application logs
- No quantitative performance targets for Iteration 1

## Dependency Analysis

### Managed-Only Constraint Validation

**Decision**: Use `dotnet list package --include-transitive` for dependency audit
**Rationale**: Ensures all dependencies are managed .NET libraries without native components
**Implementation approach**:
- Run dependency audit as part of build process
- Document results in `research.md`
- Fail build if native dependencies detected
- Validate on both Windows and Linux platforms

### QuestPDF Integration

**Decision**: Use QuestPDF as primary PDF generation library
**Rationale**: Provides managed-only PDF generation with comprehensive styling support
**Alternatives considered**:
- iTextSharp (rejected: licensing concerns)
- PdfSharp (rejected: limited styling support)
- Custom PDF generation (rejected: too complex)

**Integration approach**:
- Use QuestPDF's document model for PDF structure
- Leverage QuestPDF's styling capabilities for CSS property mapping
- Utilize QuestPDF's table support for table rendering
- Maintain QuestPDF's layering architecture

### AngleSharp Integration

**Decision**: Use AngleSharp for HTML parsing
**Rationale**: Provides robust HTML5 parsing with managed-only implementation
**Alternatives considered**:
- HtmlAgilityPack (rejected: older HTML4 parsing)
- Custom HTML parser (rejected: too complex)

**Integration approach**:
- Use AngleSharp for HTML document parsing
- Leverage AngleSharp's CSS parsing for style extraction
- Utilize AngleSharp's normalization for malformed markup
- Convert AngleSharp DOM to internal `HtmlFragment` model

## Cross-Platform Considerations

### Font Rendering Differences

**Decision**: Accept platform-specific font rendering variations
**Rationale**: Different platforms have different default fonts and rendering engines
**Implementation approach**:
- Document font rendering differences in validation reports
- Use system default fonts for consistent rendering
- Accept visual differences within reasonable tolerance

### PDF Metadata Differences

**Decision**: Accept platform-specific PDF metadata differences
**Rationale**: PDF creation dates, producer strings, and other metadata vary by platform
**Implementation approach**:
- Focus validation on content and visual appearance
- Ignore metadata differences in comparison
- Document acceptable metadata variations

## Error Handling Strategy

### CSS Resolution Failures

**Decision**: Continue processing with available valid styles
**Rationale**: CSS errors shouldn't prevent document rendering
**Implementation approach**:
- Log CSS resolution warnings
- Fall back to inherited or default styles
- Continue processing without exceptions

### Memory and Performance Constraints

**Decision**: Set specific limits for memory usage and processing time
**Rationale**: Prevent system resource exhaustion
**Implementation approach**:
- 500MB memory limit with `OutOfMemoryException`
- 30-second timeout with `TimeoutException`
- 10MB document size limit with chunked processing
- Log resource usage and completion percentage

## Validation Strategy

### Test Coverage Approach

**Decision**: Focus test coverage on business logic and complex behavior
**Rationale**: Aligns with constitution's pragmatic testing approach
**Implementation approach**:
- High Priority: Parsing, rendering, validation, complex state management
- Medium Priority: Data transformation, integration points
- Low Priority: Simple data containers, property setters
- Exempt: Auto-generated code, trivial getters/setters

### Contract Testing

**Decision**: Use contract tests for API boundaries
**Rationale**: Ensures consistent behavior across different implementations
**Implementation approach**:
- Define contracts for paragraphs, tables, lists, fallback scenarios
- Test expected parser/render outputs
- Validate API boundaries with contract tests
- Use PdfPig for PDF verification in tests

---

**Research Complete**: All technology decisions documented with rationale and implementation approach. Ready for Phase 1 design.