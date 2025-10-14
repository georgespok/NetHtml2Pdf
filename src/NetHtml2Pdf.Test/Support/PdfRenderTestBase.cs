using System.Runtime.CompilerServices;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Support;

/// <summary>
/// Base class for PDF rendering tests providing common PDF analysis and assertion helpers.
/// Disables parallelization to prevent QuestPDF memory access violations during concurrent rendering.
/// </summary>
[CollectionDefinition("PdfRendering", DisableParallelization = true)]
public abstract class PdfRenderTestBase(ITestOutputHelper output) : PdfValidationTestBase
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