using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Model;
using NetHtml2Pdf.Layout.Pagination;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Renderer.Adapters;
using QuestPDF;
using QuestPDF.Infrastructure;

namespace NetHtml2Pdf.Test.Renderer.Adapters;

public class QuestPdfAdapterTests
{
    [Fact]
    public void Render_ShouldProduceQuestPdfDocumentForSinglePage()
    {
        // Arrange
        Settings.License = LicenseType.Community;

        var adapter = new QuestPdfAdapter();
        var rendererOptions = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true
        };

        var header = CreateBlockFragment("Header:0", 400, 40);
        var footer = CreateBlockFragment("Footer:0", 400, 40);
        var fragment = CreateBlockFragment("Content:0", 400, 600);

        var slice = new FragmentSlice(
            fragment,
            new PageBounds(0, 0, 400, 600),
            FragmentSliceKind.Full,
            false,
            []);

        var page = new PageFragmentTree(
            1,
            PageBounds.FromSize(400, 600),
            [slice],
            null);

        var document = new PaginatedDocument(
            new PageConstraints(595f, 842f, BoxSpacing.Empty, 0f, 0f),
            [page]);

        var context = new RendererContext(rendererOptions, NullLogger.Instance, header, footer);

        // Act
        adapter.BeginDocument(document, context);
        adapter.Render(page, context);
        var pdfBytes = adapter.EndDocument(context);

        // Assert
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
    }

    [Fact]
    public void Render_WithDiagnosticsEnabled_EmitsStructuredLogs()
    {
        Settings.License = LicenseType.Community;

        var adapter = new QuestPdfAdapter();
        var logger = new TestLogger<QuestPdfAdapter>();
        var rendererOptions = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnablePaginationDiagnostics = true
        };

        var context = CreateContext(rendererOptions, logger, out var document, out var page);

        adapter.BeginDocument(document, context);
        adapter.Render(page, context);
        adapter.EndDocument(context);

        Assert.Contains(logger.LogEntries, entry => entry.Message.Contains("QuestPdfAdapter rendered fragment"));
    }

    [Fact]
    public void Render_WithDiagnosticsDisabled_SuppressesStructuredLogs()
    {
        Settings.License = LicenseType.Community;

        var adapter = new QuestPdfAdapter();
        var logger = new TestLogger<QuestPdfAdapter>();
        var rendererOptions = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnablePaginationDiagnostics = false
        };

        var context = CreateContext(rendererOptions, logger, out var document, out var page);

        adapter.BeginDocument(document, context);
        adapter.Render(page, context);
        adapter.EndDocument(context);

        Assert.DoesNotContain(logger.LogEntries, entry => entry.Message.Contains("QuestPdfAdapter rendered fragment"));
    }

    private static RendererContext CreateContext(RendererOptions options, ILogger logger,
        out PaginatedDocument document, out PageFragmentTree page)
    {
        var header = CreateBlockFragment("Header:0", 400, 40);
        var footer = CreateBlockFragment("Footer:0", 400, 40);
        var fragment = CreateBlockFragment("Content:0", 400, 600);

        var slice = new FragmentSlice(
            fragment,
            new PageBounds(0, 0, 400, 600),
            FragmentSliceKind.Full,
            false,
            []);

        page = new PageFragmentTree(
            1,
            PageBounds.FromSize(400, 600),
            [slice],
            null);

        document = new PaginatedDocument(
            new PageConstraints(595f, 842f, BoxSpacing.Empty, 0f, 0f),
            [page]);

        return new RendererContext(options, logger, header, footer);
    }

    private static LayoutFragment CreateBlockFragment(string nodePath, float width, float height)
    {
        var node = new DocumentNode(DocumentNodeType.Paragraph);
        var box = new LayoutBox(
            node,
            DisplayClass.Block,
            CssStyleMap.Empty,
            new LayoutSpacing(BoxSpacing.Empty, BoxSpacing.Empty, BorderInfo.Empty),
            nodePath,
            []);

        var constraints = new LayoutConstraints(
            width,
            width,
            height,
            height,
            height,
            false);

        var diagnostics = new LayoutDiagnostics("QuestPdfAdapter", constraints, width, height);

        return LayoutFragment.CreateBlock(box, width, height, [], diagnostics);
    }
}