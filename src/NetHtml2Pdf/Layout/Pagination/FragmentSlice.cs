using NetHtml2Pdf.Layout.Model;

namespace NetHtml2Pdf.Layout.Pagination;

internal sealed class FragmentSlice(
    LayoutFragment sourceFragment,
    PageBounds sliceBounds,
    FragmentSliceKind sliceKind,
    bool isBreakAllowed,
    IReadOnlyList<FragmentSlice> children)
{
    public LayoutFragment SourceFragment { get; } =
        sourceFragment ?? throw new ArgumentNullException(nameof(sourceFragment));

    public PageBounds SliceBounds { get; } = sliceBounds;

    public FragmentSliceKind SliceKind { get; } = sliceKind;

    public bool IsBreakAllowed { get; } = isBreakAllowed;

    public IReadOnlyList<FragmentSlice> Children { get; } = children ?? [];
}