using System.Runtime.CompilerServices;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Support;

/// <summary>
/// Base class for PDF rendering tests providing common PDF analysis and assertion helpers.
/// Disables parallelization to prevent QuestPDF memory access violations during concurrent rendering.
/// </summary>
[CollectionDefinition("PdfRendering", DisableParallelization = true)]
public abstract class PdfRenderTestBase(ITestOutputHelper output)
{
    protected static readonly PdfWordParser WordParser = new();

    #region PDF Word Extraction

    /// <summary>
    /// Extracts all words from a PDF with their styling attributes.
    /// </summary>
    protected static IReadOnlyList<PdfWord> GetPdfWords(byte[] pdfBytes) =>
        WordParser.GetWords(pdfBytes);

    /// <summary>
    /// Extracts all words from a PDF as strings for simple text validation.
    /// </summary>
    protected static string[] ExtractWords(byte[] pdfBytes) =>
        WordParser.GetWords(pdfBytes).Select(w => w.Text).ToArray();

    #endregion

    #region PDF Validation

    private static class PdfHeader
    {
        public const byte Percent = 0x25;  // %
        public const byte P = 0x50;        // P
        public const byte D = 0x44;        // D
        public const byte F = 0x46;        // F
        public const int MinimumLength = 4;
    }

    /// <summary>
    /// Asserts that the byte array is a valid PDF file by checking header signature.
    /// </summary>
    protected static void AssertValidPdf(byte[] pdfBytes)
    {
        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length >= PdfHeader.MinimumLength,
            $"PDF must be at least {PdfHeader.MinimumLength} bytes");

        Assert.Equal(PdfHeader.Percent, pdfBytes[0]);
        Assert.Equal(PdfHeader.P, pdfBytes[1]);
        Assert.Equal(PdfHeader.D, pdfBytes[2]);
        Assert.Equal(PdfHeader.F, pdfBytes[3]);
    }

    #endregion

    #region Test Helpers

    /// <summary>
    /// Saves PDF bytes to a temp file and logs the path for inspection.
    /// Uses the calling test method's name if no filename is provided.
    /// </summary>
    protected async Task SavePdfForInspectionAsync(
        byte[] pdfBytes,
        string? fileName = null,
        [CallerMemberName] string? callerName = null)
    {
        var baseName = string.IsNullOrWhiteSpace(fileName)
            ? (callerName ?? "output")
            : fileName;

        var safeFileName = MakeSafeFileName(baseName);
        var finalFileName = EnsurePdfExtension(safeFileName);
        var tempPath = Path.Combine(Path.GetTempPath(), finalFileName);

        await File.WriteAllBytesAsync(tempPath, pdfBytes);
        output.WriteLine($"PDF saved to: {tempPath}");
    }

    private static string MakeSafeFileName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(ch => invalidChars.Contains(ch) ? '_' : ch)).Trim();
    }

    private static string EnsurePdfExtension(string fileName) =>
        fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
            ? fileName
            : $"{fileName}.pdf";

    #endregion
}