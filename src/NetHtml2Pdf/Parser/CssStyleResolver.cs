using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Constants;
using NetHtml2Pdf.Parser.Interfaces;

namespace NetHtml2Pdf.Parser;

/// <summary>
///     Resolves the effective style map for an element by layering class-based
///     and inline declarations over inherited values.
/// </summary>
internal sealed class CssStyleResolver(
    IReadOnlyDictionary<string, CssStyleMap> classStyles,
    ICssDeclarationParser declarationParser,
    ICssDeclarationUpdater declarationUpdater)
{
    public CssStyleMap Resolve(IElement element, CssStyleMap inherited, ILogger? logger = null)
    {
        // Start with inherited styles, but filter out non-inheritable box model properties
        // Box model properties (margin, padding, border) should NOT be inherited from parent
        var styles = inherited.WithMargin(BoxSpacing.Empty)
            .WithPadding(BoxSpacing.Empty)
            .WithBorder(BorderInfo.Empty);

        styles = ApplyClassStyles(element, styles);
        styles = ApplyInlineStyles(element, styles, logger);
        return styles;
    }

    private CssStyleMap ApplyClassStyles(IElement element, CssStyleMap styles)
    {
        var classAttribute = element.GetAttribute(HtmlAttributes.Class);
        if (string.IsNullOrWhiteSpace(classAttribute)) return styles;

        var classes = classAttribute.Split(' ',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var className in classes)
            if (classStyles.TryGetValue(className, out var value))
                styles = styles.Merge(value);

        return styles;
    }

    private CssStyleMap ApplyInlineStyles(IElement element, CssStyleMap styles, ILogger? logger)
    {
        var inlineStyle = element.GetAttribute(HtmlAttributes.Style);

        if (string.IsNullOrWhiteSpace(inlineStyle)) return styles;

        foreach (var declaration in declarationParser.Parse(inlineStyle))
            styles = declarationUpdater.UpdateStyles(styles, declaration, logger);

        return styles;
    }
}