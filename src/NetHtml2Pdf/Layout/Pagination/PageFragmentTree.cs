namespace NetHtml2Pdf.Layout.Pagination;

internal sealed class PageFragmentTree
{
    public PageFragmentTree(
        int pageNumber,
        PageBounds contentBounds,
        IReadOnlyList<FragmentSlice> fragments,
        CarryPageLink? carryOver = null)
    {
        if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be >= 1.");

        PageNumber = pageNumber;
        ContentBounds = contentBounds;
        Fragments = fragments ?? [];
        CarryLink = carryOver;
    }

    public int PageNumber { get; }

    public PageBounds ContentBounds { get; }

    public IReadOnlyList<FragmentSlice> Fragments { get; }

    public CarryPageLink? CarryLink { get; }
}