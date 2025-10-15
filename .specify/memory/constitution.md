<!--
Sync Impact Report
- Version change: n/a → 1.0.0
- Modified principles: placeholder → concrete (5 principles defined)
- Added sections: Supported Scope and Contracts; Development Workflow and Quality Gates
- Removed sections: none
- Templates requiring updates:
  - .specify/templates/plan-template.md ✅ updated
  - .specify/templates/spec-template.md ✅ aligned (no change required)
  - .specify/templates/tasks-template.md ✅ aligned (no change required)
  - .specify/templates/agent-file-template.md ✅ aligned (no change required)
- Follow-up TODOs: TODO(RATIFICATION_DATE) — original adoption date unknown
-->

# NetHtml2Pdf Constitution

## Core Principles

### Layered Architecture and Facade‑Only Public API
The system is composed of four layers with strict responsibilities and one-way
flow:
- Core: data models and value objects (e.g., `DocumentNode`, styles, enums).
- Parser: HTML → DOM mapping using AngleSharp, updating `DocumentNode` types.
- Renderer: DOM → PDF using QuestPDF. No HTML parsing here.
- Facade: fluent `PdfBuilder` implementing `IPdfBuilder` as the only public API.

All new capabilities MUST:
- Extend `Core/DocumentNode` for new node types.
- Extend `Parser/HtmlParser` and related parsers for mapping.
- Implement rendering in `Renderer/PdfRenderer` (and composers/helpers as
  needed).
- Be exposed exclusively via the fluent `PdfBuilder` facade.
Rationale: Clear separation enables testability, maintainability, and safe
evolution.

### Managed‑Only Dependencies and Cross‑Platform
Only managed, cross‑platform libraries are allowed: QuestPDF and AngleSharp.
No GDI+, native renderers, or platform‑specific binaries. Cross‑platform
support is assumed; issues are addressed reactively with tests and fixes.
Rationale: Ensures portability and reduces operational risk.

### Test‑First and Coverage Discipline
Tests are first‑class. Follow Red‑Green‑Refactor:
- Write tests before implementation; ensure they fail; then implement and
  refactor.
- Prioritize tests for business logic, parsing, rendering, and validation.
- Use `[Theory]` and `[InlineData]` to consolidate scenarios, apply builders and
  helpers to remove duplication, and keep tests short and focused.
- Maintain independence; include positive/negative and edge cases.
Rationale: Guarantees quality, documents behavior, and enables safe refactoring.

### Input Validation, Contracts, and Structured Warnings
Validate inputs early and fail fast with specific exceptions
(`ArgumentException`, `ArgumentNullException`, `ApplicationException`) and clear
parameter names. Unsupported tags/properties MUST emit structured warnings
rather than failing silently. Supported features MUST be specified and backed by
fixtures; unsupported behavior MUST be explicitly documented.
Rationale: Predictability and debuggability for report generation scenarios.

### Clarity Over Cleverness (SOLID, DRY, KISS, YAGNI)
Favor small, focused functions (ideally under ten lines) with minimal
parameters; prefer composition over inheritance; avoid interfaces until multiple
implementations exist; return domain objects instead of primitives; keep only
non‑obvious comments or XML docs. Follow repository C# conventions: four‑space
indentation, braces on new lines, PascalCase for public members and test
methods, camelCase for locals/parameters, braces for every control block, prefer
`var` when type is obvious, `string.IsNullOrEmpty` by default, expression‑bodied
members and primary constructors when clearer, `<Nullable>` and
`<ImplicitUsings>` enabled, run `dotnet format` for sizable refactors, and align
namespaces with folders. Each type lives in its own file named after the type.
Rationale: Readability and maintainability over time.

## Supported Scope and Contracts
This library targets business report generation (invoices, receipts, summaries)
and intentionally supports a pragmatic subset of HTML/CSS. Requirements:
- Maintain a living document of supported tags and CSS properties (spec/contracts).
- Provide fixtures for every supported element/combination.
- Unsupported tags/properties: preserve inner text when reasonable and emit
  structured warnings.
- Pagination and layout rules are explicit via the builder; no hidden behavior.
Scope evolves progressively; each addition MUST include docs, tests, and
contracts.

## Development Workflow and Quality Gates
- Progressive iterations only: deliver small, functional, backward‑compatible
  increments. Each iteration includes spec → plan → implementation → tests.
- Code reviews MUST verify: layered boundaries, managed‑only dependencies,
  builder‑only public surface, validation and warning emission, and test
  coverage for new logic.
- Versioning: Semantic Versioning (MAJOR.MINOR.PATCH). Backward incompatible API
  or behavioral changes require a MAJOR bump and migration notes.
- Documentation: Update README and contracts when behavior or scope changes.

## Governance
This constitution supersedes conflicting practices. Amendments require:
- A documented rationale, updated tests/contracts, and migration guidance when
  applicable.
- Version bump according to impact (see versioning above).
- Compliance review in PRs verifying adherence to Core Principles and Quality
  Gates. Deviations must be justified in the PR description.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): unknown original adoption date | **Last Amended**: 2025-10-15
