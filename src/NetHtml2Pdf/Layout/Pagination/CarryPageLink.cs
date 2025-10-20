using System;

namespace NetHtml2Pdf.Layout.Pagination;

internal sealed class CarryPageLink
{
    public CarryPageLink(int? continuesFromPage, int? continuesToPage, float remainingBlockSize)
    {
        if (IsInvalidPageIndex(continuesFromPage))
        {
            throw new ArgumentOutOfRangeException(nameof(continuesFromPage), "Page index must be >= 1 when specified.");
        }

        if (IsInvalidPageIndex(continuesToPage))
        {
            throw new ArgumentOutOfRangeException(nameof(continuesToPage), "Page index must be >= 1 when specified.");
        }

        if (remainingBlockSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(remainingBlockSize), "Remaining block size must be non-negative.");
        }

        ContinuesFromPage = continuesFromPage;
        ContinuesToPage = continuesToPage;
        RemainingBlockSize = remainingBlockSize;
    }

    public int? ContinuesFromPage { get; }

    public int? ContinuesToPage { get; }

    public float RemainingBlockSize { get; }

    private static bool IsInvalidPageIndex(int? page) => page.HasValue && page.Value < 1;
}