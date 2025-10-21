using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Constants;
using NetHtml2Pdf.Parser;
using Shouldly;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Parser;

public class BorderInheritanceHtmlTests
{
    private readonly HtmlParser _parser;
    private readonly ITestOutputHelper _testOutputHelper;

    public BorderInheritanceHtmlTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        var angleSharp = new AngleSharp.Html.Parser.HtmlParser();
        var cssParser = new CssDeclarationParser();
        var cssUpdater = new CssStyleUpdater();
        var classExtractor = new CssClassStyleExtractor(cssParser, cssUpdater);
        _parser = new HtmlParser(angleSharp, classExtractor, null);
    }

    [Fact]
    public void Border_FromHTML_ShouldOnlyApplyToDefinedElement()
    {
        // Arrange - Test border shorthand in HTML doesn't affect child elements
        const string html = """
                            <div style="border: 2px solid red;">
                                <p>No Border from Parent</p>
                            </div>
                            """;

        // Act
        var document = _parser.Parse(html);

        // Assert - Only the div should have the border
        var container = document.Children.Single();
        container.Styles.Border.Width.ShouldBe(2.0);
        container.Styles.Border.Style.ShouldBe(CssBorderValues.Solid);
        container.Styles.Border.Color.ShouldBe(HexColors.Red);
        container.Styles.Border.IsVisible.ShouldBeTrue();

        // Child paragraph should have no border
        var paragraph = container.Children.Single();

        // Debug: Let's see what border the paragraph actually has
        _testOutputHelper.WriteLine(
            $"Paragraph border - Width: {paragraph.Styles.Border.Width}, Style: {paragraph.Styles.Border.Style}, Color: {paragraph.Styles.Border.Color}");
        _testOutputHelper.WriteLine(
            $"Paragraph border HasValue: {paragraph.Styles.Border.HasValue}, IsVisible: {paragraph.Styles.Border.IsVisible}");

        paragraph.Styles.Border.ShouldBe(BorderInfo.Empty);
        paragraph.Styles.Border.HasValue.ShouldBeFalse();
        paragraph.Styles.Border.IsVisible.ShouldBeFalse();
    }

    [Fact]
    public void Border_ChildWithOwnBorder_ShouldNotInheritParent()
    {
        // Arrange - Test that child element with its own border doesn't inherit parent border
        const string html = """
                            <div style="border: 3px dashed blue;">
                                <p style="border: 1px dotted green;">Child Border</p>
                            </div>
                            """;

        // Act
        var document = _parser.Parse(html);

        // Assert - Each element should have its own border, no inheritance
        var container = document.Children.Single();
        var paragraph = container.Children.Single();

        // Parent div should have its border
        container.Styles.Border.Width.ShouldBe(3.0);
        container.Styles.Border.Style.ShouldBe(CssBorderValues.Dashed);
        container.Styles.Border.Color.ShouldBe(HexColors.Blue);
        container.Styles.Border.IsVisible.ShouldBeTrue();

        // Child paragraph should have its own border (not parent's border)
        paragraph.Styles.Border.Width.ShouldBe(1.0);
        paragraph.Styles.Border.Style.ShouldBe(CssBorderValues.Dotted);
        paragraph.Styles.Border.Color.ShouldBe(HexColors.Green);
        paragraph.Styles.Border.IsVisible.ShouldBeTrue();
    }

    [Fact]
    public void Border_MultipleNestedElements_EachHasOwnBorder()
    {
        // Arrange - Test multiple nested elements with different borders
        const string html = """
                            <div style="border: 4px solid black;">
                                <section style="border: 2px dashed orange;">
                                    <p style="border: 1px solid purple;">Nested Border</p>
                                </section>
                            </div>
                            """;

        // Act
        var document = _parser.Parse(html);

        // Assert - Each element should have its own border
        var outerDiv = document.Children.Single();
        var section = outerDiv.Children.Single();
        var paragraph = section.Children.Single();

        // Outer div
        outerDiv.Styles.Border.Width.ShouldBe(4.0);
        outerDiv.Styles.Border.Style.ShouldBe(CssBorderValues.Solid);
        outerDiv.Styles.Border.Color.ShouldBe(HexColors.Black);

        // Section
        section.Styles.Border.Width.ShouldBe(2.0);
        section.Styles.Border.Style.ShouldBe(CssBorderValues.Dashed);
        section.Styles.Border.Color.ShouldBe(HexColors.Orange);

        // Paragraph
        paragraph.Styles.Border.Width.ShouldBe(1.0);
        paragraph.Styles.Border.Style.ShouldBe(CssBorderValues.Solid);
        paragraph.Styles.Border.Color.ShouldBe(HexColors.Purple);
    }
}