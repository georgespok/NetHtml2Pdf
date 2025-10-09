# Contract: Core Paragraphs Rendering

**Contract ID**: core-paragraphs  
**Version**: 1.0  
**Date**: 2025-01-27  
**Scope**: Iteration 1 - Basic paragraph and text element rendering

## Input Contract

### HTML Input
```html
<!DOCTYPE html>
<html>
<head>
    <style>
        .highlight { background-color: yellow; }
        .emphasis { font-weight: bold; }
    </style>
</head>
<body>
    <h1>Main Heading</h1>
    <p>This is a <strong>bold</strong> paragraph with <em>emphasis</em>.</p>
    <p class="highlight">This paragraph has a <span class="emphasis">highlighted</span> section.</p>
    <div>
        <p>Nested paragraph in a div container.</p>
    </div>
</body>
</html>
```

### Expected Parser Output
- **DocumentNode** with `NodeType.Document` as root
- **Children**: 1 `DocumentNode` with `NodeType.Paragraph` (body content)
- **Heading1**: `NodeType.Heading1`, `TextContent="Main Heading"`
- **Paragraph1**: `NodeType.Paragraph`, `TextContent="This is a bold paragraph with emphasis."`
- **Paragraph2**: `NodeType.Paragraph`, `TextContent="This paragraph has a highlighted section."`, `Styles` with resolved CSS
- **Div**: `NodeType.Div`, contains 1 child `DocumentNode` with `NodeType.Paragraph`

### Expected CSS Resolution
- **Class "highlight"**: `background-color: yellow`, `Source: Class`
- **Class "emphasis"**: `font-weight: bold`, `Source: Class`
- **Inline styles**: None in this example
- **Cascade order**: Class styles applied to elements with matching classes

## Output Contract

### PDF Structure
- **Document**: QuestPDF document with proper page setup
- **Heading1**: Rendered as large, bold text at top of page
- **Paragraph1**: Rendered with normal text, bold and italic emphasis preserved
- **Paragraph2**: Rendered with yellow background, bold text for emphasized span
- **Nested paragraph**: Rendered within div container context

### Visual Expectations
- **Typography**: Consistent font family throughout document
- **Spacing**: Proper line spacing between paragraphs
- **Emphasis**: Bold and italic text rendered correctly
- **Background**: Yellow background applied to highlighted paragraph
- **Layout**: Proper indentation and spacing for nested elements

## Test Scenarios

### Scenario 1: Basic Paragraph Rendering
**Input**: Simple paragraph with text content  
**Expected**: PDF with properly formatted paragraph text  
**Validation**: Text content matches input, proper spacing applied

### Scenario 2: Text Emphasis Rendering
**Input**: Paragraph with `<strong>` and `<em>` elements  
**Expected**: PDF with bold and italic text rendering  
**Validation**: Emphasis styles applied correctly

### Scenario 3: CSS Class Application
**Input**: Paragraph with CSS class containing background color  
**Expected**: PDF with background color applied  
**Validation**: Background color matches CSS specification

### Scenario 4: Nested Element Rendering
**Input**: Div containing paragraph  
**Expected**: PDF with proper nesting and indentation  
**Validation**: Nested structure preserved in output

## Failure Cases

### Invalid CSS Class
**Input**: Paragraph with non-existent CSS class  
**Expected**: Warning log, fallback to default styling  
**Validation**: Warning logged, document renders without error

### Malformed HTML
**Input**: Unclosed paragraph tags  
**Expected**: AngleSharp normalization, proper rendering  
**Validation**: Document renders correctly despite malformed input

### Empty Elements
**Input**: Empty paragraph tags  
**Expected**: Minimal height paragraph (4pt)  
**Validation**: Empty elements rendered with minimal height

## Performance Expectations

### Timing Requirements
- **Parse time**: < 100ms for document of this size
- **Render time**: < 500ms for document of this size
- **Total time**: < 1 second end-to-end

### Memory Requirements
- **Memory usage**: < 10MB for document of this size
- **Peak memory**: < 20MB during processing

## Cross-Platform Validation

### Windows Output
- **File size**: Expected range 15-25KB
- **Visual appearance**: Consistent typography and spacing
- **Metadata**: Platform-specific creation date acceptable

### Linux Output
- **File size**: Expected range 15-25KB
- **Visual appearance**: Consistent typography and spacing
- **Metadata**: Platform-specific creation date acceptable

### Comparison Criteria
- **Content**: Identical text content
- **Layout**: Identical spacing and positioning
- **Styling**: Identical font weights and background colors
- **Tolerance**: Acceptable differences in metadata and font rendering

---

**Contract Complete**: Core paragraph rendering contract defined with input/output specifications and validation criteria.