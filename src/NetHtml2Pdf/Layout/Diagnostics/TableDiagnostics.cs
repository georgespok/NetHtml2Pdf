using Microsoft.Extensions.Logging;

namespace NetHtml2Pdf.Layout.Diagnostics;

/// <summary>
///     Emits structured diagnostics for table formatting scenarios.
/// </summary>
internal static class TableDiagnostics
{
    private const string BorderDowngradeEvent = "TableContext.BorderDowngrade";

    public static void LogBorderCollapseDowngrade(
        ILogger? logger,
        string nodePath,
        string requestedModel,
        string fallbackModel)
    {
        if (logger is null) return;

        logger.LogWarning(
            "{Event} {NodePath} {RequestedModel} {FallbackModel}",
            BorderDowngradeEvent,
            nodePath,
            requestedModel,
            fallbackModel);
    }
}