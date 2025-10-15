using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Constants;
using NetHtml2Pdf.Core.Enums;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NetHtml2Pdf.Renderer;

/// <summary>
/// Provides common rendering utility methods used across multiple composer classes.
/// </summary>
internal static class RenderingHelpers
{
    /// <summary>
    /// Converts a CSS color value (named color or hex) to a hex color code.
    /// </summary>
    /// <param name="color">The color value to convert (e.g., "red", HexColors.Red).</param>
    /// <returns>A hex color code, or null if the color is invalid or unsupported.</returns>
    public static string? ConvertToHexColor(string? color)
    {
        if (string.IsNullOrEmpty(color))
        {
            return null;
        }

        if (color.StartsWith("#"))
        {
            return color;
        }

        // Convert common named colors to hex
        return color.ToLowerInvariant() switch
        {
            "red" => HexColors.Red,
            "blue" => HexColors.Blue,
            "green" => HexColors.Green,
            "yellow" => HexColors.Yellow,
            "black" => HexColors.Black,
            "white" => HexColors.White,
            "gray" or "grey" => HexColors.Gray,
            "orange" => HexColors.Orange,
            "purple" => HexColors.Purple,
            "pink" => HexColors.Pink,
            _ => null
        };
    }

    /// <summary>
    /// Applies horizontal text alignment to a container based on CSS text-align value.
    /// </summary>
    /// <param name="container">The container to apply alignment to.</param>
    /// <param name="textAlign">The CSS text-align value (left, center, right).</param>
    /// <returns>The container with alignment applied.</returns>
    public static IContainer ApplyTextAlignment(IContainer container, string? textAlign)
    {
        return textAlign?.ToLowerInvariant() switch
        {
            "center" => container.AlignCenter(),
            "right" => container.AlignRight(),
            "left" => container.AlignLeft(),
            _ => container.AlignLeft()
        };
    }

    /// <summary>
    /// Determines if a document node should be rendered as an inline element.
    /// </summary>
    /// <param name="node">The document node to check.</param>
    /// <returns>True if the node is an inline element, false otherwise.</returns>
    public static bool IsInlineNode(DocumentNode node) =>
        node.NodeType is DocumentNodeType.Text or DocumentNodeType.Span or DocumentNodeType.Strong
        or DocumentNodeType.Bold or DocumentNodeType.Italic or DocumentNodeType.LineBreak;

    /// <summary>
    /// Gets the appropriate font size for a heading level.
    /// </summary>
    /// <param name="headingType">The heading document node type (Heading1-Heading6).</param>
    /// <returns>The font size in points, or null if not a heading type.</returns>
    public static double? GetHeadingFontSize(DocumentNodeType headingType)
    {
        return headingType switch
        {
            DocumentNodeType.Heading1 => 32,
            DocumentNodeType.Heading2 => 24,
            DocumentNodeType.Heading3 => 19,
            DocumentNodeType.Heading4 => 16,
            DocumentNodeType.Heading5 => 13,
            DocumentNodeType.Heading6 => 11,
            _ => null
        };
    }
}

