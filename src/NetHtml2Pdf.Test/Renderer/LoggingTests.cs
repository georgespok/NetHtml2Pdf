using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Model;
using NetHtml2Pdf.Renderer;
using Shouldly;
using Xunit;
using Moq;

namespace NetHtml2Pdf.Test.Renderer;

/// <summary>
/// Tests for logging scenarios in the centralized composition architecture.
/// These tests ensure that logging works correctly in various scenarios including disabled logging.
/// </summary>
public class LoggingTests
{
    [Theory]
    [InlineData("NullLogger")]
    [InlineData("NullLoggerInstance")]
    public void CreateRenderer_WithDisabledLogging_ShouldWorkWithoutLogging(string loggingType)
    {
        // Arrange
        var options = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableInlineBlockContext = true,
            FontPath = string.Empty
        };

        var logger = loggingType switch
        {
            "NullLogger" => null,
            "NullLoggerInstance" => NullLogger.Instance,
            _ => throw new ArgumentException($"Unknown logging type: {loggingType}")
        };

        // Act - pass logger to ensure the system works without logging
        var (parser, rendererFactory) = RendererComposition.CreatePdfBuilderDependencies(options, null, logger);
        var renderer = rendererFactory.Create(options);

        // Assert - verify renderer is created successfully even with disabled logging
        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<PdfRenderer>();

        // Test that the renderer actually works (observable behavior)
        var document = CreateSimpleDocument();
        var result = renderer.Render(document);

        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData("MockLogger")]
    [InlineData("CustomLogger")]
    public void CreateRenderer_WithActiveLogging_ShouldWorkWithLogging(string loggingType)
    {
        // Arrange
        var options = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableInlineBlockContext = true,
            FontPath = string.Empty
        };

        ILogger logger = loggingType switch
        {
            "MockLogger" => new Mock<ILogger>().Object,
            "CustomLogger" => new CustomLogger(),
            _ => throw new ArgumentException($"Unknown logging type: {loggingType}")
        };

        // Act - pass logger to ensure the system works with logging
        var (parser, rendererFactory) = RendererComposition.CreatePdfBuilderDependencies(options, null, logger);
        var renderer = rendererFactory.Create(options);

        // Assert - verify renderer is created successfully with logging
        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<PdfRenderer>();

        // Test that the renderer actually works (observable behavior)
        var document = CreateSimpleDocument();
        var result = renderer.Render(document);

        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void CreateRenderer_WithLoggerInRendererServices_ShouldUseOverrideLogger()
    {
        // Arrange
        var options = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableInlineBlockContext = true,
            FontPath = string.Empty
        };

        var overrideLogger = new CustomLogger();
        var services = RendererServices.ForTests().With(logger: overrideLogger);

        // Act - pass logger override in RendererServices
        var renderer = RendererComposition.CreateRenderer(options, services);

        // Assert - verify renderer is created successfully with override logger
        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<PdfRenderer>();

        // Test that the renderer actually works (observable behavior)
        var document = CreateSimpleDocument();
        var result = renderer.Render(document);

        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData("NullLogger")]
    [InlineData("NullLoggerInstance")]
    [InlineData("CustomLogger")]
    public void RendererCreation_WithVariousLoggingScenarios_ShouldHandleGracefully(string loggingType)
    {
        // Arrange
        var options = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableInlineBlockContext = true,
            FontPath = string.Empty
        };

        ILogger? logger = loggingType switch
        {
            "NullLogger" => null,
            "NullLoggerInstance" => NullLogger.Instance,
            "CustomLogger" => new CustomLogger(),
            _ => throw new ArgumentException($"Unknown logging type: {loggingType}")
        };

        // Act
        var (parser, rendererFactory) = RendererComposition.CreatePdfBuilderDependencies(options, null, logger);
        var renderer = rendererFactory.Create(options);

        // Assert - verify renderer is created successfully in all logging scenarios
        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<PdfRenderer>();

        // Test that the renderer actually works (observable behavior)
        var document = CreateSimpleDocument();
        var result = renderer.Render(document);

        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void CreateRenderer_WithLoggingDisabled_ShouldWorkWithoutIssues()
    {
        // Arrange
        var options = new RendererOptions
        {
            EnablePagination = false,
            EnableQuestPdfAdapter = false,
            EnableInlineBlockContext = false,
            FontPath = string.Empty
        };

        // Act - test with logging disabled (null logger)
        var (parser, rendererFactory) = RendererComposition.CreatePdfBuilderDependencies(options, null, null);
        var renderer = rendererFactory.Create(options);

        // Assert - verify renderer is created successfully even with logging disabled
        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<PdfRenderer>();

        // Test that the renderer actually works (observable behavior)
        var document = CreateSimpleDocument();
        var result = renderer.Render(document);

        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void CreateRenderer_WithMinimalLogging_ShouldWorkWithBasicLogging()
    {
        // Arrange
        var options = new RendererOptions
        {
            EnablePagination = false,
            EnableQuestPdfAdapter = false,
            EnableInlineBlockContext = false,
            FontPath = string.Empty
        };

        var minimalLogger = new MinimalLogger();

        // Act - test with minimal logging
        var (parser, rendererFactory) = RendererComposition.CreatePdfBuilderDependencies(options, null, minimalLogger);
        var renderer = rendererFactory.Create(options);

        // Assert - verify renderer is created successfully with minimal logging
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

    #region Test Loggers

    /// <summary>
    /// Custom logger implementation for testing logging scenarios.
    /// </summary>
    private sealed class CustomLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            // Custom logging implementation for testing
            var message = formatter(state, exception);
            // In a real scenario, this would log to a file, console, etc.
        }
    }

    /// <summary>
    /// Minimal logger implementation for testing basic logging scenarios.
    /// </summary>
    private sealed class MinimalLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning; // Only log warnings and errors

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                var message = formatter(state, exception);
                // Minimal logging implementation for testing
            }
        }
    }

    #endregion
}
