namespace NetHtml2Pdf.Layout.Pagination;

/// <summary>
///     Represents a rectangle on the page using top-left origin coordinates.
/// </summary>
internal readonly record struct PageBounds(float X, float Y, float Width, float Height)
{
    public float Area => Math.Max(0, Width) * Math.Max(0, Height);

    public static PageBounds FromSize(float width, float height)
    {
        return new PageBounds(0, 0, width, height);
    }

    public void Deconstruct(out float x, out float y, out float width, out float height)
    {
        x = X;
        y = Y;
        width = Width;
        height = Height;
    }
}