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
public class InlineBlockFormattingContextTests(ITestOutputHelper output) : PdfRenderTestBase(output)
{
    [Fact]
    public void Layout_WithInlineBlockSpan_ShouldReturnInlineBlockFragment()
    {
        var (engine, layoutOptions) = CreateEngine();
        var constraints = CreateInlineConstraints();

        var inlineBlock = Span(CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock),
            Text("badge"));
        var root = Paragraph(inlineBlock);

        var result = engine.Layout(root, constraints, layoutOptions);

        result.IsSuccess.ShouldBeTrue("Inline-block nodes should be handled by a dedicated formatting context.");
        result.Fragments.ShouldNotBeEmpty();
        result.Fragments[0].Diagnostics.ContextName.ShouldBe("InlineBlockFormattingContext");
        result.Fragments[0].Display.ShouldBe(DisplayClass.InlineBlock);
    }

    [Fact]
    public void Layout_WithInlineBlockSpan_ShouldExposeBaselineForAlignment()
    {
        var (engine, layoutOptions) = CreateEngine();
        var constraints = CreateInlineConstraints();

        var inlineBlock = Span(CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock),
            Text("Aligned"));
        var root = Paragraph(inlineBlock);

        var result = engine.Layout(root, constraints, layoutOptions);

        result.IsSuccess.ShouldBeTrue();
        var fragment = result.Fragments[0];
        fragment.Baseline.ShouldNotBeNull(
            "Inline-block fragments must expose a baseline so inline layout can align them.");
        fragment.Baseline!.Value.ShouldBeGreaterThan(0f);
    }

    [Fact]
    public void Pagination_WithOverflowingInlineBlock_ShouldNotDuplicateFragments()
    {
        var (engine, layoutOptions) = CreateEngine();
        var constraints = new LayoutConstraints(0, 120, 0, 80, 80, true);

        var inlineBlock = Span(CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock),
            Text("Inline block content that needs to wrap or paginate without duplicating fragments."));
        var root = Paragraph(inlineBlock);

        var layoutResult = engine.Layout(root, constraints, layoutOptions);

        layoutResult.IsSuccess.ShouldBeTrue();

        var paginationOptions = PaginationOptions.FromRendererOptions(CreateRendererOptions());
        var pageConstraints = new PageConstraints(120, 80, BoxSpacing.Empty, 0, 0);
        var document = new PaginationService().Paginate(layoutResult.Fragments, pageConstraints, paginationOptions);

        var fragmentIds = document.Pages
            .SelectMany(page => page.Fragments)
            .Select(slice => slice.SourceFragment.NodePath)
            .ToList();

        fragmentIds.ShouldBe([.. fragmentIds.Distinct()],
            "Inline-block pagination must not duplicate fragment references when wrapping across pages.");
    }

    [Fact]
    public void Diagnostics_ShouldEmitInlineBlockEvent_WhenEnabled()
    {
        var logger = new TestLogger<LayoutEngine>();
        var (engine, layoutOptions) = CreateEngine(logger: logger);
        var constraints = CreateInlineConstraints();

        var inlineBlock = Span(CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock),
            Text("Badge"));
        var root = Paragraph(inlineBlock);

        engine.Layout(root, constraints, layoutOptions);

        logger.LogEntries
            .Any(entry =>
                entry.Level == LogLevel.Information && entry.Message.Contains("FormattingContext.InlineBlock"))
            .ShouldBeTrue();
    }

    [Fact]
    public void Diagnostics_ShouldNotEmitInlineBlockEvent_WhenDisabled()
    {
        var logger = new TestLogger<LayoutEngine>();
        var rendererOptions = CreateRendererOptions();
        rendererOptions.EnableLayoutDiagnostics = false;

        var (engine, layoutOptions) = CreateEngine(rendererOptions, logger);
        var constraints = CreateInlineConstraints();

        var inlineBlock = Span(CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock),
            Text("Badge"));
        var root = Paragraph(inlineBlock);

        engine.Layout(root, constraints, layoutOptions);

        logger.LogEntries
            .Any(entry => entry.Message.Contains("FormattingContext.InlineBlock"))
            .ShouldBeFalse();
    }

    [Fact]
    public void InlineBlockFallback_ShouldLogWarning_WhenContextDisabled()
    {
        var logger = new TestLogger<LayoutEngine>();
        var rendererOptions = CreateRendererOptions();
        rendererOptions.EnableInlineBlockContext = false;

        var (engine, layoutOptions) = CreateEngine(rendererOptions, logger);
        var constraints = CreateInlineConstraints();

        var inlineBlock = Span(CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock),
            Text("Badge"));
        var root = Paragraph(inlineBlock);

        var result = engine.Layout(root, constraints, layoutOptions);

        result.IsFallback.ShouldBeTrue();
        logger.LogEntries
            .Any(entry => entry.Message.Contains("FormattingContext.InlineBlock.Fallback"))
            .ShouldBeTrue();
    }

    private static (LayoutEngine engine, LayoutEngineOptions options) CreateEngine(
        RendererOptions? optionsOverride = null, TestLogger<LayoutEngine>? logger = null)
    {
        var rendererOptions = optionsOverride ?? CreateRendererOptions();
        var layoutOptions = LayoutEngineOptions.FromRendererOptions(rendererOptions);
        var factory = FormattingContextFactory.CreateDefault(layoutOptions);
        var classifier = new DisplayClassifier(options: rendererOptions);

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
            EnableInlineBlockContext = true,
            EnablePaginationDiagnostics = true,
            EnableLayoutDiagnostics = true,
            EnablePagination = true,
            EnableQuestPdfAdapter = true
        };
    }

    private static LayoutConstraints CreateInlineConstraints()
    {
        return new LayoutConstraints(
            0,
            400,
            0,
            200,
            200,
            true);
    }
}