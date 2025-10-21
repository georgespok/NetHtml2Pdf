using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Layout.Display;
using NetHtml2Pdf.Layout.Engines;
using NetHtml2Pdf.Layout.FormattingContexts;
using NetHtml2Pdf.Layout.Model;
using NetHtml2Pdf.Layout.Pagination;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Test.Renderer;
using NetHtml2Pdf.Test.Support;
using Shouldly;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Layout.FormattingContexts;

[Collection("PdfRendering")]
public class TableFormattingContextTests(ITestOutputHelper output) : PdfRenderTestBase(output)
{
    [Fact]
    public void Layout_ShouldRepeatHeadersAcrossPages()
    {
        var rendererOptions = CreateRendererOptions();
        rendererOptions.EnableLayoutDiagnostics = true;

        var (engine, layoutOptions) = CreateEngine(rendererOptions);
        var constraints = new LayoutConstraints(0, 400, 0, 200, 200, true);

        var table = BuildTableWithRows(30);

        var layoutResult = engine.Layout(table, constraints, layoutOptions);

        layoutResult.IsSuccess.ShouldBeTrue("TableFormattingContext should handle table when flag enabled.");

        var paginationOptions = PaginationOptions.FromRendererOptions(rendererOptions);
        var pageConstraints = new PageConstraints(400, 200, BoxSpacing.Empty, 0, 0);
        var paginated = new PaginationService().Paginate(layoutResult.Fragments, pageConstraints, paginationOptions);

        paginated.Pages.Count.ShouldBeGreaterThan(1, "Table should span multiple pages in test constraints.");

        foreach (var page in paginated.Pages.Skip(1))
            page.Fragments.Any(slice => slice.SourceFragment.NodePath.Contains("TableHead")).ShouldBeTrue(
                "Subsequent pages should repeat table header fragments.");
    }

    [Fact]
    public void Columns_ShouldMaintainStableWidths()
    {
        var rendererOptions = CreateRendererOptions();
        var (engine, layoutOptions) = CreateEngine(rendererOptions);
        var constraints = new LayoutConstraints(0, 500, 0, 300, 300, true);

        var table = BuildTableWithRows(5, 4);

        var layoutResult = engine.Layout(table, constraints, layoutOptions);

        layoutResult.IsSuccess.ShouldBeTrue();
        var tableFragment = layoutResult.Fragments[0];

        var firstRow = tableFragment.Children.FirstOrDefault();
        firstRow.ShouldNotBeNull();

        var expectedWidths = firstRow!.Children.Select(cell => cell.Width).ToArray();
        expectedWidths.Length.ShouldBeGreaterThan(0);

        foreach (var row in tableFragment.Children)
            row.Children.Select(cell => cell.Width)
                .ShouldBe(expectedWidths, "Each column should preserve its width across rows.");
    }

    [Fact]
    public void BorderCollapse_ShouldEmitDowngradeWarning_WhenFlagDisabled()
    {
        var logger = new TestLogger<LayoutEngine>();
        var rendererOptions = CreateRendererOptions();
        rendererOptions.EnableTableBorderCollapse = false;
        rendererOptions.EnableLayoutDiagnostics = true;

        var (engine, layoutOptions) = CreateEngine(rendererOptions, logger);
        var constraints = new LayoutConstraints(0, 400, 0, 200, 200, true);

        var table = BuildBorderCollapseTable();

        var result = engine.Layout(table, constraints, layoutOptions);

        result.IsSuccess.ShouldBeTrue();

        logger.LogEntries.Any(entry =>
            entry.Level == LogLevel.Warning &&
            entry.Message.Contains("TableContext.BorderDowngrade")).ShouldBeTrue(
            "Table formatting should emit downgrade warning when border-collapse support is disabled.");
    }

    [Fact]
    public void Caption_ShouldRemainAnchoredToTable()
    {
        var rendererOptions = CreateRendererOptions();
        rendererOptions.EnableLayoutDiagnostics = true;

        var (engine, layoutOptions) = CreateEngine(rendererOptions);
        var constraints = new LayoutConstraints(0, 500, 0, 300, 300, true);

        var tableWithCaption = BuildTableWithCaption();

        var result = engine.Layout(tableWithCaption, constraints, layoutOptions);

        result.IsSuccess.ShouldBeTrue();

        var tableFragment = result.Fragments.FirstOrDefault(f => f.Diagnostics.ContextName == "TableFormattingContext");
        tableFragment.ShouldNotBeNull();

        tableFragment!.Diagnostics.Metadata.ShouldNotBeNull();
        tableFragment.Diagnostics.Metadata.ContainsKey("caption").ShouldBeTrue(
            "Table formatting context should surface caption metadata for anchoring.");
    }

    private static (LayoutEngine engine, LayoutEngineOptions options) CreateEngine(RendererOptions options,
        TestLogger<LayoutEngine>? logger = null)
    {
        var layoutOptions = LayoutEngineOptions.FromRendererOptions(options);
        var factory = FormattingContextFactory.CreateDefault(layoutOptions);
        var classifier = new DisplayClassifier(options: options);

        var engine = new LayoutEngine(
            classifier,
            factory.GetBlockFormattingContext(),
            factory.GetInlineFormattingContext(),
            factory.GetInlineBlockFormattingContext(),
            factory.GetTableFormattingContext(),
            factory.GetFlexFormattingContext(),
            factory.Options,
            logger);

        return (engine, layoutOptions);
    }

    private static RendererOptions CreateRendererOptions()
    {
        return new RendererOptions
        {
            EnableNewLayoutForTextBlocks = true,
            EnableTableContext = true,
            EnablePagination = true,
            EnableQuestPdfAdapter = true
        };
    }

    private static DocumentNode BuildTableWithRows(int rowCount, int cellsPerRow = 3)
    {
        var head = TableHead(
            TableRow([.. Enumerable.Range(1, cellsPerRow).Select(i => TableHeaderCell(Text($"H{i}")))]));

        var bodyRows = new List<DocumentNode>();
        for (var row = 0; row < rowCount; row++)
        {
            var cells = Enumerable.Range(1, cellsPerRow)
                .Select(i => TableCell(Text($"Row{row}-Col{i}")))
                .ToArray();

            bodyRows.Add(TableRow(cells));
        }

        var body = TableBody([.. bodyRows]);
        return Table(head, body);
    }

    private static DocumentNode BuildBorderCollapseTable()
    {
        var tableStyles = CssStyleMap.Empty.WithBorderCollapse("collapse");

        var head = TableHead(TableRow(TableHeaderCell(Text("Col"))));
        var body = TableBody(TableRow(TableCell(Text("Value"))));

        return Table(tableStyles, head, body);
    }

    private static DocumentNode BuildTableWithCaption()
    {
        var caption = Paragraph(Text("Table caption"));
        var head = TableHead(TableRow(TableHeaderCell(Text("Header"))));
        var body = TableBody(TableRow(TableCell(Text("Cell"))));

        return Table(caption, head, body);
    }
}