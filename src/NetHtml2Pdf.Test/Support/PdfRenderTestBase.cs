using System.Runtime.CompilerServices;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Support;

/// <summary>
/// Base class for PDF rendering tests providing common PDF analysis and assertion helpers.
/// Disables parallelization to prevent QuestPDF memory access violations during concurrent rendering.
/// </summary>
[CollectionDefinition("PdfRendering", DisableParallelization = true)]
public abstract class PdfRenderTestBase(ITestOutputHelper output)
{
    #region PDF Word Extraction

    /// <summary>
    /// Extracts all words from a PDF as strings, filtering out null characters and whitespace.
    /// </summary>
    protected static string[] ExtractWords(byte[] pdfBytes)
    {
        ArgumentNullException.ThrowIfNull(pdfBytes);

        return WithPdfDocument(pdfBytes, pdf =>
        {
            var words = new List<string>();
            foreach (var page in pdf.GetPages())
            {
                var pageWords = page.GetWords()
                    .Select(word => CleanWordText(word.Text))
                    .Where(cleaned => !string.IsNullOrWhiteSpace(cleaned));
                
                words.AddRange(pageWords);
            }
            return words.ToArray();
        });
    }

    /// <summary>
    /// Extracts all Word objects from a PDF for detailed analysis (e.g., color, font).
    /// </summary>
    protected static List<Word> ExtractWordObjects(byte[] pdfBytes)
    {
        ArgumentNullException.ThrowIfNull(pdfBytes);

        return WithPdfDocument(pdfBytes, pdf =>
        {
            var words = new List<Word>();
            foreach (var page in pdf.GetPages())
            {
                words.AddRange(page.GetWords());
            }
            return words;
        });
    }

    private static string CleanWordText(string text) =>
        new string(text.Where(ch => ch != 0).ToArray());

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

    #region Color Analysis

    private const string DefaultBlackColor = "#000000";

    /// <summary>
    /// Determines the most common text color in a word by analyzing all letters.
    /// Returns hex color string (e.g., "#FF0000" for red).
    /// </summary>
    public static string GetMostCommonTextColor(Word? word)
    {
        if (word?.Letters is not { Count: > 0 } letters)
            return DefaultBlackColor;

        var colorCounts = letters
            .Select(GetLetterColorHex)
            .GroupBy(color => color)
            .ToDictionary(g => g.Key, g => g.Count());

        return colorCounts
            .OrderByDescending(kvp => kvp.Value)
            .ThenBy(kvp => kvp.Key)
            .Select(kvp => kvp.Key)
            .FirstOrDefault() ?? DefaultBlackColor;
    }

    /// <summary>
    /// Checks if a word has any letters with a background color applied.
    /// Note: PDF background colors are typically rendered as separate graphics objects,
    /// not as letter properties, so this checks if any letter has non-default fill/stroke.
    /// </summary>
    public static bool HasBackgroundColor(Word? word)
    {
        if (word?.Letters is not { Count: > 0 } letters)
            return false;

        // In QuestPDF, background colors are rendered as rectangles behind text
        // We can check if the word's bounding box area suggests background rendering
        return letters.Any(letter => 
            letter.FillColor != null || 
            letter.StrokeColor != null);
    }

    private static string GetLetterColorHex(Letter? letter)
    {
        if (letter == null)
            return DefaultBlackColor;

        try
        {
            // Check color properties in order of priority
            return TryGetColorHex(letter.FillColor)
                ?? TryGetColorHex(letter.Color)
                ?? TryGetColorHex(letter.StrokeColor)
                ?? DefaultBlackColor;
        }
        catch
        {
            return DefaultBlackColor;
        }
    }

    private static string? TryGetColorHex(object? color)
    {
        if (color == null)
            return null;

        var hex = ConvertIColorToHex(color);
        return hex != DefaultBlackColor ? hex : null;
    }

    private static string ConvertIColorToHex(object color)
    {
        try
        {
            var colorString = color.ToString();
            if (colorString?.StartsWith("RGB:") != true)
            {
                return DefaultBlackColor;
            }

            return ParseRgbString(colorString);
        }
        catch
        {
            return DefaultBlackColor;
        }
    }

    private static string ParseRgbString(string rgbString)
    {
        // Parse "RGB: (1, 0, 0)" format
        var rgbPart = rgbString[4..].Trim('(', ')', ' ');
        var values = rgbPart.Split(',');

        if (values.Length < 3)
        {
            return DefaultBlackColor;
        }

        if (!double.TryParse(values[0].Trim(), out var r) ||
            !double.TryParse(values[1].Trim(), out var g) ||
            !double.TryParse(values[2].Trim(), out var b))
        {
            return DefaultBlackColor;
        }

        // Convert from 0-1 range to 0-255 range
        var red = (int)(r * 255);
        var green = (int)(g * 255);
        var blue = (int)(b * 255);

        return $"#{red:X2}{green:X2}{blue:X2}";
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

    #region Private Helpers

    /// <summary>
    /// Opens a PDF document, executes an action, and ensures proper disposal.
    /// </summary>
    private static TResult WithPdfDocument<TResult>(byte[] pdfBytes, Func<PdfDocument, TResult> action)
    {
        using var stream = new MemoryStream(pdfBytes);
        using var pdf = PdfDocument.Open(stream);
        return action(pdf);
    }

    #endregion
}