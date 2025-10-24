using Microsoft.Extensions.Logging.Abstractions;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Model;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Renderer.Adapters;
using NetHtml2Pdf.Renderer.Interfaces;
using NetHtml2Pdf.Renderer.Inline;
using NetHtml2Pdf.Renderer.Spacing;
using Shouldly;
using Xunit;

namespace NetHtml2Pdf.Test.Renderer;

/// <summary>
/// Tests for PdfRendererFactory to verify that it correctly delegates to the composition root.
/// These tests ensure that the factory maintains its public API while using the new composition architecture.
/// </summary>
public class PdfRendererFactoryTests
{
    [Fact]
    public void Create_WithDefaultOptions_ShouldReturnValidRenderer()
    {
        // Arrange
        var factory = CreatePdfRendererFactory();
        var options = new RendererOptions();

        // Act
        var renderer = factory.Create(options);

        // Assert - verify renderer is created successfully
        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<PdfRenderer>();
    }

    [Fact]
    public void Create_WithCustomOptions_ShouldReturnValidRenderer()
    {
        // Arrange
        var factory = CreatePdfRendererFactory();
        var options = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableInlineBlockContext = true,
            FontPath = string.Empty
        };

        // Act
        var renderer = factory.Create(options);

        // Assert - verify renderer is created successfully with custom options
        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<PdfRenderer>();
    }

    [Fact]
    public void Create_ShouldAcceptNullOptions()
    {
        // Arrange
        var factory = CreatePdfRendererFactory();

        // Act & Assert - verify null options are handled (should throw ArgumentNullException)
        Should.Throw<ArgumentNullException>(() => factory.Create(null!))
            .ParamName.ShouldBe("options");
    }

    [Fact]
    public void Create_ShouldUseCompositionRoot()
    {
        // Arrange
        var factory = CreatePdfRendererFactory();
        var options = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableInlineBlockContext = true,
            FontPath = string.Empty
        };

        // Act
        var renderer = factory.Create(options);

        // Assert - verify renderer is created and can render a simple document
        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<PdfRenderer>();

        // Test that the renderer actually works (observable behavior)
        var document = CreateSimpleDocument();
        var result = renderer.Render(document);

        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Create_WithPaginationEnabled_ShouldCreateWorkingRenderer()
    {
        // Arrange
        var factory = CreatePdfRendererFactory();
        var options = new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableInlineBlockContext = true,
            FontPath = string.Empty
        };

        // Act
        var renderer = factory.Create(options);
        var document = CreateInlineBlockDocument();

        // Assert - verify renderer works with pagination enabled
        var result = renderer.Render(document);
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
    }

    [Theory]
    [InlineData("PaginationDisabled")]
    [InlineData("PaginationEnabled")]
    [InlineData("PaginationWithInlineBlock")]
    [InlineData("NewLayoutEnabled")]
    [InlineData("CustomFont")]
    public void Create_WithDifferentOptionCombinations_ShouldWork(string optionType)
    {
        // Arrange
        var factory = CreatePdfRendererFactory();
        var options = optionType switch
        {
            "PaginationDisabled" => new RendererOptions { EnablePagination = false, EnableQuestPdfAdapter = false },
            "PaginationEnabled" => new RendererOptions { EnablePagination = true, EnableQuestPdfAdapter = false },
            "PaginationWithInlineBlock" => new RendererOptions { EnablePagination = true, EnableQuestPdfAdapter = true, EnableInlineBlockContext = true },
            "NewLayoutEnabled" => new RendererOptions { EnableNewLayoutForTextBlocks = true },
            "CustomFont" => new RendererOptions { FontPath = "custom-font.ttf" },
            _ => throw new ArgumentException($"Unknown option type: {optionType}")
        };

        // Act
        var renderer = factory.Create(options);

        // Assert - verify renderer is created successfully for each combination
        renderer.ShouldNotBeNull();
        renderer.ShouldBeOfType<PdfRenderer>();
    }

    [Fact]
    public void Create_MultipleTimes_ShouldCreateIndependentInstances()
    {
        // Arrange
        var factory = CreatePdfRendererFactory();
        var options = new RendererOptions();

        // Act - create multiple renderers
        var renderer1 = factory.Create(options);
        var renderer2 = factory.Create(options);

        // Assert - verify they are independent instances
        renderer1.ShouldNotBeNull();
        renderer2.ShouldNotBeNull();
        renderer1.ShouldNotBeSameAs(renderer2);
    }

    [Fact]
    public void Create_ShouldMaintainPublicApiCompatibility()
    {
        // Arrange
        var factory = CreatePdfRendererFactory();
        var options = new RendererOptions();

        // Act - use the factory exactly as consumers would
        var renderer = factory.Create(options);

        // Assert - verify the public API works as expected
        renderer.ShouldNotBeNull();
        
        // Test that the renderer implements the expected interface
        renderer.ShouldBeAssignableTo<IPdfRenderer>();
    }

    #region Helper Methods

    private static IPdfRendererFactory CreatePdfRendererFactory()
    {
        // Create a BlockComposer to satisfy the constructor requirements
        var inlineComposer = new InlineComposer();
        var listComposer = new ListComposer(inlineComposer, new BlockSpacingApplier());
        var tableComposer = new TableComposer(inlineComposer, new BlockSpacingApplier());
        var spacingApplier = new BlockSpacingApplier();
        var blockComposer = new BlockComposer(inlineComposer, listComposer, tableComposer, spacingApplier);
        
        return new PdfRendererFactory(blockComposer);
    }

    private static DocumentNode CreateSimpleDocument()
    {
        var paragraph = new DocumentNode(DocumentNodeType.Paragraph);
        paragraph.AddChild(new DocumentNode(DocumentNodeType.Text, "Hello World"));
        
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

    #endregion
}
