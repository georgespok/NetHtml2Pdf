# NetHtml2Pdf: Layered Layout Pipeline Architecture

## Overview

Goal: separate layout logic from rendering by introducing a stable, testable layout layer between the DOM (HTML/CSS) and QuestPDF. The layout layer converts a styled box tree into positioned fragments (with sizes, baselines, and pagination-ready structure). Rendering then becomes a simple adapter from fragments to QuestPDF primitives.

This enables incremental support for complex CSS rules, keeps Block/Inline composers small, improves testability (geometry-first assertions), and isolates pagination and rendering concerns.

Related docs: `docs/coding-standards.md`, `docs/testing-guidelines.md`.

## Layered Model

- DOM + CSSOM (existing)
  - Parsing and style resolution stay in `Parser/` and `Core/` (`DocumentNode`, `CssStyleMap`).

- Style System (existing)
  - Cascade/merge into immutable `CssStyleMap`. Cache by hash for reuse.

- Box/Layout Tree (new)
  - `LayoutBox` is derived from DOM + computed styles. It normalizes margins/borders/padding and classifies display (block/inline/inline-block/etc.).
  - This tree is the layout input; layout never touches the DOM directly.

- Formatting Contexts (new, pluggable)
  - Strategy per context: Block, Inline, Inline-Block, Table, Flex (later Grid).
  - Each context handles its own flow: line building, baselines, margin rules, etc.

- Fragmentation/Pagination (new)
  - Split layout results across pages/columns into `LayoutFragment` trees per page based on `PageConstraints`.

- Rendering Adapter (new)
  - Stateless projection of fragment trees to QuestPDF primitives.

## Core Contracts (illustrative)

```csharp
public interface IFormattingContext
{
    LayoutFragment Layout(LayoutBox box, LayoutConstraints constraints, IMeasureContext measure);
}

public interface ILayoutEngine
{
    LayoutFragment LayoutTree(LayoutBox root, PageConstraints page);
}

public interface IRendererAdapter
{
    void Render(LayoutFragment fragment, ICanvas canvas);
}

public interface IDisplayClassifier
{
    DisplayClass Classify(DocumentNode node, CssStyleMap style);
}

public interface IMeasureContext
{
    FontMetrics GetFontMetrics(FontKey font, double fontSize);
    double MeasureTextWidth(string text, FontKey font, double fontSize);
}
```

## Data Flow

1) Parse HTML → DOM (`DocumentNode`) and resolve styles (`CssStyleMap`).
2) Build `LayoutBox` tree (applies display classification, normalizes box model).
3) `ILayoutEngine` selects a `IFormattingContext` per box and computes fragments via a two‑phase measure/arrange approach.
4) Pagination splits fragments by `PageConstraints`.
5) `IRendererAdapter` renders fragments to QuestPDF.

## Formatting Contexts (high level)

- Block Formatting Context
  - Normal flow, width rules, optional margin collapsing (behind a flag initially).

- Inline Formatting Context
  - Word wrap, whitespace handling, line boxes, baseline alignment, inline decorations; treats inline‑block as atomic inline.

- Inline-Block Context
  - Atomic inline with block-like box model for sizing; integrates with inline context for placement.

- Table/Flex/Grid (later)
  - Added iteratively, each behind a feature flag with graceful fallbacks.

## Fragmentation & Pagination

- Input: `LayoutFragment` tree with absolute positions in a flow.
- Output: page-partitioned fragment trees honoring `PageConstraints` (size, margins, header/footer areas).
- Pagination is orthogonal to layout rules; contexts only compute geometry.

## Rendering Adapter

- Stateless translation from `LayoutFragment` → QuestPDF calls.
- All visual styling (fills, borders, text runs) comes from fragment properties; no layout decisions here.

## Performance & Testability

- Cache computed styles and font metrics; treat `CssStyleMap` as immutable keys.
- Tests per layer:
  - Parser/Style: string → DOM/CSS asserts.
  - Layout: DOM → LayoutBox → LayoutFragment geometry (positions, sizes, baselines, wraps).
  - Pagination: fragment split assertions across pages.
  - Renderer adapter: verify mapping from fragments to QuestPDF primitives (no pixel diffs by default).

## Feature Flags & Fallbacks

- Flags: `EnableMarginCollapsing`, `EnableTableBorderCollapse`, `EnableInlineBlockBorders`, `EnablePaginationTrace`.
- Fallbacks: degrade to block formatting when unsupported; predictable behavior.

## Observability

- Structured logs: node path, computed display, chosen context, measured sizes.
- Optional trace dumps of layout tree and fragment trees for debugging.

## Migration Plan (Incremental)

Phase 1: Extract seams, preserve behavior
- Add `DisplayClassifier`: one place to map CSS/semantic defaults → `DisplayClass`.
- Add a spacing wrapper (`WrapWithSpacing`) to DRY margin/border/padding decisions (still calls QuestPDF for now).
- Introduce `InlineFlowLayoutEngine` and make `InlineComposer` delegate to it (behavior intact).

Phase 2: Introduce Layout Model and Engine
- Add `LayoutBox`, `LayoutConstraints`, `LayoutFragment`, `ILayoutEngine`.
- Implement `BlockFormattingContext` and `InlineFormattingContext` for paragraphs/headings/spans.
- Adapt `BlockComposer` to consume fragments for those elements while tables/lists use legacy paths (behind flags).

Phase 3: Pagination and Rendering Adapter
- Implement pagination to produce per-page fragment trees using `PageConstraints`.
- Move QuestPDF usage into `IRendererAdapter` (e.g., `QuestPdfAdapter`).
- `PdfRenderer` orchestrates: DOM → Layout → Fragments → Adapter.

Phase 4: Extend Context Coverage
- Add `InlineBlockFormattingContext` and migrate inline-block handling.
- Move tables into `TableFormattingContext`; gate `border-collapse` and complex rules with flags.
- Add Flex (and Grid later) incrementally with fallbacks.

## References in Code

- New layout-layer types include class-level documentation linking to this document for rationale and contracts.
- Renderer adapter and layout engine summarize their responsibilities with a pointer to this doc.

---

Document path: `docs/architecture-layout-pipeline.md`
This file is the authoritative reference for the layout architecture and migration.

