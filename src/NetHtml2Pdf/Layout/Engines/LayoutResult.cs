using NetHtml2Pdf.Layout.Model;

namespace NetHtml2Pdf.Layout.Engines;

internal sealed class LayoutResult
{
    private LayoutResult(bool isSuccess, bool isFallback, bool isDisabled,
        string? fallbackReason, IReadOnlyList<LayoutFragment> fragments)
    {
        IsSuccess = isSuccess;
        IsFallback = isFallback;
        IsDisabled = isDisabled;
        FallbackReason = fallbackReason;
        Fragments = fragments ?? [];
    }

    public bool IsSuccess { get; }

    public bool IsFallback { get; }

    public bool IsDisabled { get; }

    public string? FallbackReason { get; }

    public IReadOnlyList<LayoutFragment> Fragments { get; }

    public static LayoutResult Success(IReadOnlyList<LayoutFragment> fragments)
    {
        return new LayoutResult(true, false, false, null, fragments);
    }

    public static LayoutResult Disabled()
    {
        return new LayoutResult(false, false, true, "Layout engine disabled.", []);
    }

    public static LayoutResult Fallback(string reason)
    {
        return new LayoutResult(false, true, false, reason, []);
    }
}