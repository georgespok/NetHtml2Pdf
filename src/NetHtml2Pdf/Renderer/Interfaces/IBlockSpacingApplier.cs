using NetHtml2Pdf.Core;
using QuestPDF.Infrastructure;

namespace NetHtml2Pdf.Renderer.Interfaces;

internal interface IBlockSpacingApplier
{
    IContainer ApplySpacing(IContainer container, CssStyleMap styles);
    IContainer ApplyMargin(IContainer container, CssStyleMap styles);
    IContainer ApplyBorder(IContainer container, CssStyleMap styles);
}