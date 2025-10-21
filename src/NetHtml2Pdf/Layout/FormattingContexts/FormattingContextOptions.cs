using NetHtml2Pdf.Layout.Engines;

namespace NetHtml2Pdf.Layout.FormattingContexts;

internal sealed class FormattingContextOptions
{
    public bool EnableInlineBlockContext { get; init; }

    public bool EnableTableContext { get; init; }

    public bool EnableTableBorderCollapse { get; init; }

    public bool EnableFlexContext { get; init; }

    public static FormattingContextOptions Disabled { get; } = new()
    {
        EnableInlineBlockContext = false,
        EnableTableContext = false,
        EnableTableBorderCollapse = false,
        EnableFlexContext = false
    };

    public static FormattingContextOptions FromLayoutOptions(LayoutEngineOptions layoutOptions)
    {
        ArgumentNullException.ThrowIfNull(layoutOptions);

        return new FormattingContextOptions
        {
            EnableInlineBlockContext = layoutOptions.EnableInlineBlockContext,
            EnableTableContext = layoutOptions.EnableTableContext,
            EnableTableBorderCollapse = layoutOptions.EnableTableBorderCollapse,
            EnableFlexContext = layoutOptions.EnableFlexContext
        };
    }
}