# Azure Function - HTML to PDF Converter

This Azure Function converts HTML content to PDF using the refactored NetHtml2Pdf library with SOLID principles.

## Updated API

The function now accepts **raw HTML content** directly in the request body instead of JSON parameters.

### Endpoint
- **URL**: `POST /api/ConvertToPdf`
- **Content-Type**: `text/html` (or any text content type)
- **Body**: Raw HTML content

### Parameters
- **Request Body** (required): Raw HTML content
- **Query Parameter** `title` (optional): Custom filename for the generated PDF

### Response
- **Success**: PDF file with `Content-Type: application/pdf`
- **Error**: HTTP 400 for missing content, HTTP 500 for conversion errors

## Usage Examples

### PowerShell
```powershell
$htmlContent = Get-Content -Path "example.html" -Raw
Invoke-RestMethod -Uri "https://your-function.azurewebsites.net/api/ConvertToPdf?title=MyDocument" `
                  -Method Post `
                  -Body $htmlContent `
                  -ContentType "text/html" `
                  -OutFile "output.pdf"
```

### curl
```bash
curl -X POST \
     -H "Content-Type: text/html" \
     -d @example.html \
     "https://your-function.azurewebsites.net/api/ConvertToPdf?title=MyDocument" \
     --output output.pdf
```

### C# HttpClient
```csharp
var htmlContent = await File.ReadAllTextAsync("example.html");
var response = await httpClient.PostAsync(
    "https://your-function.azurewebsites.net/api/ConvertToPdf?title=MyDocument",
    new StringContent(htmlContent, Encoding.UTF8, "text/html")
);
var pdfBytes = await response.Content.ReadAsByteArrayAsync();
```

## Supported HTML Elements

The function supports the following HTML elements:
- `<table>` - Tables with headers and data rows
- `<p>` - Paragraphs
- `<br>` - Line breaks
- `<div>` - Division elements
- `<section>` - Section elements

## Error Handling

- **400 Bad Request**: When no HTML content is provided in the request body
- **500 Internal Server Error**: When PDF conversion fails

## Changes from Previous Version

1. **Removed**: JSON request body format (`{"html": "...", "text": "..."}`)
2. **Removed**: `inline` query parameter (always downloads as attachment)
3. **Added**: Direct HTML content in request body
4. **Simplified**: Only POST method supported (no GET)
5. **Improved**: Better error handling with specific HTTP status codes
