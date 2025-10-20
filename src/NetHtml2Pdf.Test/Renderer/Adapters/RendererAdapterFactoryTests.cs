using Microsoft.Extensions.Logging.Abstractions;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Renderer.Adapters;
using Xunit;

namespace NetHtml2Pdf.Test.Renderer.Adapters;

public class RendererAdapterFactoryTests
{
    private readonly RendererAdapterFactory _factory = new();

    [Fact]
    public void Create_WithQuestPdfAdapterEnabled_ReturnsQuestPdfAdapter()
    {
        var options = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true
        };

        var adapter = _factory.Create(options, NullLogger.Instance);

        Assert.IsType<QuestPdfAdapter>(adapter);
    }

    [Fact]
    public void Create_WhenQuestPdfAdapterDisabled_ReturnsNullRendererAdapter()
    {
        var options = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = false
        };

        var adapter = _factory.Create(options, NullLogger.Instance);

        Assert.IsType<NullRendererAdapter>(adapter);
    }

    [Fact]
    public void Create_WhenQuestPdfAdapterEnabledWithoutPagination_ThrowsInvalidOperationException()
    {
        var options = new RendererOptions
        {
            EnablePagination = false,
            EnableQuestPdfAdapter = true
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            _factory.Create(options, NullLogger.Instance));

        Assert.Equal("QuestPdfAdapter requires pagination.", exception.Message);
    }
}
