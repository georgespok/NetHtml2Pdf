using NetHtml2Pdf.Core.Enums;

namespace NetHtml2Pdf.Core;

internal sealed class CssStyleMap
{
    private readonly CssStyleState _state;

    private CssStyleMap(CssStyleState state)
    {
        _state = state;
    }

    public static CssStyleMap Empty { get; } = new(
        new CssStyleState(
        new Typography(FontStyle.Normal, false, false, false, TextDecorationStyle.None, false, null),
        new BoxModel(BoxSpacing.Empty, BoxSpacing.Empty, BorderInfo.Empty, null),
        new ColorStyle(null, null),
        new AlignmentStyle(null, null),
        new DisplayStyle(CssDisplay.Default, false, null)));

    public FontStyle FontStyle => _state.Typography.FontStyle;

    public bool FontStyleSet => _state.Typography.FontStyleSet;

    public bool Bold => _state.Typography.Bold;

    public bool BoldSet => _state.Typography.BoldSet;

    public TextDecorationStyle TextDecoration => _state.Typography.TextDecoration;

    public bool TextDecorationSet => _state.Typography.TextDecorationSet;

    public double? LineHeight => _state.Typography.LineHeight;

    public BoxSpacing Margin => _state.Box.Margin;

    public BoxSpacing Padding => _state.Box.Padding;

    public string? Color => _state.Colors.Color;

    public string? BackgroundColor => _state.Colors.BackgroundColor;

    public string? TextAlign => _state.Alignment.TextAlign;

    public string? VerticalAlign => _state.Alignment.VerticalAlign;

    public BorderInfo Border => _state.Box.Border;

    public string? BorderCollapse => _state.Box.BorderCollapse;

    public CssDisplay Display => _state.DisplayInfo.Display;

    public bool DisplaySet => _state.DisplayInfo.DisplaySet;

    public string? UnsupportedDisplay => _state.DisplayInfo.UnsupportedDisplay;

    public CssStyleMap WithFontStyle(FontStyle fontStyle)
    {
        return new CssStyleMap(_state with { Typography = _state.Typography with { FontStyle = fontStyle, FontStyleSet = true } });
    }

    public CssStyleMap WithBold(bool value = true)
    {
        return new CssStyleMap(_state with { Typography = _state.Typography with { Bold = value, BoldSet = true } });
    }

    public CssStyleMap WithTextDecoration(TextDecorationStyle decoration)
    {
        return new CssStyleMap(_state with { Typography = _state.Typography with { TextDecoration = decoration, TextDecorationSet = true } });
    }

    public CssStyleMap WithLineHeight(double? lineHeight)
    {
        return new CssStyleMap(_state with { Typography = _state.Typography with { LineHeight = lineHeight } });
    }

    public CssStyleMap WithMargin(BoxSpacing spacing)
    {
        return new CssStyleMap(_state with { Box = _state.Box with { Margin = BoxSpacing.Merge(_state.Box.Margin, spacing) } });
    }

    public CssStyleMap WithPadding(BoxSpacing spacing)
    {
        return new CssStyleMap(_state with { Box = _state.Box with { Padding = BoxSpacing.Merge(_state.Box.Padding, spacing) } });
    }

    public CssStyleMap WithMarginTop(double? value)
    {
        return new CssStyleMap(_state with { Box = _state.Box with { Margin = _state.Box.Margin.WithTop(value) } });
    }

    public CssStyleMap WithMarginRight(double? value)
    {
        return new CssStyleMap(_state with { Box = _state.Box with { Margin = _state.Box.Margin.WithRight(value) } });
    }

    public CssStyleMap WithMarginBottom(double? value)
    {
        return new CssStyleMap(_state with { Box = _state.Box with { Margin = _state.Box.Margin.WithBottom(value) } });
    }

    public CssStyleMap WithMarginLeft(double? value)
    {
        return new CssStyleMap(_state with { Box = _state.Box with { Margin = _state.Box.Margin.WithLeft(value) } });
    }

    public CssStyleMap WithPaddingTop(double? value)
    {
        return new CssStyleMap(_state with { Box = _state.Box with { Padding = _state.Box.Padding.WithTop(value) } });
    }

    public CssStyleMap WithPaddingRight(double? value)
    {
        return new CssStyleMap(_state with { Box = _state.Box with { Padding = _state.Box.Padding.WithRight(value) } });
    }

    public CssStyleMap WithPaddingBottom(double? value)
    {
        return new CssStyleMap(_state with { Box = _state.Box with { Padding = _state.Box.Padding.WithBottom(value) } });
    }

    public CssStyleMap WithPaddingLeft(double? value)
    {
        return new CssStyleMap(_state with { Box = _state.Box with { Padding = _state.Box.Padding.WithLeft(value) } });
    }

    public CssStyleMap WithColor(string? color)
    {
        return new CssStyleMap(_state with { Colors = _state.Colors with { Color = color } });
    }

    public CssStyleMap WithBackgroundColor(string? backgroundColor)
    {
        return new CssStyleMap(_state with { Colors = _state.Colors with { BackgroundColor = backgroundColor } });
    }

    public CssStyleMap WithTextAlign(string? textAlign)
    {
        return new CssStyleMap(_state with { Alignment = _state.Alignment with { TextAlign = textAlign } });
    }

    public CssStyleMap WithVerticalAlign(string? verticalAlign)
    {
        return new CssStyleMap(_state with { Alignment = _state.Alignment with { VerticalAlign = verticalAlign } });
    }

    public CssStyleMap WithBorder(BorderInfo border)
    {
        return new CssStyleMap(_state with { Box = _state.Box with { Border = border } });
    }

    public CssStyleMap WithBorderCollapse(string? borderCollapse)
    {
        return new CssStyleMap(_state with { Box = _state.Box with { BorderCollapse = borderCollapse } });
    }

    public CssStyleMap WithDisplay(CssDisplay display)
    {
        return new CssStyleMap(_state with { DisplayInfo = _state.DisplayInfo with { Display = display, DisplaySet = true, UnsupportedDisplay = null } });
    }

    public CssStyleMap WithUnsupportedDisplay(string rawDisplay)
    {
        return new CssStyleMap(_state with { DisplayInfo = _state.DisplayInfo with { Display = CssDisplay.Default, DisplaySet = true, UnsupportedDisplay = rawDisplay } });
    }

    public CssStyleMap Merge(CssStyleMap? other)
    {
        if (other is null) return this;

        var newTypography = new Typography(
            other.FontStyleSet ? other.FontStyle : FontStyle,
            FontStyleSet || other.FontStyleSet,
            other.BoldSet ? other.Bold : Bold,
            BoldSet || other.BoldSet,
            other.TextDecorationSet ? other.TextDecoration : TextDecoration,
            TextDecorationSet || other.TextDecorationSet,
            other.LineHeight ?? LineHeight);

        var newBox = new BoxModel(
            BoxSpacing.Merge(Margin, other.Margin),
            BoxSpacing.Merge(Padding, other.Padding),
            other.Border.HasValue ? other.Border : Border,
            other.BorderCollapse ?? BorderCollapse);

        var newColors = new ColorStyle(other.Color ?? Color, other.BackgroundColor ?? BackgroundColor);
        var newAlign = new AlignmentStyle(other.TextAlign ?? TextAlign, other.VerticalAlign ?? VerticalAlign);
        var newDisplay = new DisplayStyle(other.DisplaySet ? other.Display : Display, DisplaySet || other.DisplaySet, other.UnsupportedDisplay ?? UnsupportedDisplay);

        return new CssStyleMap(new CssStyleState(newTypography, newBox, newColors, newAlign, newDisplay));
    }

    private readonly record struct CssStyleState(
        Typography Typography,
        BoxModel Box,
        ColorStyle Colors,
        AlignmentStyle Alignment,
        DisplayStyle DisplayInfo);

    private readonly record struct Typography(
        FontStyle FontStyle,
        bool FontStyleSet,
        bool Bold,
        bool BoldSet,
        TextDecorationStyle TextDecoration,
        bool TextDecorationSet,
        double? LineHeight);

    private readonly record struct BoxModel(
        BoxSpacing Margin,
        BoxSpacing Padding,
        BorderInfo Border,
        string? BorderCollapse);

    private readonly record struct ColorStyle(string? Color, string? BackgroundColor);

    private readonly record struct AlignmentStyle(string? TextAlign, string? VerticalAlign);

    private readonly record struct DisplayStyle(CssDisplay Display, bool DisplaySet, string? UnsupportedDisplay);
}