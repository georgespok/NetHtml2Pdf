using NetHtml2Pdf.Layout.Model;

namespace NetHtml2Pdf.Layout.Contexts;

internal interface IInlineFormattingContext
{
    LayoutFragment Layout(LayoutBox box, LayoutConstraints constraints);
}