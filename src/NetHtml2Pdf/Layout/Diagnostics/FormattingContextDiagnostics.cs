using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Layout.Model;

namespace NetHtml2Pdf.Layout.Diagnostics;

internal static class FormattingContextDiagnostics
{
    private const string InlineBlockEvent = "FormattingContext.InlineBlock";
    private const string InlineBlockFallbackEvent = "FormattingContext.InlineBlock.Fallback";

    public static void LogInlineBlock(ILogger? logger, LayoutFragment fragment)
    {
        if (logger is null) return;

        if (!string.Equals(fragment.Diagnostics.ContextName, "InlineBlockFormattingContext",
                StringComparison.Ordinal)) return;

        logger.LogInformation(
            "{Event} {NodePath} {Width} {Height} {Baseline}",
            InlineBlockEvent,
            fragment.NodePath,
            fragment.Width,
            fragment.Height,
            fragment.Baseline ?? 0f);
    }

    public static void LogInlineBlockFallback(ILogger? logger, string nodePath, string reason)
    {
        if (logger is null) return;

        logger.LogWarning(
            "{Event} {NodePath} {Reason}",
            InlineBlockFallbackEvent,
            nodePath,
            reason);
    }
}