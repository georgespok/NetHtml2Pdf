using AngleSharp.Dom;
using NetHtml2Pdf.Core;

namespace NetHtml2Pdf.Parser.Interfaces;

internal interface ICssClassStyleExtractor
{
    IReadOnlyDictionary<string, CssStyleMap> Extract(IDocument document);

    // Expose dependencies so downstream components can reuse them
    ICssDeclarationParser DeclarationParser { get; }
    ICssDeclarationUpdater DeclarationUpdater { get; }
}