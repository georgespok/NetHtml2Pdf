# Quickstart: Cross-platform HTML-to-PDF Library

## Prerequisites
- .NET 8.0 SDK

## Install
```bash
dotnet add package NetHtml2Pdf
```

## Basic Usage
```csharp
using NetHtml2Pdf;

var builder = new PdfDocumentBuilder(new DocumentOptions
{
    PageSize = PageSize.Letter,
    Orientation = PageOrientation.Portrait,
    MarginsInches = 1.0,
    FontFamily = "Inter"
});

builder.AddPdfPage(@"<section><h1>Title</h1><p>Hello</p></section>");
var pdf = await builder.RenderAsync();
await File.WriteAllBytesAsync("output.pdf", pdf);
```

## Tables
```csharp
builder.AddPdfPage(@"<table><thead><tr><th>A</th><th>B</th></tr></thead>
<tbody><tr><td>1</td><td>2</td></tr></tbody></table>");
```
