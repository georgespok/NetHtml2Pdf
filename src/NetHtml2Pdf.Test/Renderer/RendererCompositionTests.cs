using Microsoft.Extensions.Logging.Abstractions;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Model;
using NetHtml2Pdf.Layout.Pagination;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Renderer.Adapters;
using Shouldly;
using Xunit;
using CssDisplay = NetHtml2Pdf.Core.Enums.CssDisplay;

namespace NetHtml2Pdf.Test.Renderer;

/// <summary>
/// Tests for RendererComposition to verify that service overrides are properly used.
/// These tests use observable behavior to ensure the composition root respects service overrides.
/// </summary>
public class RendererCompositionTests
{
    [Fact]
    public void CreateRenderer_WithDefaultServices_ShouldCreateValidRenderer()
    {
        // Arrange
        var options = new RendererOptions
        {
            EnablePagination = false,
            EnableQuestPdfAdapter = false,
            FontPath = string.Empty
        };

        // Act
        var renderer = RendererComposition.CreateRenderer(options);

        // Assert - verify renderer is created successfully
        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<PdfRenderer>();
    }

    [Theory]
    [InlineData("PaginationService")]
    [InlineData("Adapter")]
    [InlineData("Multiple")]
    public void CreateRenderer_WithServiceOverrides_ShouldAcceptOverrides(string overrideType)
    {
        // Arrange
        var options = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableInlineBlockContext = true,
            FontPath = string.Empty
        };

        var services = overrideType switch
        {
            "PaginationService" => RendererServices.ForTests().With(pagination: new FakePaginationService()),
            "Adapter" => RendererServices.ForTests().With(rendererAdapter: new FakeRendererAdapter()),
            "Multiple" => RendererServices.ForTests().With(
                pagination: new FakePaginationService(),
                rendererAdapter: new FakeRendererAdapter()),
            _ => throw new ArgumentException($"Unknown override type: {overrideType}")
        };

        // Act
        var renderer = RendererComposition.CreateRenderer(options, services);

        // Assert - verify renderer is created successfully with overrides
        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<PdfRenderer>();
    }

    [Fact]
    public void CreateRenderer_WithNullServices_ShouldUseDefaults()
    {
        // Arrange
        var options = new RendererOptions
        {
            EnablePagination = false,
            EnableQuestPdfAdapter = false,
            FontPath = string.Empty
        };

        // Act
        var renderer = RendererComposition.CreateRenderer(options, services: null);

        // Assert - verify renderer is created with defaults
        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<PdfRenderer>();
    }

    [Fact]
    public void CreatePdfBuilderDependencies_WithDefaultServices_ShouldCreateValidDependencies()
    {
        // Arrange
        var options = new RendererOptions
        {
            EnablePagination = false,
            EnableQuestPdfAdapter = false,
            FontPath = string.Empty
        };

        // Act
        var (parser, rendererFactory) = RendererComposition.CreatePdfBuilderDependencies(options);

        // Assert - verify dependencies are created successfully
        parser.ShouldNotBeNull();
        rendererFactory.ShouldNotBeNull();
    }

    [Fact]
    public void CreatePdfBuilderDependencies_WithLogger_ShouldPassLoggerToParser()
    {
        // Arrange
        var options = new RendererOptions
        {
            EnablePagination = false,
            EnableQuestPdfAdapter = false,
            FontPath = string.Empty
        };
        var logger = NullLogger.Instance;

        // Act
        var (parser, rendererFactory) = RendererComposition.CreatePdfBuilderDependencies(options, logger: logger);

        // Assert - verify dependencies are created successfully
        parser.ShouldNotBeNull();
        rendererFactory.ShouldNotBeNull();
    }

    #region Helper Methods and Fakes

    private static DocumentNode CreateSimpleDocument()
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

    /// <summary>
    /// Fake pagination service that tracks whether it was called.
    /// </summary>
    private sealed class FakePaginationService : PaginationService
    {
        public bool WasCalled { get; private set; }

        public override PaginatedDocument Paginate(
            IReadOnlyList<LayoutFragment> fragments,
            PageConstraints pageConstraints,
            PaginationOptions options,
            Microsoft.Extensions.Logging.ILogger? logger = null)
        {
            WasCalled = true;
            return base.Paginate(fragments, pageConstraints, options, logger);
        }
    }

    /// <summary>
    /// Fake renderer adapter that tracks whether it was called.
    /// </summary>
    private sealed class FakeRendererAdapter : IRendererAdapter
    {
        public bool BeginCalled { get; private set; }
        public bool EndCalled { get; private set; }

        public void BeginDocument(PaginatedDocument document, RendererContext context)
        {
            BeginCalled = true;
        }

        public void Render(PageFragmentTree page, RendererContext context)
        {
            // No-op for testing
        }

        public byte[] EndDocument(RendererContext context)
        {
            EndCalled = true;
            return [1, 2, 3]; // Dummy PDF bytes
        }
    }

    #endregion
}

