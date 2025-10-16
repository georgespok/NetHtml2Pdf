# Quickstart (Phase 1 Refactor)

## Goal
Refactor composers to use DisplayClassifier, WrapWithSpacing, and InlineFlowLayoutEngine without changing behavior.

## Steps
1. Run tests: `dotnet test`
2. Implement DisplayClassifier and unit tests
3. Wire classifier into composers; run tests
4. Add WrapWithSpacing; refactor call sites; run tests
5. Extract InlineFlowLayoutEngine; delegate from InlineComposer; run tests

## Flags
- Classifier trace (global bool): logs node path, source (explicit vs semantic), and classification.
