# Feature Specification: Iteration 1 - Core HTML parsing & rendering

**Feature Branch**: 001-iteration-1-parse  
**Created**: 2025-10-06  
**Status**: Draft  
**Input**: User description: "Iteration 1 - Parse and render core html tags."

## Execution Flow (main)
```
1. Parse user description from Input.
   -> If empty: ERROR "No feature description provided".
2. Extract key concepts from description.
   -> Identify actors, actions, data, constraints, and supported HTML/CSS elements/styles.
3. For each unclear aspect:
   -> Mark with [NEEDS CLARIFICATION: specific question].
4. Capture cross-platform expectations (Windows, Linux) and markup limitations.
5. Fill User Scenarios & Testing section.
   -> If no clear user flow: ERROR "Cannot determine user scenarios".
6. Generate Functional Requirements.
   -> Each requirement must be testable and traceable to HTML/CSS coverage or workflow behavior.
7. Identify Key Entities (if data involved).
8. Run Review Checklist.
   -> If any [NEEDS CLARIFICATION]: WARN "Spec has uncertainties".
   -> If implementation details found: ERROR "Remove tech details".
9. Return: SUCCESS (spec ready for planning).
```

---

## Quick Guidelines
- [Yes] Focus on WHAT users need and WHY (include required HTML elements, CSS classes, styling behaviors).
- [Yes] State platform expectations (Windows vs Linux parity) and observable outcomes.
- [No] Avoid HOW to implement (no language, framework, or class design).
- [Audience] Written for business stakeholders, not developers.

### Section Requirements
- **Mandatory sections**: Must be completed for every feature.
- **Optional sections**: Include only when relevant to the feature.
- When a section does not apply, remove it entirely (do not leave "N/A").

### For AI Generation
1. Mark all ambiguities with [NEEDS CLARIFICATION: question].
2. Do not guess at unsupported markup or styling; confirm scope with stakeholders.
3. Think like a tester: every vague requirement should fail the "testable" check.
4. Common underspecified areas:
   - User types and permissions
   - HTML surface area (elements, classes, inline styles)
   - Cross-platform behavior (Windows vs Linux)
   - Performance targets and scale
   - Error handling behaviors
   - Integration requirements
   - Compliance or accessibility needs

---

## Clarifications

### Session 2025-10-06

- Q: Which CSS properties must Iteration 1 handle beyond typography and simple spacing? -> A: Include table borders and cell alignment
- Q: What performance target should Iteration 1 meet for HTML-to-PDF rendering? -> A: No explicit performance target for Iteration 1
- Q: When Iteration 1 encounters unsupported HTML tags, how should the converter respond? -> A: Pass the tag to a generic fallback renderer attempting best effort
- Q: What cross-platform validation approach must Iteration 1 use before release? -> A: Ensure cross-platform compatibility through managed-only dependencies; address issues reactively as they arise
- Q: Should Iteration 1 capture analytics/diagnostics for unsupported CSS or rendering fallbacks? -> A: No - logging warnings is sufficient
- Q: Which statement best captures the out-of-scope limits for Iteration 1? -> A: Exclude media/interactive elements, complex layout (flex/floats/absolute), and external CSS assets

### Session 2025-10-09

- Q: For the new multi-page PDF feature with AddPage(string html), SetHeader(string html), and SetFooter(string html) methods, what is the intended call sequence and behavior? -> A: Builder Pattern - SetHeader() → AddPage() × N → SetFooter() → ConvertToPdf() returns all accumulated pages. Headers/footers apply to all pages.
- Q: For the new margin and border CSS shorthand properties, how should they interact with existing longhand properties when both are present? -> A: Source Order - Later declaration wins regardless of shorthand/longhand (standard CSS cascade behavior)
- Q: For headers and footers in multi-page PDFs, how should the system handle content height and available space? -> A: Dynamic Height - Headers/footers expand to fit content. Page content area adjusts automatically based on header/footer size.
- Q: When the system encounters malformed or invalid CSS shorthand values (e.g., margin: 10px invalid 20px), what should the behavior be? -> A: Reject Entire Declaration - Ignore the entire shorthand property, emit warning, fall back to defaults or inherited values
- Q: When multi-page methods (AddPage, SetHeader, SetFooter) are NOT used, how should the existing ConvertToPdf(string html) behavior work? -> A: Breaking Change - Remove ConvertToPdf(string html) signature entirely and implement BuildPdf() as the finalization method. Use classical builder pattern: SetHeader() → AddPage() × N → SetFooter() → BuildPdf() returns byte array.
- Q: Should we rename HtmlConverter to PdfBuilder to better reflect builder pattern semantics? -> A: Complete Replacement - Remove IHtmlConverter and HtmlConverter entirely. Replace with IPdfBuilder interface and PdfBuilder class. Interface: Reset() (full reset of pages/header/footer), SetHeader(string html), SetFooter(string html), AddPage(string html), Build(ConverterOptions? options = null). All methods return IPdfBuilder for fluent chaining. DI: Add services.AddPdfBuilder() extension, deprecate AddHtml2Pdf().

## User Scenarios & Testing *(mandatory)*

### Primary User Story
An SDK consumer wants to convert simple HTML documents containing headings, paragraphs, emphasis, lists, and tables into single-page or multi-page PDFs using a cross-platform managed library. Users interact with the `PdfBuilder` class through the `IPdfBuilder` interface using a fluent builder pattern: optionally call Reset() to clear state, optionally set header/footer content with SetHeader()/SetFooter(), add one or more pages with AddPage(), then finalize by calling Build() to receive the complete PDF as a byte array. All methods return IPdfBuilder to support method chaining.

### Out of Scope
- Iteration 1 excludes media or interactive HTML (audio, video, canvas), complex layout constructs such as flexbox, floats, and absolute positioning, and loading external CSS asset files; documents requiring these capabilities are deferred to later iterations.

### Edge Cases

#### Unsupported HTML Elements
- **Behavior**: Unsupported tags (e.g., `<video>`, `<audio>`, `<canvas>`) are processed by a generic fallback renderer
- **Response**: System emits structured warning log using `ILogger.LogWarning` with structured data including component, element type, and context
- **Recovery**: Element content is rendered as plain text with preserved structure, element is marked with appropriate DocumentNodeType
- **Error Handling**: If fallback rendering fails, element is skipped with error log using `ILogger.LogError` with structured data

#### Empty and Malformed Elements
- **Empty inline elements**: Collapse without emitting content, no warning generated
- **Empty block elements**: Render as empty paragraph with minimal height (4pt)
- **Malformed markup**: AngleSharp normalizes before parsing; if normalization fails, element becomes `Fallback` entry
- **Recovery**: Invalidly nested markup is restructured according to HTML5 parsing rules
- **Error Handling**: Severely malformed markup that cannot be parsed raises `HtmlParsingException` with specific error message

#### CSS Class and Style Issues
- **Missing CSS classes**: Fall back to inherited styles, emit structured warning with className and context
- **Invalid CSS properties**: Ignore invalid properties, emit structured warning with propertyName and context
- **Malformed CSS values**: Use default value for property type, emit structured warning with propertyName, value, and context
- **Recovery**: System continues processing with available valid styles, no exceptions thrown for CSS issues

#### Performance and Resource Constraints
- **Large HTML documents**: Process in chunks if document exceeds 10MB, emit structured info log with documentSize and chunkCount
- **Memory constraints**: If memory usage exceeds 500MB, abort with `OutOfMemoryException` and clear error message
- **Timeout handling**: If rendering exceeds 30 seconds, abort with `TimeoutException` and partial PDF output
- **Recovery**: System attempts to complete partial rendering before aborting, logs completion percentage

#### PdfBuilder API Usage
- **Build() without AddPage()**: Throws `InvalidOperationException` with message "At least one page must be added before building PDF"
- **Multiple SetHeader() calls**: Last call wins, previous header content is replaced
- **Multiple SetFooter() calls**: Last call wins, previous footer content is replaced
- **SetHeader/SetFooter with null or empty HTML**: Throws `ArgumentException` with clear error message
- **AddPage() with null or empty HTML**: Throws `ArgumentException` with clear error message
- **Multiple Build() calls on same instance**: Each call produces independent PDF with accumulated state at time of call
- **Reset() behavior**: Clears all accumulated pages, header, footer, and internal state; returns builder to initial state for reuse
- **Reset() return value**: Returns IPdfBuilder to support fluent chaining (e.g., `builder.Reset().AddPage(html).Build()`)
- **Recovery**: Clear error messages guide users to correct API usage pattern


## Requirements *(mandatory)*

### Functional Requirements
- **FR-001**: System MUST parse and render the following HTML elements into PDF output: div, p, span, strong, b, i, br, ul, li, ol, table, thead, tbody, tr, th, td, section, h1, h2, h3, h4, h5, h6.
- **FR-002**: System MUST support CSS class references and inline styles limited to the following properties on supported elements: `font-weight`, `font-style`, `text-decoration`, `line-height`, `color`, `background-color`, `margin`, `margin-top`, `margin-bottom`, `margin-left`, `margin-right`, `padding`, `padding-top`, `padding-bottom`, `padding-left`, `padding-right`. CSS classes MUST be defined within `<style>` tags in the same HTML document and map to this property set. Classes defined within the document that map to this property set MUST resolve consistently with inline styles.
- **FR-002a**: System MUST support CSS shorthand properties `margin` and `border` with standard CSS syntax. The `margin` shorthand MUST support 1-4 value patterns: 1-value (all sides), 2-value (vertical horizontal), 3-value (top horizontal bottom), 4-value (top right bottom left). The `border` shorthand MUST parse width, style, and color components in any order with support for standard CSS border-width values (thin/medium/thick/length), border-style keywords (solid/dashed/dotted/none/hidden), and color values (named colors, hex, rgb). When both shorthand and longhand properties are present, later declaration wins following standard CSS cascade behavior (source order precedence). Invalid or malformed shorthand values MUST be rejected entirely, emitting a structured warning log, with the system falling back to default or inherited values for that property.
- **FR-003**: Users MUST be able to provide HTML content through the public `IPdfBuilder` interface implemented by `PdfBuilder` class. The API MUST follow a classical fluent builder pattern with `Reset()` to clear state, `SetHeader(string html)` and `SetFooter(string html)` for defining repeated content, `AddPage(string html)` for adding pages, and `Build(ConverterOptions? options = null)` as the finalization method returning byte array. All methods MUST return `IPdfBuilder` to support method chaining. The legacy `IHtmlConverter` interface and `HtmlConverter` class are completely removed (breaking change). Dependency injection MUST provide `services.AddPdfBuilder()` extension method; the legacy `AddHtml2Pdf()` is deprecated.
- **FR-004**: System MUST rely solely on managed .NET dependencies (QuestPDF, AngleSharp) without introducing GDI+ or native rendering libraries to ensure inherent cross-platform compatibility. **MUST** validate managed-only constraint through dependency audit (`dotnet list package --include-transitive`) confirming all dependencies are managed .NET libraries. Cross-platform issues will be addressed reactively as they arise.
- **FR-005**: System MUST route unsupported HTML elements to a generic fallback renderer that attempts best-effort output and surfaces clear warnings describing the degraded behavior.
- **FR-006**: System MUST include regression tests and fixtures covering each supported tag and representative combinations (paragraphs, lists, tables). **MUST** follow Test-Driven Development (TDD) approach using the red-green-refactor cycle: write ONE failing test → implement minimal code to pass → refactor → repeat. Only ONE test should be failing at a time. Write test FIRST before implementing any feature, make it pass with minimal code, then write the next test. **MUST** test concrete implementations only - do NOT create tests for interfaces themselves (interfaces are contracts validated by implementation tests). **MUST** treat tests as first-class code applying clean code principles, SOLID, DRY, and KISS. **MUST** use `[Theory]` with `[InlineData]` to consolidate similar test scenarios, create helper methods to eliminate repetitive arrange sections, and keep test methods short and focused (ideally under 15 lines). Integration tests prefixed with `Iteration1_*` in `HtmlConverterTests` serve as contract validation for the specifications defined in `contracts/` directory.
- **FR-007**: System MUST emit structured warning logs describing fallback rendering events using `Microsoft.Extensions.Logging.ILogger` with severity level WARNING. Logs MUST include structured data (component, element type, context) rather than plain string messages. No additional analytics storage is required in Iteration 1.
- **FR-008**: System MUST support table border styling including `border`, `border-collapse`, `border-width`, `border-style`, `border-color` properties and cell alignment properties including `text-align` (horizontal alignment) and `vertical-align` (vertical alignment) in addition to typography and spacing controls. Default values: `border-collapse: separate`, `text-align: left`, `vertical-align: top`. CSS inheritance follows standard cascade rules with inline styles taking precedence over class styles.
- **FR-009**: System MUST capture render timing metrics for monitoring and future performance optimization. No explicit performance targets are established for Iteration 1 - focus is on measurement and data collection only. Timing data MUST include total render duration and be stored in application logs.
- **FR-010**: System MUST achieve comprehensive test coverage for classes with business logic, algorithms, and complex behavior. **MUST** create unit tests for individual methods, integration tests for component interactions, and contract tests for API boundaries. Focus test coverage on High Priority areas: classes with business logic, algorithms, validation, parsing, rendering, and complex state management. Medium Priority areas include classes with moderate complexity, data transformation, or integration points. Low Priority areas (simple data containers, property setters, enums, basic value objects) are optional. Exempt areas include auto-generated code, simple constructors without logic, and trivial getters/setters.
- **FR-011**: System MUST support multi-page PDF generation through a classical fluent builder pattern API via `IPdfBuilder` interface. `PdfBuilder` MUST provide `Reset()` method to clear state for reuse, `AddPage(string html)` method to accumulate pages, `SetHeader(string html)` and `SetFooter(string html)` methods to define repeated header/footer content, and `Build(ConverterOptions? options = null)` as the finalization method. Typical call sequence: Reset() → SetHeader() → AddPage() × N → SetFooter() → Build(). The Build() method MUST return byte array containing all accumulated pages with headers/footers applied to each page. Headers and footers apply globally to all pages in the document. Headers and footers MUST use dynamic height, expanding to fit content automatically. Page content area MUST adjust based on actual header/footer height, ensuring no overlap or content clipping.

### Acceptance Criteria

#### FR-001: HTML Element Parsing & Rendering
- **AC-001.1**: All 22 supported HTML elements (div, p, span, strong, b, i, br, ul, li, ol, table, thead, tbody, tr, th, td, section, h1, h2, h3, h4, h5, h6) parse correctly into DocumentNode entities
- **AC-001.2**: Each element renders to PDF output with correct QuestPDF document structure
- **AC-001.3**: Element nesting is preserved in both parsing and rendering phases
- **AC-001.4**: Text content within elements is preserved exactly as provided in HTML input
- **AC-001.5**: Contract tests pass for all supported element combinations

#### FR-002: CSS Class and Inline Style Support
- **AC-002.1**: All 15 supported CSS properties parse correctly from inline styles and CSS classes
- **AC-002.2**: CSS classes defined within `<style>` tags in same document resolve correctly
- **AC-002.3**: CSS inheritance follows standard cascade rules (inline > class > inherited > default)
- **AC-002.4**: Invalid CSS properties are ignored with warning logs
- **AC-002.5**: CSS property values are validated for correct syntax and applied to PDF rendering

#### FR-002a: CSS Shorthand Properties
- **AC-002a.1**: Margin shorthand with 1 value applies to all sides (e.g., `margin: 10px` → all margins = 10px)
- **AC-002a.2**: Margin shorthand with 2 values applies vertical/horizontal (e.g., `margin: 10px 20px` → top/bottom=10px, left/right=20px)
- **AC-002a.3**: Margin shorthand with 3 values applies top/horizontal/bottom (e.g., `margin: 10px 20px 30px` → top=10px, left/right=20px, bottom=30px)
- **AC-002a.4**: Margin shorthand with 4 values applies top/right/bottom/left (e.g., `margin: 10px 20px 30px 40px`)
- **AC-002a.5**: Border shorthand parses width, style, and color in any order (e.g., `border: solid 2px red`, `border: #000 1px dashed`)
- **AC-002a.6**: Border shorthand supports standard width keywords (thin, medium, thick) and length units (px, pt, em)
- **AC-002a.7**: Border shorthand supports style keywords (solid, dashed, dotted, none, hidden)
- **AC-002a.8**: Border shorthand supports color formats (named colors, hex values, rgb())
- **AC-002a.9**: When shorthand appears after longhand, shorthand values override (e.g., `margin-top: 20px; margin: 10px` → all margins = 10px)
- **AC-002a.10**: When longhand appears after shorthand, longhand values override (e.g., `margin: 10px; margin-top: 20px` → top=20px, others=10px)
- **AC-002a.11**: Invalid shorthand values are rejected entirely and emit structured warning logs with property name and invalid value
- **AC-002a.12**: When invalid shorthand is encountered, system falls back to default or inherited values for that property
- **AC-002a.13**: Rendering continues successfully despite invalid shorthand values

#### FR-003: Public API Access
- **AC-003.1**: IPdfBuilder interface defines fluent builder methods: Reset(), SetHeader(string html), SetFooter(string html), AddPage(string html), Build(ConverterOptions? options = null)
- **AC-003.2**: PdfBuilder class implements IPdfBuilder interface with all required methods
- **AC-003.3**: All IPdfBuilder methods return IPdfBuilder to support fluent method chaining
- **AC-003.4**: AddPage() throws ArgumentNullException for null input with clear error message
- **AC-003.5**: AddPage() throws ArgumentException for empty input with clear error message
- **AC-003.6**: Build() returns valid PDF byte array when at least one page has been added
- **AC-003.7**: Build() completes within 30 seconds for documents up to 10MB
- **AC-003.8**: Reset() clears all accumulated state (pages, header, footer) and returns IPdfBuilder
- **AC-003.9**: services.AddPdfBuilder() extension method registers IPdfBuilder with DI container
- **AC-003.10**: Legacy IHtmlConverter interface and HtmlConverter class are completely removed (breaking change)
- **AC-003.11**: Legacy AddHtml2Pdf() extension method is deprecated with clear migration guidance
- **AC-003.12**: All existing unit tests are updated to use new PdfBuilder API

#### FR-004: Managed Dependencies
- **AC-004.1**: Dependency audit (`dotnet list package --include-transitive`) confirms all dependencies are managed .NET libraries
- **AC-004.2**: No GDI+ or native rendering libraries are referenced in project files
- **AC-004.3**: Solution builds successfully using only managed dependencies

#### FR-005: Fallback Renderer
- **AC-005.1**: Unsupported HTML elements are processed by fallback renderer without exceptions
- **AC-005.2**: Fallback elements are rendered as plain text with preserved structure
- **AC-005.3**: Warning logs are emitted for each fallback element processed
- **AC-005.4**: Fallback elements are marked with appropriate DocumentNodeType (Generic or Fallback)
- **AC-005.5**: Document processing continues successfully despite unsupported elements
- **AC-005.6**: PDF is generated with supported elements rendered correctly
- **AC-005.7**: Unsupported elements are converted to plain text with preserved structure
- **AC-005.8**: Warning logs are emitted for each fallback event

#### FR-006: Regression Tests and TDD
- **AC-006.1**: Tests are written FIRST before implementation (red-green-refactor cycle)
- **AC-006.2**: Each supported HTML element has corresponding test coverage
- **AC-006.3**: Representative combinations (paragraphs, lists, tables) are tested
- **AC-006.4**: All tests pass before implementation is considered complete
- **AC-006.5**: PdfPig is used for PDF verification in tests
- **AC-006.6**: Tests apply clean code principles: SOLID, DRY, KISS
- **AC-006.7**: Similar test scenarios are consolidated using `[Theory]` with `[InlineData]`
- **AC-006.8**: Helper methods and test data builders eliminate repetitive arrange sections
- **AC-006.9**: Test methods are short and focused (ideally under 15 lines)
- **AC-006.10**: Integration tests prefixed with `Iteration_*` in `PdfBuilderTests` serve as contract validation

#### FR-007: Structured Warning Logs
- **AC-007.1**: Warning logs use `Microsoft.Extensions.Logging.ILogger` interface
- **AC-007.2**: Warning severity level is set to LogLevel.Warning
- **AC-007.3**: Fallback rendering events generate warning logs with structured data
- **AC-007.4**: CSS resolution issues generate warning logs with structured data
- **AC-007.5**: Logs include structured properties (component, elementType, context) not just strings
- **AC-007.6**: Logs are stored in application logs without external analytics storage

#### FR-008: Table Border Styling and Alignment
- **AC-008.1**: All 8 table CSS properties (border, border-collapse, border-width, border-style, border-color, text-align, vertical-align) are supported
- **AC-008.2**: Default values are applied correctly: border-collapse=separate, text-align=left, vertical-align=top
- **AC-008.3**: CSS inheritance follows standard cascade rules for table elements
- **AC-008.4**: Table borders render correctly in PDF output
- **AC-008.5**: Cell alignment properties affect text positioning within table cells

#### FR-009: Performance Timing Capture
- **AC-009.1**: Render timing data is captured in milliseconds with 3 decimal precision
- **AC-009.2**: Timing data includes total render duration from HTML input to PDF output
- **AC-009.3**: Timing data is stored in application logs for future benchmarking
- **AC-009.4**: No quantitative performance targets are set for Iteration 1
- **AC-009.5**: Timing data is included in PdfRenderSnapshot entity

#### FR-010: Comprehensive Test Coverage
- **AC-010.1**: High Priority classes (business logic, algorithms, validation, parsing, rendering, complex state management) have comprehensive test coverage
- **AC-010.2**: Medium Priority classes (moderate complexity, data transformation, integration points) have appropriate test coverage
- **AC-010.3**: Unit tests cover individual methods with Arrange-Act-Assert pattern
- **AC-010.4**: Integration tests cover component interactions
- **AC-010.5**: Contract tests cover API boundaries

#### FR-011: Multi-Page PDF Generation
- **AC-011.1**: AddPage(string html) method accepts valid HTML and accumulates pages internally
- **AC-011.2**: SetHeader(string html) method defines header content applied to all pages
- **AC-011.3**: SetFooter(string html) method defines footer content applied to all pages
- **AC-011.4**: Build(ConverterOptions? options = null) returns byte array containing all accumulated pages with headers/footers
- **AC-011.5**: Fluent call sequence (Reset() → SetHeader() → AddPage() × N → SetFooter() → Build()) produces correct multi-page output
- **AC-011.6**: Headers appear at top of each page with correct positioning
- **AC-011.7**: Footers appear at bottom of each page with correct positioning
- **AC-011.8**: Multiple AddPage() calls produce corresponding number of pages in output PDF
- **AC-011.9**: Header/footer HTML uses same parsing and rendering as page content
- **AC-011.10**: Headers and footers expand dynamically to fit content without clipping
- **AC-011.11**: Page content area adjusts automatically based on actual header/footer height
- **AC-011.12**: No overlap occurs between header/footer and page content areas
- **AC-011.13**: Build() can be called without SetHeader/SetFooter (headers/footers are optional)
- **AC-011.14**: Build() requires at least one AddPage() call or throws InvalidOperationException with clear error message
- **AC-011.15**: Reset() clears all pages, header, footer, and returns PdfBuilder to initial state
- **AC-011.16**: All fluent methods (Reset, SetHeader, SetFooter, AddPage) return IPdfBuilder for chaining

### Key Entities *(include if feature involves data)*
- **DocumentNode**: Represents a normalized HTML element optimized for PDF rendering with resolved CSS styles and clean tree structure.
- **PdfRenderSnapshot**: Captures output metadata including platform parity status, warnings for unsupported markup, and references to regression fixtures.

---

## Review & Acceptance Checklist
*Gate: Automated checks run during main() execution.*

### Content Quality
- [ ] No implementation details (languages, frameworks, APIs).
- [ ] Focused on user value, HTML contract, and platform expectations.
- [ ] Written for non-technical stakeholders.
- [ ] All mandatory sections completed.

### Requirement Completeness
- [ ] No [NEEDS CLARIFICATION] markers remain.
- [ ] Requirements are testable and observable in PDF output.
- [ ] Success criteria include cross-platform confirmation when applicable.
- [ ] Scope is clearly bounded by supported markup.
- [ ] Dependencies and assumptions identified.

---

## Execution Status
*Updated by main() during processing.*

- [ ] User description parsed
- [ ] Key concepts extracted
- [ ] Ambiguities marked
- [ ] User scenarios defined
- [ ] Requirements generated
- [ ] Entities identified
- [ ] Review checklist passed

---


