using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core.Enums;
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

        if (fragments.Count == 0) throw new ArgumentException("At least one fragment is required.", nameof(fragments));

        var contentHeight = pageConstraints.ContentHeight;
        if (contentHeight <= 0)
            throw new InvalidOperationException("Page constraints must provide a positive content height.");

        var pages = new List<PageFragmentTree>();
        var pageNumber = 1;
        var currentPage = new PageBuilder(pageNumber, contentHeight);

        for (var index = 0; index < fragments.Count; index++)
        {
            var fragment = fragments[index];
            ArgumentNullException.ThrowIfNull(fragment);

            if (TryPaginateTableFragment(
                    fragment,
                    pageConstraints,
                    options,
                    logger,
                    pages,
                    ref currentPage,
                    ref pageNumber,
                    contentHeight))
                continue;

            var keepTogether = HasKeepTogether(fragment);
            var keepWithNext = HasKeepWithNext(fragment) && index + 1 < fragments.Count;
            var nextFragment = keepWithNext ? fragments[index + 1] : null;

            if (keepTogether && fragment.Height > contentHeight + Epsilon)
                throw new PaginationException(
                    $"Fragment '{fragment.NodePath}' marked keep-together cannot fit within the available page content height.");

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
                    false,
                    []);

                currentPage.Slices.Add(slice);
                currentPage.RemainingHeight -= sliceHeight;
                remainingHeight -= sliceHeight;

                if (sliceKind is FragmentSliceKind.Start or FragmentSliceKind.Continuation)
                    PaginationDiagnostics.LogFragmentSplit(logger, options, fragment.NodePath, currentPage.PageNumber,
                        sliceHeight);

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
                    if (!isFirstSlice && currentPage.ContinuesFromPrevious) currentPage.RemainingBlockSize = 0;

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
        if (builder.Slices.Count == 0) return;

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

    private static FragmentSliceKind DetermineSliceKind(bool isFirstSlice, float remainingHeightBeforeSlice,
        float sliceHeight)
    {
        var remainingAfterSlice = remainingHeightBeforeSlice - sliceHeight;

        if (isFirstSlice && remainingAfterSlice <= Epsilon) return FragmentSliceKind.Full;

        if (isFirstSlice) return FragmentSliceKind.Start;

        return remainingAfterSlice > Epsilon ? FragmentSliceKind.Continuation : FragmentSliceKind.End;
    }

    private static void AddFullSlice(PageBuilder page, LayoutFragment fragment)
    {
        var topOffset = page.ContentHeight - page.RemainingHeight;
        var slice = new FragmentSlice(
            fragment,
            new PageBounds(0, topOffset, fragment.Width, fragment.Height),
            FragmentSliceKind.Full,
            false,
            []);

        page.Slices.Add(slice);
        page.RemainingHeight = Math.Max(0, page.RemainingHeight - fragment.Height);
    }

    private static bool HasKeepTogether(LayoutFragment fragment)
    {
        return HasBooleanMetadata(fragment, "pagination:keepTogether");
    }

    private static bool HasKeepWithNext(LayoutFragment fragment)
    {
        return HasBooleanMetadata(fragment, "pagination:keepWithNext");
    }

    private static bool HasBooleanMetadata(LayoutFragment fragment, string key)
    {
        var metadata = fragment.Diagnostics.Metadata;
        if (metadata is null) return false;

        return metadata.TryGetValue(key, out var value) && bool.TryParse(value, out var flag) && flag;
    }

    private static bool IsTableFragment(LayoutFragment fragment)
    {
        return fragment.Diagnostics.ContextName == "TableFormattingContext" ||
               fragment.Diagnostics.Metadata.ContainsKey("table:columnCount");
    }

    private bool TryPaginateTableFragment(
        LayoutFragment fragment,
        PageConstraints pageConstraints,
        PaginationOptions options,
        ILogger? logger,
        List<PageFragmentTree> pages,
        ref PageBuilder currentPage,
        ref int pageNumber,
        float contentHeight)
    {
        if (!IsTableFragment(fragment)) return false;

        LayoutFragment? caption = null;
        var headerRows = new List<LayoutFragment>();
        var bodyRows = new List<LayoutFragment>();
        var footerRows = new List<LayoutFragment>();

        foreach (var child in fragment.Children)
        {
            if (IsCaptionFragment(child))
            {
                caption ??= child;
                continue;
            }

            if (TryGetTableSection(child, out var section))
                switch (section)
                {
                    case DocumentNodeType.TableHead:
                        headerRows.Add(child);
                        continue;
                    case DocumentNodeType.TableBody:
                        bodyRows.Add(child);
                        continue;
                    case DocumentNodeType.TableSection:
                        footerRows.Add(child);
                        continue;
                }

            bodyRows.Add(child);
        }

        var remainingTableHeight = bodyRows.Sum(static row => row.Height) + footerRows.Sum(static row => row.Height);
        var headerHeight = headerRows.Sum(static row => row.Height);
        var headerExists = headerRows.Count > 0;

        var headerAddedOnPage = !headerExists;
        var hasTableContentOnPage = false;

        if (caption is not null)
        {
            if (caption.Height > currentPage.RemainingHeight + Epsilon)
            {
                var shouldCarry = hasTableContentOnPage && remainingTableHeight > Epsilon;
                hasTableContentOnPage = StartNewTablePage(
                    ref currentPage,
                    ref pageNumber,
                    contentHeight,
                    pageConstraints,
                    options,
                    logger,
                    pages,
                    remainingTableHeight,
                    shouldCarry,
                    headerExists,
                    ref headerAddedOnPage);
            }

            if (caption.Height > currentPage.RemainingHeight + Epsilon)
                throw new PaginationException(
                    $"Fragment '{caption.NodePath}' cannot fit within the available page content height.");

            AddTableSlice(currentPage, caption);
            hasTableContentOnPage = true;
        }

        foreach (var row in bodyRows)
        {
            EnsureTableHeader(
                row.Height,
                headerRows,
                headerHeight,
                headerExists,
                ref headerAddedOnPage,
                ref hasTableContentOnPage,
                ref currentPage,
                ref pageNumber,
                contentHeight,
                pageConstraints,
                options,
                logger,
                pages,
                remainingTableHeight);

            if (row.Height > currentPage.RemainingHeight + Epsilon)
            {
                var shouldCarry = hasTableContentOnPage && remainingTableHeight > Epsilon;
                hasTableContentOnPage = StartNewTablePage(
                    ref currentPage,
                    ref pageNumber,
                    contentHeight,
                    pageConstraints,
                    options,
                    logger,
                    pages,
                    remainingTableHeight,
                    shouldCarry,
                    headerExists,
                    ref headerAddedOnPage);

                EnsureTableHeader(
                    row.Height,
                    headerRows,
                    headerHeight,
                    headerExists,
                    ref headerAddedOnPage,
                    ref hasTableContentOnPage,
                    ref currentPage,
                    ref pageNumber,
                    contentHeight,
                    pageConstraints,
                    options,
                    logger,
                    pages,
                    remainingTableHeight);
            }

            AddTableSlice(currentPage, row);
            hasTableContentOnPage = true;

            remainingTableHeight -= row.Height;
            currentPage.RemainingBlockSize = remainingTableHeight;
        }

        foreach (var footer in footerRows)
        {
            EnsureTableHeader(
                footer.Height,
                headerRows,
                headerHeight,
                headerExists,
                ref headerAddedOnPage,
                ref hasTableContentOnPage,
                ref currentPage,
                ref pageNumber,
                contentHeight,
                pageConstraints,
                options,
                logger,
                pages,
                remainingTableHeight);

            if (footer.Height > currentPage.RemainingHeight + Epsilon)
            {
                var shouldCarry = hasTableContentOnPage && remainingTableHeight > Epsilon;
                hasTableContentOnPage = StartNewTablePage(
                    ref currentPage,
                    ref pageNumber,
                    contentHeight,
                    pageConstraints,
                    options,
                    logger,
                    pages,
                    remainingTableHeight,
                    shouldCarry,
                    headerExists,
                    ref headerAddedOnPage);

                EnsureTableHeader(
                    footer.Height,
                    headerRows,
                    headerHeight,
                    headerExists,
                    ref headerAddedOnPage,
                    ref hasTableContentOnPage,
                    ref currentPage,
                    ref pageNumber,
                    contentHeight,
                    pageConstraints,
                    options,
                    logger,
                    pages,
                    remainingTableHeight);
            }

            var footerFragment = footer;
            if (!footer.NodePath.Contains("TableFoot", StringComparison.Ordinal))
            {
                var updatedPath = footer.NodePath.Replace("TableSection", "TableFoot", StringComparison.Ordinal);
                footerFragment = CloneFragmentWithNodePath(footer, updatedPath);
            }

            AddTableSlice(currentPage, footerFragment);
            hasTableContentOnPage = true;

            remainingTableHeight -= footer.Height;
            currentPage.RemainingBlockSize = remainingTableHeight;
        }

        currentPage.ContinuesToNext = false;
        currentPage.RemainingBlockSize = 0;

        return true;
    }

    private static void EnsureTableHeader(
        float upcomingHeight,
        IReadOnlyList<LayoutFragment> headerRows,
        float headerHeight,
        bool headerExists,
        ref bool headerAddedOnPage,
        ref bool hasTableContentOnPage,
        ref PageBuilder currentPage,
        ref int pageNumber,
        float contentHeight,
        PageConstraints pageConstraints,
        PaginationOptions options,
        ILogger? logger,
        List<PageFragmentTree> pages,
        float remainingTableHeight)
    {
        if (!headerExists || headerAddedOnPage) return;

        var requiredHeight = headerHeight + upcomingHeight;
        if (headerHeight > currentPage.RemainingHeight + Epsilon ||
            (currentPage.Slices.Count > 0 && requiredHeight > currentPage.RemainingHeight + Epsilon))
        {
            var shouldCarry = hasTableContentOnPage && remainingTableHeight > Epsilon;
            hasTableContentOnPage = StartNewTablePage(
                ref currentPage,
                ref pageNumber,
                contentHeight,
                pageConstraints,
                options,
                logger,
                pages,
                remainingTableHeight,
                shouldCarry,
                headerExists,
                ref headerAddedOnPage);
        }

        foreach (var header in headerRows) AddTableSlice(currentPage, header);

        headerAddedOnPage = true;
        hasTableContentOnPage = true;
    }

    private static bool StartNewTablePage(
        ref PageBuilder currentPage,
        ref int pageNumber,
        float contentHeight,
        PageConstraints pageConstraints,
        PaginationOptions options,
        ILogger? logger,
        List<PageFragmentTree> pages,
        float remainingTableHeight,
        bool shouldCarry,
        bool headerExists,
        ref bool headerAddedOnPage)
    {
        if (shouldCarry)
        {
            currentPage.ContinuesToNext = true;
            currentPage.RemainingBlockSize = remainingTableHeight;
        }
        else
        {
            currentPage.ContinuesToNext = false;
            currentPage.RemainingBlockSize = 0;
        }

        FinalizePage(currentPage, pageConstraints, options, logger, pages);
        pageNumber++;

        currentPage = new PageBuilder(pageNumber, contentHeight)
        {
            ContinuesFromPrevious = shouldCarry,
            RemainingBlockSize = shouldCarry ? remainingTableHeight : 0
        };

        headerAddedOnPage = !headerExists;
        return false;
    }

    private static bool IsCaptionFragment(LayoutFragment fragment)
    {
        return fragment.Diagnostics.Metadata.TryGetValue("table:caption", out var value) &&
               bool.TryParse(value, out var flag) &&
               flag;
    }

    private static bool TryGetTableSection(LayoutFragment fragment, out DocumentNodeType section)
    {
        section = DocumentNodeType.TableBody;

        if (fragment.Diagnostics.Metadata.TryGetValue("table:section", out var value) &&
            Enum.TryParse<DocumentNodeType>(value, out var parsed))
        {
            section = parsed;
            return true;
        }

        return false;
    }

    private static void AddTableSlice(PageBuilder page, LayoutFragment fragment)
    {
        var topOffset = page.ContentHeight - page.RemainingHeight;
        var slice = new FragmentSlice(
            fragment,
            new PageBounds(0, topOffset, fragment.Width, fragment.Height),
            FragmentSliceKind.Full,
            false,
            []);

        page.Slices.Add(slice);
        page.RemainingHeight = Math.Max(0, page.RemainingHeight - fragment.Height);
    }

    private static LayoutFragment CloneFragmentWithNodePath(LayoutFragment fragment, string nodePath)
    {
        var box = fragment.Box;
        var clonedBox = new LayoutBox(
            box.Node,
            box.Display,
            box.Style,
            box.Spacing,
            nodePath,
            box.Children);

        return new LayoutFragment(
            fragment.Kind,
            clonedBox,
            fragment.Width,
            fragment.Height,
            fragment.Baseline,
            fragment.Children,
            fragment.Diagnostics);
    }

    private sealed class PageBuilder(int pageNumber, float contentHeight)
    {
        public int PageNumber { get; } = pageNumber;

        public float ContentHeight { get; } = contentHeight;

        public float RemainingHeight { get; set; } = contentHeight;

        public bool ContinuesFromPrevious { get; init; }

        public bool ContinuesToNext { get; set; }

        public float RemainingBlockSize { get; set; }

        public List<FragmentSlice> Slices { get; } = [];
    }
}