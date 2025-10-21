using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core;

namespace NetHtml2Pdf.Parser.Interfaces;

internal interface ICssClassStyleExtractor
{
    // Expose dependencies so downstream components can reuse them
    ICssDeclarationParser DeclarationParser { get; }
    ICssDeclarationUpdater DeclarationUpdater { get; }
    IReadOnlyDictionary<string, CssStyleMap> Extract(IDocument document, ILogger? logger = null);
}