using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Model;
using NetHtml2Pdf.Renderer.Inline;

namespace NetHtml2Pdf.Layout.Contexts;

/// <summary>
/// Wraps the existing InlineFlowLayoutEngine to produce layout fragments.
/// </summary>
internal sealed class InlineFormattingContext : IInlineFormattingContext
{
    private readonly InlineFlowLayoutEngine _inlineFlowLayoutEngine;

    public InlineFormattingContext(InlineFlowLayoutEngine inlineFlowLayoutEngine)
    {
        _inlineFlowLayoutEngine = inlineFlowLayoutEngine ?? throw new ArgumentNullException(nameof(inlineFlowLayoutEngine));
    }

    public LayoutFragment Layout(LayoutBox box, LayoutConstraints constraints)
    {
        ArgumentNullException.ThrowIfNull(box);

        if (box.Children.Count == 0)
        {
            var leafDiagnostics = new LayoutDiagnostics(
                "InlineFormattingContext.Leaf",
                constraints,
                0,
                0);

            return LayoutFragment.CreateInline(box, 0, 0, baseline: null, [], leafDiagnostics);
        }

        var lineFragments = new List<LayoutFragment>();

        foreach (var child in box.Children)
        {
            if (child.Display == DisplayClass.Inline)
            {
                lineFragments.Add(Layout(child, constraints));
            }
        }

        var diagnostics = new LayoutDiagnostics(
            "InlineFormattingContext",
            constraints,
            constraints.InlineMax,
            0);

        return LayoutFragment.CreateInline(box, constraints.InlineMax, 0, baseline: null, lineFragments, diagnostics);
    }
}
