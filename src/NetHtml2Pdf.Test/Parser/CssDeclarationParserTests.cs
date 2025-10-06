using System.Linq;
using NetHtml2Pdf.Parser;
using Shouldly;
using Xunit;

namespace NetHtml2Pdf.Test.Parser;

public class CssDeclarationParserTests
{
    private readonly CssDeclarationParser _parser = new();

    [Fact]
    public void Parse_ShouldNormalizeNamesAndTrimValues()
    {
        const string declarations = " font-weight: bold ; margin-top : 10px; invalid ; padding-left:  4em;";

        var results = _parser.Parse(declarations).ToList();

        results.Count.ShouldBe(3);
        results[0].Name.ShouldBe("font-weight");
        results[0].Value.ShouldBe("bold");
        results[1].Name.ShouldBe("margin-top");
        results[1].Value.ShouldBe("10px");
        results[2].Name.ShouldBe("padding-left");
        results[2].Value.ShouldBe("4em");
    }
}
