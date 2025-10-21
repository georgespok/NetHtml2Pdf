using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Contexts;
using NetHtml2Pdf.Layout.Diagnostics;
using NetHtml2Pdf.Layout.Engines;
using NetHtml2Pdf.Layout.Model;

namespace NetHtml2Pdf.Layout.FormattingContexts;

/// <summary>
///     Preview flex formatting context. Implements a minimal one-dimensional layout
///     that measures children using existing contexts and aggregates them into a
///     single block fragment with alignment metadata. Wrapping is not supported.
/// </summary>
internal sealed class FlexFormattingContext(
    BlockFormattingContext blockFormattingContext,
    IInlineFormattingContext inlineFormattingContext)
{
    private readonly BlockFormattingContext _blockFormattingContext =
        blockFormattingContext ?? throw new ArgumentNullException(nameof(blockFormattingContext));

    private readonly IInlineFormattingContext _inlineFormattingContext =
        inlineFormattingContext ?? throw new ArgumentNullException(nameof(inlineFormattingContext));

    public LayoutFragment Layout(LayoutBox box, LayoutConstraints constraints, ILogger<LayoutEngine>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(box);

        var childFragments = new List<LayoutFragment>();
        foreach (var child in box.Children)
        {
            if (child.Display == DisplayClass.None) continue;

            var layoutFragment = child.Display switch
            {
                DisplayClass.Block => _blockFormattingContext.Layout(child, constraints.ForBlockChild()),
                DisplayClass.Inline => _inlineFormattingContext.Layout(child, constraints.ForInlineChild()),
                DisplayClass.InlineBlock => _inlineFormattingContext.Layout(child, constraints.ForInlineChild()),
                _ => _inlineFormattingContext.Layout(child, constraints.ForInlineChild())
            };

            childFragments.Add(layoutFragment);
        }

        // Minimal aggregation: horizontal (row) flow, no wrapping
        var width = constraints.InlineMax;
        var height = childFragments.Sum(fragment => fragment.Height);
        if (height <= 0 && childFragments.Count > 0)
            height = Math.Max(constraints.BlockMin, 16f * childFragments.Count);
        else if (height <= 0) height = Math.Max(constraints.BlockMin, 16f);

        var metadata = new Dictionary<string, string>
        {
            ["flex:direction"] = "row",
            ["flex:justify"] = "flex-start",
            ["flex:align"] = "stretch",
            ["flex:wrap"] = "nowrap"
        };

        var diagnostics = new LayoutDiagnostics(
            "FlexFormattingContext",
            constraints,
            width,
            height,
            metadata);

        var fragment = LayoutFragment.CreateBlock(box, width, height, childFragments, diagnostics);

        // Emit a simple measured event; downgrade events would be logged when unsupported properties are encountered.
        FlexDiagnostics.LogFlexMeasured(logger, fragment);

        return fragment;
    }
}