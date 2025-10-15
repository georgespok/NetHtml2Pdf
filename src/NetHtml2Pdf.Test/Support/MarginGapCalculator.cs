using UglyToad.PdfPig.Content;
using UglyToad.PdfPig;

namespace NetHtml2Pdf.Test.Support;

/// <summary>
/// Helper class for calculating and validating margin gaps in PDF documents.
/// </summary>
public static class MarginGapCalculator
{
    /// <summary>
    /// Calculates gaps between words in a PDF document.
    /// </summary>
    public static MarginGapResult CalculateGaps(List<Word> words, string topWordText, string testWordText, string bottomWordText)
    {
        var analyzer = PdfWordParser.FindWords(words, topWordText, testWordText, bottomWordText);

        var topWord = analyzer.GetWord(topWordText);
        var testWord = analyzer.GetWord(testWordText);
        var bottomWord = analyzer.GetWord(bottomWordText);

        return CalculateGaps(topWord, testWord, bottomWord);
    }

    /// <summary>
    /// Calculates gaps between three words.
    /// </summary>
    public static MarginGapResult CalculateGaps(Word topWord, Word testWord, Word bottomWord)
    {
        // Extract Y coordinates (higher Y values are higher on the page)
        var topWordBottom = topWord.BoundingBox.BottomLeft.Y;
        var testWordTop = testWord.BoundingBox.TopLeft.Y;
        var testWordBottom = testWord.BoundingBox.BottomLeft.Y;
        var bottomWordTop = bottomWord.BoundingBox.TopLeft.Y;

        // Calculate gaps
        var gapAboveTest = topWordBottom - testWordTop;
        var gapBelowTest = testWordBottom - bottomWordTop;

        return new MarginGapResult
        {
            GapAboveTest = gapAboveTest,
            GapBelowTest = gapBelowTest,
            TopWordBottom = topWordBottom,
            TestWordTop = testWordTop,
            TestWordBottom = testWordBottom,
            BottomWordTop = bottomWordTop
        };
    }

    /// <summary>
    /// Validates that gaps meet the expected margin requirements.
    /// </summary>
    public static MarginValidationResult ValidateGaps(MarginGapResult gaps, double expectedGapPoints, double tolerance = 2.0)
    {
        var minExpectedGap = expectedGapPoints - tolerance;

        var gapAboveValid = gaps.GapAboveTest >= minExpectedGap;
        var gapBelowValid = gaps.GapBelowTest >= minExpectedGap;

        return new MarginValidationResult
        {
            GapAboveValid = gapAboveValid,
            GapBelowValid = gapBelowValid,
            ExpectedGapPoints = expectedGapPoints,
            MinExpectedGap = minExpectedGap,
            Tolerance = tolerance
        };
    }

    /// <summary>
    /// Converts CSS pixels to PDF points.
    /// CSS px units are typically 96 DPI, PDF points are 72 DPI.
    /// So 1px ≈ 0.75 points in PDF coordinates.
    /// </summary>
    public static double ConvertPixelsToPoints(double pixels) => pixels * 0.75;

    /// <summary>
    /// Logs gap analysis for debugging purposes.
    /// </summary>
    public static void LogGapAnalysis(MarginGapResult gaps, MarginValidationResult validation, Action<string> writeLine)
    {
        writeLine("Word positioning:");
        writeLine($"  Top word bottom: {gaps.TopWordBottom:F1} points");
        writeLine($"  Test word top: {gaps.TestWordTop:F1} points, bottom: {gaps.TestWordBottom:F1} points");
        writeLine($"  Bottom word top: {gaps.BottomWordTop:F1} points");
        writeLine("Gaps:");
        writeLine($"  Gap above Test: {gaps.GapAboveTest:F1} points (expected: ~{validation.ExpectedGapPoints} points)");
        writeLine($"  Gap below Test: {gaps.GapBelowTest:F1} points (expected: ~{validation.ExpectedGapPoints} points)");
        writeLine($"Validation:");
        writeLine($"  Gap above valid: {validation.GapAboveValid} (min required: {validation.MinExpectedGap})");
        writeLine($"  Gap below valid: {validation.GapBelowValid} (min required: {validation.MinExpectedGap})");
    }
}

/// <summary>
/// Result of margin gap calculations.
/// </summary>
public class MarginGapResult
{
    public double GapAboveTest { get; set; }
    public double GapBelowTest { get; set; }
    public double TopWordBottom { get; set; }
    public double TestWordTop { get; set; }
    public double TestWordBottom { get; set; }
    public double BottomWordTop { get; set; }
}

/// <summary>
/// Result of margin gap validation.
/// </summary>
public class MarginValidationResult
{
    public bool GapAboveValid { get; set; }
    public bool GapBelowValid { get; set; }
    public double ExpectedGapPoints { get; set; }
    public double MinExpectedGap { get; set; }
    public double Tolerance { get; set; }
}
