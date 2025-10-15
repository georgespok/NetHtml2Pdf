# Research: Iteration 1 – Core HTML parsing & rendering

**Branch**: 001-iteration-1-parse | **Date**: 2025-10-14 | **Phase**: 0 Complete

## Technology Decisions

### Fluent Facade (PdfBuilder)  
**Decision**: Expose all public HTML→PDF functionality via `IPdfBuilder`/`PdfBuilder`.  
**Rationale**: Aligns with Constitution Principle III and spec FR-003; fluent chaining simplifies multi-page, header/footer workflows.  
**Alternatives Considered**: Retaining `HtmlConverter` API (rejected – violates constitution, harder to extend to multi-page).  
**Implementation**: Builder aggregates pages (unordered list of HTML strings), optional header/footer markup, and `Build()` orchestrates parsing and rendering. DI registers `IPdfBuilder` as transient.

### HTML Parsing & Normalization  
**Decision**: Use AngleSharp to parse HTML5, resolve classes, and normalize malformed markup.  
**Rationale**: Mature, managed-only library; integrates with CSS extraction.  
**Implementation**: `HtmlParser` converts AngleSharp nodes to `DocumentNode` graph, applying inheritance, merging adjacent text nodes, and tagging unsupported elements for fallback.

### CSS Processing (including shorthands)  
**Decision**: Extend `CssStyleUpdater` to support FR-002/FR-002a properties including `margin` and `border` shorthands.  
**Rationale**: Enables spec-mandated styling coverage without introducing a full CSS engine.  
**Implementation**:  
- Tokenize shorthand values, validate patterns (1–4 values for margin; width/style/color combinations for border).  
- Expand to longhand entries stored in `CssStyleMap`.  
- Reject invalid shorthand atomically, emit structured warning.

### Rendering Engine (QuestPDF)  
**Decision**: QuestPDF composes pages, handles multi-page documents, and renders tables with borders/alignment.  
**Rationale**: Managed-only dependency with rich layout primitives matching FR-001/FR-008/FR-011.  
**Implementation**:  
- `PdfRenderer` maps `DocumentNode` trees to QuestPDF containers.  
- `BlockComposer`, `InlineComposer`, `TableComposer`, and forthcoming list/table helpers render nodes.  
- Header/footer DocumentNodes rendered via `page.Header()` / `page.Footer()`.

### Fallback Strategy  
**Decision**: Unsupported tags fall back to plain text while logging warnings.  
**Rationale**: FR-005 requires graceful degradation without breaking render flow.  
**Implementation**:  
- `FallbackRenderer` transforms unsupported nodes, marks them as `DocumentNodeType.Fallback`.  
- Logs use `ILogger` with structured properties (component, tag, context).  
- Contract tests validate warning emission and content preservation.

### Telemetry & Performance  
**Decision**: Capture render duration, memory, and warning counts via `PdfRenderSnapshot`.  
**Rationale**: FR-009/FR-015 require tracking without hard performance targets.  
**Implementation**:  
- Snapshot populated during `Build()`.  
- Optional performance tests assert non-negative timing and warn counts are measurable.  
- Values surfaced through logs and return objects for integration testing.

## Dependency Analysis

| Component | Decision | Notes |
|-----------|----------|-------|
| QuestPDF | Keep | Primary renderer, managed-only. |
| AngleSharp | Keep | HTML parsing and CSS extraction. |
| Microsoft.Extensions.Logging.Abstractions | Keep | Structured logging for warnings/telemetry. |
| PdfPig (tests) | Keep | PDF inspection in unit/integration tests. |

**Audit Plan**: Run `dotnet list package --include-transitive` weekly and before release; fail CI if unmanaged/native dependency detected.

**Audit Results (2025-10-14 - T003)**: All dependencies confirmed managed-only .NET packages:
- **Top-level packages**: AngleSharp 1.3.0, Microsoft.Extensions.DependencyInjection.Abstractions 8.0.0, Microsoft.Extensions.Logging.Abstractions 8.0.0, Microsoft.Extensions.Options 8.0.2, QuestPDF 2025.7.1
- **Transitive packages**: Microsoft.Extensions.Primitives 8.0.0
- **Status**: ✅ PASS - No native/unmanaged dependencies detected

## Layering & Architecture

- **Core**: `DocumentNode`, `CssStyleMap`, `PdfRenderSnapshot`, constants.  
- **Parser**: `HtmlParser`, `CssStyleUpdater`, `CssClassStyleExtractor`.  
- **Renderer**: `PdfRenderer`, composers (block/inline/list/table).  
- **Facade**: `IPdfBuilder` (DI transient), `PdfBuilder` (stateful builder), `ConverterOptions`.

All new work must respect this flow and avoid cross-layer leakage (e.g., renderer must not depend on AngleSharp types).

## Risk & Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| Invalid CSS shorthands degrade output | Styling regressions | Reject invalid declarations atomically, emit warnings, add contract tests. |
| Multi-page layout regressions | Headers/footers overlap content | Add unit + integration tests simulating tall headers/footers, assert page content shifts appropriately. |
| Performance regressions with large documents | Timeout/memory | Enforce edge case rules (10 MB, 30 s, 500 MB) with integration tests; log telemetry for analysis. |
| Fallback coverage gaps | Unsupported tags break output | Maintain contract `fallback-unsupported-tag`, ensure warnings logged and content preserved. |

## Outstanding Research Items

- Determine long-term font strategy (currently using QuestPDF defaults; evaluate embedding Inter/OpenSans for future parity).  
- Investigate additional CSS shorthand expansions (e.g., `padding`, `border-radius`) for future iterations; out of scope for Iteration 1.  
- Plan automated PDF visual diff tooling for Iteration 2 now that telemetry foundation exists.

**Research Complete** – Ready to proceed to Phase 1 design artifacts.
