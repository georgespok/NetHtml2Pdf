using NetHtml2Pdf.Core;

namespace NetHtml2Pdf.Layout.Model;

/// <summary>
///     Captures the margin, padding, and border settings that should be applied when rendering a fragment.
/// </summary>
internal readonly struct LayoutSpacing(BoxSpacing margin, BoxSpacing padding, BorderInfo border)
{
    public BoxSpacing Margin { get; } = margin;

    public BoxSpacing Padding { get; } = padding;

    public BorderInfo Border { get; } = border;

    public static LayoutSpacing FromStyles(CssStyleMap style)
    {
        return new LayoutSpacing(style.Margin, style.Padding, style.Border);
    }
}