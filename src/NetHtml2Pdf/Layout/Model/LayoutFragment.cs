using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;

namespace NetHtml2Pdf.Layout.Model;

/// <summary>
/// Represents a measured fragment of layout output.
/// </summary>
internal sealed class LayoutFragment
{
    public LayoutFragment(
        LayoutFragmentKind kind,
        LayoutBox box,
        float width,
        float height,
        float? baseline,
        IReadOnlyList<LayoutFragment> children,
        LayoutDiagnostics diagnostics)
    {
        Kind = kind;
        Box = box ?? throw new ArgumentNullException(nameof(box));
        Width = width;
        Height = height;
        Baseline = baseline;
        Children = children ?? Array.Empty<LayoutFragment>();
        Diagnostics = diagnostics;
    }

    public LayoutFragmentKind Kind { get; }

    public LayoutBox Box { get; }

    public DocumentNode Node => Box.Node;

    public DisplayClass Display => Box.Display;

    public string NodePath => Box.NodePath;

    public float Width { get; }

    public float Height { get; }

    public float? Baseline { get; }

    public IReadOnlyList<LayoutFragment> Children { get; }

    public LayoutDiagnostics Diagnostics { get; }

    public static LayoutFragment CreateBlock(LayoutBox box, float width, float height, IReadOnlyList<LayoutFragment> children, LayoutDiagnostics diagnostics)
    {
        return new LayoutFragment(LayoutFragmentKind.Block, box, width, height, baseline: null, children, diagnostics);
    }

    public static LayoutFragment CreateInline(LayoutBox box, float width, float height, float? baseline, IReadOnlyList<LayoutFragment> children, LayoutDiagnostics diagnostics)
    {
        return new LayoutFragment(LayoutFragmentKind.Inline, box, width, height, baseline, children, diagnostics);
    }

    public static LayoutFragment CreateLine(LayoutBox box, float width, float height, float baseline, IReadOnlyList<LayoutFragment> children, LayoutDiagnostics diagnostics)
    {
        return new LayoutFragment(LayoutFragmentKind.Line, box, width, height, baseline, children, diagnostics);
    }
}
