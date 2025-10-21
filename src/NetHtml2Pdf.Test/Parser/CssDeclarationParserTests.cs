using NetHtml2Pdf.Core.Constants;
using NetHtml2Pdf.Parser;
using Shouldly;

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

    [Theory]
    [InlineData("display: block", CssDisplayValues.Block)]
    [InlineData("display: inline-block", CssDisplayValues.InlineBlock)]
    [InlineData("display: none", CssDisplayValues.None)]
    [InlineData("DISPLAY: BLOCK", "BLOCK")]
    [InlineData("Display: Inline-Block", "Inline-Block")]
    [InlineData(" display : none ", CssDisplayValues.None)]
    public void Parse_DisplayProperty_ShouldParseCorrectly(string declaration, string expectedValue)
    {
        var results = _parser.Parse(declaration).ToList();

        results.ShouldHaveSingleItem();
        results[0].Name.ShouldBe(CssProperties.Display);
        results[0].Value.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData("display: flex")]
    [InlineData("display: grid")]
    [InlineData("display: inline")]
    [InlineData("display: table")]
    [InlineData("display: invalid")]
    public void Parse_UnsupportedDisplayValues_ShouldParseAsIs(string declaration)
    {
        var results = _parser.Parse(declaration).ToList();

        results.ShouldHaveSingleItem();
        results[0].Name.ShouldBe(CssProperties.Display);
        results[0].Value.ShouldBe(declaration.Split(':')[1].Trim());
    }

    [Fact]
    public void Parse_MultipleDisplayDeclarations_ShouldParseAll()
    {
        const string declarations = "display: block; color: red; display: none;";

        var results = _parser.Parse(declarations).ToList();

        results.Count.ShouldBe(3);
        results[0].Name.ShouldBe(CssProperties.Display);
        results[0].Value.ShouldBe(CssDisplayValues.Block);
        results[1].Name.ShouldBe("color");
        results[1].Value.ShouldBe("red");
        results[2].Name.ShouldBe(CssProperties.Display);
        results[2].Value.ShouldBe(CssDisplayValues.None);
    }

    [Fact]
    public void Parse_DisplayWithOtherProperties_ShouldParseCorrectly()
    {
        const string declarations = "margin: 10px; display: inline-block; padding: 5px;";

        var results = _parser.Parse(declarations).ToList();

        results.Count.ShouldBe(3);
        results[0].Name.ShouldBe("margin");
        results[0].Value.ShouldBe("10px");
        results[1].Name.ShouldBe(CssProperties.Display);
        results[1].Value.ShouldBe(CssDisplayValues.InlineBlock);
        results[2].Name.ShouldBe("padding");
        results[2].Value.ShouldBe("5px");
    }

    [Fact]
    public void Parse_EmptyDisplayDeclaration_ShouldSkip()
    {
        const string declarations = "display: ; color: red;";

        var results = _parser.Parse(declarations).ToList();

        results.ShouldHaveSingleItem();
        results[0].Name.ShouldBe("color");
        results[0].Value.ShouldBe("red");
    }

    [Fact]
    public void Parse_InvalidDisplayDeclaration_ShouldSkip()
    {
        const string declarations = "display; color: red;";

        var results = _parser.Parse(declarations).ToList();

        results.ShouldHaveSingleItem();
        results[0].Name.ShouldBe("color");
        results[0].Value.ShouldBe("red");
    }
}