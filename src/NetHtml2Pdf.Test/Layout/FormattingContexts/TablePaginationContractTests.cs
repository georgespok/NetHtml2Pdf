using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
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
public class TablePaginationContractTests(ITestOutputHelper output) : PdfRenderTestBase(output)
{
    [Fact]
    public void Pagination_ShouldRepeatHeadersAndFooters()
    {
        var rendererOptions = CreateRendererOptions();
        rendererOptions.EnableLayoutDiagnostics = true;

        var (engine, layoutOptions) = CreateEngine(rendererOptions);
        var constraints = new LayoutConstraints(0, 450, 0, 220, 220, true);

        var table = BuildTableWithHeaderFooter(20);

        var layoutResult = engine.Layout(table, constraints, layoutOptions);

        layoutResult.IsSuccess.ShouldBeTrue(
            "Table pagination requires dedicated formatting context when flag enabled.");

        var pageConstraints = new PageConstraints(450, 220, BoxSpacing.Empty, 0, 0);
        var paginationOptions = PaginationOptions.FromRendererOptions(rendererOptions);
        var paginated = new PaginationService().Paginate(layoutResult.Fragments, pageConstraints, paginationOptions);

        paginated.Pages.Count.ShouldBeGreaterThan(1,
            "Fixture should span multiple pages to exercise header/footer repetition.");

        foreach (var page in paginated.Pages)
            page.Fragments.Any(slice => slice.SourceFragment.NodePath.Contains("TableHead")).ShouldBeTrue(
                "Each page should include header fragments");

        paginated.Pages[
                paginated.Pages.Count - 1].Fragments.Any(slice => slice.SourceFragment.NodePath.Contains("TableFoot"))
            .ShouldBeTrue(
                "Final page should include footer fragments");
    }

    [Fact]
    public void Pagination_ShouldEmitCarryLinkMetadata_ForPartialRows()
    {
        var rendererOptions = CreateRendererOptions();
        rendererOptions.EnableLayoutDiagnostics = true;

        var (engine, layoutOptions) = CreateEngine(rendererOptions);
        var constraints = new LayoutConstraints(0, 400, 0, 180, 180, true);

        var table = BuildTableWithHeaderFooter(25);

        var layoutResult = engine.Layout(table, constraints, layoutOptions);

        layoutResult.IsSuccess.ShouldBeTrue();

        var pageConstraints = new PageConstraints(400, 180, BoxSpacing.Empty, 0, 0);
        var paginationOptions = PaginationOptions.FromRendererOptions(rendererOptions);
        var paginated = new PaginationService().Paginate(layoutResult.Fragments, pageConstraints, paginationOptions);

        paginated.Pages.Count.ShouldBeGreaterThan(1);

        paginated.Pages.Any(page => page.CarryLink is not null).ShouldBeTrue(
            "Carry-over metadata should be surfaced when rows span multiple pages.");
    }

    [Fact]
    public void Pagination_ShouldDowngradeBorderCollapse_WhenFeatureDisabled()
    {
        var logger = new TestLogger<LayoutEngine>();
        var rendererOptions = CreateRendererOptions();
        rendererOptions.EnableTableBorderCollapse = false;
        rendererOptions.EnableLayoutDiagnostics = true;

        var (engine, layoutOptions) = CreateEngine(rendererOptions, logger);
        var constraints = new LayoutConstraints(0, 450, 0, 220, 220, true);

        var table = BuildTableWithBorderCollapse();

        var result = engine.Layout(table, constraints, layoutOptions);

        result.IsSuccess.ShouldBeTrue();

        logger.LogEntries.Any(entry =>
            entry.Level == LogLevel.Warning &&
            entry.Message.Contains("TableContext.BorderDowngrade")).ShouldBeTrue(
            "Pagination should log downgrade warnings when collapse support is disabled.");
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

    private static DocumentNode BuildTableWithHeaderFooter(int rowCount)
    {
        var table = new DocumentNode(DocumentNodeType.Table);
        var caption = new DocumentNode(DocumentNodeType.Section);
        caption.AddChild(new DocumentNode(DocumentNodeType.Text, "Quarterly Revenue"));
        table.AddChild(caption);

        var head = new DocumentNode(DocumentNodeType.TableHead);
        head.AddChild(BuildRow(DocumentNodeType.TableHeaderCell, "Product", "Q1", "Q2"));
        table.AddChild(head);

        var body = new DocumentNode(DocumentNodeType.TableBody);
        for (var i = 0; i < rowCount; i++)
            body.AddChild(BuildRow(DocumentNodeType.TableCell,
                $"Product {i + 1}",
                $"{i * 10}",
                $"{i * 12}"));
        table.AddChild(body);

        var foot = new DocumentNode(DocumentNodeType.TableSection);
        foot.AddChild(BuildRow(DocumentNodeType.TableCell, "Totals", "100", "110"));
        table.AddChild(foot);

        return table;
    }

    private static DocumentNode BuildTableWithBorderCollapse()
    {
        var table = new DocumentNode(DocumentNodeType.Table, styles: CssStyleMap.Empty.WithBorderCollapse("collapse"));
        var head = new DocumentNode(DocumentNodeType.TableHead);
        head.AddChild(BuildRow(DocumentNodeType.TableHeaderCell, "Header"));
        table.AddChild(head);

        var body = new DocumentNode(DocumentNodeType.TableBody);
        body.AddChild(BuildRow(DocumentNodeType.TableCell, "Value"));
        table.AddChild(body);

        return table;
    }

    private static DocumentNode BuildRow(DocumentNodeType cellType, params string[] values)
    {
        var row = new DocumentNode(DocumentNodeType.TableRow);
        foreach (var value in values)
            if (cellType == DocumentNodeType.TableHeaderCell)
            {
                var cell = new DocumentNode(DocumentNodeType.TableHeaderCell);
                cell.AddChild(new DocumentNode(DocumentNodeType.Text, value));
                row.AddChild(cell);
            }
            else
            {
                var cell = new DocumentNode(DocumentNodeType.TableCell);
                cell.AddChild(new DocumentNode(DocumentNodeType.Text, value));
                row.AddChild(cell);
            }

        return row;
    }
}