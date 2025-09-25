# Research: Cross-platform HTML-to-PDF Library

## Inter Font Coverage and Embedding
- Decision: Bundle Inter regular/italic/bold for deterministic output.
- Rationale: Avoid platform font differences; ensure Unicode coverage for common scripts.
- Alternatives: Use PDF Base-14 (Helvetica/Times) — rejected due to determinism and licensing nuances.

## AngleSharp Normalization Behavior
- Decision: Rely on AngleSharp’s HTML5 parsing/normalization; accept fragments.
- Rationale: Mature, standards-compliant parser reduces custom logic.
- Alternatives: Custom sanitizer — rejected as out of scope.

## Pagination Composition in QuestPDF
- Decision: Explicit pages via `AddPdfPage(string html)`; no implicit pagination.
- Rationale: Keeps responsibilities clear and predictable; caller controls paging.
- Alternatives: Auto-paginate — rejected for initial version simplicity and determinism.
