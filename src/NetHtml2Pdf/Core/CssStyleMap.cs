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
        string? borderCollapse,
        CssDisplay display,
        bool displaySet,
        string? unsupportedDisplay)
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
        Display = display;
        DisplaySet = displaySet;
        UnsupportedDisplay = unsupportedDisplay;
    }

    public static CssStyleMap Empty { get; } = new(FontStyle.Normal, false, false, false, TextDecorationStyle.None,
        false, null, BoxSpacing.Empty, BoxSpacing.Empty, null, null, null, null, BorderInfo.Empty, null,
        CssDisplay.Default, false, null);

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

    public CssDisplay Display { get; }

    public bool DisplaySet { get; }

    public string? UnsupportedDisplay { get; }

    public CssStyleMap WithFontStyle(FontStyle fontStyle)
    {
        return new CssStyleMap(fontStyle, true, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight, Margin,
            Padding,
            Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse, Display, DisplaySet,
            UnsupportedDisplay);
    }

    public CssStyleMap WithBold(bool value = true)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, value, true, TextDecoration, TextDecorationSet, LineHeight,
            Margin, Padding,
            Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse, Display, DisplaySet,
            UnsupportedDisplay);
    }

    public CssStyleMap WithTextDecoration(TextDecorationStyle decoration)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, Bold, BoldSet, decoration, true, LineHeight, Margin, Padding,
            Color,
            BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse, Display, DisplaySet, UnsupportedDisplay);
    }

    public CssStyleMap WithLineHeight(double? lineHeight)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, lineHeight,
            Margin,
            Padding, Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse, Display, DisplaySet,
            UnsupportedDisplay);
    }

    public CssStyleMap WithMargin(BoxSpacing spacing)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight,
            BoxSpacing.Merge(Margin, spacing), Padding, Color, BackgroundColor, TextAlign, VerticalAlign, Border,
            BorderCollapse, Display, DisplaySet, UnsupportedDisplay);
    }

    public CssStyleMap WithPadding(BoxSpacing spacing)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight,
            Margin,
            BoxSpacing.Merge(Padding, spacing), Color, BackgroundColor, TextAlign, VerticalAlign, Border,
            BorderCollapse, Display, DisplaySet, UnsupportedDisplay);
    }

    public CssStyleMap WithMarginTop(double? value)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight,
            Margin.WithTop(value), Padding, Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse,
            Display, DisplaySet, UnsupportedDisplay);
    }

    public CssStyleMap WithMarginRight(double? value)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight,
            Margin.WithRight(value), Padding, Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse,
            Display, DisplaySet, UnsupportedDisplay);
    }

    public CssStyleMap WithMarginBottom(double? value)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight,
            Margin.WithBottom(value), Padding, Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse,
            Display, DisplaySet, UnsupportedDisplay);
    }

    public CssStyleMap WithMarginLeft(double? value)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight,
            Margin.WithLeft(value), Padding, Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse,
            Display, DisplaySet, UnsupportedDisplay);
    }

    public CssStyleMap WithPaddingTop(double? value)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight,
            Margin,
            Padding.WithTop(value), Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse, Display,
            DisplaySet, UnsupportedDisplay);
    }

    public CssStyleMap WithPaddingRight(double? value)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight,
            Margin,
            Padding.WithRight(value), Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse, Display,
            DisplaySet, UnsupportedDisplay);
    }

    public CssStyleMap WithPaddingBottom(double? value)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight,
            Margin,
            Padding.WithBottom(value), Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse,
            Display, DisplaySet, UnsupportedDisplay);
    }

    public CssStyleMap WithPaddingLeft(double? value)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight,
            Margin,
            Padding.WithLeft(value), Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse, Display,
            DisplaySet, UnsupportedDisplay);
    }

    public CssStyleMap WithColor(string? color)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight,
            Margin,
            Padding, color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse, Display, DisplaySet,
            UnsupportedDisplay);
    }

    public CssStyleMap WithBackgroundColor(string? backgroundColor)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight,
            Margin,
            Padding, Color, backgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse, Display, DisplaySet,
            UnsupportedDisplay);
    }

    public CssStyleMap WithTextAlign(string? textAlign)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight,
            Margin,
            Padding, Color, BackgroundColor, textAlign, VerticalAlign, Border, BorderCollapse, Display, DisplaySet,
            UnsupportedDisplay);
    }

    public CssStyleMap WithVerticalAlign(string? verticalAlign)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight,
            Margin,
            Padding, Color, BackgroundColor, TextAlign, verticalAlign, Border, BorderCollapse, Display, DisplaySet,
            UnsupportedDisplay);
    }

    public CssStyleMap WithBorder(BorderInfo border)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight,
            Margin,
            Padding, Color, BackgroundColor, TextAlign, VerticalAlign, border, BorderCollapse, Display, DisplaySet,
            UnsupportedDisplay);
    }

    public CssStyleMap WithBorderCollapse(string? borderCollapse)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight,
            Margin,
            Padding, Color, BackgroundColor, TextAlign, VerticalAlign, Border, borderCollapse, Display, DisplaySet,
            UnsupportedDisplay);
    }

    public CssStyleMap WithDisplay(CssDisplay display)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight,
            Margin,
            Padding, Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse, display, true, null);
    }

    public CssStyleMap WithUnsupportedDisplay(string rawDisplay)
    {
        return new CssStyleMap(FontStyle, FontStyleSet, Bold, BoldSet, TextDecoration, TextDecorationSet, LineHeight,
            Margin,
            Padding, Color, BackgroundColor, TextAlign, VerticalAlign, Border, BorderCollapse, CssDisplay.Default, true,
            rawDisplay);
    }

    public CssStyleMap Merge(CssStyleMap? other)
    {
        if (other is null) return this;

        var fontStyle = other.FontStyleSet ? other.FontStyle : FontStyle;
        var fontStyleSet = FontStyleSet || other.FontStyleSet;

        var bold = other.BoldSet ? other.Bold : Bold;
        var boldSet = BoldSet || other.BoldSet;

        var decoration = other.TextDecorationSet ? other.TextDecoration : TextDecoration;
        var decorationSet = TextDecorationSet || other.TextDecorationSet;

        var lineHeight = other.LineHeight ?? LineHeight;

        var margin = BoxSpacing.Merge(Margin, other.Margin);
        var padding = BoxSpacing.Merge(Padding, other.Padding);
        var border = other.Border.HasValue ? other.Border : Border;

        // Other properties (color, font, etc.) should be inherited from parent
        var color = other.Color ?? Color;
        var backgroundColor = other.BackgroundColor ?? BackgroundColor;
        var textAlign = other.TextAlign ?? TextAlign;
        var verticalAlign = other.VerticalAlign ?? VerticalAlign;
        var borderCollapse = other.BorderCollapse ?? BorderCollapse;
        var display = other.DisplaySet ? other.Display : Display;
        var displaySet = DisplaySet || other.DisplaySet;
        var unsupportedDisplay = other.UnsupportedDisplay ?? UnsupportedDisplay;

        return new CssStyleMap(fontStyle, fontStyleSet, bold, boldSet, decoration, decorationSet, lineHeight, margin,
            padding, color, backgroundColor, textAlign, verticalAlign, border, borderCollapse, display, displaySet,
            unsupportedDisplay);
    }
}