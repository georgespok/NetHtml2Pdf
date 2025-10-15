using NetHtml2Pdf.Core;
using NetHtml2Pdf.Renderer.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NetHtml2Pdf.Renderer;

internal sealed class BlockSpacingApplier : IBlockSpacingApplier
{
    public IContainer ApplySpacing(IContainer container, CssStyleMap styles)
    {
        // Apply padding - this affects the content area inside the element
        if (styles.Padding.HasValue)
        {
            if (styles.Padding.Top.HasValue)
            {
                container = container.PaddingTop((float)styles.Padding.Top.Value);
            }

            if (styles.Padding.Right.HasValue)
            {
                container = container.PaddingRight((float)styles.Padding.Right.Value);
            }

            if (styles.Padding.Bottom.HasValue)
            {
                container = container.PaddingBottom((float)styles.Padding.Bottom.Value);
            }

            if (styles.Padding.Left.HasValue)
            {
                container = container.PaddingLeft((float)styles.Padding.Left.Value);
            }
        }

        // Note: Margin is NOT applied here anymore
        // Margin should be applied at the parent level to affect positioning relative to siblings
        // This method only applies padding which affects the content area

        return container;
    }

    /// <summary>
    /// Applies margin to a container. This should be called at the parent level
    /// to affect positioning relative to sibling elements.
    /// </summary>
    public IContainer ApplyMargin(IContainer container, CssStyleMap styles)
    {
        if (styles.Margin.HasValue)
        {
            // QuestPDF doesn't have individual margin methods, so we'll use padding for now
            // This is a workaround - in CSS, margin creates space outside the element
            // In QuestPDF, we'll simulate this with padding on the parent container
            if (styles.Margin.Top.HasValue)
            {
                container = container.PaddingTop((float)styles.Margin.Top.Value);
            }

            if (styles.Margin.Right.HasValue)
            {
                container = container.PaddingRight((float)styles.Margin.Right.Value);
            }

            if (styles.Margin.Bottom.HasValue)
            {
                container = container.PaddingBottom((float)styles.Margin.Bottom.Value);
            }

            if (styles.Margin.Left.HasValue)
            {
                container = container.PaddingLeft((float)styles.Margin.Left.Value);
            }
        }

        return container;
    }

    /// <summary>
    /// Applies border to a container. This should be called at the element level
    /// to add a border around the element's content area.
    /// </summary>
    public IContainer ApplyBorder(IContainer container, CssStyleMap styles)
    {
        if (styles.Border.IsVisible)
        {
            var borderWidth = (float)styles.Border.GetWidthInPixels();
            var borderColor = styles.Border.GetColor();

            // QuestPDF doesn't support different border styles (solid, dashed, etc.) in the basic API
            // We'll render all visible borders as solid borders
            container = container.Border(borderWidth).BorderColor(borderColor);
        }

        return container;
    }
}
