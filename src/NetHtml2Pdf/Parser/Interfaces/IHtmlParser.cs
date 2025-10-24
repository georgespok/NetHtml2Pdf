using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core;

namespace NetHtml2Pdf.Parser.Interfaces;

internal interface IHtmlParser
{
    DocumentNode Parse(string html, ILogger? logger = null, Action<string>? onFallbackElement = null);
}