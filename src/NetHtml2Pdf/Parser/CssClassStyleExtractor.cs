using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Constants;
using NetHtml2Pdf.Parser.Interfaces;

namespace NetHtml2Pdf.Parser;

/// <summary>
///     Scans inline "style" blocks to build reusable class-based style maps for the parser pipeline.
/// </summary>
internal sealed partial class CssClassStyleExtractor(
    ICssDeclarationParser declarationParser,
    ICssDeclarationUpdater declarationUpdater) : ICssClassStyleExtractor
{
    private static readonly Regex ClassRuleRegex =
        MyRegex();

    public ICssDeclarationParser DeclarationParser { get; } = declarationParser;
    public ICssDeclarationUpdater DeclarationUpdater { get; } = declarationUpdater;

    public IReadOnlyDictionary<string, CssStyleMap> Extract(IDocument document, ILogger? logger = null)
    {
        var result = new Dictionary<string, CssStyleMap>(StringComparer.OrdinalIgnoreCase);
        foreach (var styleElement in document.QuerySelectorAll("style"))
        {
            var cssContent = styleElement.TextContent;
            foreach (Match match in ClassRuleRegex.Matches(cssContent))
            {
                var className = match.Groups["name"].Value;
                if (string.IsNullOrEmpty(className)) continue;

                var declarations = match.Groups["body"].Value;
                var styleMap = BuildStyleMap(declarations, logger);

                if (result.TryGetValue(className, out var existing))
                    result[className] = existing.Merge(styleMap);
                else
                    result[className] = styleMap;
            }
        }

        return result;
    }

    private CssStyleMap BuildStyleMap(string declarations, ILogger? logger)
    {
        var styles = CssStyleMap.Empty;
        foreach (var declaration in DeclarationParser.Parse(declarations))
            styles = DeclarationUpdater.UpdateStyles(styles, declaration, logger);

        return styles;
    }

    [GeneratedRegex(CssRegexPatterns.ClassRule, RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}