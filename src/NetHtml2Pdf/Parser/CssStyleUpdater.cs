using System.Globalization;
using NetHtml2Pdf.Core;
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
            "font-style" => styles.WithFontStyle(IsItalic(declaration.Value) ? FontStyle.Italic : FontStyle.Normal),
            "font-weight" => styles.WithBold(IsBoldValue(declaration.Value)),
            "text-decoration" => styles.WithTextDecoration(ParseTextDecoration(declaration.Value)),
            "line-height" => styles.WithLineHeight(ParseNumeric(declaration.Value)),
            "color" => styles.WithColor(declaration.Value?.Trim()),
            "background-color" => styles.WithBackgroundColor(declaration.Value?.Trim()),
            "margin" => styles.WithMargin(ParseBoxSpacing(declaration.Value)),
            "margin-top" => styles.WithMarginTop(ParseNumeric(declaration.Value)),
            "margin-right" => styles.WithMarginRight(ParseNumeric(declaration.Value)),
            "margin-bottom" => styles.WithMarginBottom(ParseNumeric(declaration.Value)),
            "margin-left" => styles.WithMarginLeft(ParseNumeric(declaration.Value)),
            "padding" => styles.WithPadding(ParseBoxSpacing(declaration.Value)),
            "padding-top" => styles.WithPaddingTop(ParseNumeric(declaration.Value)),
            "padding-right" => styles.WithPaddingRight(ParseNumeric(declaration.Value)),
            "padding-bottom" => styles.WithPaddingBottom(ParseNumeric(declaration.Value)),
            "padding-left" => styles.WithPaddingLeft(ParseNumeric(declaration.Value)),
            "text-align" => styles.WithTextAlign(declaration.Value?.Trim()),
            "vertical-align" => styles.WithVerticalAlign(declaration.Value?.Trim()),
            "border" => styles.WithBorder(declaration.Value?.Trim()),
            "border-collapse" => styles.WithBorderCollapse(declaration.Value?.Trim()),
            _ => styles
        };
    }

    private static bool IsItalic(string value) => value.Trim().Equals("italic", StringComparison.OrdinalIgnoreCase);

    private static bool IsBoldValue(string value)
    {
        var trimmed = value.Trim().ToLowerInvariant();
        return trimmed is "bold" or "bolder" or "600" or "700" or "800" or "900";
    }

    private static TextDecorationStyle ParseTextDecoration(string value)
    {
        var trimmed = value.Trim().ToLowerInvariant();
        return trimmed.Contains("underline", StringComparison.Ordinal) ? TextDecorationStyle.Underline : TextDecorationStyle.None;
    }

    private static double? ParseNumeric(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim().ToLowerInvariant();
        if (trimmed.EndsWith("px", StringComparison.Ordinal))
        {
            trimmed = trimmed[..^2];
        }
        else if (trimmed.EndsWith("rem", StringComparison.Ordinal) || trimmed.EndsWith("em", StringComparison.Ordinal))
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
            _ => BoxSpacing.FromSpecific(ParsePart(parts[0]), ParsePart(parts[1]), ParsePart(parts[2]), ParsePart(parts[3]))
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
            if (!vertical.HasValue && !horizontal.HasValue)
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
            if (!top.HasValue && !horizontal.HasValue && !bottom.HasValue)
            {
                return BoxSpacing.Empty;
            }

            return BoxSpacing.FromSpecific(top, horizontal, bottom, horizontal);
        }
    }
}
