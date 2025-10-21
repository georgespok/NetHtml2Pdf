using NetHtml2Pdf.Core;

namespace NetHtml2Pdf.Layout.Pagination;

/// <summary>
///     Immutable constraints applied to each page during pagination.
/// </summary>
internal sealed class PageConstraints
{
    public PageConstraints(
        float pageWidth,
        float pageHeight,
        BoxSpacing margin,
        float headerBand,
        float footerBand)
    {
        if (pageWidth <= 0) throw new ArgumentOutOfRangeException(nameof(pageWidth), "Page width must be positive.");

        if (pageHeight <= 0) throw new ArgumentOutOfRangeException(nameof(pageHeight), "Page height must be positive.");

        ArgumentNullException.ThrowIfNull(margin);
        if (headerBand < 0)
            throw new ArgumentOutOfRangeException(nameof(headerBand), "Header band must be non-negative.");

        if (footerBand < 0)
            throw new ArgumentOutOfRangeException(nameof(footerBand), "Footer band must be non-negative.");

        PageWidth = pageWidth;
        PageHeight = pageHeight;
        Margin = margin;
        HeaderBand = headerBand;
        FooterBand = footerBand;
    }

    public float PageWidth { get; }

    public float PageHeight { get; }

    public BoxSpacing Margin { get; }

    public float HeaderBand { get; }

    public float FooterBand { get; }

    public float ContentHeight =>
        Math.Max(0, PageHeight - ToFloat(Margin.Top) - ToFloat(Margin.Bottom) - HeaderBand - FooterBand);

    public float ContentWidth =>
        Math.Max(0, PageWidth - ToFloat(Margin.Left) - ToFloat(Margin.Right));

    private static float ToFloat(double? value)
    {
        return value.HasValue ? (float)value.Value : 0f;
    }
}