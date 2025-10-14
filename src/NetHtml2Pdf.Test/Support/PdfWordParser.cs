using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Constants;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;

namespace NetHtml2Pdf.Test.Support;

/// <summary>
/// Parses PDF documents to extract structured word information including text and styling attributes.
/// </summary>
public class PdfWordParser
{
    private static readonly string DefaultBlackColor = HexColors.Black;

    /// <summary>
    /// Extracts all words from a PDF with their styling attributes.
    /// </summary>
    public IReadOnlyList<PdfWord> GetWords(byte[] pdfBytes)
    {
        ArgumentNullException.ThrowIfNull(pdfBytes);

        using var stream = new MemoryStream(pdfBytes);
        using var pdf = PdfDocument.Open(stream);

        var words = new List<PdfWord>();
        foreach (var page in pdf.GetPages())
        {
            foreach (var word in page.GetWords())
            {
                var pdfWord = ExtractWordInfo(word);
                if (!string.IsNullOrWhiteSpace(pdfWord.Text))
                {
                    words.Add(pdfWord);
                }
            }
        }

        return words;
    }

    private static PdfWord ExtractWordInfo(Word word)
    {
        var cleanText = CleanWordText(word.Text);
        var hexColor = GetMostCommonTextColor(word);
        var isBold = IsBold(word);
        var isItalic = IsItalic(word);
        var fontSize = GetAverageFontSize(word);

        return new PdfWord(cleanText, hexColor, isBold, isItalic, fontSize);
    }

    private static string CleanWordText(string text) =>
        new string(text.Where(ch => ch != 0).ToArray());

    private static string GetMostCommonTextColor(Word word)
    {
        if (word.Letters is not { Count: > 0 } letters)
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

    private static string GetLetterColorHex(Letter? letter)
    {
        if (letter == null)
            return DefaultBlackColor;

        try
        {
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
        var rgbPart = rgbString[4..].Trim('(', ')', ' ');
        var values = rgbPart.Split(',');

        if (values.Length < 3)
            return DefaultBlackColor;

        if (!double.TryParse(values[0].Trim(), out var r) ||
            !double.TryParse(values[1].Trim(), out var g) ||
            !double.TryParse(values[2].Trim(), out var b))
        {
            return DefaultBlackColor;
        }

        var red = (int)(r * 255);
        var green = (int)(g * 255);
        var blue = (int)(b * 255);

        return $"#{red:X2}{green:X2}{blue:X2}";
    }

    private static bool IsBold(Word word) =>
        word.Letters.Any(l => l.FontName?.Contains("Bold") == true ||
                              l.FontName?.Contains("Black") == true ||
                              l.FontName?.Contains("Heavy") == true);

    private static bool IsItalic(Word word) =>
        word.Letters.Any(l => l.FontName?.Contains("Italic") == true ||
                              l.FontName?.Contains("Oblique") == true);

    private static double GetAverageFontSize(Word word)
    {
        if (word.Letters is not { Count: > 0 } letters)
            return 0;

        var fontSizes = letters
            .Select(l => l.PointSize)
            .Where(size => size > 0)
            .ToArray();

        return fontSizes.Length > 0 ? fontSizes.Average() : 0;
    }
    
}

/// <summary>
/// Represents a word extracted from a PDF with its styling attributes.
/// </summary>
public record PdfWord(
    string Text,
    string HexColor,
    bool IsBold,
    bool IsItalic,
    double FontSize);



