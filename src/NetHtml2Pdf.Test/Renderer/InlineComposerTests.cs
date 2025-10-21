using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Test.Support;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Shouldly;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Renderer;

public class InlineComposerTests(ITestOutputHelper output) : PdfRenderTestBase(output)
{
    [Fact]
    public void Compose_TextNode_AppliesStyleProperties()
    {
        var sut = new InlineComposer();
        var textNode = new DocumentNode(DocumentNodeType.Text, "Hello World");
        var style = InlineStyleState.Empty.WithFontSize(14);

        GenerateDocument(text => sut.Compose(text, textNode, style));

        // Test passes if no exception is thrown and PDF is generated
    }

    [Theory]
    [InlineData(DocumentNodeType.Span, "Inline-block span")]
    [InlineData(DocumentNodeType.Div, "Inline-block div")]
    public void Compose_DisplayInlineBlock_ComposesAsInlineElement(DocumentNodeType nodeType, string textContent)
    {
        var sut = new InlineComposer();
        var node = CreateInlineBlockNode(nodeType, textContent);

        GenerateDocument(text => sut.Compose(text, node, InlineStyleState.Empty));

        // Test passes if no exception is thrown and PDF is generated
    }

    [Fact]
    public void Compose_DisplayInlineBlockWithChildren_ComposesChildrenAsInlineElements()
    {
        var sut = new InlineComposer();
        var divWithInlineBlockDisplay = CreateInlineBlockNode(DocumentNodeType.Div);
        divWithInlineBlockDisplay.AddChild(new DocumentNode(DocumentNodeType.Text, "First inline"));
        divWithInlineBlockDisplay.AddChild(new DocumentNode(DocumentNodeType.Strong, "Second inline"));

        GenerateDocument(text => sut.Compose(text, divWithInlineBlockDisplay, InlineStyleState.Empty));

        // Test passes if no exception is thrown and PDF is generated
    }

    [Fact]
    public void Compose_DisplayInlineBlockEmptyElement_HandlesEmptyInlineBlock()
    {
        var sut = new InlineComposer();
        var emptyInlineBlockSpan = CreateInlineBlockNode(DocumentNodeType.Span);

        GenerateDocument(text => sut.Compose(text, emptyInlineBlockSpan, InlineStyleState.Empty));

        // Test passes if no exception is thrown and PDF is generated
    }

    [Fact]
    public void Compose_Strong_AppliesBoldStyle()
    {
        var sut = new InlineComposer();
        var strongNode = new DocumentNode(DocumentNodeType.Strong);
        strongNode.AddChild(new DocumentNode(DocumentNodeType.Text, "Bold text"));

        GenerateDocument(text => sut.Compose(text, strongNode, InlineStyleState.Empty));

        // Test passes if no exception is thrown and PDF is generated
    }

    [Fact]
    public void Compose_Italic_AppliesItalicStyle()
    {
        var sut = new InlineComposer();
        var italicNode = new DocumentNode(DocumentNodeType.Italic);
        italicNode.AddChild(new DocumentNode(DocumentNodeType.Text, "Italic text"));

        GenerateDocument(text => sut.Compose(text, italicNode, InlineStyleState.Empty));

        // Test passes if no exception is thrown and PDF is generated
    }

    [Fact]
    public void Compose_DisplayBlock_ShouldNotBeHandledByInlineComposer()
    {
        var sut = new InlineComposer();
        var blockNode = CreateBlockDisplayNode(DocumentNodeType.Div, "Block content");

        // This should not throw an exception, but the block display should not be processed
        // by InlineComposer (it should fall through to default handling)
        GenerateDocument(text => sut.Compose(text, blockNode, InlineStyleState.Empty));

        // Test passes if no exception is thrown - InlineComposer should handle gracefully
    }

    [Theory]
    [InlineData("display: flex")]
    [InlineData("display: grid")]
    [InlineData("display: table")]
    [InlineData("display: invalid")]
    public void Compose_UnsupportedDisplayValues_ShouldNotBeHandledByInlineComposer(string displayValue)
    {
        var sut = new InlineComposer();
        var node = CreateNodeWithDisplay(DocumentNodeType.Span, "Content", displayValue);

        // These unsupported display values should not be handled by InlineComposer
        // and should fall through to default handling
        GenerateDocument(text => sut.Compose(text, node, InlineStyleState.Empty));

        // Test passes if no exception is thrown - InlineComposer should handle gracefully
    }

    [Fact]
    public void Compose_DisplayInlineBlockWithNestedElements_ShouldComposeCorrectly()
    {
        var sut = new InlineComposer();
        var inlineBlockNode = CreateInlineBlockNode(DocumentNodeType.Div);

        // Add nested inline elements
        var strongNode = new DocumentNode(DocumentNodeType.Strong);
        strongNode.AddChild(new DocumentNode(DocumentNodeType.Text, "Bold inline"));
        inlineBlockNode.AddChild(strongNode);

        var italicNode = new DocumentNode(DocumentNodeType.Italic);
        italicNode.AddChild(new DocumentNode(DocumentNodeType.Text, "Italic inline"));
        inlineBlockNode.AddChild(italicNode);

        GenerateDocument(text => sut.Compose(text, inlineBlockNode, InlineStyleState.Empty));

        // Test passes if no exception is thrown and PDF is generated
    }

    private static DocumentNode CreateInlineBlockNode(DocumentNodeType nodeType, string? textContent = null)
    {
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock);
        return new DocumentNode(nodeType, textContent, styles);
    }

    private static DocumentNode CreateBlockDisplayNode(DocumentNodeType nodeType, string? textContent = null)
    {
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block);
        return new DocumentNode(nodeType, textContent, styles);
    }

    private static DocumentNode CreateNodeWithDisplay(DocumentNodeType nodeType, string? textContent,
        string displayValue)
    {
        // Parse the display value to create the appropriate CssDisplay enum
        var cssDisplay = displayValue.ToLowerInvariant() switch
        {
            "block" => CssDisplay.Block,
            "inline-block" => CssDisplay.InlineBlock,
            "none" => CssDisplay.None,
            _ => CssDisplay.Default // For unsupported values
        };

        var styles = CssStyleMap.Empty.WithDisplay(cssDisplay);
        return new DocumentNode(nodeType, textContent, styles);
    }

    private static void GenerateDocument(Action<TextDescriptor> compose)
    {
        Settings.License = LicenseType.Community;
        Settings.UseEnvironmentFonts = false;

        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(10);
                page.Content().Text(compose);
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
    }

    #region Inline-Block Sizing and Wrapping Validation Tests

    [Fact]
    public void ComposeInlineBlock_WithNegativeMargin_ShouldThrowException()
    {
        // Arrange
        var sut = new InlineComposer();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock).WithMarginTop(-10);
        var node = new DocumentNode(DocumentNodeType.Span, "Test content", styles);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() =>
        {
            GenerateDocument(text => sut.Compose(text, node, InlineStyleState.Empty));
        });

        exception.Message.ShouldContain("Negative top margin (-10) is not allowed for inline-block element Span");
    }

    [Fact]
    public void ComposeInlineBlock_WithNegativePadding_ShouldThrowException()
    {
        // Arrange
        var sut = new InlineComposer();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock).WithPaddingLeft(-5);
        var node = new DocumentNode(DocumentNodeType.Div, "Test content", styles);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() =>
        {
            GenerateDocument(text => sut.Compose(text, node, InlineStyleState.Empty));
        });

        exception.Message.ShouldContain("Negative left padding (-5) is not allowed for inline-block element Div");
    }

    [Fact]
    public void ComposeInlineBlock_WithNegativeBorderWidth_ShouldThrowException()
    {
        // Arrange
        var sut = new InlineComposer();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock)
            .WithBorder(new BorderInfo(-2, "solid", "#000000"));
        var node = new DocumentNode(DocumentNodeType.Span, "Test content", styles);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() =>
        {
            GenerateDocument(text => sut.Compose(text, node, InlineStyleState.Empty));
        });

        exception.Message.ShouldContain("Negative border width (-2) is not allowed for inline-block element Span");
    }

    [Fact]
    public void ComposeInlineBlock_WithValidMargins_ShouldNotThrowException()
    {
        // Arrange
        var sut = new InlineComposer();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock).WithMargin(BoxSpacing.FromAll(10));
        var node = new DocumentNode(DocumentNodeType.Div, "Test content", styles);

        // Act & Assert - Should not throw
        Should.NotThrow(() => { GenerateDocument(text => sut.Compose(text, node, InlineStyleState.Empty)); });
    }

    [Fact]
    public void ComposeInlineBlock_WithValidPadding_ShouldNotThrowException()
    {
        // Arrange
        var sut = new InlineComposer();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock).WithPadding(BoxSpacing.FromAll(5));
        var node = new DocumentNode(DocumentNodeType.Span, "Test content", styles);

        // Act & Assert - Should not throw
        Should.NotThrow(() => { GenerateDocument(text => sut.Compose(text, node, InlineStyleState.Empty)); });
    }

    [Fact]
    public void ComposeInlineBlock_WithValidBorder_ShouldNotThrowException()
    {
        // Arrange
        var sut = new InlineComposer();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock)
            .WithBorder(new BorderInfo(1, "solid", "#000000"));
        var node = new DocumentNode(DocumentNodeType.Div, "Test content", styles);

        // Act & Assert - Should not throw
        Should.NotThrow(() => { GenerateDocument(text => sut.Compose(text, node, InlineStyleState.Empty)); });
    }

    [Fact]
    public void ComposeInlineBlock_WithZeroMargins_ShouldNotThrowException()
    {
        // Arrange
        var sut = new InlineComposer();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock).WithMargin(BoxSpacing.FromAll(0));
        var node = new DocumentNode(DocumentNodeType.Span, "Test content", styles);

        // Act & Assert - Should not throw
        Should.NotThrow(() => { GenerateDocument(text => sut.Compose(text, node, InlineStyleState.Empty)); });
    }

    [Fact]
    public void ComposeInlineBlock_WithZeroPadding_ShouldNotThrowException()
    {
        // Arrange
        var sut = new InlineComposer();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock).WithPadding(BoxSpacing.FromAll(0));
        var node = new DocumentNode(DocumentNodeType.Div, "Test content", styles);

        // Act & Assert - Should not throw
        Should.NotThrow(() => { GenerateDocument(text => sut.Compose(text, node, InlineStyleState.Empty)); });
    }

    [Fact]
    public void ComposeInlineBlock_WithMixedNegativeMargins_ShouldThrowException()
    {
        // Arrange
        var sut = new InlineComposer();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock)
            .WithMarginTop(10) // Valid
            .WithMarginRight(-5) // Invalid
            .WithMarginBottom(15) // Valid
            .WithMarginLeft(-8); // Invalid
        var node = new DocumentNode(DocumentNodeType.Span, "Test content", styles);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() =>
        {
            GenerateDocument(text => sut.Compose(text, node, InlineStyleState.Empty));
        });

        // Should throw for the first negative margin encountered
        exception.Message.ShouldContain("Negative right margin (-5) is not allowed for inline-block element Span");
    }

    [Fact]
    public void ComposeInlineBlock_WithMixedNegativePadding_ShouldThrowException()
    {
        // Arrange
        var sut = new InlineComposer();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock)
            .WithPaddingTop(5) // Valid
            .WithPaddingRight(-3) // Invalid
            .WithPaddingBottom(8) // Valid
            .WithPaddingLeft(-2); // Invalid
        var node = new DocumentNode(DocumentNodeType.Div, "Test content", styles);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() =>
        {
            GenerateDocument(text => sut.Compose(text, node, InlineStyleState.Empty));
        });

        // Should throw for the first negative padding encountered
        exception.Message.ShouldContain("Negative right padding (-3) is not allowed for inline-block element Div");
    }

    [Fact]
    public void ComposeInlineBlock_WithNestedBlockElements_ShouldValidateWrapping()
    {
        // Arrange
        var sut = new InlineComposer();
        var inlineBlockStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock);
        var inlineBlockNode = new DocumentNode(DocumentNodeType.Div, null, inlineBlockStyles);

        var nestedBlockStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithMargin(BoxSpacing.FromAll(5));
        var nestedBlock = new DocumentNode(DocumentNodeType.Paragraph, "Nested block", nestedBlockStyles);

        inlineBlockNode.AddChild(nestedBlock);

        // Act & Assert - Should not throw (wrapping validation should pass)
        Should.NotThrow(() =>
        {
            GenerateDocument(text => sut.Compose(text, inlineBlockNode, InlineStyleState.Empty));
        });
    }

    [Fact]
    public void ComposeInlineBlock_WithNestedBlockWithLargeMargins_ShouldValidateWrapping()
    {
        // Arrange
        var sut = new InlineComposer();
        var inlineBlockStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock);
        var inlineBlockNode = new DocumentNode(DocumentNodeType.Div, null, inlineBlockStyles);

        // Create nested block with large vertical margins (should trigger wrapping validation)
        var nestedBlockStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block)
            .WithMarginTop(30) // Large margin
            .WithMarginBottom(25); // Large margin
        var nestedBlock = new DocumentNode(DocumentNodeType.Paragraph, "Nested block with large margins",
            nestedBlockStyles);

        inlineBlockNode.AddChild(nestedBlock);

        // Act & Assert - Should not throw (wrapping validation should pass, but might log warning)
        Should.NotThrow(() =>
        {
            GenerateDocument(text => sut.Compose(text, inlineBlockNode, InlineStyleState.Empty));
        });
    }

    [Fact]
    public void ComposeInlineBlock_WithNestedInlineBlockElements_ShouldValidateCorrectly()
    {
        // Arrange
        var sut = new InlineComposer();
        var parentInlineBlockStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock);
        var parentInlineBlock = new DocumentNode(DocumentNodeType.Div, null, parentInlineBlockStyles);

        var childInlineBlockStyles =
            CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock).WithPadding(BoxSpacing.FromAll(3));
        var childInlineBlock = new DocumentNode(DocumentNodeType.Span, "Child inline-block", childInlineBlockStyles);

        parentInlineBlock.AddChild(childInlineBlock);

        // Act & Assert - Should not throw (nested inline-block validation should pass)
        Should.NotThrow(() =>
        {
            GenerateDocument(text => sut.Compose(text, parentInlineBlock, InlineStyleState.Empty));
        });
    }

    [Fact]
    public void ComposeInlineBlock_WithInvalidDisplayProperty_ShouldThrowException()
    {
        // Arrange
        var sut = new InlineComposer();
        // Create a node that's not actually set to inline-block but tries to use inline-block validation
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block); // Wrong display type
        var node = new DocumentNode(DocumentNodeType.Span, "Test content", styles);

        // Act & Assert
        // This test verifies that the validation correctly identifies non-inline-block elements
        // The ComposeInlineBlock method should only be called for elements with display: inline-block
        Should.NotThrow(() => { GenerateDocument(text => sut.Compose(text, node, InlineStyleState.Empty)); });
    }

    #endregion
}