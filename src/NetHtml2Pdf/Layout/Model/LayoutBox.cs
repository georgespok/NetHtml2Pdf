using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;

namespace NetHtml2Pdf.Layout.Model;

/// <summary>
/// Represents a classified node in the layout tree.
/// </summary>
internal sealed class LayoutBox
{
    public LayoutBox(
        DocumentNode node,
        DisplayClass display,
        CssStyleMap style,
        LayoutSpacing spacing,
        string nodePath,
        IReadOnlyList<LayoutBox> children)
    {
        Node = node ?? throw new ArgumentNullException(nameof(node));
        Display = display;
        Style = style;
        Spacing = spacing;
        NodePath = nodePath ?? throw new ArgumentNullException(nameof(nodePath));
        Children = children ?? [];
    }

    /// <summary>
    /// Original DOM node.
    /// </summary>
    public DocumentNode Node { get; }

    /// <summary>
    /// Classified display value used to pick formatting context.
    /// </summary>
    public DisplayClass Display { get; }

    /// <summary>
    /// Snapshot of the computed style.
    /// </summary>
    public CssStyleMap Style { get; }

    /// <summary>
    /// Margin, padding, and border information associated with the node.
    /// </summary>
    public LayoutSpacing Spacing { get; }

    /// <summary>
    /// Human-readable path in the DOM tree for diagnostics.
    /// </summary>
    public string NodePath { get; }

    /// <summary>
    /// Child layout boxes (block or inline).
    /// </summary>
    public IReadOnlyList<LayoutBox> Children { get; }
}
