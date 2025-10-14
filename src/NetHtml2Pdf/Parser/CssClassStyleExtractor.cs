using System.Text.RegularExpressions;
using AngleSharp.Dom;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Constants;
using NetHtml2Pdf.Parser.Interfaces;

namespace NetHtml2Pdf.Parser;

/// <summary>
/// Scans inline "style" blocks to build reusable class-based style maps for the parser pipeline.
/// </summary>
internal sealed class CssClassStyleExtractor(ICssDeclarationParser declarationParser, ICssDeclarationUpdater declarationUpdater) : ICssClassStyleExtractor
{
    public ICssDeclarationParser DeclarationParser { get; } = declarationParser;
    public ICssDeclarationUpdater DeclarationUpdater { get; } = declarationUpdater;
    private static readonly Regex ClassRuleRegex = 
        new(CssRegexPatterns.ClassRule, 
        RegexOptions.Compiled | RegexOptions.Multiline);

    public IReadOnlyDictionary<string, CssStyleMap> Extract(IDocument document)
    {
        var result = new Dictionary<string, CssStyleMap>(StringComparer.OrdinalIgnoreCase);
        foreach (var styleElement in document.QuerySelectorAll("style"))
        {
            var cssContent = styleElement.TextContent;
            foreach (Match match in ClassRuleRegex.Matches(cssContent))
            {
                var className = match.Groups["name"].Value;
                if (string.IsNullOrEmpty(className))
                {
                    continue;
                }

                var declarations = match.Groups["body"].Value;
                var styleMap = BuildStyleMap(declarations);

                if (result.TryGetValue(className, out var existing))
                {
                    result[className] = existing.Merge(styleMap);
                }
                else
                {
                    result[className] = styleMap;
                }
            }
        }

        return result;
    }

    private CssStyleMap BuildStyleMap(string declarations)
    {
        var styles = CssStyleMap.Empty;
        foreach (var declaration in DeclarationParser.Parse(declarations))
        {
            styles = DeclarationUpdater.UpdateStyles(styles, declaration);
        }

        return styles;
    }
}
