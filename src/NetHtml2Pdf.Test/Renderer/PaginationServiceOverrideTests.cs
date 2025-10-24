using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Model;
using NetHtml2Pdf.Layout.Pagination;
using NetHtml2Pdf.Renderer;
using Shouldly;
using Xunit;
using CssDisplay = NetHtml2Pdf.Core.Enums.CssDisplay;

namespace NetHtml2Pdf.Test.Renderer;

/// <summary>
/// Tests for verifying that PaginationService can be overridden in tests using RendererServices.
/// These tests demonstrate the test-time override pattern using observable behavior.
/// </summary>
public class PaginationServiceOverrideTests
{
    [Theory]
    [InlineData("Custom")]
    [InlineData("Tracking")]
    public void Renderer_WithPaginationServiceOverride_ShouldUseCustomService(string serviceType)
    {
        // Arrange
        var options = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableInlineBlockContext = true,
            EnableNewLayoutForTextBlocks = true,
            FontPath = string.Empty
        };

        PaginationService customService;
        RendererServices services;

        if (serviceType == "Custom")
        {
            customService = new CustomPaginationService();
            services = RendererServices.ForTests().With(pagination: customService);

            // Act
            var renderer = RendererComposition.CreateRenderer(options, services);

            // Assert - verify renderer is created successfully with custom pagination
            renderer.ShouldNotBeNull();
            renderer.ShouldBeOfType<PdfRenderer>();
        }
        else // Tracking
        {
            var trackingPagination = new TrackingPaginationService();
            services = RendererServices.ForTests().With(pagination: trackingPagination);

            // Act
            var renderer = (PdfRenderer)RendererComposition.CreateRenderer(options, services);
            var document = CreateInlineBlockDocument();
            renderer.Render(document);

            // Assert - verify pagination was called (observable behavior)
            trackingPagination.CallCount.ShouldBeGreaterThanOrEqualTo(1,
                "Pagination service should be called when adapter and pagination are enabled");
        }
    }

    [Fact]
    public void Renderer_WithThrowingPaginationService_ShouldPropagateException()
    {
        // Arrange
        var options = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableInlineBlockContext = true,
            EnableNewLayoutForTextBlocks = true,
            FontPath = string.Empty
        };

        var throwingPagination = new ThrowingPaginationService();
        var services = RendererServices.ForTests().With(pagination: throwingPagination);

        // Act
        var renderer = (PdfRenderer)RendererComposition.CreateRenderer(options, services);
        var document = CreateInlineBlockDocument();

        // Assert - verify custom pagination exception is thrown
        Should.Throw<PaginationException>(() => renderer.Render(document))
            .Message.ShouldBe("Custom pagination error");
    }

    [Fact]
    public void Renderer_WithValidatingPaginationService_ShouldReceiveCorrectInputs()
    {
        // Arrange
        var options = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableInlineBlockContext = true,
            EnableNewLayoutForTextBlocks = true,
            FontPath = string.Empty
        };

        var validatingPagination = new ValidatingPaginationService();
        var services = RendererServices.ForTests().With(pagination: validatingPagination);

        // Act
        var renderer = (PdfRenderer)RendererComposition.CreateRenderer(options, services);
        var document = CreateInlineBlockDocument();
        
        // Render to trigger pagination
        renderer.Render(document);

        // Assert - verify pagination received correct inputs
        validatingPagination.LastFragments.ShouldNotBeNull("Pagination should receive fragment list");
        validatingPagination.LastFragments!.Count.ShouldBeGreaterThan(0, "Should have at least one fragment");
    }

    [Fact]
    public void Renderer_WithDefaultPagination_ShouldWorkNormally()
    {
        // Arrange
        var options = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableInlineBlockContext = true,
            EnableNewLayoutForTextBlocks = true,
            FontPath = string.Empty
        };

        // Act - no pagination override, should use default
        var renderer = (PdfRenderer)RendererComposition.CreateRenderer(options, services: null);
        var document = CreateInlineBlockDocument();
        
        // Render should complete without throwing
        var bytes = renderer.Render(document);

        // Assert - verify render completed successfully
        bytes.ShouldNotBeNull();
        bytes.Length.ShouldBeGreaterThan(0);
    }

    #region Helper Methods and Custom Services

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

    /// <summary>
    /// Custom pagination service for testing override functionality.
    /// </summary>
    private sealed class CustomPaginationService : PaginationService
    {
        // This custom service inherits default behavior but could override methods if needed
    }

    /// <summary>
    /// Pagination service that tracks how many times it was called.
    /// </summary>
    private sealed class TrackingPaginationService : PaginationService
    {
        public int CallCount { get; private set; }

        public override PaginatedDocument Paginate(
            IReadOnlyList<LayoutFragment> fragments,
            PageConstraints pageConstraints,
            PaginationOptions options,
            ILogger? logger = null)
        {
            CallCount++;
            return base.Paginate(fragments, pageConstraints, options, logger);
        }
    }

    /// <summary>
    /// Pagination service that throws an exception for testing error handling.
    /// </summary>
    private sealed class ThrowingPaginationService : PaginationService
    {
        public override PaginatedDocument Paginate(
            IReadOnlyList<LayoutFragment> fragments,
            PageConstraints pageConstraints,
            PaginationOptions options,
            ILogger? logger = null)
        {
            throw new PaginationException("Custom pagination error");
        }
    }

    /// <summary>
    /// Pagination service that validates and captures input parameters.
    /// </summary>
    private sealed class ValidatingPaginationService : PaginationService
    {
        public IReadOnlyList<LayoutFragment>? LastFragments { get; private set; }
        public PageConstraints? LastPageConstraints { get; private set; }

        public override PaginatedDocument Paginate(
            IReadOnlyList<LayoutFragment> fragments,
            PageConstraints pageConstraints,
            PaginationOptions options,
            ILogger? logger = null)
        {
            LastFragments = fragments;
            LastPageConstraints = pageConstraints;
            return base.Paginate(fragments, pageConstraints, options, logger);
        }
    }

    #endregion
}

