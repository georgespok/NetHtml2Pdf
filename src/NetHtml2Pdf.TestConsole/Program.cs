using Microsoft.Extensions.Logging;
namespace NetHtml2Pdf.TestConsole
{
    /// <summary>
    /// Console application for converting HTML files to PDF using the NetHtml2Pdf library
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Parse command line arguments
            if (args.Length < 1)
            {
                ShowUsage();
                return;
            }

            var inputFile = args[0];
            var outputFile = args.Length > 1 ? args[1] : "output.pdf";

            // Validate input file
            if (!File.Exists(inputFile))
            {
                Console.WriteLine($"Error: Input file '{inputFile}' not found.");
                return;
            }

            // Determine output path
            string outputPath;
            if (Path.IsPathRooted(outputFile))
            {
                // Absolute path provided
                outputPath = outputFile;
            }
            else
            {
                // Relative path - create in temp folder
                var tempDir = Path.GetTempPath();
                outputPath = Path.Combine(tempDir, outputFile);
            }

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            try
            {
                // Read HTML content from file
                var htmlContent = await File.ReadAllTextAsync(inputFile);

                Console.WriteLine("Converting HTML to PDF...");
                Console.WriteLine($"Input file: {Path.GetFullPath(inputFile)}");
                Console.WriteLine($"Output file: {Path.GetFullPath(outputPath)}");
                Console.WriteLine();

                // Create console logger and options, pass into PdfBuilder
                using var loggerFactory = LoggerFactory.Create(b =>
                {
                    b.AddSimpleConsole();
                    b.SetMinimumLevel(LogLevel.Information);
                });
                var builder = new PdfBuilder(loggerFactory.CreateLogger<PdfBuilder>());
                var pdfBytes = builder.AddPage(htmlContent).Build();

                // Write PDF to output file
                await File.WriteAllBytesAsync(outputPath, pdfBytes);

                Console.WriteLine("✅ PDF created successfully!");
                Console.WriteLine($"📄 File size: {pdfBytes.Length:N0} bytes");
                Console.WriteLine();

                // Warnings are logged via ILogger; no need to query builder for warnings
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creating PDF: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine("Usage: NetHtml2Pdf.TestConsole <input.html> [output.pdf]");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  NetHtml2Pdf.TestConsole example.html");
            Console.WriteLine("  NetHtml2Pdf.TestConsole example.html output.pdf");
            Console.WriteLine("  NetHtml2Pdf.TestConsole example.html C:\\temp\\report.pdf");
            Console.WriteLine("  NetHtml2Pdf.TestConsole example.html ./reports/monthly-report.pdf");
            Console.WriteLine();
            Console.WriteLine("Features:");
            Console.WriteLine("  • Converts HTML files to PDF");
            Console.WriteLine("  • Displays warnings for unsupported elements");
            Console.WriteLine("  • Shows fallback element processing information");
            Console.WriteLine("  • Supports relative and absolute output paths");
        }
    }
}
