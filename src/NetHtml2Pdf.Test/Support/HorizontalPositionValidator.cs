using UglyToad.PdfPig.Content;
using Shouldly;

namespace NetHtml2Pdf.Test.Support;

/// <summary>
/// Helper class for validating horizontal positioning of words in PDF documents.
/// </summary>
public static class HorizontalPositionValidator
{
    /// <summary>
    /// Validates that a word is positioned near the left edge (for zero horizontal margin).
    /// </summary>
    public static void ValidateLeftEdgePositioning(Word word, double maxDistanceFromLeft = 50.0)
    {
        var wordLeft = word.BoundingBox.TopLeft.X;
        wordLeft.ShouldBeLessThan(maxDistanceFromLeft, 
            $"With 0 horizontal margin, content should be positioned near the left edge (within {maxDistanceFromLeft} points), but was at {wordLeft:F1} points");
    }

    /// <summary>
    /// Logs horizontal positioning information for debugging.
    /// </summary>
    public static void LogHorizontalPositioning(Word word, Action<string> writeLine)
    {
        var wordLeft = word.BoundingBox.TopLeft.X;
        var wordRight = word.BoundingBox.TopRight.X;
        
        writeLine($"Horizontal positioning:");
        writeLine($"  Left edge: {wordLeft:F1} points");
        writeLine($"  Right edge: {wordRight:F1} points");
        writeLine($"  Width: {wordRight - wordLeft:F1} points");
    }
}
