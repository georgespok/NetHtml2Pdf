using AngleSharp.Dom;
using NetHtml2Pdf.Core;

namespace NetHtml2Pdf.Parser.Interfaces;

internal interface ICssClassStyleExtractor
{
    IReadOnlyDictionary<string, CssStyleMap> Extract(IDocument document);
}