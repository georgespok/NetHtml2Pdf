using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Test.Support;
using Shouldly;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Renderer.Regression;

[Collection("PdfRendering")]
public class FormattingContextRegressionTests(ITestOutputHelper output) : PdfRenderTestBase(output)
{
    private const double AllowedDelta = 0.02; // 2%

    [Fact]
    public void InlineBlock_OutputParity_FlagOffVsOn_ShouldBeWithinTwoPercent()
    {
        var doc = CreateInlineBlockDocument();

        var disabledOptions = CreateRendererOptions(false, false);
        var enabledOptions = CreateRendererOptions(true, false);

        var disabledRenderer = (PdfRenderer)RendererComposition.CreateRenderer(disabledOptions);
        var enabledRenderer = (PdfRenderer)RendererComposition.CreateRenderer(enabledOptions);

        var disabledPdf = disabledRenderer.Render(doc);
        var enabledPdf = enabledRenderer.Render(doc);

        var disabledWords = GetPdfWords(disabledPdf);
        var enabledWords = GetPdfWords(enabledPdf);

        var delta = ComputeDelta(disabledWords.Count, enabledWords.Count);
        delta.ShouldBeLessThanOrEqualTo(AllowedDelta);
    }

    [Fact]
    public void Table_OutputParity_FlagOffVsOn_ShouldBeWithinTwoPercent()
    {
        var doc = CreateSimpleTableDocument();

        var disabledOptions = CreateRendererOptions(false, false);
        var enabledOptions = CreateRendererOptions(false, true);

        var disabledRenderer = (PdfRenderer)RendererComposition.CreateRenderer(disabledOptions);
        var enabledRenderer = (PdfRenderer)RendererComposition.CreateRenderer(enabledOptions);

        var disabledPdf = disabledRenderer.Render(doc);
        var enabledPdf = enabledRenderer.Render(doc);

        var disabledWords = GetPdfWords(disabledPdf);
        var enabledWords = GetPdfWords(enabledPdf);

        var delta = ComputeDelta(disabledWords.Count, enabledWords.Count);
        delta.ShouldBeLessThanOrEqualTo(AllowedDelta);
    }

    private static double ComputeDelta(int a, int b)
    {
        var baseline = Math.Max(1, Math.Max(a, b));
        return Math.Abs(a - b) / (double)baseline;
    }

    private static RendererOptions CreateRendererOptions(bool enableInlineBlock, bool enableTable)
    {
        return new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableNewLayoutForTextBlocks = true,
            EnableInlineBlockContext = enableInlineBlock,
            EnableTableContext = enableTable,
            FontPath = string.Empty
        };
    }

    private static DocumentNode CreateInlineBlockDocument()
    {
        var inlineBlock = new DocumentNode(DocumentNodeType.Span,
            styles: CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock));
        inlineBlock.AddChild(new DocumentNode(DocumentNodeType.Text, "Badge"));

        var paragraph = new DocumentNode(DocumentNodeType.Paragraph);
        paragraph.AddChild(new DocumentNode(DocumentNodeType.Text, "Prefix "));
        paragraph.AddChild(inlineBlock);
        paragraph.AddChild(new DocumentNode(DocumentNodeType.Text, " Suffix"));

        var root = new DocumentNode(DocumentNodeType.Div);
        root.AddChild(paragraph);
        return root;
    }

    private static DocumentNode CreateSimpleTableDocument()
    {
        var table = new DocumentNode(DocumentNodeType.Table);
        var head = new DocumentNode(DocumentNodeType.TableHead);
        head.AddChild(Row(DocumentNodeType.TableHeaderCell, "Header"));
        table.AddChild(head);

        var body = new DocumentNode(DocumentNodeType.TableBody);
        body.AddChild(Row(DocumentNodeType.TableCell, "Value 1"));
        body.AddChild(Row(DocumentNodeType.TableCell, "Value 2"));
        table.AddChild(body);

        var root = new DocumentNode(DocumentNodeType.Div);
        root.AddChild(table);
        return root;
    }

    private static DocumentNode Row(DocumentNodeType cellType, params string[] cells)
    {
        var row = new DocumentNode(DocumentNodeType.TableRow);
        foreach (var text in cells)
        {
            var cell = new DocumentNode(cellType);
            cell.AddChild(new DocumentNode(DocumentNodeType.Text, text));
            row.AddChild(cell);
        }

        return row;
    }
}