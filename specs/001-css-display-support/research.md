# Research: CSS Display Support (Block, Inline-Block, None)

## Decisions

- Display values supported: `block`, `inline-block`, `none`.
- Unsupported values (`flex`, `grid`, etc.) → structured warning + fallback to HTML semantic default.
- Inline-block vertical alignment: middle.
- Inline-block top/bottom margins: do not affect line box height.

## Rationale

- Aligns with pragmatic subset for reports; avoids overreach into complex layout models.
- Predictable behavior and compatibility with existing renderer abstractions.
- Warnings make unsupported usage visible without failing documents.

## Alternatives Considered

- Fallback to project-wide default (block) → Rejected: breaks inline semantics for spans.
- Inline-block baseline alignment → Rejected: middle yields more predictable visual centering across typical report fonts.
- Increasing line height for inline-block margins → Rejected: complicates line layout and deviates from simplified model.
