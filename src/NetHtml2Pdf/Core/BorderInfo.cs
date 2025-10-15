using NetHtml2Pdf.Core.Constants;

namespace NetHtml2Pdf.Core;

/// <summary>
/// Represents parsed border information with width, style, and color components.
/// </summary>
internal readonly struct BorderInfo(double? width, string? style, string? color) : IEquatable<BorderInfo>
{
    public static BorderInfo Empty { get; } = new(null, null, null);

    public double? Width { get; } = width;
    public string? Style { get; } = style;
    public string? Color { get; } = color;

    public bool HasValue => Width.HasValue || !string.IsNullOrEmpty(Style) || !string.IsNullOrEmpty(Color);

    public bool IsVisible => HasValue && Style != CssBorderValues.None && Style != CssBorderValues.Hidden;

    /// <summary>
    /// Gets the border width in pixels, defaulting to 1px if not specified but border is visible.
    /// </summary>
    public double GetWidthInPixels()
    {
        if (!IsVisible)
            return 0;

        return Width ?? CssUnits.DefaultBorderWidth;
    }

    /// <summary>
    /// Gets the border style, defaulting to "solid" if not specified but border is visible.
    /// </summary>
    public string GetStyle()
    {
        if (!IsVisible)
            return CssBorderValues.None;

        return Style ?? CssUnits.DefaultBorderStyle;
    }

    /// <summary>
    /// Gets the border color, defaulting to "black" if not specified but border is visible.
    /// </summary>
    public string GetColor()
    {
        if (!IsVisible)
            return "transparent";

        return Color ?? CssUnits.DefaultBorderColor;
    }

    public bool Equals(BorderInfo other)
    {
        return Nullable.Equals(Width, other.Width) &&
               Style == other.Style &&
               Color == other.Color;
    }

    public override bool Equals(object? obj)
    {
        return obj is BorderInfo other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Width, Style, Color);
    }

    public static bool operator ==(BorderInfo left, BorderInfo right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BorderInfo left, BorderInfo right)
    {
        return !left.Equals(right);
    }
}
