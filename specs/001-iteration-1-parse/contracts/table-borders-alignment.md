# Contract: Table Borders and Alignment

**Contract ID**: table-borders-alignment  
**Version**: 1.0  
**Date**: 2025-01-27  
**Scope**: Iteration 1 - Table rendering with borders and cell alignment

## Input Contract

### HTML Input
```html
<!DOCTYPE html>
<html>
<head>
    <style>
        .bordered-table {
            border: 2px solid black;
            border-collapse: collapse;
        }
        .header-cell {
            background-color: #f0f0f0;
            text-align: center;
            vertical-align: middle;
        }
        .data-cell {
            text-align: left;
            vertical-align: top;
        }
        .right-aligned {
            text-align: right;
        }
    </style>
</head>
<body>
    <table class="bordered-table">
        <thead>
            <tr>
                <th class="header-cell">Name</th>
                <th class="header-cell">Age</th>
                <th class="header-cell">City</th>
            </tr>
        </thead>
        <tbody>
            <tr>
                <td class="data-cell">John Doe</td>
                <td class="data-cell right-aligned">25</td>
                <td class="data-cell">New York</td>
            </tr>
            <tr>
                <td class="data-cell">Jane Smith</td>
                <td class="data-cell right-aligned">30</td>
                <td class="data-cell">Los Angeles</td>
            </tr>
        </tbody>
    </table>
</body>
</html>
```

### Expected Parser Output
- **Table**: `NodeType.Table`, `Styles` with resolved CSS
- **TableHead**: `NodeType.TableHead`, contains 1 `DocumentNode` with `NodeType.TableRow`
- **TableRow**: `NodeType.TableRow`, contains 3 `DocumentNode` elements with `NodeType.TableHeaderCell`
- **TableHeader**: Each with `NodeType.TableHeaderCell`, `Styles` with resolved CSS
- **TableBody**: `NodeType.TableBody`, contains 2 `DocumentNode` elements with `NodeType.TableRow`
- **TableData**: Each with `NodeType.TableCell`, appropriate resolved styles

### Expected CSS Resolution
- **Table borders**: `border: 2px solid black`, `border-collapse: collapse`
- **Header styling**: `background-color: #f0f0f0`, `text-align: center`, `vertical-align: middle`
- **Data styling**: `text-align: left`, `vertical-align: top`
- **Right alignment**: `text-align: right` for age column

## Output Contract

### PDF Structure
- **Table**: QuestPDF table with proper border styling
- **Headers**: Centered text with background color and middle vertical alignment
- **Data cells**: Left-aligned text with top vertical alignment
- **Age column**: Right-aligned numbers
- **Borders**: 2px solid black borders around table and cells

### Visual Expectations
- **Table borders**: Consistent 2px black borders around entire table
- **Cell borders**: Borders between all cells (collapse mode)
- **Header background**: Light gray background (#f0f0f0) for header cells
- **Text alignment**: Center-aligned headers, left-aligned data, right-aligned numbers
- **Vertical alignment**: Middle-aligned headers, top-aligned data
- **Spacing**: Proper padding within cells

## Test Scenarios

### Scenario 1: Basic Table Rendering
**Input**: Simple table with headers and data  
**Expected**: PDF with properly formatted table structure  
**Validation**: Table structure preserved, borders applied

### Scenario 2: Border Styling
**Input**: Table with border-collapse and border styling  
**Expected**: PDF with collapsed borders and consistent border width  
**Validation**: Borders render correctly, collapse mode applied

### Scenario 3: Cell Alignment
**Input**: Table with different text alignments per column  
**Expected**: PDF with proper text alignment in each column  
**Validation**: Text alignment matches CSS specifications

### Scenario 4: Background Colors
**Input**: Table with background colors on header cells  
**Expected**: PDF with background colors applied to headers  
**Validation**: Background colors render correctly

### Scenario 5: Vertical Alignment
**Input**: Table with different vertical alignments  
**Expected**: PDF with proper vertical text positioning  
**Validation**: Vertical alignment applied correctly

## Failure Cases

### Missing CSS Classes
**Input**: Table without CSS styling  
**Expected**: Default table rendering with basic borders  
**Validation**: Table renders with default styling

### Invalid Border Properties
**Input**: Table with invalid border values  
**Expected**: Warning log, fallback to default borders  
**Validation**: Warning logged, default borders applied

### Empty Table Cells
**Input**: Table with empty cells  
**Expected**: Empty cells rendered with proper spacing  
**Validation**: Empty cells maintain table structure

## Performance Expectations

### Timing Requirements
- **Parse time**: < 200ms for table of this size
- **Render time**: < 800ms for table of this size
- **Total time**: < 1.5 seconds end-to-end

### Memory Requirements
- **Memory usage**: < 15MB for table of this size
- **Peak memory**: < 30MB during processing

## Cross-Platform Validation

### Windows Output
- **File size**: Expected range 20-35KB
- **Visual appearance**: Consistent table borders and alignment
- **Metadata**: Platform-specific creation date acceptable

### Linux Output
- **File size**: Expected range 20-35KB
- **Visual appearance**: Consistent table borders and alignment
- **Metadata**: Platform-specific creation date acceptable

### Comparison Criteria
- **Content**: Identical table data and structure
- **Borders**: Identical border styling and collapse behavior
- **Alignment**: Identical text and vertical alignment
- **Backgrounds**: Identical background colors
- **Tolerance**: Acceptable differences in metadata and font rendering

## CSS Property Support

### Supported Properties
- `border`: Border width, style, and color
- `border-collapse`: Table border collapse behavior
- `border-width`: Border width specification
- `border-style`: Border style (solid, dashed, etc.)
- `border-color`: Border color specification
- `text-align`: Horizontal text alignment (left, center, right)
- `vertical-align`: Vertical text alignment (top, middle, bottom)
- `background-color`: Cell background color

### Default Values
- `border-collapse`: separate
- `text-align`: left
- `vertical-align`: top
- `border`: 1px solid black (if not specified)

---

**Contract Complete**: Table borders and alignment contract defined with comprehensive styling specifications and validation criteria.