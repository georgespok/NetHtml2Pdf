using NetHtml2Pdf.Core.Enums;

namespace NetHtml2Pdf.Core;

internal sealed class CssStyleMap
{
    private CssStyleMap(
        FontStyle fontStyle,
        bool fontStyleSet,
        bool bold,
        bool boldSet,
        TextDecorationStyle textDecoration,
        bool textDecorationSet,
        double? lineHeight,
        BoxSpacing margin,
        BoxSpacing padding,
        string? color,
        string? backgroundColor,
        string? textAlign,
        string? verticalAlign,
        BorderInfo border,
        string? borderCollapse)
    {
        FontStyle = fontStyle;
        FontStyleSet = fontStyleSet;
        Bold = bold;
        BoldSet = boldSet;
        TextDecoration = textDecoration;
        TextDecorationSet = textDecorationSet;
        LineHeight = lineHeight;
        Margin = margin;
        Padding = padding;
        Color = color;
        BackgroundColor = backgroundColor;
        TextAlign = textAlign;
        VerticalAlign = verticalAlign;
        Border = border;
        BorderCollapse = borderCollapse;
    }

    public static CssStyleMap Empty { get; } = new(FontStyle.Normal, false, false, false, TextDecorationStyle.None, false, null, BoxSpacing.Empty, BoxSpacing.Empty, null, null, null, null, BorderInfo.Empty, null);

    public FontStyle FontStyle { get; }

    public bool FontStyleSet { get; }

    public bool Bold { get; }

    public bool BoldSet { get; }

    public TextDecorationStyle TextDecoration { get; }

    public bool TextDecorationSet { get; }

    public double? LineHeight { get; }

    public BoxSpacing Margin { get; }

    public BoxSpacing Padding { get; }

    public string? Color { get; }

    public string? BackgroundColor { get; }

    public string? TextAlign { get; }

    public string? VerticalAlign { get; }

    public BorderInfo Border { get; }

    public string? BorderCollapse { get; }

    public CssStyleMap WithFontStyle(FontStyle fontStyle) => new(fontStyle, true, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight, Margin, Padding, Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse);

    public CssStyleMap WithBold(bool value = true) => new(FontStyle, FontStyleSet, value, true, TextDecoration, TextDecorationSet, LineHeight, Margin, Padding, Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse);

    public CssStyleMap WithTextDecoration(TextDecorationStyle decoration) => new(FontStyle, FontStyleSet, Bold, BoldSet, decoration, true, LineHeight, Margin, Padding, Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse);

    public CssStyleMap WithLineHeight(double? lineHeight) => new(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, lineHeight, Margin, Padding, Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse);

    public CssStyleMap WithMargin(BoxSpacing spacing) => new(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight, BoxSpacing.Merge(Margin, spacing), Padding, Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse);

    public CssStyleMap WithPadding(BoxSpacing spacing) => new(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight, Margin, BoxSpacing.Merge(Padding, spacing), Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse);

    public CssStyleMap WithMarginTop(double? value) => new(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight, Margin.WithTop(value), Padding, Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse);

    public CssStyleMap WithMarginRight(double? value) => new(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight, Margin.WithRight(value), Padding, Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse);

    public CssStyleMap WithMarginBottom(double? value) => new(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight, Margin.WithBottom(value), Padding, Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse);

    public CssStyleMap WithMarginLeft(double? value) => new(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight, Margin.WithLeft(value), Padding, Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse);

    public CssStyleMap WithPaddingTop(double? value) => new(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight, Margin, Padding.WithTop(value), Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse);

    public CssStyleMap WithPaddingRight(double? value) => new(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight, Margin, Padding.WithRight(value), Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse);

    public CssStyleMap WithPaddingBottom(double? value) => new(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight, Margin, Padding.WithBottom(value), Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse);

    public CssStyleMap WithPaddingLeft(double? value) => new(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight, Margin, Padding.WithLeft(value), Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse);

    public CssStyleMap WithColor(string? color) => new(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight, Margin, Padding, color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse);

    public CssStyleMap WithBackgroundColor(string? backgroundColor) => new(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight, Margin, Padding, Color, backgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse);

    public CssStyleMap WithTextAlign(string? textAlign) => new(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight, Margin, Padding, Color, BackgroundColor, textAlign, VerticalAlign, Border, BorderCollapse);

    public CssStyleMap WithVerticalAlign(string? verticalAlign) => new(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight, Margin, Padding, Color, BackgroundColor, TextAlign, verticalAlign, Border, BorderCollapse);

    public CssStyleMap WithBorder(BorderInfo border) => new(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight, Margin, Padding, Color, BackgroundColor, TextAlign, VerticalAlign, border, BorderCollapse);

    public CssStyleMap WithBorderCollapse(string? borderCollapse) => new(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight, Margin, Padding, Color, BackgroundColor, TextAlign, VerticalAlign, Border, borderCollapse);

    public CssStyleMap Merge(CssStyleMap? other)
    {
        if (other is null)
        {
            return this;
        }

        var fontStyle = other.FontStyleSet ? other.FontStyle : FontStyle;
        var fontStyleSet = FontStyleSet || other.FontStyleSet;

        var bold = other.BoldSet ? other.Bold : Bold;
        var boldSet = BoldSet || other.BoldSet;

        var decoration = other.TextDecorationSet ? other.TextDecoration : TextDecoration;
        var decorationSet = TextDecorationSet || other.TextDecorationSet;

        var lineHeight = other.LineHeight ?? LineHeight;

        var margin = BoxSpacing.Merge(Margin, other.Margin);
        var padding = BoxSpacing.Merge(Padding, other.Padding);

        var color = other.Color ?? Color;
        var backgroundColor = other.BackgroundColor ?? BackgroundColor;
        var textAlign = other.TextAlign ?? TextAlign;
        var verticalAlign = other.VerticalAlign ?? VerticalAlign;
        var border = other.Border.HasValue ? other.Border : Border;
        var borderCollapse = other.BorderCollapse ?? BorderCollapse;

        return new CssStyleMap(fontStyle, fontStyleSet, bold, boldSet, decoration, decorationSet, lineHeight, margin, padding, color, backgroundColor, textAlign, verticalAlign, border, borderCollapse);
    }
}
