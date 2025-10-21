using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;

namespace NetHtml2Pdf.Layout.Model;

/// <summary>
///     Represents a measured fragment of layout output.
/// </summary>
internal sealed class LayoutFragment(
    LayoutFragmentKind kind,
    LayoutBox box,
    float width,
    float height,
    float? baseline,
    IReadOnlyList<LayoutFragment> children,
    LayoutDiagnostics diagnostics)
{
    public LayoutFragmentKind Kind { get; } = kind;

    public LayoutBox Box { get; } = box ?? throw new ArgumentNullException(nameof(box));

    public DocumentNode Node => Box.Node;

    public DisplayClass Display => Box.Display;

    public string NodePath => Box.NodePath;

    public float Width { get; } = width;

    public float Height { get; } = height;

    public float? Baseline { get; } = baseline;

    public IReadOnlyList<LayoutFragment> Children { get; } = children ?? [];

    public LayoutDiagnostics Diagnostics { get; } = diagnostics;

    public static LayoutFragment CreateBlock(LayoutBox box, float width, float height,
        IReadOnlyList<LayoutFragment> children, LayoutDiagnostics diagnostics)
    {
        return new LayoutFragment(LayoutFragmentKind.Block, box, width, height, null, children, diagnostics);
    }

    public static LayoutFragment CreateInline(LayoutBox box, float width, float height, float? baseline,
        IReadOnlyList<LayoutFragment> children, LayoutDiagnostics diagnostics)
    {
        return new LayoutFragment(LayoutFragmentKind.Inline, box, width, height, baseline, children, diagnostics);
    }
}