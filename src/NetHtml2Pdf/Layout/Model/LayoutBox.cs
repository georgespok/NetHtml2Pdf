using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;

namespace NetHtml2Pdf.Layout.Model;

/// <summary>
///     Represents a classified node in the layout tree.
/// </summary>
internal sealed class LayoutBox(
    DocumentNode node,
    DisplayClass display,
    CssStyleMap style,
    LayoutSpacing spacing,
    string nodePath,
    IReadOnlyList<LayoutBox> children)
{
    /// <summary>
    ///     Original DOM node.
    /// </summary>
    public DocumentNode Node { get; } = node ?? throw new ArgumentNullException(nameof(node));

    /// <summary>
    ///     Classified display value used to pick formatting context.
    /// </summary>
    public DisplayClass Display { get; } = display;

    /// <summary>
    ///     Snapshot of the computed style.
    /// </summary>
    public CssStyleMap Style { get; } = style;

    /// <summary>
    ///     Margin, padding, and border information associated with the node.
    /// </summary>
    public LayoutSpacing Spacing { get; } = spacing;

    /// <summary>
    ///     Human-readable path in the DOM tree for diagnostics.
    /// </summary>
    public string NodePath { get; } = nodePath ?? throw new ArgumentNullException(nameof(nodePath));

    /// <summary>
    ///     Child layout boxes (block or inline).
    /// </summary>
    public IReadOnlyList<LayoutBox> Children { get; } = children ?? [];
}