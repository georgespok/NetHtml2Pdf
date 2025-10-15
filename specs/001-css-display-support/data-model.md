# Data Model: CSS Display

## Entities

### Style (conceptual)
- Fields:
  - Display: enum `CssDisplay` (Default, Block, InlineBlock, None)
- Rules:
  - Display derives from merged class styles overridden by inline styles.
  - Unsupported values map to Default and raise a structured warning.

## Relationships
- Style attaches to `DocumentNode` instances via `CssStyleMap`.

## Validation Rules
- Invalid/unknown `display` values are ignored → Default; emit a single warning.
- `display: none` on a node suppresses rendering for that node and its descendants.
