namespace NetHtml2Pdf.Core;

internal readonly struct BoxSpacing
{
    public BoxSpacing(double? top, double? right, double? bottom, double? left)
    {
        Top = top;
        Right = right;
        Bottom = bottom;
        Left = left;
    }

    public double? Top { get; }
    public double? Right { get; }
    public double? Bottom { get; }
    public double? Left { get; }

    public bool HasValue => Top.HasValue || Right.HasValue || Bottom.HasValue || Left.HasValue;

    public static BoxSpacing Empty => new(null, null, null, null);

    public static BoxSpacing FromAll(double value) => new(value, value, value, value);

    public static BoxSpacing FromVerticalHorizontal(double vertical, double horizontal) => new(vertical, horizontal, vertical, horizontal);

    public static BoxSpacing FromSpecific(double? top, double? right, double? bottom, double? left) => new(top, right, bottom, left);

    public BoxSpacing WithTop(double? value)
    {
        return value.HasValue ? new(value, Right, Bottom, Left) : this;
    }

    public BoxSpacing WithRight(double? value)
    {
        return value.HasValue ? new(Top, value, Bottom, Left) : this;
    }

    public BoxSpacing WithBottom(double? value)
    {
        return value.HasValue ? new(Top, Right, value, Left) : this;
    }

    public BoxSpacing WithLeft(double? value)
    {
        return value.HasValue ? new(Top, Right, Bottom, value) : this;
    }

    public static BoxSpacing Merge(BoxSpacing current, BoxSpacing other)
    {
        if (!other.HasValue)
        {
            return current;
        }

        return new(
            other.Top ?? current.Top,
            other.Right ?? current.Right,
            other.Bottom ?? current.Bottom,
            other.Left ?? current.Left);
    }
}
