using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Renderer.Interfaces;

namespace NetHtml2Pdf.Test.Renderer;

public class FeatureFlagCombinationTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void PdfRenderer_ReturnsBytesWhenConfigurationIsValid(bool enablePagination, bool enableAdapter)
    {
        var document = SampleDocumentFactory.CreateParagraph("Hello world");
        var options = CreateOptions(enablePagination, enableAdapter);

        var renderer = CreateRenderer(options);
        var result = renderer.Render(document);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void PdfRenderer_ThrowsWhenAdapterEnabledWithoutPagination()
    {
        var options = CreateOptions(false, true);
        Assert.Throws<InvalidOperationException>(() => CreateRenderer(options));
    }

    private static RendererOptions CreateOptions(bool enablePagination, bool enableAdapter)
    {
        return new RendererOptions
        {
            EnableNewLayoutForTextBlocks = true,
            EnablePagination = enablePagination,
            EnableQuestPdfAdapter = enableAdapter,
            FontPath = SampleDocumentFactory.ResolveTestFont()
        };
    }

    private static IPdfRenderer CreateRenderer(RendererOptions options)
    {
        var blockComposer = PdfRenderer.CreateDefaultBlockComposer(options);
        var rendererFactory = new PdfRendererFactory(blockComposer);
        return rendererFactory.Create(options);
    }
}

internal static class SampleDocumentFactory
{
    private const string EmbeddedFont = "docs/fonts/Inter-Regular.ttf";

    public static DocumentNode CreateParagraph(string text)
    {
        var paragraph = new DocumentNode(DocumentNodeType.Paragraph);
        paragraph.AddChild(new DocumentNode(DocumentNodeType.Text, text));

        var root = new DocumentNode(DocumentNodeType.Div);
        root.AddChild(paragraph);
        return root;
    }

    public static string ResolveTestFont()
    {
        var fontPath = Path.Combine(AppContext.BaseDirectory, EmbeddedFont);
        return File.Exists(fontPath) ? fontPath : Path.Combine(AppContext.BaseDirectory, "Fonts", "Inter-Regular.ttf");
    }
}