using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Constants;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Test.Support;
using Shouldly;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Renderer;

[Collection("PdfRendering")]
public class PdfRendererTests : PdfRenderTestBase
{
    public PdfRendererTests(ITestOutputHelper output) : base(output)
    {
    }

    private readonly PdfRenderer _renderer = new();

    [Fact]
    public void Paragraphs_RenderExpectedTextOrdering()
    {
        var document = Document(
            Div(
                Paragraph(
                    Text("Welcome to "),
                    Strong(Text("NetHtml2Pdf."))),
                Paragraph(
                    Italic(Text("Iteration 1")),
                    Text(" handles "),
                    LineBreak(),
                    Text("line breaks."))
            )
        );

        var pdfBytes = _renderer.Render(document);
        var words = ExtractWords(pdfBytes);

        words.ShouldContain("Welcome");
        words.ShouldContain("NetHtml2Pdf.");
        words.ShouldContain(word => word.StartsWith("Iter", StringComparison.OrdinalIgnoreCase));
        words.ShouldContain("1");
        words.ShouldContain("handles");
        words.ShouldContain("line");
        words.ShouldContain("breaks.");
    }

    [Fact]
    public void Lists_RenderBulletsAndNumbers()
    {
        var document = Document(
            Section(
                UnorderedList(
                    ListItem(Text("First")),
                    ListItem(Text("Second"))),
                OrderedList(
                    ListItem(Text("One"))))
        );

        var pdfBytes = _renderer.Render(document);
        var words = ExtractWords(pdfBytes);

        words.Any(w => w.Contains("First", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("Second", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.ShouldContain("One");
        words.ShouldContain("1.");
    }

    [Theory]
    [InlineData("h1", 32.0)]
    [InlineData("h2", 24.0)]
    [InlineData("h3", 19.0)]
    [InlineData("h4", 16.0)]
    [InlineData("h5", 13.0)]
    [InlineData("h6", 11.0)]
    public void Headings_ShouldRenderWithProperSizing(string tag, double expectedFontSize)
    {
        var headingType = tag.ToLowerInvariant() switch
        {
            "h1" => DocumentNodeType.Heading1,
            "h2" => DocumentNodeType.Heading2,
            "h3" => DocumentNodeType.Heading3,
            "h4" => DocumentNodeType.Heading4,
            "h5" => DocumentNodeType.Heading5,
            _ => DocumentNodeType.Heading6
        };

        var document = Document(
            Heading(headingType, Text("Heading Text"))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);

        var words = GetPdfWords(pdfBytes);
        var headingWord = words.FirstOrDefault(w => w.Text.Contains("Heading", StringComparison.OrdinalIgnoreCase));

        headingWord.ShouldNotBeNull();
        headingWord.IsBold.ShouldBeTrue();
        headingWord.FontSize.ShouldBe(expectedFontSize, tolerance: 0.5);
    }

    [Fact]
    public void ColorStyles_ShouldRenderWithColors()
    {
        var styles = CssStyleMap.Empty
            .WithColor(HexColors.Red)
            .WithBackgroundColor(HexColors.Yellow);

        var document = Document(
            Paragraph(Text("Colored text", styles))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);

        var words = GetPdfWords(pdfBytes);
        var coloredWord = words.FirstOrDefault(w => w.Text.Contains("Colored", StringComparison.OrdinalIgnoreCase));
        coloredWord.ShouldNotBeNull();
        coloredWord.HexColor.ShouldBe(HexColors.Red);
    }

    [Theory]
    [InlineData("blue", HexColors.Blue)]
    [InlineData(HexColors.BrightGreen, HexColors.BrightGreen)]
    [InlineData("purple", HexColors.Purple)]
    public void ColorStyles_ShouldRenderVariousColors(string cssColor, string expectedHex)
    {
        var normalizedColor = RenderingHelpers.ConvertToHexColor(cssColor) ?? cssColor;
        var styles = CssStyleMap.Empty.WithColor(normalizedColor);
        var document = Document(
            Paragraph(Text("Test", styles))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);

        var words = GetPdfWords(pdfBytes);
        var testWord = words.FirstOrDefault(w => w.Text.Contains("Test", StringComparison.OrdinalIgnoreCase));
        testWord.ShouldNotBeNull();
        testWord.HexColor.ShouldBe(expectedHex);
    }

    [Theory]
    [InlineData("yellow")]
    [InlineData(HexColors.BrightGreen)]
    [InlineData("pink")]
    public void BackgroundColorStyles_ShouldRenderWithoutErrors(string backgroundColor)
    {
        var normalizedColor = RenderingHelpers.ConvertToHexColor(backgroundColor) ?? backgroundColor;
        var styles = CssStyleMap.Empty.WithBackgroundColor(normalizedColor);
        var document = Document(
            Paragraph(Text("Background Test", styles))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.ShouldContain("Background");
        words.ShouldContain("Test");
    }

    [Fact]
    public void CombinedColorStyles_ShouldRenderBothTextAndBackground()
    {
        var document = Document(
            Div(
                Paragraph(Text("Blue on Yellow",
                    CssStyleMap.Empty.WithColor(HexColors.Blue).WithBackgroundColor(HexColors.Yellow))),
                Paragraph(Text("White on Red",
                    CssStyleMap.Empty.WithColor(HexColors.White).WithBackgroundColor(HexColors.Red))),
                Paragraph(Text("Green on Navy",
                    CssStyleMap.Empty.WithColor(HexColors.BrightGreen).WithBackgroundColor(HexColors.Navy))))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);

        var words = GetPdfWords(pdfBytes);

        words.FirstOrDefault(w => w.Text.Contains("Blue", StringComparison.OrdinalIgnoreCase))?.HexColor.ShouldBe(HexColors.Blue);
        words.FirstOrDefault(w => w.Text.Contains("White", StringComparison.OrdinalIgnoreCase))?.HexColor.ShouldBe(HexColors.White);
        words.FirstOrDefault(w => w.Text.Contains("Green", StringComparison.OrdinalIgnoreCase))?.HexColor.ShouldBe(HexColors.BrightGreen);
    }

    [Fact]
    public void UnorderedList_RendersWithBulletMarkers()
    {
        var document = Document(
            UnorderedList(
                ListItem(Text("First")),
                ListItem(Text("Second")),
                ListItem(Text("Third")))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("First", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("Second", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("Third", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
    }

    [Fact]
    public void OrderedList_RendersWithNumericMarkers()
    {
        var document = Document(
            OrderedList(
                ListItem(Text("First")),
                ListItem(Text("Second")),
                ListItem(Text("Third")))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("First", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("Second", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("Third", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.ShouldContain("1.");
        words.ShouldContain("2.");
        words.ShouldContain("3.");
    }

    [Fact]
    public void NestedLists_RenderHierarchically()
    {
        var document = Document(
            UnorderedList(
                ListItem(
                    Text("Parent 1"),
                    UnorderedList(
                        ListItem(Text("Child 1.1")),
                        ListItem(Text("Child 1.2")))),
                ListItem(Text("Parent 2")))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Parent", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("Child", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
    }

    [Fact]
    public void MixedLists_RenderBothBulletsAndNumbers()
    {
        var document = Document(
            UnorderedList(ListItem(Text("Bullet item"))),
            OrderedList(ListItem(Text("Numbered item")))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Bullet", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("Numbered", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.ShouldContain("1.");
    }

    [Fact]
    public void ListWithStyledItems_AppliesFormattingToText()
    {
        var document = Document(
            UnorderedList(
                ListItem(Strong(Text("Bold item"))),
                ListItem(Italic(Text("Italic item"))))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);

        var pdfWords = GetPdfWords(pdfBytes);

        var boldWord = pdfWords.FirstOrDefault(w => w.Text.Contains("Bold", StringComparison.OrdinalIgnoreCase));
        boldWord.ShouldNotBeNull();
        boldWord.IsBold.ShouldBeTrue();

        var italicWord = pdfWords.FirstOrDefault(w => w.Text.Contains("Italic", StringComparison.OrdinalIgnoreCase));
        italicWord.ShouldNotBeNull();
        italicWord.IsItalic.ShouldBeTrue();
    }

    [Theory]
    [InlineData("div")]
    [InlineData("section")]
    public void StructuralContainers_ShouldRenderChildParagraphs(string tag)
    {
        DocumentNode container = tag.Equals("div", StringComparison.OrdinalIgnoreCase)
            ? Div(Paragraph(Text("First")), Paragraph(Text("Second")))
            : Section(Paragraph(Text("First")), Paragraph(Text("Second")));

        var document = Document(container);

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("First", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("Second", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
    }

    [Fact]
    public void NestedStructuralContainers_ShouldRenderHierarchy()
    {
        var document = Document(
            Section(
                Div(
                    Paragraph(Text("Nested content"))
                )
            )
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Nested", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("content", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
    }

    [Fact]
    public void MultipleContainersAtSameLevel_ShouldRenderAllChildren()
    {
        var document = Document(
            Div(Text("Alpha")),
            Section(Text("Beta")),
            Div(Text("Gamma"))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Alpha", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("Beta", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("Gamma", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
    }

    [Fact]
    public async Task Table_ShouldRenderBasicStructure()
    {
        var document = Document(
            Table(
                TableHead(
                    TableRow(
                        TableHeaderCell(Text("Header 1")),
                        TableHeaderCell(Text("Header 2")))),
                TableBody(
                    TableRow(
                        TableCell(Text("Data 1")),
                        TableCell(Text("Data 2")))))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Header", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("Data", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
    }

    [Fact]
    public void Table_WithMultipleRows_ShouldRenderAllContent()
    {
        var document = Document(
            Table(
                TableBody(
                    TableRow(TableCell(Text("Row 1 Col 1")), TableCell(Text("Row 1 Col 2"))),
                    TableRow(TableCell(Text("Row 2 Col 1")), TableCell(Text("Row 2 Col 2"))),
                    TableRow(TableCell(Text("Row 3 Col 1")), TableCell(Text("Row 3 Col 2")))))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Row", StringComparison.OrdinalIgnoreCase) || w.Contains("Col", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("1", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("2", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("3", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
    }

    [Fact]
    public async Task Table_WithHeaderAndDataCells_ShouldRenderDistinctly()
    {
        var document = Document(
            Table(
                TableHead(
                    TableRow(
                        TableHeaderCell(Text("Name")),
                        TableHeaderCell(Text("Age")),
                        TableHeaderCell(Text("City")))),
                TableBody(
                    TableRow(
                        TableCell(Text("John Doe")),
                        TableCell(Text("25")),
                        TableCell(Text("New York"))),
                    TableRow(
                        TableCell(Text("Jane Smith")),
                        TableCell(Text("30")),
                        TableCell(Text("Los Angeles")))))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Name", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("Age", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("City", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("John", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("Jane", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("25", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("30", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
    }

    [Fact]
    public void Table_WithEmptyCells_ShouldRenderWithoutErrors()
    {
        var document = Document(
            Table(
                TableBody(
                    TableRow(
                        TableCell(Text("Filled")),
                        TableCell(),
                        TableCell(Text("Also Filled")))))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Filled", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("Also", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
    }

    [Fact]
    public void Table_WithNestedInlineElements_ShouldRenderFormatting()
    {
        var document = Document(
            Table(
                TableBody(
                    TableRow(
                        TableCell(Strong(Text("Bold Text"))),
                        TableCell(Italic(Text("Italic Text"))),
                        TableCell(Span(CssStyleMap.Empty, Text("Span Text"))))))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Bold", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("Italic", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("Span", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
    }

    [Fact]
    public async Task Table_WithBorders_ShouldRenderWithBorderStyling()
    {
        var borderedCell = CssStyleMap.Empty.WithBorder(new BorderInfo(2, CssBorderValues.Solid, HexColors.Black));
        var document = Document(
            Table(
                TableBody(
                    TableRow(
                        TableCell(borderedCell, Text("Cell 1")),
                        TableCell(borderedCell, Text("Cell 2")))))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Cell", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
    }

    [Fact]
    public async Task Table_WithTextAlignment_ShouldRenderAlignedContent()
    {
        var document = Document(
            Table(
                TableBody(
                    TableRow(
                        TableCell(CssStyleMap.Empty.WithTextAlign(CssAlignmentValues.Left), Text("Left Text")),
                        TableCell(CssStyleMap.Empty.WithTextAlign(CssAlignmentValues.Center), Text("Center Text")),
                        TableCell(CssStyleMap.Empty.WithTextAlign(CssAlignmentValues.Right), Text("Right Text")))))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Text", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Count().ShouldBeGreaterThan(2);
    }

    [Fact]
    public async Task Table_WithBackgroundColors_ShouldRenderColoredCells()
    {
        var document = Document(
            Table(
                TableHead(
                    TableRow(
                        TableHeaderCell(CssStyleMap.Empty.WithBackgroundColor(HexColors.LightGray), Text("Header 1")),
                        TableHeaderCell(CssStyleMap.Empty.WithBackgroundColor(HexColors.LightGray), Text("Header 2")))),
                TableBody(
                    TableRow(
                        TableCell(Text("Normal Cell")),
                        TableCell(CssStyleMap.Empty.WithBackgroundColor(RenderingHelpers.ConvertToHexColor("yellow") ?? "yellow"), Text("Highlighted Cell")))))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Header", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("Normal", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("Highlighted", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
    }

    [Fact]
    public async Task Table_WithBorderCollapse_ShouldRenderCorrectly()
    {
        var cellStyle = CssStyleMap.Empty
            .WithPadding(BoxSpacing.FromAll(5))
            .WithBorder(new BorderInfo(1, CssBorderValues.Solid, HexColors.Black));

        var document = Document(
            Table(
                CssStyleMap.Empty.WithBorderCollapse(CssTableValues.Collapse),
                TableBody(
                    TableRow(
                        TableCell(cellStyle, Text("A1")),
                        TableCell(cellStyle, Text("A2"))),
                    TableRow(
                        TableCell(cellStyle, Text("B1")),
                        TableCell(cellStyle, Text("B2")))))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("A1", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("B2", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
    }

    [Fact]
    public async Task Table_WithVerticalAlignment_ShouldRenderCorrectly()
    {
        var document = Document(
            Table(
                TableBody(
                    TableRow(
                        TableCell(CssStyleMap.Empty.WithVerticalAlign(CssAlignmentValues.Top), Text("Top Content")),
                        TableCell(CssStyleMap.Empty.WithVerticalAlign(CssAlignmentValues.Middle), Text("Middle Content")),
                        TableCell(CssStyleMap.Empty.WithVerticalAlign(CssAlignmentValues.Bottom), Text("Bottom Content")))))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Content", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Count().ShouldBeGreaterThan(2);
    }

    [Fact]
    public async Task Table_WithCombinedStyling_ShouldRenderAllStyles()
    {
        var style = CssStyleMap.Empty
            .WithBorder(new BorderInfo(2, CssBorderValues.Solid, HexColors.Black))
            .WithBackgroundColor(HexColors.LightGray)
            .WithTextAlign(CssAlignmentValues.Center)
            .WithVerticalAlign(CssAlignmentValues.Middle)
            .WithPadding(BoxSpacing.FromAll(10));

        var document = Document(
            Table(
                TableBody(
                    TableRow(TableCell(style, Text("Fully Styled Cell")))))
        );

        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes);

        var words = ExtractWords(pdfBytes);
        words.Any(w => w.Contains("Fully", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
        words.Any(w => w.Contains("Styled", StringComparison.OrdinalIgnoreCase)).ShouldBeTrue();
    }

    [Fact]
    public async Task MarginShorthand_WithTwoParameters_ShouldRenderWithCorrectSpacing()
    {
        // Arrange - Test margin: 24px 0 (vertical: 24px, horizontal: 0)
        var styles = CssStyleMap.Empty.WithMargin(BoxSpacing.FromVerticalHorizontal(24, 0));
        var document = Document(
            Div(CssStyleMap.Empty, Text("Top")),
            Div(styles, Text("Test")),
            Div(CssStyleMap.Empty, Text("Bottom"))
        );

        // Act
        var pdfBytes = _renderer.Render(document);
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes, "margin-24px-0-test");

        // Assert - Verify margin gaps using helper classes
        var words = PdfWordParser.GetRawWords(pdfBytes);
        PdfWordParser.LogWordPositions(words, Output.WriteLine);

        var gaps = MarginGapCalculator.CalculateGaps(words, "Top", "Test", "Boom");
        var expectedGapPoints = MarginGapCalculator.ConvertPixelsToPoints(24);
        var validation = MarginGapCalculator.ValidateGaps(gaps, expectedGapPoints);
        
        MarginGapCalculator.LogGapAnalysis(gaps, validation, Output.WriteLine);

        // Verify gaps meet requirements
        gaps.GapAboveTest.ShouldBeGreaterThanOrEqualTo(validation.MinExpectedGap, 
            $"Gap above 'Test' should be at least {validation.MinExpectedGap} points, but was {gaps.GapAboveTest}");
        gaps.GapBelowTest.ShouldBeGreaterThanOrEqualTo(validation.MinExpectedGap,
            $"Gap below 'Test' should be at least {validation.MinExpectedGap} points, but was {gaps.GapBelowTest}");

        // Verify horizontal positioning
        var testWord = PdfWordParser.FindWordByText(words, "Test");
        testWord.ShouldNotBeNull("Test word should be found in the PDF");
        HorizontalPositionValidator.ValidateLeftEdgePositioning(testWord);
        HorizontalPositionValidator.LogHorizontalPositioning(testWord, Output.WriteLine);
    }
}









