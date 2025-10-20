using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Layout.Model;

namespace NetHtml2Pdf.Layout.Pagination;

internal class PaginationService
{
    private const float Epsilon = 0.01f;

    public virtual PaginatedDocument Paginate(
        IReadOnlyList<LayoutFragment> fragments,
        PageConstraints pageConstraints,
        PaginationOptions options,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(fragments);
        ArgumentNullException.ThrowIfNull(pageConstraints);
        ArgumentNullException.ThrowIfNull(options);

        if (fragments.Count == 0)
        {
            throw new ArgumentException("At least one fragment is required.", nameof(fragments));
        }

        var contentHeight = pageConstraints.ContentHeight;
        if (contentHeight <= 0)
        {
            throw new InvalidOperationException("Page constraints must provide a positive content height.");
        }

        var pages = new List<PageFragmentTree>();
        var pageNumber = 1;
        var currentPage = new PageBuilder(pageNumber, contentHeight);

        for (var index = 0; index < fragments.Count; index++)
        {
            var fragment = fragments[index];
            ArgumentNullException.ThrowIfNull(fragment);

            var keepTogether = HasKeepTogether(fragment);
            var keepWithNext = HasKeepWithNext(fragment) && index + 1 < fragments.Count;
            var nextFragment = keepWithNext ? fragments[index + 1] : null;

            if (keepTogether && fragment.Height > contentHeight + Epsilon)
            {
                throw new PaginationException(
                    $"Fragment '{fragment.NodePath}' marked keep-together cannot fit within the available page content height.");
            }

            if (keepWithNext && currentPage.Slices.Count > 0 && nextFragment is not null)
            {
                var combinedHeight = fragment.Height + nextFragment.Height;
                if (combinedHeight > currentPage.RemainingHeight + Epsilon)
                {
                    FinalizePage(currentPage, pageConstraints, options, logger, pages);
                    pageNumber++;
                    currentPage = new PageBuilder(pageNumber, contentHeight);
                }
            }

            if (keepTogether)
            {
                if (fragment.Height > currentPage.RemainingHeight + Epsilon && currentPage.Slices.Count > 0)
                {
                    FinalizePage(currentPage, pageConstraints, options, logger, pages);
                    pageNumber++;
                    currentPage = new PageBuilder(pageNumber, contentHeight);
                }

                AddFullSlice(currentPage, fragment);
                continue;
            }

            var remainingHeight = fragment.Height;
            var isFirstSlice = true;

            while (remainingHeight > 0)
            {
                if (currentPage.RemainingHeight <= Epsilon)
                {
                    FinalizePage(currentPage, pageConstraints, options, logger, pages);
                    pageNumber++;
                    currentPage = new PageBuilder(pageNumber, contentHeight)
                    {
                        ContinuesFromPrevious = currentPage.ContinuesToNext,
                        RemainingBlockSize = currentPage.ContinuesToNext ? currentPage.RemainingBlockSize : 0
                    };
                }

                var sliceHeight = Math.Min(remainingHeight, currentPage.RemainingHeight);
                var topOffset = currentPage.ContentHeight - currentPage.RemainingHeight;
                var sliceKind = DetermineSliceKind(isFirstSlice, remainingHeight, sliceHeight);

                var slice = new FragmentSlice(
                    fragment,
                    new PageBounds(0, topOffset, fragment.Width, sliceHeight),
                    sliceKind,
                    isBreakAllowed: false,
                    []);

                currentPage.Slices.Add(slice);
                currentPage.RemainingHeight -= sliceHeight;
                remainingHeight -= sliceHeight;

                if (sliceKind is FragmentSliceKind.Start or FragmentSliceKind.Continuation)
                {
                    PaginationDiagnostics.LogFragmentSplit(logger, options, fragment.NodePath, currentPage.PageNumber, sliceHeight);
                }

                if (remainingHeight > 0)
                {
                    currentPage.ContinuesToNext = true;
                    currentPage.RemainingBlockSize = remainingHeight;

                    FinalizePage(currentPage, pageConstraints, options, logger, pages);
                    pageNumber++;
                    currentPage = new PageBuilder(pageNumber, contentHeight)
                    {
                        ContinuesFromPrevious = true,
                        RemainingBlockSize = remainingHeight
                    };

                    isFirstSlice = false;
                }
                else
                {
                    if (!isFirstSlice && currentPage.ContinuesFromPrevious)
                    {
                        currentPage.RemainingBlockSize = 0;
                    }

                    isFirstSlice = false;
                }
            }
        }

        FinalizePage(currentPage, pageConstraints, options, logger, pages);

        return new PaginatedDocument(pageConstraints, pages);
    }

    private static void FinalizePage(
        PageBuilder builder,
        PageConstraints constraints,
        PaginationOptions options,
        ILogger? logger,
        ICollection<PageFragmentTree> pages)
    {
        if (builder.Slices.Count == 0)
        {
            return;
        }

        CarryPageLink? carry = null;
        if (builder.ContinuesFromPrevious || builder.ContinuesToNext)
        {
            var continuesFrom = builder.ContinuesFromPrevious ? builder.PageNumber - 1 : (int?)null;
            var continuesTo = builder.ContinuesToNext ? builder.PageNumber + 1 : (int?)null;
            carry = new CarryPageLink(continuesFrom, continuesTo, builder.RemainingBlockSize);
        }

        var page = new PageFragmentTree(
            builder.PageNumber,
            PageBounds.FromSize(constraints.ContentWidth, constraints.ContentHeight),
            builder.Slices.ToArray(),
            carry);

        pages.Add(page);
        PaginationDiagnostics.LogPageCreated(logger, options, builder.PageNumber, builder.RemainingBlockSize);
    }

    private static FragmentSliceKind DetermineSliceKind(bool isFirstSlice, float remainingHeightBeforeSlice, float sliceHeight)
    {
        var remainingAfterSlice = remainingHeightBeforeSlice - sliceHeight;

        if (isFirstSlice && remainingAfterSlice <= Epsilon)
        {
            return FragmentSliceKind.Full;
        }

        if (isFirstSlice)
        {
            return FragmentSliceKind.Start;
        }

        return remainingAfterSlice > Epsilon ? FragmentSliceKind.Continuation : FragmentSliceKind.End;
    }

    private static void AddFullSlice(PageBuilder page, LayoutFragment fragment)
    {
        var topOffset = page.ContentHeight - page.RemainingHeight;
        var slice = new FragmentSlice(
            fragment,
            new PageBounds(0, topOffset, fragment.Width, fragment.Height),
            FragmentSliceKind.Full,
            isBreakAllowed: false,
            []);

        page.Slices.Add(slice);
        page.RemainingHeight = Math.Max(0, page.RemainingHeight - fragment.Height);
    }

    private static bool HasKeepTogether(LayoutFragment fragment) => HasBooleanMetadata(fragment, "pagination:keepTogether");

    private static bool HasKeepWithNext(LayoutFragment fragment) => HasBooleanMetadata(fragment, "pagination:keepWithNext");

    private static bool HasBooleanMetadata(LayoutFragment fragment, string key)
    {
        var metadata = fragment.Diagnostics.Metadata;
        if (metadata is null)
        {
            return false;
        }

        return metadata.TryGetValue(key, out var value) && bool.TryParse(value, out var flag) && flag;
    }

    private sealed class PageBuilder
    {
        public PageBuilder(int pageNumber, float contentHeight)
        {
            PageNumber = pageNumber;
            ContentHeight = contentHeight;
            RemainingHeight = contentHeight;
        }

        public int PageNumber { get; }

        public float ContentHeight { get; }

        public float RemainingHeight { get; set; }

        public bool ContinuesFromPrevious { get; init; }

        public bool ContinuesToNext { get; set; }

        public float RemainingBlockSize { get; set; }

        public List<FragmentSlice> Slices { get; } = [];
    }
}
