using System.Runtime.CompilerServices;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Renderer.Test
{
    public abstract class PdfRendererTestBase
    {
        protected ITestOutputHelper Output { get; }
        protected readonly PdfRenderer Renderer;

        protected PdfRendererTestBase(ITestOutputHelper output)
        {
            Output = output;
            Renderer = new PdfRenderer();
            var fontPath = Path.Combine(AppContext.BaseDirectory, "Fonts", "Inter-Regular.ttf");
            Renderer.Setup(fontPath);
        }

        /// <summary>
        /// Asserts that PDF bytes are valid and not empty
        /// </summary>
        protected static void AssertValidPdf(byte[] pdfBytes)
        {
            Assert.NotNull(pdfBytes);
            Assert.True(pdfBytes.Length > 0);
            Assert.Equal(0x25, pdfBytes[0]); // PDF file starts with %PDF
            Assert.Equal(0x50, pdfBytes[1]); // P
            Assert.Equal(0x44, pdfBytes[2]); // D
            Assert.Equal(0x46, pdfBytes[3]); // F
        }
        
        /// <summary>
        /// Saves PDF bytes to a temp file and logs the path for inspection.
        /// When no file name is provided, uses the calling test method's name.
        /// </summary>
        protected async Task SavePdfForInspectionAsync(
            byte[] pdfBytes,
            string? fileName = null,
            [CallerMemberName] string? callerName = null)
        {
            var baseName = string.IsNullOrWhiteSpace(fileName) ? (callerName ?? "output") : fileName;
            var safeBase = MakeSafeFileName(baseName);
            var finalFileName = safeBase.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? safeBase : $"{safeBase}.pdf";
            var tempPath = Path.Combine(Path.GetTempPath(), finalFileName);
            await File.WriteAllBytesAsync(tempPath, pdfBytes);
            Output.WriteLine($"PDF saved to: {tempPath}");
        }

        protected static string MakeSafeFileName(string name)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var builder = new System.Text.StringBuilder(name.Length);
            foreach (var ch in name)
            {
                builder.Append(invalidChars.Contains(ch) ? '_' : ch);
            }
            return builder.ToString().Trim();
        }

        protected static List<Word> GetWords(byte[] pdf)
        {
            using var pdfStream = new System.IO.MemoryStream(pdf);
            using var document = PdfDocument.Open(pdfStream);

            var page = document.GetPage(1);
            var words = page.GetWords().ToList();
            return words;
        }
    }
}
