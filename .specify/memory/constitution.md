<!--
Sync Impact Report
- Version change: 0.0.0 → 1.0.0
- Modified principles: n/a (initial adoption)
- Added sections: Core Principles; Additional Constraints; Development Workflow & Quality Gates; Governance
- Removed sections: none
- Templates requiring updates:
  - .specify/templates/plan-template.md: ✅ aligned (no changes required)
  - .specify/templates/spec-template.md: ✅ aligned (no changes required)
  - .specify/templates/tasks-template.md: ✅ aligned (no changes required)
- Follow-up TODOs: none
-->

# NetHtml2Pdf Constitution

## Core Principles

### Code Quality & Standards
- MUST follow repository C# standards in `.cursor/rules/csharp-standards.mdc` and keep code
  clear, readable, and well-structured.
- MUST enable nullable reference types and Roslyn analyzers; new warnings MUST be addressed
  or suppressed with justification; CI treats warnings as errors for `src/*`.
- MUST maintain small, cohesive PRs; one change per PR; include rationale in description.
- MUST document public APIs with XML docs and provide minimal usage examples in README.

### Testing Standards
- MUST write tests before or alongside implementation for new behavior (unit and integration).
- MUST cover core conversion paths: element handling (headings, paragraphs, `br`, `div`,
  `section`), tables (`table/thead/tbody/tr/th/td`), and inline CSS subset.
- MUST validate output determinism with either hash-based checks on generated PDFs for fixed
  inputs/options or structural assertions (page count, text order, table cell text).
- MUST include regression tests for fixed defects to prevent reintroduction.
- SHOULD place unit tests in project-scoped test projects:
  - Parser unit tests in `src/NetHtml2Pdf.Parser.Test` (scope: parsing only)
  - Renderer unit tests in `src/NetHtml2Pdf.Renderer.Test` (scope: rendering only)
- MUST keep `src/NetHtml2Pdf.Test` for a minimal set of public API integration tests only (end-to-end HTML → PDF).
- MUST avoid cross-project unit tests; keep concerns isolated to the owning project.

### User Experience Consistency (Determinism)
- MUST produce deterministic or visually equivalent PDFs for the same input/options across
  Windows, macOS, and Linux.
- MUST avoid environment-dependent behavior (no headless browsers, no display servers, no
  external processes, no network access during conversion).
- MUST surface parser feedback from AngleSharp clearly; proceed with its normalization where
  possible; fail fast when no DOM can be produced or budget limits are exceeded.

### Performance & Resource Budgets
- SHOULD convert a typical 3–5 page document (including a table) in under 2 seconds on
  commodity hardware.
- SHOULD keep peak memory under 512 MB for typical inputs; avoid excessive CPU usage.
- MUST document and benchmark performance-sensitive changes; add/update performance tests
  when regressions are possible.

### Extensibility & API Stability
- MUST support pluggable handlers for elements/attributes/styles to extend behavior without
  modifying core conversion logic.
- MUST follow Semantic Versioning; avoid breaking public APIs outside a major version.
- SHOULD keep external dependencies minimal (AngleSharp, QuestPDF, .NET runtime only) to
  preserve footprint and portability.

## Additional Constraints
- Parsing MUST use AngleSharp; do not implement a custom HTML parser.
- CSS scope: support a minimal inline CSS subset (height, width, padding, margin, color,
  background-color, border, font-size, font-weight) and CSS classes; ignore others safely.
- Pagination: initial implementation may render long content on a single long page; provide
  a way to place content on a specific page (e.g., `AddPdfPage(string html)`).
- Offline operation: no external process invocation or network fetches during conversion.

## Development Workflow & Quality Gates
- All PRs MUST pass: build, analyzers (warnings-as-errors), unit/integration tests, basic
  benchmarks smoke, and formatting checks.
- PRs MUST include a Constitution Compliance checklist confirming adherence to principles
  (quality, testing, determinism) or a clear, temporary justification.
- Significant architectural decisions MUST include an ADR (Architecture Decision Record)
  documenting decision, rationale, alternatives, and consequences.
- Code Owners MUST review changes to core converter, parsing integration, and public API.

## Governance
- This Constitution guides technical decisions and implementation choices; in case of
  conflict, it supersedes informal practices.
- Amendments MUST be made via PR with version bump and Sync Impact Report at the top of this
  file.
- Versioning policy for this document:
  - MAJOR: backward-incompatible principle redefinitions or removals.
  - MINOR: new principle/section or materially expanded guidance.
  - PATCH: clarifications, wording, or typo fixes with no semantic change.
- Compliance: CI and reviewers MUST verify compliance; deviations require documented
  justification and, when applicable, follow-up tasks to return to compliance.

**Version**: 1.0.0 | **Ratified**: 2025-09-24 | **Last Amended**: 2025-09-24
# [PROJECT_NAME] Constitution
<!-- Example: Spec Constitution, TaskFlow Constitution, etc. -->

## Core Principles

### [PRINCIPLE_1_NAME]
<!-- Example: I. Library-First -->
[PRINCIPLE_1_DESCRIPTION]
<!-- Example: Every feature starts as a standalone library; Libraries must be self-contained, independently testable, documented; Clear purpose required - no organizational-only libraries -->

### [PRINCIPLE_2_NAME]
<!-- Example: II. CLI Interface -->
[PRINCIPLE_2_DESCRIPTION]
<!-- Example: Every library exposes functionality via CLI; Text in/out protocol: stdin/args → stdout, errors → stderr; Support JSON + human-readable formats -->

### [PRINCIPLE_3_NAME]
<!-- Example: III. Test-First (NON-NEGOTIABLE) -->
[PRINCIPLE_3_DESCRIPTION]
<!-- Example: TDD mandatory: Tests written → User approved → Tests fail → Then implement; Red-Green-Refactor cycle strictly enforced -->

### [PRINCIPLE_4_NAME]
<!-- Example: IV. Integration Testing -->
[PRINCIPLE_4_DESCRIPTION]
<!-- Example: Focus areas requiring integration tests: New library contract tests, Contract changes, Inter-service communication, Shared schemas -->

### [PRINCIPLE_5_NAME]
<!-- Example: V. Observability, VI. Versioning & Breaking Changes, VII. Simplicity -->
[PRINCIPLE_5_DESCRIPTION]
<!-- Example: Text I/O ensures debuggability; Structured logging required; Or: MAJOR.MINOR.BUILD format; Or: Start simple, YAGNI principles -->

## [SECTION_2_NAME]
<!-- Example: Additional Constraints, Security Requirements, Performance Standards, etc. -->

[SECTION_2_CONTENT]
<!-- Example: Technology stack requirements, compliance standards, deployment policies, etc. -->

## [SECTION_3_NAME]
<!-- Example: Development Workflow, Review Process, Quality Gates, etc. -->

[SECTION_3_CONTENT]
<!-- Example: Code review requirements, testing gates, deployment approval process, etc. -->

## Governance
<!-- Example: Constitution supersedes all other practices; Amendments require documentation, approval, migration plan -->

[GOVERNANCE_RULES]
<!-- Example: All PRs/reviews must verify compliance; Complexity must be justified; Use [GUIDANCE_FILE] for runtime development guidance -->

**Version**: [CONSTITUTION_VERSION] | **Ratified**: [RATIFICATION_DATE] | **Last Amended**: [LAST_AMENDED_DATE]
<!-- Example: Version: 2.1.1 | Ratified: 2025-06-13 | Last Amended: 2025-07-16 -->