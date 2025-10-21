using NetHtml2Pdf.Core.Constants;

namespace NetHtml2Pdf.Core;

/// <summary>
///     Normalizes CSS color values to hexadecimal format during parsing.
/// </summary>
public static class ColorNormalizer
{
    /// <summary>
    ///     Normalizes a CSS color value to hexadecimal format.
    /// </summary>
    /// <param name="color">The color value to normalize (named color, hex, or rgb).</param>
    /// <returns>A hex color code, or null if the color is invalid or unsupported.</returns>
    public static string? NormalizeToHex(string? color)
    {
        if (string.IsNullOrEmpty(color)) return null;

        var trimmedColor = color.Trim();

        // Already hex color - normalize to 6-digit format
        if (trimmedColor.StartsWith(CssUnits.HexPrefix)) return NormalizeHexColor(trimmedColor);

        // RGB color function
        if (trimmedColor.StartsWith(CssUnits.RgbFunction, StringComparison.OrdinalIgnoreCase) &&
            trimmedColor.EndsWith(CssUnits.RgbFunctionEnd))
            return ParseRgbToHex(trimmedColor);

        // Named colors
        return ConvertNamedColorToHex(trimmedColor);
    }

    /// <summary>
    ///     Converts a named color to its hexadecimal equivalent.
    /// </summary>
    private static string? ConvertNamedColorToHex(string color)
    {
        return color.ToLowerInvariant() switch
        {
            CssColorNames.Black => HexColors.Black,
            CssColorNames.White => HexColors.White,
            CssColorNames.Red => HexColors.Red,
            CssColorNames.Green => HexColors.Green,
            CssColorNames.Blue => HexColors.Blue,
            CssColorNames.Yellow => HexColors.Yellow,
            CssColorNames.Orange => HexColors.Orange,
            CssColorNames.Purple => HexColors.Purple,
            CssColorNames.Pink => HexColors.Pink,
            CssColorNames.Brown => "#A52A2A", // brown
            CssColorNames.Gray or CssColorNames.Grey => HexColors.Gray,
            CssColorNames.Cyan => "#00FFFF", // cyan
            CssColorNames.Magenta => "#FF00FF", // magenta
            _ => null
        };
    }

    /// <summary>
    ///     Parses an RGB color function to hexadecimal format.
    /// </summary>
    private static string? ParseRgbToHex(string rgbColor)
    {
        try
        {
            // Extract values from rgb(255, 0, 0) format
            var startIndex = rgbColor.IndexOf('(') + 1;
            var endIndex = rgbColor.LastIndexOf(')');

            if (startIndex <= 0 || endIndex <= startIndex)
                return null;

            var valuesString = rgbColor[startIndex..endIndex];
            var values =
                valuesString.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (values.Length != 3)
                return null;

            // Parse RGB values
            if (int.TryParse(values[0], out var r) &&
                int.TryParse(values[1], out var g) &&
                int.TryParse(values[2], out var b))
            {
                // Clamp values to 0-255 range
                r = Math.Max(0, Math.Min(255, r));
                g = Math.Max(0, Math.Min(255, g));
                b = Math.Max(0, Math.Min(255, b));

                return $"#{r:X2}{g:X2}{b:X2}";
            }
        }
        catch
        {
            // Return null for any parsing errors
        }

        return null;
    }

    /// <summary>
    ///     Normalizes a hex color to 6-digit format.
    /// </summary>
    private static string? NormalizeHexColor(string hexColor)
    {
        if (hexColor.Length == 4) // #RGB format
            return $"#{hexColor[1]}{hexColor[1]}{hexColor[2]}{hexColor[2]}{hexColor[3]}{hexColor[3]}";

        if (hexColor.Length == 7) // #RRGGBB format
            return hexColor;

        return null; // Invalid hex format
    }
}