using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Model;
using NetHtml2Pdf.Layout.Pagination;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Renderer.Adapters;
using NetHtml2Pdf.Test.Support;
using Shouldly;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Renderer;

[Collection("PdfRendering")]
public class PdfRendererAdapterTests(ITestOutputHelper output) : PdfRenderTestBase(output)
{
    [Fact]
    public void FlagEnabled_ShouldInvokePaginationAndAdapterPath()
    {
        var rendererOptions = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableNewLayoutForTextBlocks = true,
            FontPath = string.Empty
        };

        var adapter = new RecordingAdapter();

        var renderer = new PdfRenderer(rendererOptions, rendererAdapter: adapter);
        var document = CreateDocument();

        var result = renderer.Render(document);

        Assert.True(adapter.BeginCalled);
        Assert.Equal(1, adapter.RenderCallCount);
        Assert.True(adapter.EndCalled);
        Assert.Equal(adapter.GeneratedBytes, result);
    }

    [Fact]
    public void PaginationException_BubblesThroughRenderer()
    {
        var rendererOptions = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableNewLayoutForTextBlocks = true,
            FontPath = string.Empty
        };

        var renderer = new PdfRenderer(rendererOptions, rendererAdapter: new RecordingAdapter(),
            paginationService: new ThrowingPaginationService());
        var document = CreateDocument();

        Assert.Throws<PaginationException>(() => renderer.Render(document));
    }

    [Fact]
    public void PaginationReceivesLayoutEngineFragments_WhenAdapterPathEnabled()
    {
        var rendererOptions = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableNewLayoutForTextBlocks = true,
            FontPath = string.Empty
        };

        var paginationService = new CapturingPaginationService();
        var adapter = new RecordingAdapter();
        var renderer = new PdfRenderer(rendererOptions, rendererAdapter: adapter, paginationService: paginationService);
        var document = CreateDocument();

        renderer.Render(document);

        Assert.NotNull(paginationService.LastFragments);
        Assert.NotEmpty(paginationService.LastFragments);
        Assert.All(paginationService.LastFragments!, fragment =>
            Assert.NotEqual("AdapterStub", fragment.Diagnostics.ContextName));
    }

    [Fact]
    public void AdapterMethods_InvokeInExpectedOrder_WhenFlagsEnabled()
    {
        var rendererOptions = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableNewLayoutForTextBlocks = true,
            FontPath = string.Empty
        };

        var adapter = new RecordingAdapter();
        var renderer = new PdfRenderer(rendererOptions, rendererAdapter: adapter);
        var document = CreateDocument();

        renderer.Render(document);

        Assert.True(adapter.CallSequence.Count >= 3);
        Assert.Equal("BeginDocument", adapter.CallSequence[0]);
        for (var i = 1; i < adapter.CallSequence.Count - 1; i++) Assert.Equal("Render", adapter.CallSequence[i]);
        Assert.Equal("EndDocument", adapter.CallSequence[^1]);
    }

    [Fact]
    public void InlineBlockFlag_ShouldControlAdapterRouting()
    {
        var enabledOptions = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableNewLayoutForTextBlocks = true,
            EnableInlineBlockContext = true,
            FontPath = string.Empty
        };

        var enabledPagination = new CapturingPaginationService();
        var enabledAdapter = new RecordingAdapter();
        var enabledRenderer = new PdfRenderer(enabledOptions, rendererAdapter: enabledAdapter,
            paginationService: enabledPagination);

        enabledRenderer.Render(CreateInlineBlockDocument());

        enabledAdapter.BeginCalled.ShouldBeTrue();
        enabledPagination.LastFragments.ShouldNotBeNull();
        enabledPagination.LastFragments!
            .Any(fragment => fragment.Diagnostics.ContextName == "InlineBlockFormattingContext")
            .ShouldBeTrue(
                "Inline-block fragments should originate from InlineBlockFormattingContext when flag enabled.");

        var disabledOptions = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableNewLayoutForTextBlocks = true,
            EnableInlineBlockContext = false,
            FontPath = string.Empty
        };

        var disabledAdapter = new RecordingAdapter();
        var disabledRenderer = new PdfRenderer(disabledOptions, rendererAdapter: disabledAdapter,
            paginationService: new CapturingPaginationService());

        disabledRenderer.Render(CreateInlineBlockDocument());

        disabledAdapter.BeginCalled.ShouldBeFalse(
            "Renderer should fall back to legacy pipeline when inline-block context is disabled.");
    }

    private static DocumentNode CreateDocument()
    {
        var paragraph = new DocumentNode(DocumentNodeType.Paragraph);
        paragraph.AddChild(new DocumentNode(DocumentNodeType.Text, "Hello"));

        var root = new DocumentNode(DocumentNodeType.Div);
        root.AddChild(paragraph);
        return root;
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

    private sealed class ThrowingPaginationService : PaginationService
    {
        public override PaginatedDocument Paginate(IReadOnlyList<LayoutFragment> fragments,
            PageConstraints pageConstraints, PaginationOptions options, ILogger? logger = null)
        {
            throw new PaginationException("Pagination failure");
        }
    }

    private sealed class CapturingPaginationService : PaginationService
    {
        public IReadOnlyList<LayoutFragment>? LastFragments { get; private set; }

        public override PaginatedDocument Paginate(IReadOnlyList<LayoutFragment> fragments,
            PageConstraints pageConstraints, PaginationOptions options, ILogger? logger = null)
        {
            LastFragments = fragments;
            return base.Paginate(fragments, pageConstraints, options, logger);
        }
    }

    private sealed class RecordingAdapter : IRendererAdapter
    {
        private readonly List<string> _callSequence = [];

        public bool BeginCalled { get; private set; }
        public bool EndCalled { get; private set; }
        public int RenderCallCount { get; private set; }
        public byte[] GeneratedBytes { get; } = [1, 2, 3];
        public IReadOnlyList<string> CallSequence => _callSequence;

        public void BeginDocument(PaginatedDocument document, RendererContext context)
        {
            _callSequence.Clear();
            _callSequence.Add("BeginDocument");
            BeginCalled = true;
        }

        public void Render(PageFragmentTree page, RendererContext context)
        {
            _callSequence.Add("Render");
            RenderCallCount++;
        }

        public byte[] EndDocument(RendererContext context)
        {
            _callSequence.Add("EndDocument");
            EndCalled = true;
            return GeneratedBytes;
        }
    }
}