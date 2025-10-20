using System;
using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Renderer;

namespace NetHtml2Pdf.Layout.Pagination;

/// <summary>
/// Configuration applied to the pagination pass for diagnostics and behavior tweaks.
/// </summary>
internal sealed class PaginationOptions
{
    private PaginationOptions(bool enableDiagnostics)
    {
        EnableDiagnostics = enableDiagnostics;
    }

    /// <summary>
    /// Enables detailed structured logs (per-page, per-fragment).
    /// </summary>
    public bool EnableDiagnostics { get; }

    public static PaginationOptions FromRendererOptions(RendererOptions rendererOptions)
    {
        ArgumentNullException.ThrowIfNull(rendererOptions);
        return new PaginationOptions(rendererOptions.EnablePaginationDiagnostics);
    }
}

internal static class PaginationDiagnostics
{
    private const string PageCreatedEvent = "Pagination.PageCreated";
    private const string FragmentSplitEvent = "Pagination.FragmentSplit";

    public static void LogPageCreated(ILogger? logger, PaginationOptions options, int pageNumber, float remainingContent)
    {
        if (!ShouldLog(logger, options))
        {
            return;
        }

        logger!.LogDebug("{Event} {PageNumber} {RemainingContent}", PageCreatedEvent, pageNumber, remainingContent);
    }

    public static void LogFragmentSplit(ILogger? logger, PaginationOptions options, string nodePath, int pageNumber, float splitHeight)
    {
        if (!ShouldLog(logger, options))
        {
            return;
        }

        logger!.LogDebug("{Event} {NodePath} {PageNumber} {SplitHeight}", FragmentSplitEvent, nodePath, pageNumber, splitHeight);
    }

    private static bool ShouldLog(ILogger? logger, PaginationOptions options) =>
        logger is not null && options is not null && options.EnableDiagnostics;
}
