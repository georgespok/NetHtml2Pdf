# Contract: Phase 1 Seams

## DisplayClassifier
- Input: (DocumentNode node, CssStyleMap style)
- Output: DisplayClass = { Block | Inline | InlineBlock | None }
- Constructor: Accepts optional ILogger<DisplayClassifier> for structured logging
- Policies:
  - Explicit CSS display wins
  - Semantic defaults:
    - Block: div, section, p, h1–h6, ul/ol, li, table
    - Inline: span, strong, bold, italic, text, br
  - ListItem defaults to Block
  - Table sub-elements (thead/tbody/tr/th/td/section) handled via Table context (no explicit classification)
  - Unknown/unsupported types: warn once per node type using ILogger.LogWarning; treat as Block
  - Unsupported CSS display values: warn once per value using ILogger.LogWarning; fallback to semantic default

## WrapWithSpacing
- Order: Margin (parent) → Border (element) → Padding (element)
- Behavior: Preserve current visual output exactly
- Nested containers: cumulative parent-padding simulation (no clamping)

## InlineFlowLayoutEngine
- Traversal: text, span, strong/bold, italic, br, inline-block (as currently)
- Style inheritance: bold/italic/underline/font size per current behavior
- Validations: keep simplified inline-block checks
