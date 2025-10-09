namespace NetHtml2Pdf.Parser.Interfaces;

internal interface ICssDeclarationParser
{
    IEnumerable<CssDeclaration> Parse(string declarations);
}