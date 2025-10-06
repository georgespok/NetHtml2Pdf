using NetHtml2Pdf.Core;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NetHtml2Pdf.Renderer;

internal sealed class BlockSpacingApplier : IBlockSpacingApplier
{
    public IContainer ApplySpacing(IContainer container, CssStyleMap styles)
    {
        if (!styles.Padding.HasValue)
        {
            return container;
        }             

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

        return container;
    }
}
