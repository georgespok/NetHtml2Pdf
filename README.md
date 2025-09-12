# NetHtml2Pdf

A .NET library for converting HTML content to PDF using QuestPDF with extensible architecture.

## Overview

NetHtml2Pdf is a modern, extensible HTML-to-PDF conversion library that supports various HTML elements including tables, paragraphs, line breaks, divisions, and sections. The library is designed to ensure maintainability, testability, and extensibility. The goal is to create a pure .NET cross-platform library that runs on Windows and Linux without requiring GDI+ dependencies.


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