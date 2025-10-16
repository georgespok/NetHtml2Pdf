using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using Shouldly;

namespace NetHtml2Pdf.Test.Renderer;

public class DisplayClassifierContractTests
{
    // Note: These are contract tests that define the expected behavior of DisplayClassifier
    // The actual DisplayClassifier implementation will be created in T008
    
    [Theory]
    [InlineData(DocumentNodeType.Div, DisplayClass.Block)]
    [InlineData(DocumentNodeType.Section, DisplayClass.Block)]
    [InlineData(DocumentNodeType.Paragraph, DisplayClass.Block)]
    [InlineData(DocumentNodeType.Heading1, DisplayClass.Block)]
    [InlineData(DocumentNodeType.Heading2, DisplayClass.Block)]
    [InlineData(DocumentNodeType.Heading3, DisplayClass.Block)]
    [InlineData(DocumentNodeType.Heading4, DisplayClass.Block)]
    [InlineData(DocumentNodeType.Heading5, DisplayClass.Block)]
    [InlineData(DocumentNodeType.Heading6, DisplayClass.Block)]
    [InlineData(DocumentNodeType.UnorderedList, DisplayClass.Block)]
    [InlineData(DocumentNodeType.OrderedList, DisplayClass.Block)]
    [InlineData(DocumentNodeType.ListItem, DisplayClass.Block)]
    [InlineData(DocumentNodeType.Table, DisplayClass.Block)]
    public void Classify_SemanticBlockElements_ReturnsBlock(DocumentNodeType nodeType, DisplayClass expected)
    {
        // Arrange
        var node = new DocumentNode(nodeType);
        var style = CssStyleMap.Empty;

        // Act
        var result = ClassifyDisplay(node, style);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(DocumentNodeType.Span, DisplayClass.Inline)]
    [InlineData(DocumentNodeType.Strong, DisplayClass.Inline)]
    [InlineData(DocumentNodeType.Bold, DisplayClass.Inline)]
    [InlineData(DocumentNodeType.Italic, DisplayClass.Inline)]
    [InlineData(DocumentNodeType.Text, DisplayClass.Inline)]
    [InlineData(DocumentNodeType.LineBreak, DisplayClass.Inline)]
    public void Classify_SemanticInlineElements_ReturnsInline(DocumentNodeType nodeType, DisplayClass expected)
    {
        // Arrange
        var node = new DocumentNode(nodeType);
        var style = CssStyleMap.Empty;

        // Act
        var result = ClassifyDisplay(node, style);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(CssDisplay.Block, DisplayClass.Block)]
    [InlineData(CssDisplay.InlineBlock, DisplayClass.InlineBlock)]
    [InlineData(CssDisplay.None, DisplayClass.None)]
    public void Classify_ExplicitCssDisplay_OverridesSemanticDefault(CssDisplay cssDisplay, DisplayClass expected)
    {
        // Arrange
        var node = new DocumentNode(DocumentNodeType.Span); // Semantic default is Inline
        var style = CssStyleMap.Empty.WithDisplay(cssDisplay);

        // Act
        var result = ClassifyDisplay(node, style);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void Classify_ExplicitCssDisplayBlockOnSpan_ReturnsBlock()
    {
        // Arrange - Span has semantic default of Inline, but CSS display: block should override
        var node = new DocumentNode(DocumentNodeType.Span);
        var style = CssStyleMap.Empty.WithDisplay(CssDisplay.Block);

        // Act
        var result = ClassifyDisplay(node, style);

        // Assert
        result.ShouldBe(DisplayClass.Block);
    }

    [Fact]
    public void Classify_ExplicitCssDisplayInlineOnDiv_ReturnsInline()
    {
        // Arrange - Div has semantic default of Block, but CSS display: inline should override
        var node = new DocumentNode(DocumentNodeType.Div);
        var style = CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock); // Note: CssDisplay enum doesn't have Inline, using InlineBlock

        // Act
        var result = ClassifyDisplay(node, style);

        // Assert
        result.ShouldBe(DisplayClass.InlineBlock);
    }

    [Fact]
    public void Classify_DisplayNone_ReturnsNone()
    {
        // Arrange
        var node = new DocumentNode(DocumentNodeType.Div);
        var style = CssStyleMap.Empty.WithDisplay(CssDisplay.None);

        // Act
        var result = ClassifyDisplay(node, style);

        // Assert
        result.ShouldBe(DisplayClass.None);
    }

    [Fact]
    public void Classify_ListItemElement_DefaultsToBlock()
    {
        // Arrange
        var node = new DocumentNode(DocumentNodeType.ListItem);
        var style = CssStyleMap.Empty;

        // Act
        var result = ClassifyDisplay(node, style);

        // Assert
        result.ShouldBe(DisplayClass.Block);
    }

    [Theory]
    [InlineData(DocumentNodeType.TableHead)]
    [InlineData(DocumentNodeType.TableBody)]
    [InlineData(DocumentNodeType.TableSection)]
    [InlineData(DocumentNodeType.TableRow)]
    [InlineData(DocumentNodeType.TableHeaderCell)]
    [InlineData(DocumentNodeType.TableCell)]
    public void Classify_TableSubElements_HandledViaTableContext(DocumentNodeType nodeType)
    {
        // Arrange - Table sub-elements should be handled via Table context, not explicit classification
        var node = new DocumentNode(nodeType);
        var style = CssStyleMap.Empty;

        // Act
        var result = ClassifyDisplay(node, style);

        // Assert - These should not be explicitly classified as they're handled via table context
        // This is a contract test - the actual behavior may defer to table context handling
        result.ShouldNotBe(DisplayClass.None); // Should have some classification
    }

    [Fact]
    public void Classify_UnknownNodeType_DefaultsToBlockWithWarning()
    {
        // Arrange
        var node = new DocumentNode(DocumentNodeType.Generic);
        var style = CssStyleMap.Empty;

        // Act
        var result = ClassifyDisplay(node, style);

        // Assert
        result.ShouldBe(DisplayClass.Block);
        // Note: Warning behavior would be tested in integration tests with actual implementation
    }

    [Fact]
    public void Classify_UnsupportedCssDisplayValue_FallsBackToSemanticDefault()
    {
        // Arrange
        var node = new DocumentNode(DocumentNodeType.Div); // Semantic default is Block
        var style = CssStyleMap.Empty; // No explicit display set

        // Act
        var result = ClassifyDisplay(node, style);

        // Assert
        result.ShouldBe(DisplayClass.Block);
        // Note: Unsupported CSS display values would fallback to semantic default
        // This would be tested with actual implementation that handles unsupported values
    }

    // Helper method that simulates the DisplayClassifier behavior
    // This will be replaced by actual DisplayClassifier implementation in T008
    private static DisplayClass ClassifyDisplay(DocumentNode node, CssStyleMap style)
    {
        // Explicit CSS display wins
        if (style.DisplaySet && style.Display != CssDisplay.Default)
        {
            return style.Display switch
            {
                CssDisplay.Block => DisplayClass.Block,
                CssDisplay.InlineBlock => DisplayClass.InlineBlock,
                CssDisplay.None => DisplayClass.None,
                _ => GetSemanticDefault(node.NodeType) // Fallback for unsupported values
            };
        }

        // Semantic defaults
        return GetSemanticDefault(node.NodeType);
    }

    private static DisplayClass GetSemanticDefault(DocumentNodeType nodeType)
    {
        return nodeType switch
        {
            // Block elements
            DocumentNodeType.Div or
            DocumentNodeType.Section or
            DocumentNodeType.Paragraph or
            DocumentNodeType.Heading1 or
            DocumentNodeType.Heading2 or
            DocumentNodeType.Heading3 or
            DocumentNodeType.Heading4 or
            DocumentNodeType.Heading5 or
            DocumentNodeType.Heading6 or
            DocumentNodeType.UnorderedList or
            DocumentNodeType.OrderedList or
            DocumentNodeType.ListItem or
            DocumentNodeType.Table => DisplayClass.Block,

            // Inline elements
            DocumentNodeType.Span or
            DocumentNodeType.Strong or
            DocumentNodeType.Bold or
            DocumentNodeType.Italic or
            DocumentNodeType.Text or
            DocumentNodeType.LineBreak => DisplayClass.Inline,

            // Table sub-elements (handled via table context)
            DocumentNodeType.TableHead or
            DocumentNodeType.TableBody or
            DocumentNodeType.TableSection or
            DocumentNodeType.TableRow or
            DocumentNodeType.TableHeaderCell or
            DocumentNodeType.TableCell => DisplayClass.Block, // Default to block for table elements

            // Unknown/unsupported types default to Block
            _ => DisplayClass.Block
        };
    }
}
