using NetHtml2Pdf.Core;

namespace NetHtml2Pdf.Parser.Interfaces;

internal interface ICssDeclarationUpdater
{
    CssStyleMap UpdateStyles(CssStyleMap styles, CssDeclaration declaration);
}