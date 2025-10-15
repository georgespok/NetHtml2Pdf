using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;

namespace NetHtml2Pdf.Renderer;

internal readonly struct InlineStyleState(bool bold,
    bool italic, bool underline, double? lineHeight, string? color, string? backgroundColor, double? fontSize)
{
    public static InlineStyleState Empty { get; } = new(false, false, false, null, null, null, null);

    public bool Bold { get; } = bold;
    public bool Italic { get; } = italic;
    public bool Underline { get; } = underline;
    public double? LineHeight { get; } = lineHeight;
    public string? Color { get; } = color;
    public string? BackgroundColor { get; } = backgroundColor;
    public double? FontSize { get; } = fontSize;

    public InlineStyleState ApplyCss(CssStyleMap css)
    {
        var bold = css.BoldSet ? css.Bold : Bold;
        var italic = css.FontStyleSet ? css.FontStyle == FontStyle.Italic : Italic;
        var underline = css.TextDecorationSet ? css.TextDecoration.HasFlag(TextDecorationStyle.Underline) : Underline;
        var lineHeight = css.LineHeight ?? LineHeight;
        var color = css.Color ?? Color;
        var backgroundColor = css.BackgroundColor ?? BackgroundColor;

        return new InlineStyleState(bold, italic, underline, lineHeight, color, backgroundColor, FontSize);
    }

    public InlineStyleState WithBold() =>
        Bold
            ? this
            : new InlineStyleState(true, Italic, Underline, LineHeight, Color, BackgroundColor, FontSize);

    public InlineStyleState WithItalic() =>
        Italic
            ? this
            : new InlineStyleState(Bold, true, Underline, LineHeight, Color, BackgroundColor, FontSize);

    public InlineStyleState WithFontSize(double fontSize) =>
        new InlineStyleState(Bold, Italic, Underline, LineHeight, Color, BackgroundColor, fontSize);
}
