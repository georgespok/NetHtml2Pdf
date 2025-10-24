using Microsoft.Extensions.Logging.Abstractions;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Model;
using NetHtml2Pdf.Renderer;
using Shouldly;
using Xunit;

namespace NetHtml2Pdf.Test.Renderer;

/// <summary>
/// Tests for edge cases and cross-cutting concerns in the centralized composition architecture.
/// These tests ensure that null overrides and other edge cases are handled correctly.
/// </summary>
public class EdgeCaseTests
{
    [Theory]
    [InlineData("NullServices")]
    [InlineData("EmptyServices")]
    public void CreateRenderer_WithRendererServicesOverrides_ShouldUseDefaults(string servicesType)
    {
        // Arrange
        var options = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableInlineBlockContext = true,
            FontPath = string.Empty
        };

        var services = servicesType switch
        {
            "NullServices" => null,
            "EmptyServices" => RendererServices.ForTests(),
            _ => throw new ArgumentException($"Unknown services type: {servicesType}")
        };

        // Act - pass RendererServices to ensure defaults are used
        var renderer = RendererComposition.CreateRenderer(options, services);

        // Assert - verify renderer is created successfully with defaults
        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<PdfRenderer>();

        // Test that the renderer actually works (observable behavior)
        var document = CreateSimpleDocument();
        var result = renderer.Render(document);

        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData("NullServices")]
    [InlineData("EmptyServices")]
    public void CreatePdfBuilderDependencies_WithRendererServicesOverrides_ShouldUseDefaults(string servicesType)
    {
        // Arrange
        var options = new RendererOptions();
        var logger = NullLogger<EdgeCaseTests>.Instance;

        var services = servicesType switch
        {
            "NullServices" => null,
            "EmptyServices" => RendererServices.ForTests(),
            _ => throw new ArgumentException($"Unknown services type: {servicesType}")
        };

        // Act - pass RendererServices to ensure defaults are used
        var (parser, rendererFactory) = RendererComposition.CreatePdfBuilderDependencies(options, services, logger);

        // Assert - verify dependencies are created successfully with defaults
        parser.ShouldNotBeNull();
        rendererFactory.ShouldNotBeNull();

        // Test that the parser actually works (observable behavior)
        var document = parser.Parse("<html><body><p>Test</p></body></html>");
        document.ShouldNotBeNull();
        document.Children.ShouldNotBeEmpty();

        // Test that the renderer factory actually works (observable behavior)
        var renderer = rendererFactory.Create(options);
        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<PdfRenderer>();
    }

    [Fact]
    public void CreateRenderer_WithPartialRendererServicesOverrides_ShouldUseDefaultsForNulls()
    {
        // Arrange
        var options = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableInlineBlockContext = true,
            FontPath = string.Empty
        };

        // Create services with only some overrides (others remain null)
        var partialServices = RendererServices.ForTests()
            .With(logger: NullLogger<EdgeCaseTests>.Instance);

        // Act - pass partial overrides to ensure defaults are used for null services
        var renderer = RendererComposition.CreateRenderer(options, partialServices);

        // Assert - verify renderer is created successfully with mixed defaults and overrides
        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<PdfRenderer>();

        // Test that the renderer actually works (observable behavior)
        var document = CreateSimpleDocument();
        var result = renderer.Render(document);

        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void CreateRenderer_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = RendererServices.ForTests();

        // Act & Assert - verify null options are handled correctly
        Should.Throw<ArgumentNullException>(() => RendererComposition.CreateRenderer(null!, services))
            .ParamName.ShouldBe("rendererOptions");
    }

    [Fact]
    public void CreatePdfBuilderDependencies_WithNullOptions_ShouldHandleGracefully()
    {
        // Arrange
        var logger = NullLogger<EdgeCaseTests>.Instance;
        var services = RendererServices.ForTests();

        // Act - pass null options to see how the method handles it
        var (parser, rendererFactory) = RendererComposition.CreatePdfBuilderDependencies(null!, services, logger);

        // Assert - verify dependencies are created successfully even with null options
        parser.ShouldNotBeNull();
        rendererFactory.ShouldNotBeNull();
    }

    [Fact]
    public void CreateRenderer_WithNullLogger_ShouldUseNullLoggerInstance()
    {
        // Arrange
        var options = new RendererOptions();

        // Act - pass null logger to ensure NullLogger.Instance is used
        var (parser, rendererFactory) = RendererComposition.CreatePdfBuilderDependencies(options, null, null);

        // Assert - verify dependencies are created successfully even with null logger
        parser.ShouldNotBeNull();
        rendererFactory.ShouldNotBeNull();

        // Test that the parser actually works (observable behavior)
        var document = parser.Parse("<html><body><p>Test</p></body></html>");
        document.ShouldNotBeNull();
        document.Children.ShouldNotBeEmpty();
    }

    [Theory]
    [InlineData("NullServices")]
    [InlineData("EmptyServices")]
    [InlineData("PartialServices")]
    public void RendererCreation_WithVariousNullOverrides_ShouldHandleGracefully(string overrideType)
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
            "NullServices" => null,
            "EmptyServices" => RendererServices.ForTests(),
            "PartialServices" => RendererServices.ForTests().With(logger: NullLogger<EdgeCaseTests>.Instance),
            _ => throw new ArgumentException($"Unknown override type: {overrideType}")
        };

        // Act
        var renderer = RendererComposition.CreateRenderer(options, services);

        // Assert - verify renderer is created successfully in all cases
        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<PdfRenderer>();

        // Test that the renderer actually works (observable behavior)
        var document = CreateSimpleDocument();
        var result = renderer.Render(document);

        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
    }

    #region Helper Methods

    private static DocumentNode CreateSimpleDocument()
    {
        var paragraph = new DocumentNode(DocumentNodeType.Paragraph);
        paragraph.AddChild(new DocumentNode(DocumentNodeType.Text, "Hello World"));

        var root = new DocumentNode(DocumentNodeType.Div);
        root.AddChild(paragraph);
        return root;
    }

    #endregion
}
