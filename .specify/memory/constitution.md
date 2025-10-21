<!--
Sync Impact Report
- Version change: 2.0.0 → 2.1.0
- Modified principles:
  - Test Strategy and Coverage Discipline (clarified incremental TDD, observable behavior, and reflection ban)
- Added sections: none
- Removed sections: none
- Templates requiring updates:
  - .specify/templates/plan-template.md ✅ updated
  - .specify/templates/spec-template.md ✅ no change required
  - .specify/templates/tasks-template.md ✅ updated
  - docs/testing-guidelines.md ✅ updated
- Follow-up TODOs: none
-->

# NetHtml2Pdf Constitution

## Core Principles

### Layering and Public API
Respect the layering documented in current architecture documents. Cross-layer access MUST occur only via interfaces or sanctioned adapters; skip-level dependencies are forbidden. The public API surface MUST remain stable; expose capabilities through approved facades/entry points only. Internal architecture names and structure may evolve as documented in design artifacts.
Rationale: Clear boundaries enable testability, maintainability, and safe evolution without leaking internal details.

### Dependencies and Portability
Prefer managed, cross-platform dependencies. Introducing native or platform-specific dependencies requires an Architecture Decision Record (ADR), justification, and rollback plan. Wrap third-party libraries behind adapters so they can be replaced without touching core logic.
Rationale: Ensures portability, reduces operational risk, and preserves swap-ability.

### Test Strategy and Coverage Discipline
Tests are first-class and focus on observable behavior, not implementation details. Code MUST follow incremental TDD: introduce one failing test, implement the minimal passing code, then refactor before the next test. Trivial scaffolding (constructors, simple properties, passive DTOs) is exempt. Tests MUST exercise outcomes such as rendered output, pagination results, logging, or API responses and MUST NOT rely on reflection-based contract checks. Reflection APIs (e.g., Activator.CreateInstance, Type.GetType, MethodInfo.Invoke) are prohibited in test code. Prioritize tests for business logic and complex flows, use parameterized tests for multi-scenario logic, and keep tests independent and readable. Each layer/module MUST be testable in isolation.
Rationale: Guarantees quality, documents behavior, and enables safe refactoring.

### Input Validation and Structured Diagnostics
Validate inputs early and fail fast with specific exceptions (e.g., ArgumentException, ArgumentNullException) and clear parameter names. Unsupported capabilities SHOULD emit structured warnings rather than failing silently, and the supported feature set MUST be documented with fixtures when applicable.
Rationale: Predictability and debuggability in production scenarios.

### Clarity Over Cleverness (SOLID, DRY, KISS, YAGNI)
Favor small, focused functions with minimal parameters; prefer composition over inheritance; avoid premature abstractions; return domain objects instead of primitives; keep comments for non-obvious behavior. Follow repository C# conventions (four-space indentation, brace style, PascalCase for public members, camelCase for locals/parameters, explicit braces for control blocks, prefer var when type is obvious, enable Nullable and ImplicitUsings). Use dotnet format for sizable refactors; align namespaces with folders; one type per file.
Rationale: Readability and maintainability over time.

## Supported Scope and Contracts
This project targets practical document generation. The supported feature set is defined in specifications and contracts and may be a subset of a broader standard. Requirements:
- Maintain a living document of supported capabilities and constraints.
- Provide fixtures/examples for representative combinations.
- For unsupported inputs, preserve content where reasonable and emit structured warnings.
- Avoid hidden behavior; make layout/pagination decisions explicit in docs.
Scope evolves progressively; each addition MUST include docs, tests, and contracts.

## Development Workflow and Quality Gates
- Progressive iterations only: deliver small, functional, backward-compatible increments. Each iteration includes spec → plan → implementation → tests.
- Code reviews MUST verify: layering compliance, dependency policy, public surface containment, validation/warnings, and adequate tests for new logic (following incremental TDD and behavior-focused rules).
- Versioning: Semantic Versioning (MAJOR.MINOR.PATCH). Backward incompatible API or behavioral changes require a MAJOR bump and migration notes.
- Documentation: Update README/specs/contracts when behavior or scope changes.

## Governance
This constitution supersedes conflicting practices. Amendments require:
- A documented rationale, updated tests/contracts, and migration guidance when applicable.
- Version bump according to impact (see versioning above).
- Compliance review in PRs verifying adherence to Core Principles and Quality Gates. Deviations MUST be justified in the PR description.

Governance metadata:
- Public API changes and architecture-level changes require an ADR.
- Compliance reviews are expected at least once per release.

**Version**: 2.1.0 | **Ratified**: 2025-10-16 | **Last Amended**: 2025-10-21
