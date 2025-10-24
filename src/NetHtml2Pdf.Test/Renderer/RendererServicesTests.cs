using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetHtml2Pdf.Layout.Display;
using NetHtml2Pdf.Layout.Engines;
using NetHtml2Pdf.Layout.FormattingContexts;
using NetHtml2Pdf.Layout.Pagination;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Renderer.Adapters;
using NetHtml2Pdf.Renderer.Inline;
using Shouldly;
using Xunit;

namespace NetHtml2Pdf.Test.Renderer;

/// <summary>
/// Tests for RendererServices to verify service override functionality.
/// These tests use observable behavior to ensure proper service overrides work correctly.
/// </summary>
public class RendererServicesTests
{
    [Fact]
    public void ForTests_ShouldCreateEmptyInstance()
    {
        // Act
        var services = RendererServices.ForTests();

        // Assert - verify all properties are null (no defaults)
        services.ShouldNotBeNull();
        services.PaginationService.ShouldBeNull();
        services.Logger.ShouldBeNull();
        services.DisplayClassifier.ShouldBeNull();
        services.InlineFlowLayoutEngine.ShouldBeNull();
        services.FormattingContextFactory.ShouldBeNull();
        services.LayoutEngine.ShouldBeNull();
        services.RendererAdapter.ShouldBeNull();
        services.BlockComposer.ShouldBeNull();
    }

    [Theory]
    [InlineData("PaginationService")]
    [InlineData("Logger")]
    [InlineData("DisplayClassifier")]
    [InlineData("RendererAdapter")]
    public void With_ShouldOverrideSingleService(string serviceType)
    {
        // Arrange & Act
        var services = serviceType switch
        {
            "PaginationService" => RendererServices.ForTests().With(pagination: new PaginationService()),
            "Logger" => RendererServices.ForTests().With(logger: NullLogger.Instance),
            "DisplayClassifier" => RendererServices.ForTests().With(displayClassifier: new DisplayClassifier()),
            "RendererAdapter" => RendererServices.ForTests().With(rendererAdapter: new QuestPdfAdapter()),
            _ => throw new ArgumentException($"Unknown service type: {serviceType}")
        };

        // Assert - verify only the specified service is overridden, others are null
        var overriddenCount = 0;
        if (services.PaginationService != null) overriddenCount++;
        if (services.Logger != null) overriddenCount++;
        if (services.DisplayClassifier != null) overriddenCount++;
        if (services.InlineFlowLayoutEngine != null) overriddenCount++;
        if (services.FormattingContextFactory != null) overriddenCount++;
        if (services.LayoutEngine != null) overriddenCount++;
        if (services.RendererAdapter != null) overriddenCount++;
        if (services.BlockComposer != null) overriddenCount++;

        overriddenCount.ShouldBe(1, $"Only {serviceType} should be overridden");
    }

    [Fact]
    public void With_ShouldOverrideMultipleServices()
    {
        // Arrange
        var customPagination = new PaginationService();
        var customLogger = NullLogger.Instance;
        var customClassifier = new DisplayClassifier();

        // Act
        var services = RendererServices.ForTests().With(
            pagination: customPagination,
            logger: customLogger,
            displayClassifier: customClassifier);

        // Assert - verify all overridden services are set
        services.PaginationService.ShouldBe(customPagination);
        services.Logger.ShouldBe(customLogger);
        services.DisplayClassifier.ShouldBe(customClassifier);
        services.InlineFlowLayoutEngine.ShouldBeNull();
        services.FormattingContextFactory.ShouldBeNull();
        services.LayoutEngine.ShouldBeNull();
        services.RendererAdapter.ShouldBeNull();
        services.BlockComposer.ShouldBeNull();
    }

    [Fact]
    public void With_ShouldChainOverrides()
    {
        // Arrange
        var customPagination = new PaginationService();
        var customLogger = NullLogger.Instance;

        // Act - chain multiple With calls
        var services = RendererServices.ForTests()
            .With(pagination: customPagination)
            .With(logger: customLogger);

        // Assert - verify both overrides are applied
        services.PaginationService.ShouldBe(customPagination);
        services.Logger.ShouldBe(customLogger);
        services.DisplayClassifier.ShouldBeNull();
        services.InlineFlowLayoutEngine.ShouldBeNull();
        services.FormattingContextFactory.ShouldBeNull();
        services.LayoutEngine.ShouldBeNull();
        services.RendererAdapter.ShouldBeNull();
        services.BlockComposer.ShouldBeNull();
    }

    [Fact]
    public void With_ShouldPreserveExistingOverrides()
    {
        // Arrange
        var firstPagination = new PaginationService();
        var firstLogger = NullLogger.Instance;
        var secondLogger = NullLogger<RendererServicesTests>.Instance;

        // Act - override logger while keeping pagination
        var services = RendererServices.ForTests()
            .With(pagination: firstPagination, logger: firstLogger)
            .With(logger: secondLogger);

        // Assert - verify pagination is preserved and logger is updated
        services.PaginationService.ShouldBe(firstPagination);
        services.Logger.ShouldBe(secondLogger);
    }
}

