namespace NetHtml2Pdf.Core;

internal readonly struct BoxSpacing(double? top, double? right, double? bottom, double? left)
{
    public double? Top { get; } = top;
    public double? Right { get; } = right;
    public double? Bottom { get; } = bottom;
    public double? Left { get; } = left;

    public bool HasValue => Top.HasValue || Right.HasValue || Bottom.HasValue || Left.HasValue;

    public static BoxSpacing Empty => new(null, null, null, null);

    public static BoxSpacing FromAll(double value)
    {
        return new BoxSpacing(value, value, value, value);
    }

    public static BoxSpacing FromVerticalHorizontal(double vertical, double horizontal)
    {
        return new BoxSpacing(vertical, horizontal, vertical, horizontal);
    }

    public static BoxSpacing FromSpecific(double? top, double? right, double? bottom, double? left)
    {
        return new BoxSpacing(top, right, bottom, left);
    }

    public BoxSpacing WithTop(double? value)
    {
        return value.HasValue ? new BoxSpacing(value, Right, Bottom, Left) : this;
    }

    public BoxSpacing WithRight(double? value)
    {
        return value.HasValue ? new BoxSpacing(Top, value, Bottom, Left) : this;
    }

    public BoxSpacing WithBottom(double? value)
    {
        return value.HasValue ? new BoxSpacing(Top, Right, value, Left) : this;
    }

    public BoxSpacing WithLeft(double? value)
    {
        return value.HasValue ? new BoxSpacing(Top, Right, Bottom, value) : this;
    }

    public static BoxSpacing Merge(BoxSpacing current, BoxSpacing other)
    {
        if (!other.HasValue) return current;

        return new BoxSpacing(
            other.Top ?? current.Top,
            other.Right ?? current.Right,
            other.Bottom ?? current.Bottom,
            other.Left ?? current.Left);
    }
}