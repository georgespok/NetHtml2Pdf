<!--
Sync Impact Report:
- Version change: 3.0.0 → 3.0.1 (PATCH: clarify sync note to reference PdfBuilder facade only)
- Modified principles:
  * III. Layered Rendering Architecture (clarified PdfBuilder/IPdfBuilder facade mandate)
- Added sections: None
- Removed sections: None
- Templates requiring updates:
  ✅ .specify/templates/plan-template.md (facade-agnostic, no changes required)
  ✅ .specify/templates/spec-template.md (facade-agnostic, no changes required)
  ✅ .specify/templates/tasks-template.md (facade-agnostic, no changes required)
- Follow-up TODOs: None
-->

# NetHtml2Pdf Constitution

## Core Principles

### I. Test-Driven Delivery (NON-NEGOTIABLE)
**MUST** practice the red-green-refactor loop, developing tests FIRST before implementing any feature. **MUST** focus test coverage on classes with business logic, algorithms, and complex behavior—not simple data containers or property setters. Structure tests using Arrange-Act-Assert, name them `MethodOrFeature_Scenario_ExpectedResult`, rely on Shouldly assertions, expand regression coverage for every new HTML edge case or bug fix, use `PdfPig` or byte-length checks when verifying rendered output, and introduce helpers/builders to keep fixtures readable. **MUST** write failing tests before writing implementation code, then implement only the minimum code required to make tests pass. TDD keeps the rendering pipeline verifiable and prevents regressions in complex document layouts.

### II. Managed Cross-Platform Compatibility
**MUST** maintain managed-only dependencies (QuestPDF, AngleSharp) without introducing GDI+ or native rendering libraries. By using .NET and cross-platform managed dependencies, the library remains cross-platform without explicit validation testing. Cross-platform issues will be addressed reactively as they arise. **MUST** enable nullable reference types and Roslyn analyzers; new warnings MUST be addressed or suppressed with justification, and CI treats warnings as errors for `src/*`.

### III. Layered Rendering Architecture
**MUST** maintain clear separation between Core (data models), Parser (HTML → DOM), Renderer (DOM → PDF), and Facade (public API) layers. **MUST** update `Core/DocumentNode` for new node types, extend `Parser/HtmlParser` for mapping, implement rendering in `Renderer/PdfRenderer`, and expose the public surface exclusively through the fluent `PdfBuilder` facade implementing `IPdfBuilder`. The builder pattern methods (`Reset`, `SetHeader`, `SetFooter`, `AddPage`, `Build`) are the sanctioned entry points for consumers.

### IV. HTML & CSS Contract Clarity
**MUST** document supported tags and CSS properties in spec/contracts, add fixtures for each supported element combination, and ensure unsupported tags emit structured warnings. **MUST** validate inputs early, fail fast, and raise specific exceptions (ArgumentException, ArgumentNullException, ApplicationException) with meaningful parameter names, especially around file system and configuration boundaries.

### V. Extensible Experience Stewardship
**MUST** follow progressive feature iteration—building simple, working functionality first, then expanding support in controlled, well-documented layers. Each iteration must be functional and testable in isolation, introduce only a small set of new HTML elements/attributes/rendering features, preserve backward compatibility, and produce a complete specification–plan–implementation cycle before advancing.

### VI. Architecture Planning & Validation
**MUST** use `/plan` to outline architecture, capture constitution checks, and document technical context before coding. **MUST** complete the planning phase with documented technical context and constitution compliance verification before beginning implementation. **MUST** ensure all architectural decisions align with constitution principles and are validated through the planning process. This prevents architectural drift and ensures consistent application of project standards.

## Code Quality Standards

### Design Principles
**MUST** design features for clarity over cleverness: apply SOLID, DRY, KISS, and YAGNI principles. **MUST** keep functions focused (ideally under ten lines) with minimal parameters, favor composition over inheritance, avoid creating interfaces until multiple implementations exist, return domain objects instead of primitives, and reserve explanatory comments or XML docs for non-obvious intent.

### C# Conventions
**MUST** follow repository C# conventions: four-space indentation, braces on new lines, PascalCase for public members and test methods, camelCase for locals and parameters, use braces for every control block, prefer `var` when the type is obvious, default to `string.IsNullOrEmpty`, adopt expression-bodied members and primary constructors when they improve clarity, enable `<Nullable>` and `<ImplicitUsings>`, run `dotnet format` before landing sizable refactors, and enforce namespace alignment with the folder structure.

### File Organization
**MUST** place each class, enum, interface, and structure in a separate file with a name that matches the type name. **MUST** design all new classes following clean code architecture principles and SOLID principles, ensuring single responsibility, proper encapsulation, and clear separation of concerns.

### Testing Standards
**MUST** achieve comprehensive test coverage for classes with business logic, algorithms, and complex behavior. **MUST** write tests FIRST before implementing any feature, following the red-green-refactor cycle. **MUST** treat tests as first-class code applying the same quality standards as production code: SOLID principles, clean code practices, DRY, and KISS. **MUST** use `[Theory]` with `[InlineData]` to consolidate similar test scenarios and reduce code duplication. **MUST** create helper methods and test data builders to eliminate repetitive arrange sections. **MUST** keep test methods short and focused (ideally under 15 lines). **MUST** create unit tests for individual methods, integration tests for component interactions, and contract tests for API boundaries. **MUST** use descriptive test names that clearly indicate the scenario and expected outcome. **MUST** maintain test independence—each test must be able to run in isolation without dependencies on other tests. **MUST** include both positive and negative test cases, covering edge cases and error conditions. **MUST** refactor tests alongside implementation code to maintain clarity and maintainability.

**Test Coverage Guidelines**:
- **High Priority**: Classes with business logic, algorithms, validation, parsing, rendering, and complex state management
- **Medium Priority**: Classes with moderate complexity, data transformation, or integration points
- **Low Priority**: Simple data containers, property setters, enums, and basic value objects
- **Exempt**: Auto-generated code, simple constructors without logic, and trivial getters/setters

## Development Workflow

### Pull Request Standards
**MUST** maintain small, cohesive PRs with a single change focus and supply rationale in the description. **MUST** begin each new iteration with a `/specify` phase defining the new features, follow with a `/plan` phase for architecture and constitution validation, and end with an `/analyze` phase validating cross-iteration stability. **MUST** complete planning phase before beginning any implementation work.

### Quality Gates
**MUST** address all Roslyn analyzer warnings or suppress with justification. **MUST** run `dotnet format` before committing sizable refactors. **MUST** achieve comprehensive test coverage for classes with business logic and complex behavior before merging any PR. **MUST** ensure all tests pass and new tests are written before implementing features.

## Governance

This constitution supersedes all other practices. Amendments require documentation, approval, and migration plan. All PRs/reviews must verify compliance; complexity must be justified. Constitution violations are automatically CRITICAL and require adjustment of specifications, plans, or tasks—not dilution, reinterpretation, or silent ignoring of principles.

**Version**: 3.0.1 | **Ratified**: 2025-01-27 | **Last Amended**: 2025-10-14
