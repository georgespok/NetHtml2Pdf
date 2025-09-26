using NetHtml2Pdf;

var html = @"<section><h1>Sandbox Test</h1><p>This is a minimal conversion.</p></section>";

var converter = new HtmlConverter();
var pdfBytes = await converter.ConvertToPdfBytes(html);

var output = Path.Combine(Path.GetTempPath(), "sandbox-test.pdf");
await File.WriteAllBytesAsync(output, pdfBytes);

Console.WriteLine($"PDF created: {output}");
