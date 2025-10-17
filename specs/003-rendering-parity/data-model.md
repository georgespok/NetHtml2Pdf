# Data Model (Phase 2): Layout Fragments Behind Flag

## Core Layout Types

- **LayoutBox**
  - Represents a classified DOM node (paragraph/heading/span) prepared for layout.
  - Fields: `DocumentNode node`, `DisplayClass display`, `CssStyleMap style`, `BoxSpacing spacingPlan`, `IReadOnlyList<LayoutBox> children`.
  - Lifecycle: constructed from DOM traversal; cached per render invocation; consumed by formatting contexts once per layout pass.

- **LayoutConstraints**
  - Captures available geometry for a node: `FloatRange inlineSize`, `FloatRange blockSize`, `float pageRemainingBlockSize`, `bool allowBreaks`.
  - Derived from caller-supplied page size; passed top-down with adjustments per formatting context.

- **LayoutFragment**
  - Immutable measurement result: `Size size`, `float? baseline`, `IReadOnlyList<LayoutFragment> children`, `string nodePath` (for diagnostics), `LayoutDiagnostics diagnostics` (optional extra metrics).
  - Produced by formatting contexts and consumed by BlockComposer conversion loop.

## Formatting Contexts & Facade

- **BlockFormattingContext**
  - Input: parent `LayoutConstraints`, sequence of block-level `LayoutBox` children.
  - Output: ordered fragments with block offsets, respecting spacing plan and ensuring parity with legacy composer.
  - Responsibilities: spacing application, line stacking strategy, inline context delegation.

- **InlineFormattingContext**
  - Input: inline `LayoutBox` tree; uses `InlineFlowLayoutEngine` to produce line boxes and baseline metrics.
  - Output: `LayoutFragment` list representing measured lines with baseline/inline extents.
  - Preserves existing inline flow semantics by reusing Phase 1 engine outputs.

- **ILayoutEngine** (facade)
  - API: `LayoutResult Layout(DocumentNode root, LayoutConstraints constraints, LayoutContext context)` (exact signature TBD but internal).
  - Responsibilities: build `LayoutBox` tree, dispatch to appropriate formatting context (block/inline), collate fragments, emit diagnostics.
  - Implementation: `LayoutEngine` (internal) orchestrates `DisplayClassifier`, contexts, and diagnostics strategy.

## Feature Flag & Options

- **ConverterOptions.EnableNewLayoutForTextBlocks**
  - Boolean; default `false`.
  - Propagated through `PdfBuilder` -> `PdfRendererFactory` -> `BlockComposer` to toggle new pipeline.

## Diagnostics Artifacts

- **LayoutDiagnostics**
  - Optional struct stored on fragments when diagnostics enabled.
  - Captures: `LayoutConstraints appliedConstraints`, `Size measuredSize`, `string contextName`, `Dictionary<string,string>` extras for future pagination trace.

## Relationships & Flow

1. `PdfBuilder` receives `ConverterOptions` with layout flag.
2. `PdfRenderer` resolves `ILayoutEngine` and passes root DOM node + page constraints when flag is true.
3. `LayoutEngine` builds `LayoutBox` tree using `DisplayClassifier` and existing style maps.
4. `BlockFormattingContext` iterates block children; delegates inline descendants to `InlineFormattingContext`.
5. Fragments returned to `BlockComposer`, which iterates fragments -> emits QuestPDF elements (no structural change to final PDF).
6. Diagnostics pipeline logs fragment payloads when tracing enabled.

## Deferred / Future Phases

- Pagination splitting (`PageConstraints`, `FragmentPaginator`).
- Additional formatting contexts: inline-block, tables, flex.
- Renderer abstraction (`QuestPdfAdapter`) to replace direct composer usage.
