using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Model;

namespace NetHtml2Pdf.Layout.Contexts;

/// <summary>
/// Implements block layout for paragraphs and headings.
/// </summary>
internal sealed class BlockFormattingContext
{
    private readonly IInlineFormattingContext _inlineFormattingContext;

    public BlockFormattingContext(IInlineFormattingContext inlineFormattingContext)
    {
        _inlineFormattingContext = inlineFormattingContext ?? throw new ArgumentNullException(nameof(inlineFormattingContext));
    }

    public LayoutFragment Layout(LayoutBox box, LayoutConstraints constraints)
    {
        ArgumentNullException.ThrowIfNull(box);

        var childFragments = new List<LayoutFragment>();
        foreach (var child in box.Children)
        {
            if (child.Display == DisplayClass.None)
            {
                continue;
            }

            LayoutFragment fragment = child.Display switch
            {
                DisplayClass.Block => Layout(child, constraints.ForBlockChild()),
                DisplayClass.Inline => _inlineFormattingContext.Layout(child, constraints.ForInlineChild()),
                _ => CreateFallbackInlineFragment(child, constraints)
            };

            childFragments.Add(fragment);
        }

        var height = Math.Max(constraints.BlockMin, childFragments.Sum(fragment => fragment.Height));
        if (height <= 0 && childFragments.Count > 0)
        {
            height = Math.Max(constraints.BlockMin, 16f * childFragments.Count);
        }
        else if (height <= 0)
        {
            height = Math.Max(constraints.BlockMin, 16f);
        }

        var diagnostics = new LayoutDiagnostics(
            "BlockFormattingContext",
            constraints,
            constraints.InlineMax,
            height);

        return LayoutFragment.CreateBlock(box, constraints.InlineMax, height, childFragments, diagnostics);
    }

    private static LayoutFragment CreateFallbackInlineFragment(LayoutBox child, LayoutConstraints constraints)
    {
        var diagnostics = new LayoutDiagnostics(
            "BlockFormattingContext.FallbackInline",
            constraints,
            constraints.InlineMax,
            0);

        return LayoutFragment.CreateInline(child, constraints.InlineMax, 0, baseline: null, [], diagnostics);
    }
}
