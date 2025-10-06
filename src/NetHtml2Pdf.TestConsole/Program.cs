namespace NetHtml2Pdf.TestConsole
{
    /// <summary>
    /// Console application for converting HTML files to PDF using the NetHtml2Pdf library
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point for the console application
        /// </summary>
        /// <param name="args">Command line arguments: input.html [output.pdf]</param>
        /// <returns>Task representing the asynchronous operation</returns>
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

                // Convert HTML to PDF
                var converter = new HtmlConverter();
                var pdfBytes = converter.ConvertToPdf(htmlContent);
                
                // Write PDF to output file
                await File.WriteAllBytesAsync(outputPath, pdfBytes);
                
                Console.WriteLine("PDF created successfully!");
                Console.WriteLine($"Input file: {Path.GetFullPath(inputFile)}");
                Console.WriteLine($"Output file: {Path.GetFullPath(outputPath)}");
                Console.WriteLine($"File size: {pdfBytes.Length:N0} bytes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating PDF: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Displays usage information for the console application
        /// </summary>
        private static void ShowUsage()
        {
            Console.WriteLine("Usage: NetHtml2Pdf.TestConsole <input.html> [output.pdf]");
            Console.WriteLine("Examples:");
            Console.WriteLine("  NetHtml2Pdf.TestConsole example.html");
            Console.WriteLine("  NetHtml2Pdf.TestConsole example.html output.pdf");
            Console.WriteLine("  NetHtml2Pdf.TestConsole example.html C:\\temp\\report.pdf");
            Console.WriteLine("  NetHtml2Pdf.TestConsole example.html ./reports/monthly-report.pdf");
        }
    }
}
