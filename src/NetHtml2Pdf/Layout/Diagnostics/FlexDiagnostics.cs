using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Layout.Model;

namespace NetHtml2Pdf.Layout.Diagnostics;

/// <summary>
///     Emits structured diagnostics for flex formatting scenarios.
/// </summary>
internal static class FlexDiagnostics
{
    private const string FlexMeasuredEvent = "FormattingContext.Flex";
    private const string FlexDowngradeEvent = "FlexContext.Downgrade";

    public static void LogFlexMeasured(ILogger? logger, LayoutFragment fragment)
    {
        if (logger is null) return;

        if (!string.Equals(fragment.Diagnostics.ContextName, "FlexFormattingContext", StringComparison.Ordinal)) return;

        logger.LogInformation(
            "{Event} {NodePath} {Width} {Height}",
            FlexMeasuredEvent,
            fragment.NodePath,
            fragment.Width,
            fragment.Height);
    }

    public static void LogDowngrade(ILogger? logger, string nodePath, string reason)
    {
        if (logger is null) return;

        logger.LogWarning(
            "{Event} {NodePath} {Reason}",
            FlexDowngradeEvent,
            nodePath,
            reason);
    }
}