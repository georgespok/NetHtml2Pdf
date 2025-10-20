using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Model;
using NetHtml2Pdf.Layout.Pagination;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Test.Renderer;
using Xunit;

namespace NetHtml2Pdf.Test.Layout.Pagination;

public class PaginationServiceDiagnosticsTests
{
    [Fact]
    public void Paginate_WithDiagnosticsEnabled_EmitsStructuredLogs()
    {
        var logger = new TestLogger<PaginationService>();
        var service = new PaginationService();
        var options = PaginationOptions.FromRendererOptions(new RendererOptions
        {
            EnablePaginationDiagnostics = true
        });

        var constraints = CreatePageConstraints();
        var fragments = new[] { CreateTallFragment("Fragment:0") };

        service.Paginate(fragments, constraints, options, logger);

        Assert.Contains(logger.LogEntries, entry => entry.Message.Contains("Pagination.PageCreated"));
        Assert.Contains(logger.LogEntries, entry => entry.Message.Contains("Pagination.FragmentSplit"));
    }

    [Fact]
    public void Paginate_WithDiagnosticsDisabled_DoesNotEmitStructuredLogs()
    {
        var logger = new TestLogger<PaginationService>();
        var service = new PaginationService();
        var options = PaginationOptions.FromRendererOptions(new RendererOptions
        {
            EnablePaginationDiagnostics = false
        });

        var constraints = CreatePageConstraints();
        var fragments = new[] { CreateTallFragment("Fragment:0") };

        service.Paginate(fragments, constraints, options, logger);

        Assert.DoesNotContain(logger.LogEntries, entry => entry.Message.Contains("Pagination.PageCreated"));
        Assert.DoesNotContain(logger.LogEntries, entry => entry.Message.Contains("Pagination.FragmentSplit"));
    }

    private static PageConstraints CreatePageConstraints()
    {
        return new PageConstraints(
            pageWidth: 595f,
            pageHeight: 842f,
            margin: BoxSpacing.FromAll(20),
            headerBand: 40f,
            footerBand: 40f);
    }

    private static LayoutFragment CreateTallFragment(string nodePath)
    {
        var node = new DocumentNode(DocumentNodeType.Paragraph);
        var box = new LayoutBox(
            node,
            DisplayClass.Block,
            CssStyleMap.Empty,
            new LayoutSpacing(BoxSpacing.Empty, BoxSpacing.Empty, BorderInfo.Empty),
            nodePath,
            []);

        const float width = 400f;
        const float height = 900f;

        var constraints = new LayoutConstraints(
            inlineMin: width,
            inlineMax: width,
            blockMin: height,
            blockMax: height,
            pageRemainingBlockSize: height,
            allowBreaks: true);

        var diagnostics = new LayoutDiagnostics("PaginationServiceDiagnostics", constraints, width, height);

        return LayoutFragment.CreateBlock(box, width, height, [], diagnostics);
    }
}
