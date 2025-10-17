# Contract: Phase 2 Layout Model

## BlockFormattingContext
- Input: `(LayoutBox parent, LayoutConstraints constraints)` with paragraphs/headings children.
- Output: Ordered `LayoutFragment` collection preserving DOM order, spacing, and baseline metrics.
- Rules:
  - Applies spacing in order margin(parent) -> border(element) -> padding(element).
  - Collapses vertical spacing equivalently to legacy BlockComposer.
  - Delegates inline descendants to InlineFormattingContext and embeds returned fragments.
  - Emits diagnostics payload `{ nodePath, context:"Block", constraints, size }` when tracing enabled.

## InlineFormattingContext
- Input: Inline `LayoutBox` tree representing spans/text nodes.
- Output: List of `LayoutFragment` line boxes with `baseline`, `size.Width`, `size.Height` identical to InlineFlowLayoutEngine results.
- Rules:
  - Uses existing InlineFlowLayoutEngine to compute line breaks and glyph runs.
  - Inherits typography (bold/italic/font size) as per Phase 1 behavior.
  - Includes DOM node path in fragment diagnostics.

## LayoutEngine (Facade)
- Input: `(DocumentNode node, LayoutConstraints constraints, LayoutDiagnosticsOptions options)`.
- Output: `LayoutResult` containing fragment tree and aggregated diagnostics metadata.
- Rules:
  - Builds `LayoutBox` tree using DisplayClassifier (Phase 1).
  - Chooses Block vs Inline context based on DisplayClass; unsupported displays return `LayoutResult.LegacyFallback` marker.
  - Respects `EnableNewLayoutForTextBlocks` flag; when false returns `LayoutResult.Disabled` without invoking contexts.

## BlockComposer Integration
- When flag enabled and node is paragraph/heading/span, composer MUST:
  - Call `ILayoutEngine` and iterate returned fragments `foreach fragment -> QuestPDF`.
  - On `LayoutResult.LegacyFallback`, defer to legacy composer path.
  - On exception, bubble error to caller (no silent fallback).
- When flag disabled, legacy path remains unchanged.

## Diagnostics & Logging
- Structured log event `LayoutEngine.FragmentMeasured` recorded per fragment with fields:
  - `nodePath`, `context`, `width`, `height`, `baseline`, `constraints.inlineMax`, `constraints.blockMax`.
- Fragment tree serialization occurs only when diagnostics flag is ON; payload is attached to the same log event as `fragments` JSON.

## Parity Tests
- Fixture HTML documents featuring paragraphs, headings, spans (nested emphasis, soft breaks, long words).
- Assertions:
  - Flag OFF output matches baseline snapshot.
  - Flag ON output (PDF text extraction + layout metadata) matches baseline.
  - Diagnostics payload present only when tracing enabled.

## Feature Flag
- `ConverterOptions.EnableNewLayoutForTextBlocks` default `false`.
- `PdfBuilder` MUST propagate option to `PdfRenderer` and composers.
- Contracts verify toggling flag at runtime affects only targeted elements.
