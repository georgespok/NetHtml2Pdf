# Data Model: Cross-platform HTML-to-PDF Library

## Entities

### DocumentOptions
- pageSize: Letter (8.5x11 in)
- orientation: Portrait
- margins: 1 inch on all sides
- fontFamily: Inter (bundled)

### Page
- htmlContent: string (HTML fragment)

### PdfDocument
- pages: List<Page>
- options: DocumentOptions

## Validation Rules
- htmlContent MUST be non-empty and valid Unicode
- Input size per page MUST be <= 64KB (configurable)
- Unsupported elements preserved as inner text
- Unsupported CSS properties ignored silently
