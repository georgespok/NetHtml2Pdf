using NetHtml2Pdf.Core;

namespace NetHtml2Pdf.Parser.Interfaces;

internal interface IHtmlParser
{
    DocumentNode Parse(string html);
}
