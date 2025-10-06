using NetHtml2Pdf.Parser.Interfaces;

namespace NetHtml2Pdf.Parser;

internal readonly record struct CssDeclaration(string Name, string Value);

/// <summary>
/// Parses raw CSS declaration lists into normalized name/value pairs for downstream consumers.
/// </summary>
internal sealed class CssDeclarationParser : ICssDeclarationParser
{
    public IEnumerable<CssDeclaration> Parse(string declarations)
    {
        if (string.IsNullOrWhiteSpace(declarations))
        {
            yield break;
        }

        var tokens = declarations.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var token in tokens)
        {
            var parts = token.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                continue;
            }

            var name = parts[0].Trim();
            var value = parts[1].Trim();
            if (name.Length == 0 || value.Length == 0)
            {
                continue;
            }

            yield return new CssDeclaration(name.ToLowerInvariant(), value);
        }
    }
}
