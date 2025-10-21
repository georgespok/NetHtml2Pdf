using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Contexts;
using NetHtml2Pdf.Layout.Model;

namespace NetHtml2Pdf.Layout.FormattingContexts;

/// <summary>
///     Measures inline-block nodes while preserving child layout fragments so pagination
///     and adapters can reason about their geometry.
/// </summary>
internal sealed class InlineBlockFormattingContext(
    BlockFormattingContext blockFormattingContext,
    IInlineFormattingContext inlineFormattingContext)
{
    private readonly BlockFormattingContext _blockFormattingContext =
        blockFormattingContext ?? throw new ArgumentNullException(nameof(blockFormattingContext));

    private readonly IInlineFormattingContext _inlineFormattingContext =
        inlineFormattingContext ?? throw new ArgumentNullException(nameof(inlineFormattingContext));

    public LayoutFragment Layout(LayoutBox box, LayoutConstraints constraints)
    {
        ArgumentNullException.ThrowIfNull(box);

        var childFragments = new List<LayoutFragment>();
        foreach (var child in box.Children)
        {
            var fragment = child.Display switch
            {
                DisplayClass.Block => _blockFormattingContext.Layout(child, constraints.ForBlockChild()),
                DisplayClass.Inline => _inlineFormattingContext.Layout(child, constraints.ForInlineChild()),
                DisplayClass.InlineBlock => Layout(child, constraints),
                _ => CreateEmptyInlineFragment(child, constraints)
            };

            childFragments.Add(fragment);
        }

        var intrinsicWidth = childFragments.Count == 0
            ? constraints.InlineMin
            : childFragments.Max(fragment => fragment.Width);

        if (intrinsicWidth <= 0f)
            intrinsicWidth = Math.Clamp(40f, constraints.InlineMin, constraints.InlineMax);
        else
            intrinsicWidth = Math.Clamp(intrinsicWidth, constraints.InlineMin, constraints.InlineMax);

        var intrinsicHeight = childFragments.Count == 0
            ? constraints.BlockMin
            : childFragments.Sum(fragment => fragment.Height);

        if (intrinsicHeight <= 0f)
            intrinsicHeight = Math.Clamp(24f, constraints.BlockMin, constraints.BlockMax);
        else
            intrinsicHeight = Math.Clamp(intrinsicHeight, Math.Max(1f, constraints.BlockMin), constraints.BlockMax);

        var baseline = intrinsicHeight > 0f ? intrinsicHeight : (float?)null;

        var diagnostics = new LayoutDiagnostics(
            "InlineBlockFormattingContext",
            constraints,
            intrinsicWidth,
            intrinsicHeight);

        return LayoutFragment.CreateInline(
            box,
            intrinsicWidth,
            intrinsicHeight,
            baseline,
            childFragments,
            diagnostics);
    }

    private static LayoutFragment CreateEmptyInlineFragment(LayoutBox child, LayoutConstraints constraints)
    {
        var diagnostics = new LayoutDiagnostics(
            "InlineBlockFormattingContext.Empty",
            constraints,
            constraints.InlineMin,
            constraints.BlockMin);

        return LayoutFragment.CreateInline(child, constraints.InlineMin, constraints.BlockMin, null, [], diagnostics);
    }
}