# NetHtml2Pdf

A .NET library for converting HTML content to PDF using QuestPDF with extensible architecture.

## Overview

NetHtml2Pdf is a modern, extensible HTML-to-PDF conversion library that supports various HTML elements including tables, paragraphs, line breaks, divisions, and sections. The library is designed to ensure maintainability, testability, and extensibility. The goal is to create a pure .NET cross-platform library that runs on Windows and Linux without requiring GDI+ dependencies.

## Features

- **Multiple HTML Element Support**: Tables, paragraphs (`<p>`), line breaks (`<br>`), divisions (`<div>`), and sections (`<section>`)
- **Extensible Design**: Easy to add support for new HTML elements
- **Console Application**: Command-line tool for local HTML-to-PDF conversion
- **Comprehensive Testing**: Full test coverage with unit tests


## Quick Start

### 1. Install the Package

```bash
dotnet add package NetHtml2Pdf
```

### 2. Basic Usage

```csharp
using NetHtml2Pdf;

var html = @"
    <section>
        <h1>My Document</h1>
        <p>This is a sample document.</p>
    </section>
    
    <table>
        <thead>
            <tr><th>Name</th><th>Age</th></tr>
        </thead>
        <tbody>
            <tr><td>Alice</td><td>30</td></tr>
            <tr><td>Bob</td><td>25</td></tr>
        </tbody>
    </table>
";

var converter = new HtmlConverter();
var pdfBytes = await converter.ConvertToPdfBytes(html);
await File.WriteAllBytesAsync("output.pdf", pdfBytes);
```


## Extending the Library

### Adding New HTML Element Support

1. Create a new converter class:

```csharp
public class HeadingElementConverter : BaseHtmlElementConverter
{
    protected override string[] SupportedTags => ["h1", "h2", "h3", "h4", "h5", "h6"];

    public override void Convert(HtmlElement element, IContainer container)
    {
        var text = GetTextContent(element);
        var fontSize = GetFontSizeForHeading(element.TagName);
        
        container.Text(text)
                 .FontSize(fontSize)
                 .Bold();
    }
    
    private float GetFontSizeForHeading(string tagName)
    {
        return tagName switch
        {
            "h1" => 24f,
            "h2" => 20f,
            "h3" => 18f,
            "h4" => 16f,
            "h5" => 14f,
            "h6" => 12f,
            _ => 12f
        };
    }
}
```

2. Register the converter:

```csharp
var converter = new HtmlConverter();
converter.RegisterConverter(new HeadingElementConverter());
```

## Projects

### NetHtml2Pdf (Core Library)
The main library containing the HTML-to-PDF conversion logic.

### NetHtml2Pdf.Test
Comprehensive unit tests covering all functionality.

### NetHtml2Pdf.TestConsole
Command-line application for converting HTML files to PDF.

**Usage:**
```bash
dotnet run <input.html> [output.pdf]
```

**Examples:**
```bash
# Basic usage (creates output.pdf in temp folder)
dotnet run example.html

# Custom filename
dotnet run example.html report.pdf

# Absolute path
dotnet run example.html "C:\temp\report.pdf"

# Relative path with subdirectory
dotnet run example.html "./reports/monthly-report.pdf"
```

### NetHtml2Pdf.TestAzureFunction
Azure Function for cloud-based PDF generation.

## Examples

### Simple Table
```html
<table>
    <thead>
        <tr><th>Name</th><th>Age</th><th>City</th></tr>
    </thead>
    <tbody>
        <tr><td>Alice</td><td>30</td><td>New York</td></tr>
        <tr><td>Bob</td><td>25</td><td>London</td></tr>
    </tbody>
</table>
```

### Mixed Content Document
```html
<section>
    <h1>Company Report</h1>
    <p>This report demonstrates various HTML elements.</p>
</section>

<div>
    <p>This paragraph is inside a div element.</p>
    <p>Here's another paragraph with a line break.<br>This text appears on a new line.</p>
</div>

<table>
    <thead>
        <tr><th>Department</th><th>Employees</th><th>Budget</th></tr>
    </thead>
    <tbody>
        <tr><td>Engineering</td><td>25</td><td>$500,000</td></tr>
        <tr><td>Marketing</td><td>15</td><td>$200,000</td></tr>
    </tbody>
</table>

<section>
    <p>This concludes our mixed content example.</p>
</section>
```

## Error Handling

The library provides comprehensive error handling:

- **Invalid HTML**: Gracefully handles malformed HTML
- **Missing Content**: Returns appropriate error messages
- **File System Errors**: Handles file I/O exceptions
- **PDF Generation Errors**: Catches and reports conversion failures

## Dependencies

- **QuestPDF**: PDF generation library
- **AngleSharp**: HTML parsing library
- **.NET 8.0**: Target framework

## Building and Testing

```bash
# Clone the repository
git clone <repository-url>
cd NetHtml2Pdf

# Restore dependencies
dotnet restore

# Build all projects
dotnet build

# Run tests
dotnet test

# Run console application
cd src/NetHtml2Pdf.TestConsole
dotnet run example.html
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes following good coding practices
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

