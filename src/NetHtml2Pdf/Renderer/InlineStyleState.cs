using NetHtml2Pdf.Core;

namespace NetHtml2Pdf.Renderer;

internal readonly struct InlineStyleState(bool bold,
    bool italic, bool underline, double? lineHeight)
{
    public static InlineStyleState Empty { get; } = new(false, false, false, null);

    public bool Bold { get; } = bold;
    public bool Italic { get; } = italic;
    public bool Underline { get; } = underline;
    public double? LineHeight { get; } = lineHeight;

    public InlineStyleState ApplyCss(CssStyleMap css)
    {
        var bold = css.BoldSet ? css.Bold : Bold;
        var italic = css.FontStyleSet ? css.FontStyle == FontStyle.Italic : Italic;
        var underline = css.TextDecorationSet ? css.TextDecoration.HasFlag(TextDecorationStyle.Underline) : Underline;
        var lineHeight = css.LineHeight ?? LineHeight;

        return new InlineStyleState(bold, italic, underline, lineHeight);
    }

    public InlineStyleState WithBold() =>
        Bold
            ? this 
            : new InlineStyleState(true, Italic, Underline, LineHeight);
}
