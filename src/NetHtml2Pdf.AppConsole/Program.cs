using NetHtml2Pdf;

var html = @"
            <table>
                <thead>
                    <tr><th>Name</th><th>Age</th><th>City</th></tr>
                </thead>
                <tbody>
                    <tr><td>Alice</td><td>30</td><td>New York</td></tr>
                    <tr><td>Bob</td><td>25</td><td>London</td></tr>
                    <tr><td>Charlie</td><td>28</td><td>Berlin</td></tr>
                </tbody>
            </table>";

try
{
    var converter = new HtmlConverter();
    var pdfBytes = await converter.Convert(html);
    
    var fileName = $"table_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
    
    await File.WriteAllBytesAsync(filePath, pdfBytes);
    
    Console.WriteLine($"PDF created successfully!");
    Console.WriteLine($"File path: {filePath}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error creating PDF: {ex.Message}");
}
