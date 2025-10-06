using NetHtml2Pdf.Parser;
using NetHtml2Pdf.Renderer;
using System.Runtime.CompilerServices;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Support;

public abstract class PdfRenderingTestBase(ITestOutputHelper output)
{
    private readonly HtmlParser _parser = new();
    private readonly PdfRenderer _renderer = new();

    protected byte[] RenderPdf(string html)
    {
        var document = _parser.Parse(html);
        return _renderer.Render(document);
    }

    protected string[] RenderWords(string html)
    {
        var pdfBytes = RenderPdf(html);
        return PdfParser.ExtractWords(pdfBytes);
    }

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
        output.WriteLine($"PDF saved to: {tempPath}");
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
}
