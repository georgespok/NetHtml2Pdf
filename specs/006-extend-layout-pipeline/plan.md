# Implementation Plan: Phase 4 - Extended Formatting Context Coverage

**Branch**: 006-extend-layout-pipeline | **Date**: 2025-10-21 | **Spec**: [/specs/006-extend-layout-pipeline/spec.md](specs/006-extend-layout-pipeline/spec.md)
**Input**: Feature specification from /specs/006-extend-layout-pipeline/spec.md

## Summary

Phase 4 expands the NetHtml2Pdf layout pipeline to support additional CSS display modes behind feature flags. The work introduces dedicated formatting contexts for inline-block elements, data tables, and a preview flex layout, ensuring that pagination, adapters, and diagnostics operate over fragment trees rather than legacy composers. Each context remains flag-guarded so existing behaviour and parity workflows stay intact while teams iterate toward full adoption.

## Technical Context

**Language/Version**: C# on .NET 8.0  
**Primary Dependencies**: QuestPDF 2025.x, AngleSharp, Microsoft.Extensions.Logging  
**Storage**: N/A (all rendering in-memory)  
**Testing**: xUnit with FluentAssertions; behaviour-first tests per constitution  
**Target Platform**: Cross-platform (.NET 8)  
**Project Type**: Single library (src/NetHtml2Pdf) plus test projects  
**Performance Goals**: Maintain <=5% render-time delta versus legacy pipeline; hold adapter output size within +/-2% per success criteria  
**Constraints**: No flex-wrap support in preview (maps to `nowrap`); treat `border-collapse` as `separate` unless the advanced flag is enabled; tests must avoid reflection APIs and follow incremental TDD; adapters must stay QuestPDF-agnostic  
**Scale/Scope**: Library consumed by internal services producing 1-50 page PDFs with optional headers/footers

## Constitution Check

Status: **PASS** - plan adheres to current principles.

- Respect documented architectural layering; cross-layer access only via interfaces/adapters.  
  *Mitigation*: All new contexts live under src/NetHtml2Pdf/Layout/ and expose services via existing pagination/adapters seams.
- Public surface via approved facades/entry points; no leakage of internal types.  
  *Mitigation*: Feature flags remain surfaced through RendererOptions/PdfBuilder only.
- Prefer managed-only dependencies; native/platform deps require ADR.  
  *Mitigation*: No new dependencies introduced; reuse existing managed stack.
- Validate inputs with explicit exceptions and parameter names.  
  *Mitigation*: Extend validation helpers in formatting contexts and flag toggles.
- Emit structured warnings for unsupported features; preserve content when sensible.  
  *Mitigation*: Diagnostics hooks updated for each context with structured logging.
- Follow incremental TDD; trivial scaffolding exempt.  
  *Mitigation*: Task sequencing enforces tests before implementation per story.
- Tests must exercise observable behaviour and MUST NOT rely on reflection APIs.  
  *Mitigation*: All new tests verify rendered fragments/pagination artefacts.
- Keep docs/specs/contracts in sync with behaviour and supported capabilities.  
  *Mitigation*: Research/data-model/contracts/quickstart generated in this plan.

## Project Structure

### Documentation (this feature)

```
specs/006-extend-layout-pipeline/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── formatting-contexts.md
└── tasks.md (generated via /tasks)
```

### Source Code (repository root)

```
src/
├── NetHtml2Pdf/
│   ├── Layout/
│   │   ├── Pagination/
│   │   ├── FormattingContexts/
│   │   └── Diagnostics/
│   ├── Renderer/
│   │   ├── Adapters/
│   │   └── Interfaces/
│   └── PdfBuilder.cs
├── NetHtml2Pdf.Test/
│   ├── Layout/
│   └── Renderer/
└── NetHtml2Pdf.TestConsole/
```

**Structure Decision**: Retain the existing single-library layout. New contexts slot into Layout/FormattingContexts, and tests mirror namespace folders under NetHtml2Pdf.Test.

## Phases & Deliverables

| Phase | Goal | Key Outputs | Gate Criteria |
|-------|------|-------------|---------------|
| Phase 0 - Research | Capture current renderer state, feature flags, and risks | research.md | Research document reviewed; clarifications reflected |
| Phase 1 - Setup (Shared Infrastructure) | Confirm baseline health and stakeholder comms | Verified build/test log, roadmap flag section | Baseline build/test succeed; roadmap updated |
| Phase 2 - Foundational (Blocking Prerequisites) | Wire feature flags and context options before story work | RendererOptions/PdfBuilder updates, context factory scaffolding | Flags exposed end-to-end; layout pipeline flag-aware |
| Phase 3 - Contracts & Shared Tests | Establish shared regression scaffolding | FormattingContextsContractTests failing first | Contract tests authored and failing |
| Phase 4 - User Story 1 (Inline-Block) | Deliver inline-block formatting context end-to-end | Inline-block tests, implementation, diagnostics | New tests passing; legacy path preserved |
| Phase 5 - User Story 2 (Table) | Deliver table formatting context with pagination support | Table tests (headers, captions), implementation, diagnostics | Multi-page table scenarios pass with downgrades logged |
| Phase 6 - User Story 3 (Flex Preview) | Deliver preview flex formatting context | Flex tests/implementation/diagnostics | Preview constraints enforced; warnings emitted |
| Phase 7 - Integration & Regression | Validate combined behaviour, docs, telemetry | Regression suite, quickstart update, flex telemetry analysis | Layout deviation <2%; telemetry shows ≥80% legacy reduction |
| Phase 8 - Stabilisation & Polish | Performance validation, cleanup, final QA | Benchmark report, formatting cleanup, final e2e run | Performance delta <=5%; artefacts archived |

## Milestones & Gate Criteria

1. **M1 - Research Complete**: Clarified scope, risks, and open questions.
2. **M2 - Design Sign-off**: Data model and contracts approved; quickstart ready.
3. **M3 - Tasks Ready**: Work queue prioritised by user story and dependency gates.
4. **M4 - Functionality Delivered**: Inline-block, table, and flex flags functional with tests.
5. **M5 - Performance & Diagnostics**: Benchmarks captured; structured diagnostics verified.

## Risk & Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| Regression in legacy composer path | High | Preserve flag defaults; add regression fixtures comparing legacy vs flagged pipeline |
| Pagination edge cases for tables | Medium | Expand test fixtures covering multi-page tables, captions, mixed row heights |
| Flex preview misused in production | Medium | Document limitations (no wrapping, warning logs); keep flag default-off |
| Performance degradation | Medium | Benchmark 50-page fixture pre/post change; optimise hot spots before rollout |

## Testing Strategy

- Adopt Red-Green-Refactor for each context feature; tests precede implementation.
- Behaviour tests verify fragment geometry, pagination metadata, adapter logging.
- Regression suites run for legacy pipeline (flags off) and new pipeline (flags on).
- No reflection-based assertions; prefer public APIs or controlled diagnostics hooks.

## Progress Tracking

| Deliverable | Status | Notes |
|-------------|--------|-------|
| Phase 0 - Research | ✅ Completed | research.md created in this plan |
| Phase 1 - Setup | ⏳ Pending | Execute T001-T002 to confirm baseline health and roadmap update |
| Phase 2 - Foundational | ⏳ Pending | T003-T005 wire feature flags and context options |
| Phase 3 - Contracts & Shared Tests | ⏳ Pending | T006 establishes shared contract tests (fail first) |
| Phase 4 - User Story 1 | ⏳ Pending | T007-T011 deliver inline-block context and diagnostics |
| Phase 5 - User Story 2 | ⏳ Pending | T012-T016 cover multi-page tables with captions and downgrades |
| Phase 6 - User Story 3 | ⏳ Pending | T017-T019 enforce flex preview constraints |
| Phase 7 - Integration & Regression | ⏳ Pending | T020-T022 and T026 handle combined validation, docs, telemetry |
| Phase 8 - Stabilisation & Polish | ⏳ Pending | T023-T025 capture benchmarks, cleanup, and final QA |

## Complexity Tracking

_No constitutional violations anticipated; table intentionally left empty._
