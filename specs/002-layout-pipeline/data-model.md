# Data Model (Phase 1): Seams and Services

## Entities / Services

- DisplayClassifier
  - Input: DocumentNode, CssStyleMap
  - Output: { Block | Inline | InlineBlock | None }
  - Rules: Explicit CSS display wins; otherwise semantic defaults; `display:none` → None

- WrapWithSpacing
  - Input: parent container, CssStyleMap
  - Behavior: Apply margin (parent), then border (element), then padding (element)
  - Parity: Preserve current visual output exactly

- InlineFlowLayoutEngine
  - Input: TextDescriptor, DocumentNode, InlineStyleState
  - Behavior: Traverse inline content (text/span/strong/bold/italic/br/inline-block),
    apply style inheritance, and emit runs via TextDescriptor

## Deferred (Phase 2+)
- LayoutBox, LayoutFragment, LayoutEngine, Pagination
