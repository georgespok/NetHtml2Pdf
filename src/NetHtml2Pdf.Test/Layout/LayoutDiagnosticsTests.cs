using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Contexts;
using NetHtml2Pdf.Layout.Display;
using NetHtml2Pdf.Layout.Engines;
using NetHtml2Pdf.Layout.Model;
using NetHtml2Pdf.Renderer.Inline;
using NetHtml2Pdf.Test.Renderer;
using Shouldly;
using Xunit;

namespace NetHtml2Pdf.Test.Layout;

public class LayoutDiagnosticsTests
{
    private static LayoutEngine CreateEngine(ILogger<LayoutEngine> logger)
    {
        var displayClassifier = new DisplayClassifier();
        var inlineFlow = new InlineFlowLayoutEngine();
        var inlineContext = new InlineFormattingContext(inlineFlow);
        var blockContext = new BlockFormattingContext(inlineContext);
        return new LayoutEngine(displayClassifier, blockContext, inlineContext, logger);
    }

    private static DocumentNode CreateParagraph(string text)
    {
        var paragraph = new DocumentNode(DocumentNodeType.Paragraph);
        paragraph.AddChild(new DocumentNode(DocumentNodeType.Text, text));
        return paragraph;
    }

    [Fact]
    public void LayoutEngine_WithDiagnosticsEnabled_LogsFragmentMeasurements()
    {
        var logger = new TestLogger<LayoutEngine>();
        var engine = CreateEngine(logger);
        var node = CreateParagraph("Hello diagnostics");

        var result = engine.Layout(
            node,
            new LayoutConstraints(0, 400, 0, 1000, 1000, allowBreaks: true),
            new LayoutEngineOptions
            {
                EnableNewLayoutForTextBlocks = true,
                EnableDiagnostics = true
            });

        result.IsSuccess.ShouldBeTrue();
        logger.LogEntries.ShouldContain(entry =>
            entry.Level == LogLevel.Information &&
            entry.Message.Contains("LayoutEngine.FragmentMeasured"));
    }

    [Fact]
    public void LayoutEngine_WithDiagnosticsDisabled_DoesNotLogMeasurements()
    {
        var logger = new TestLogger<LayoutEngine>();
        var engine = CreateEngine(logger);
        var node = CreateParagraph("Diagnostics off");

        var result = engine.Layout(
            node,
            new LayoutConstraints(0, 400, 0, 1000, 1000, allowBreaks: true),
            new LayoutEngineOptions
            {
                EnableNewLayoutForTextBlocks = true,
                EnableDiagnostics = false
            });

        result.IsSuccess.ShouldBeTrue();
        logger.LogEntries.ShouldBeEmpty();
    }
}

