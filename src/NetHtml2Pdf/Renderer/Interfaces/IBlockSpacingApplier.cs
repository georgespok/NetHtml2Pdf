using NetHtml2Pdf.Core;
using QuestPDF.Infrastructure;

namespace NetHtml2Pdf.Renderer;

internal interface IBlockSpacingApplier
{
    IContainer ApplySpacing(IContainer container, CssStyleMap styles);
}