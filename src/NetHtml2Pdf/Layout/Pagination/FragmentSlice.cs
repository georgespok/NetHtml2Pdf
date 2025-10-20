using NetHtml2Pdf.Layout.Model;

namespace NetHtml2Pdf.Layout.Pagination;

internal sealed class FragmentSlice
{
    public FragmentSlice(
        LayoutFragment sourceFragment,
        PageBounds sliceBounds,
        FragmentSliceKind sliceKind,
        bool isBreakAllowed,
        IReadOnlyList<FragmentSlice> children)
    {
        SourceFragment = sourceFragment ?? throw new ArgumentNullException(nameof(sourceFragment));
        SliceBounds = sliceBounds;
        SliceKind = sliceKind;
        IsBreakAllowed = isBreakAllowed;
        Children = children ?? [];
    }

    public LayoutFragment SourceFragment { get; }

    public PageBounds SliceBounds { get; }

    public FragmentSliceKind SliceKind { get; }

    public bool IsBreakAllowed { get; }

    public IReadOnlyList<FragmentSlice> Children { get; }
}
