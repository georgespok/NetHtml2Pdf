# Implementation Plan: Iteration 1 - Core HTML parsing & rendering

**Branch**: 001-iteration-1-parse | **Date**: 2025-10-06 | **Spec**: C:/Projects/Html2Pdf/specs/001-iteration-1-parse/spec.md
**Input**: Feature specification from C:/Projects/Html2Pdf/specs/001-iteration-1-parse/spec.md

## Execution Flow (/plan command scope)
1. Load feature spec from the input path.
   - If not found: ERROR "No feature spec at {path}".
2. Fill the Technical Context section (resolve NEEDS CLARIFICATION markers).
   - Detect project type from repository layout (single library vs multi-app).
   - Record the Structure Decision tied to Core, Parser, Renderer, and HtmlConverter layers.
   - Outline cross-platform compatibility approach for Iteration 1: managed-only dependencies ensure inherent cross-platform support.
3. Populate the Constitution Check section referencing the current NetHtml2Pdf Constitution.
   - Address every principle: Test-Driven Delivery, Managed Cross-Platform Fidelity, Layered Rendering Architecture, HTML & CSS Contract Clarity, Extensible Experience Stewardship, Architecture Planning & Validation.
4. Evaluate the Constitution Check results.
   - If violations exist: document them in Complexity Tracking with mitigation plans.
   - If violations cannot be justified: ERROR "Simplify approach first".
   - Update Progress Tracking: Initial Constitution Check.
5. Execute Phase 0 outputs -> research.md.
   - If NEEDS CLARIFICATION remain: ERROR "Resolve unknowns".
6. Execute Phase 1 outputs -> data-model.md, quickstart.md, contracts/, agent guidance file.
   - Ensure every new scenario has a failing test description.
7. Re-run the Constitution Check with Phase 1 insights.
   - If new violations appear: adjust design and repeat Phase 1.
   - Update Progress Tracking: Post-Design Constitution Check.
8. Outline Phase 2 task generation strategy (DO NOT create tasks.md).
9. STOP - handoff to `/tasks` command for task synthesis.

**IMPORTANT**: The /plan command stops at step 7. Phases 2+ are executed by other commands:
- Phase 2: `/tasks` command produces tasks.md.
- Phase 3-4: Implementation and validation follow task execution.

## Summary
Deliver Iteration 1 support for core HTML block/inline elements, lists, and tables by extending the parser and QuestPDF renderer while introducing fallback handling and diagnostics using managed-only dependencies for inherent cross-platform compatibility.

## Technical Context
**Language/Version**: C# 12 / .NET 8  
**Primary Dependencies**: QuestPDF, AngleSharp  
**Storage**: Streams and in-memory document graphs  
**Testing**: xUnit + Shouldly with coverlet  
**Target Platform**: Windows and Linux (CI agents)  
**Cross-Platform Compatibility**: Ensured through managed-only dependencies (.NET Core, QuestPDF, AngleSharp). Cross-platform issues addressed reactively as they arise.  
**Supported Markup Impact**: div, section, p, span, strong, b, i, br, ul, ol, li, table, thead, tbody, tr, th, td  
**Performance Goals**: No explicit target; capture render timings for monitoring and data collection via HtmlConverter timing logs (Tasks T039-T040).  
**Constraints**: Managed-only dependencies (verify via dependency audit task T004), questpdf renderer layering, fallback logging requirement  
**Scale/Scope**: Initial fixtures cover 2 paragraph scenarios, 1 table scenario, and 1 fallback scenario

## Constitution Check
*Gate: must pass before Phase 0 begins and after Phase 1 concludes.*

- **Test-Driven Delivery**: Add failing tests before implementation: (1) `HtmlParserTests.ParagraphsWithInlineStyles_ProducesInlineNodes`, (2) `PdfRendererTests.Lists_RenderBulletsAndNumbers`, (3) `HtmlConverterTests.Iteration1_Lists_RenderConsistentPdf`, (4) `PdfRendererTests.TableBorders_RenderConsistentStroke`, (5) `HtmlConverterTests.Iteration1_TableAlignment_MatchesExpectations`, (6) `HtmlConverterTests.Iteration1_Fallback_EmitsWarning`.
- **Managed Cross-Platform Compatibility**: Keep solution managed-only using .NET Core and cross-platform managed dependencies (QuestPDF, AngleSharp) to ensure inherent cross-platform compatibility. Cross-platform issues will be addressed reactively as they arise.
- **Layered Rendering Architecture**: Extend `Core/DocumentNode` for new node types, extend `Parser/HtmlNodeConverter` for mapping, implement rendering in `Renderer/PdfRenderer`, and expose only via `HtmlConverter` facade.
- **HTML & CSS Contract Clarity**: Document supported tags and CSS properties in spec/contracts, add fixtures for paragraphs, tables, and fallback, and ensure unsupported tags emit structured warnings.
- **Extensible Experience Stewardship**: Update README/changelog entries and add quickstart guidance for new fixtures and cross-platform validation steps.
- **Architecture Planning & Validation**: Complete planning phase with documented technical context and constitution compliance verification before beginning implementation. Ensure all architectural decisions align with constitution principles and are validated through the planning process.

## Project Structure

### Documentation (this feature)
```
specs/001-iteration-1-parse/
|-- plan.md              # /plan output (this file)
|-- research.md          # Phase 0 findings
|-- data-model.md        # Phase 1 entities/contracts
|-- quickstart.md        # Phase 1 validation walkthrough
|-- contracts/           # Phase 1 rendering contracts
|__ tasks.md             # /tasks output (Phase 2)
```

### Source Code (repository root)
```
src/
|-- NetHtml2Pdf/
|   |-- Core/
|   |-- Parser/
|   |-- Renderer/
|   |-- HtmlConverter.cs
|   |__ NetHtml2Pdf.csproj
|-- NetHtml2Pdf.Test/
|   |-- Core/
|   |-- Parser/
|   |-- Renderer/
|   |__ NetHtml2Pdf.Test.csproj
|__ NetHtml2Pdf.sln
```

`NetHtml2Pdf.TestConsole.csproj` provides the integration test console referenced in Phase 3.5.

**Structure Decision**: Parser updates occur under `src/NetHtml2Pdf/Parser/HtmlNodeConverter` with new element mappings; rendering logic lands in `src/NetHtml2Pdf/Renderer/` with table alignment helpers; style contracts remain in `src/NetHtml2Pdf/Core/`. Integration facades stay within `src/NetHtml2Pdf/HtmlConverter.cs`, regression tests mirror structure under `src/NetHtml2Pdf.Test`, and the integration console exercises end-to-end rendering scenarios.

## Phase 0: Outline & Research ✅ COMPLETE
1. ✅ Capture CSS-to-QuestPDF mapping references (alignment, borders) in `research.md`.
2. ✅ Document fallback logging approach and empty-element handling decisions.
3. ✅ Define cross-platform observation strategy including regression filter and integration test console comparison on both platforms.
4. ✅ Output: `research.md` (complete).

## Phase 1: Design & Contracts ✅ COMPLETE
*Prerequisite: research.md complete.*
1. ✅ Describe DocumentNode entities and style structures in `data-model.md`.
2. ✅ Author contracts in `contracts/` for paragraphs, tables, lists, and fallback scenarios with expected parser/render outputs.
3. ✅ Outline failing tests and integration expectations in the contracts and quickstart.
4. ✅ Provide validation walkthrough in `quickstart.md` covering Windows/Linux regression runs and integration test console comparison steps.
5. ✅ Queue `.specify/scripts/powershell/update-agent-context.ps1 -AgentType codex` to refresh agent guidance after design.
6. ✅ Output: `data-model.md`, `contracts/`, `quickstart.md` (complete).

## Phase 2: Task Planning Approach
*Describe what `/tasks` will generate; do not create tasks.md here.*
- Inputs: plan.md, research.md, data-model.md, contracts/.
- Strategy: Generate tasks that write tests before implementation, extend existing DocumentNode architecture, and include documentation + logging updates.
- Ordering: Setup (restore + dependency audit) -> Tests -> Core style mapping -> Parser element mapping -> Renderer alignment/border rendering -> HtmlConverter wiring -> docs/logging polish -> cross-platform validation.
- Parallelization: Mark [P] for independent test files (parser vs renderer) and for doc updates separate from code changes.

## Phase 3+: Future Implementation
- Phase 3: `/tasks` command emits tasks.md.
- Phase 4: Execute tasks (implementation, refactor, documentation).
- Phase 5: Validation (dotnet test filter, integration test console on Windows/Linux, quickstart walkthrough, performance sampling).

## Complexity Tracking
| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|---------------------------------------|
|           |            |                                       |

## Progress Tracking
**Phase Status**:
- [x] Phase 0: Research complete (/plan) ✅
- [x] Phase 1: Design complete (/plan) ✅
- [x] Phase 2: Task planning strategy recorded (/plan) ✅
- [ ] Phase 3: Tasks generated (/tasks)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS
- [x] All NEEDS CLARIFICATION resolved
- [ ] Complexity deviations documented

---
*Based on NetHtml2Pdf Constitution v1.3.0*

