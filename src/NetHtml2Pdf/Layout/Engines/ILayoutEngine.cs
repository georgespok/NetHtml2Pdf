using NetHtml2Pdf.Core;
using NetHtml2Pdf.Layout.Model;

namespace NetHtml2Pdf.Layout.Engines;

internal interface ILayoutEngine
{
    LayoutResult Layout(DocumentNode root, LayoutConstraints constraints, LayoutEngineOptions options);
}