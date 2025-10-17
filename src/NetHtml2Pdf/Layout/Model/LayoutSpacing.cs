using NetHtml2Pdf.Core;

namespace NetHtml2Pdf.Layout.Model;

/// <summary>
/// Captures the margin, padding, and border settings that should be applied when rendering a fragment.
/// </summary>
internal readonly struct LayoutSpacing
{
    public LayoutSpacing(BoxSpacing margin, BoxSpacing padding, BorderInfo border)
    {
        Margin = margin;
        Padding = padding;
        Border = border;
    }

    public BoxSpacing Margin { get; }

    public BoxSpacing Padding { get; }

    public BorderInfo Border { get; }

    public static LayoutSpacing FromStyles(CssStyleMap style)
    {
        return new LayoutSpacing(style.Margin, style.Padding, style.Border);
    }
}
