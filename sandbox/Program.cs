using NetHtml2Pdf;

var html = @"<section>
<h1>Sandbox Test</h1>
<p>This is a minimal conversion.</p>
<p>Timestamp:" + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + @"</p>
</section>";

var converter = new PdfBuilder();
var pdfBytes = converter
    .AddPage(html)
    .Build();

var output = Path.Combine(Path.GetTempPath(), "sandbox-test.pdf");
await File.WriteAllBytesAsync(output, pdfBytes);

Console.WriteLine($"PDF created: {output}");