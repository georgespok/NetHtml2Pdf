using System.Globalization;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Constants;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Parser.Interfaces;

namespace NetHtml2Pdf.Parser;

/// <summary>
/// Updates a <see cref="CssStyleMap"/> with the values encoded in a single CSS declaration.
/// </summary>
internal sealed class CssStyleUpdater : ICssDeclarationUpdater
{
    public CssStyleMap UpdateStyles(CssStyleMap styles, CssDeclaration declaration)
    {
        return declaration.Name switch
        {
            CssProperties.FontStyle => styles.WithFontStyle(IsItalic(declaration.Value) ? FontStyle.Italic : FontStyle.Normal),
            CssProperties.FontWeight => styles.WithBold(IsBoldValue(declaration.Value)),
            CssProperties.TextDecoration => styles.WithTextDecoration(ParseTextDecoration(declaration.Value)),
            CssProperties.LineHeight => styles.WithLineHeight(ParseNumeric(declaration.Value)),
            CssProperties.Color => styles.WithColor(ColorNormalizer.NormalizeToHex(declaration.Value)),
            CssProperties.BackgroundColor => styles.WithBackgroundColor(ColorNormalizer.NormalizeToHex(declaration.Value)),
            CssProperties.Margin => styles.WithMargin(ParseBoxSpacing(declaration.Value)),
            CssProperties.MarginTop => styles.WithMarginTop(ParseNumeric(declaration.Value)),
            CssProperties.MarginRight => styles.WithMarginRight(ParseNumeric(declaration.Value)),
            CssProperties.MarginBottom => styles.WithMarginBottom(ParseNumeric(declaration.Value)),
            CssProperties.MarginLeft => styles.WithMarginLeft(ParseNumeric(declaration.Value)),
            CssProperties.Padding => styles.WithPadding(ParseBoxSpacing(declaration.Value)),
            CssProperties.PaddingTop => styles.WithPaddingTop(ParseNumeric(declaration.Value)),
            CssProperties.PaddingRight => styles.WithPaddingRight(ParseNumeric(declaration.Value)),
            CssProperties.PaddingBottom => styles.WithPaddingBottom(ParseNumeric(declaration.Value)),
            CssProperties.PaddingLeft => styles.WithPaddingLeft(ParseNumeric(declaration.Value)),
            CssProperties.TextAlign => styles.WithTextAlign(declaration.Value?.Trim()),
            CssProperties.VerticalAlign => styles.WithVerticalAlign(declaration.Value?.Trim()),
            CssProperties.Border => styles.WithBorder(ParseBorderShorthand(declaration.Value)),
            CssProperties.BorderCollapse => styles.WithBorderCollapse(declaration.Value?.Trim()),
            _ => styles
        };
    }

    private static bool IsItalic(string value) => value.Trim().Equals(CssFontValues.Italic, StringComparison.OrdinalIgnoreCase);

    private static bool IsBoldValue(string value)
    {
        var trimmed = value.Trim().ToLowerInvariant();
        return trimmed is CssFontValues.Bold or CssFontValues.Bolder or CssFontValues.FontWeight600 or CssFontValues.FontWeight700 or CssFontValues.FontWeight800 or CssFontValues.FontWeight900;
    }

    private static TextDecorationStyle ParseTextDecoration(string value)
    {
        var trimmed = value.Trim().ToLowerInvariant();
        return trimmed.Contains(CssFontValues.Underline, StringComparison.Ordinal) ? TextDecorationStyle.Underline : TextDecorationStyle.None;
    }

    private static double? ParseNumeric(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim().ToLowerInvariant();
        if (trimmed.EndsWith(CssUnits.Pixels, StringComparison.Ordinal))
        {
            trimmed = trimmed[..^2];
        }
        else if (trimmed.EndsWith(CssUnits.Rem, StringComparison.Ordinal) || trimmed.EndsWith(CssUnits.Em, StringComparison.Ordinal))
        {
            trimmed = trimmed[..^3];
        }

        return double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
    }

    private static BoxSpacing ParseBoxSpacing(string value)
    {
        var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return BoxSpacing.Empty;
        }

        double? ParsePart(string part) => ParseNumeric(part);

        return parts.Length switch
        {
            1 => CreateUniform(parts[0]),
            2 => CreateTwoValue(parts[0], parts[1]),
            3 => CreateThreeValue(parts[0], parts[1], parts[2]),
            _ => CreateFourValue(parts[0], parts[1], parts[2], parts[3])
        };

        BoxSpacing CreateUniform(string part)
        {
            var length = ParsePart(part);
            return length.HasValue ? BoxSpacing.FromAll(length.Value) : BoxSpacing.Empty;
        }

        BoxSpacing CreateTwoValue(string verticalPart, string horizontalPart)
        {
            var vertical = ParsePart(verticalPart);
            var horizontal = ParsePart(horizontalPart);
            
            // If any part is invalid, reject entire declaration (CSS contract requirement)
            if (!vertical.HasValue || !horizontal.HasValue)
            {
                return BoxSpacing.Empty;
            }

            return BoxSpacing.FromSpecific(vertical, horizontal, vertical, horizontal);
        }

        BoxSpacing CreateThreeValue(string topPart, string horizontalPart, string bottomPart)
        {
            var top = ParsePart(topPart);
            var horizontal = ParsePart(horizontalPart);
            var bottom = ParsePart(bottomPart);
            
            // If any part is invalid, reject entire declaration (CSS contract requirement)
            if (!top.HasValue || !horizontal.HasValue || !bottom.HasValue)
            {
                return BoxSpacing.Empty;
            }

            return BoxSpacing.FromSpecific(top, horizontal, bottom, horizontal);
        }

        BoxSpacing CreateFourValue(string topPart, string rightPart, string bottomPart, string leftPart)
        {
            var top = ParsePart(topPart);
            var right = ParsePart(rightPart);
            var bottom = ParsePart(bottomPart);
            var left = ParsePart(leftPart);
            
            // If any part is invalid, reject entire declaration (CSS contract requirement)
            if (!top.HasValue || !right.HasValue || !bottom.HasValue || !left.HasValue)
            {
                return BoxSpacing.Empty;
            }

            return BoxSpacing.FromSpecific(top, right, bottom, left);
        }
    }

    private static BorderInfo ParseBorderShorthand(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return BorderInfo.Empty;
        }

        var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return BorderInfo.Empty;
        }

        double? width = null;
        string? style = null;
        string? color = null;

        foreach (var part in parts)
        {
            var trimmedPart = part.Trim();

            // Check if it's a width value (numeric or keyword)
            if (IsWidthValue(trimmedPart, out var parsedWidth))
            {
                width = parsedWidth;
            }
            // Check if it's a style value
            else if (IsStyleValue(trimmedPart))
            {
                style = trimmedPart.ToLowerInvariant();
            }
            // Check if it's a color value
            else if (IsColorValue(trimmedPart))
            {
                color = ColorNormalizer.NormalizeToHex(trimmedPart);
            }
            else
            {
                // Invalid token found - reject entire declaration
                return BorderInfo.Empty;
            }
        }

        return new BorderInfo(width, style, color);
    }

    private static bool IsWidthValue(string value, out double? width)
    {
        width = null;

        // Check for width keywords
        switch (value.ToLowerInvariant())
        {
            case CssBorderValues.Thin:
                width = CssUnits.ThinWidth;
                return true;
            case CssBorderValues.Medium:
                width = CssUnits.MediumWidth;
                return true;
            case CssBorderValues.Thick:
                width = CssUnits.ThickWidth;
                return true;
        }

        // Check for numeric values (px, pt, em, etc.)
        var numericWidth = ParseNumeric(value);
        if (numericWidth.HasValue)
        {
            width = numericWidth.Value;
            return true;
        }

        return false;
    }

    private static bool IsStyleValue(string value)
    {
        var lowerValue = value.ToLowerInvariant();
        return lowerValue is CssBorderValues.Solid or CssBorderValues.Dashed or CssBorderValues.Dotted or CssBorderValues.None or CssBorderValues.Hidden;
    }

    private static bool IsColorValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        var lowerValue = value.ToLowerInvariant();

        // Named colors (basic set)
        if (lowerValue is CssColorNames.Black or CssColorNames.White or CssColorNames.Red or CssColorNames.Green or CssColorNames.Blue or CssColorNames.Yellow or 
            CssColorNames.Orange or CssColorNames.Purple or CssColorNames.Pink or CssColorNames.Brown or CssColorNames.Gray or CssColorNames.Grey or CssColorNames.Cyan or CssColorNames.Magenta)
        {
            return true;
        }

        // Hex colors (#fff, #ffffff)
        if (value.StartsWith(CssUnits.HexPrefix) && (value.Length == 4 || value.Length == 7))
        {
            return true;
        }

        // RGB colors (rgb(255, 0, 0))
        if (value.StartsWith(CssUnits.RgbFunction, StringComparison.OrdinalIgnoreCase) && value.EndsWith(CssUnits.RgbFunctionEnd))
        {
            return true;
        }

        return false;
    }
}
