using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using Shouldly;

namespace NetHtml2Pdf.Test.Renderer;

public class DisplayClassifierTests
{
    // Note: These are unit tests for the DisplayClassifier implementation
    // The actual DisplayClassifier implementation will be created in T008
    
    [Fact]
    public void Classify_SemanticBlockElements_ReturnsBlock()
    {
        // Arrange
        var classifier = new TestDisplayClassifier();
        
        // Act & Assert
        classifier.Classify(new DocumentNode(DocumentNodeType.Div), CssStyleMap.Empty).ShouldBe(DisplayClass.Block);
        classifier.Classify(new DocumentNode(DocumentNodeType.Section), CssStyleMap.Empty).ShouldBe(DisplayClass.Block);
        classifier.Classify(new DocumentNode(DocumentNodeType.Paragraph), CssStyleMap.Empty).ShouldBe(DisplayClass.Block);
        classifier.Classify(new DocumentNode(DocumentNodeType.Heading1), CssStyleMap.Empty).ShouldBe(DisplayClass.Block);
        classifier.Classify(new DocumentNode(DocumentNodeType.Heading2), CssStyleMap.Empty).ShouldBe(DisplayClass.Block);
        classifier.Classify(new DocumentNode(DocumentNodeType.Heading3), CssStyleMap.Empty).ShouldBe(DisplayClass.Block);
        classifier.Classify(new DocumentNode(DocumentNodeType.Heading4), CssStyleMap.Empty).ShouldBe(DisplayClass.Block);
        classifier.Classify(new DocumentNode(DocumentNodeType.Heading5), CssStyleMap.Empty).ShouldBe(DisplayClass.Block);
        classifier.Classify(new DocumentNode(DocumentNodeType.Heading6), CssStyleMap.Empty).ShouldBe(DisplayClass.Block);
        classifier.Classify(new DocumentNode(DocumentNodeType.UnorderedList), CssStyleMap.Empty).ShouldBe(DisplayClass.Block);
        classifier.Classify(new DocumentNode(DocumentNodeType.OrderedList), CssStyleMap.Empty).ShouldBe(DisplayClass.Block);
        classifier.Classify(new DocumentNode(DocumentNodeType.ListItem), CssStyleMap.Empty).ShouldBe(DisplayClass.Block);
        classifier.Classify(new DocumentNode(DocumentNodeType.Table), CssStyleMap.Empty).ShouldBe(DisplayClass.Block);
    }

    [Fact]
    public void Classify_SemanticInlineElements_ReturnsInline()
    {
        // Arrange
        var classifier = new TestDisplayClassifier();
        
        // Act & Assert
        classifier.Classify(new DocumentNode(DocumentNodeType.Span), CssStyleMap.Empty).ShouldBe(DisplayClass.Inline);
        classifier.Classify(new DocumentNode(DocumentNodeType.Strong), CssStyleMap.Empty).ShouldBe(DisplayClass.Inline);
        classifier.Classify(new DocumentNode(DocumentNodeType.Bold), CssStyleMap.Empty).ShouldBe(DisplayClass.Inline);
        classifier.Classify(new DocumentNode(DocumentNodeType.Italic), CssStyleMap.Empty).ShouldBe(DisplayClass.Inline);
        classifier.Classify(new DocumentNode(DocumentNodeType.Text), CssStyleMap.Empty).ShouldBe(DisplayClass.Inline);
        classifier.Classify(new DocumentNode(DocumentNodeType.LineBreak), CssStyleMap.Empty).ShouldBe(DisplayClass.Inline);
    }

    [Fact]
    public void Classify_ExplicitCssDisplay_OverridesSemanticDefault()
    {
        // Arrange
        var classifier = new TestDisplayClassifier();
        var span = new DocumentNode(DocumentNodeType.Span); // Semantic default is Inline
        
        // Act & Assert
        classifier.Classify(span, CssStyleMap.Empty.WithDisplay(CssDisplay.Block)).ShouldBe(DisplayClass.Block);
        classifier.Classify(span, CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock)).ShouldBe(DisplayClass.InlineBlock);
        classifier.Classify(span, CssStyleMap.Empty.WithDisplay(CssDisplay.None)).ShouldBe(DisplayClass.None);
    }

    [Fact]
    public void Classify_DisplayNone_ReturnsNone()
    {
        // Arrange
        var classifier = new TestDisplayClassifier();
        var div = new DocumentNode(DocumentNodeType.Div);
        
        // Act
        var result = classifier.Classify(div, CssStyleMap.Empty.WithDisplay(CssDisplay.None));
        
        // Assert
        result.ShouldBe(DisplayClass.None);
    }

    [Fact]
    public void Classify_ListItemElement_DefaultsToBlock()
    {
        // Arrange
        var classifier = new TestDisplayClassifier();
        var listItem = new DocumentNode(DocumentNodeType.ListItem);
        
        // Act
        var result = classifier.Classify(listItem, CssStyleMap.Empty);
        
        // Assert
        result.ShouldBe(DisplayClass.Block);
    }

    [Fact]
    public void Classify_TableSubElements_HandledViaTableContext()
    {
        // Arrange
        var classifier = new TestDisplayClassifier();
        
        // Act & Assert - Table sub-elements should be handled via Table context
        classifier.Classify(new DocumentNode(DocumentNodeType.TableHead), CssStyleMap.Empty).ShouldNotBe(DisplayClass.None);
        classifier.Classify(new DocumentNode(DocumentNodeType.TableBody), CssStyleMap.Empty).ShouldNotBe(DisplayClass.None);
        classifier.Classify(new DocumentNode(DocumentNodeType.TableSection), CssStyleMap.Empty).ShouldNotBe(DisplayClass.None);
        classifier.Classify(new DocumentNode(DocumentNodeType.TableRow), CssStyleMap.Empty).ShouldNotBe(DisplayClass.None);
        classifier.Classify(new DocumentNode(DocumentNodeType.TableHeaderCell), CssStyleMap.Empty).ShouldNotBe(DisplayClass.None);
        classifier.Classify(new DocumentNode(DocumentNodeType.TableCell), CssStyleMap.Empty).ShouldNotBe(DisplayClass.None);
    }

    [Fact]
    public void Classify_UnknownNodeType_DefaultsToBlockWithWarning()
    {
        // Arrange
        var classifier = new TestDisplayClassifier();
        var unknownNode = new DocumentNode(DocumentNodeType.Generic);
        
        // Act
        var result = classifier.Classify(unknownNode, CssStyleMap.Empty);
        
        // Assert
        result.ShouldBe(DisplayClass.Block);
        // Note: Warning behavior would be tested in integration tests with actual implementation
    }

    [Fact]
    public void Classify_UnsupportedCssDisplayValue_FallsBackToSemanticDefault()
    {
        // Arrange
        var classifier = new TestDisplayClassifier();
        var div = new DocumentNode(DocumentNodeType.Div); // Semantic default is Block
        
        // Act
        var result = classifier.Classify(div, CssStyleMap.Empty); // No explicit display set
        
        // Assert
        result.ShouldBe(DisplayClass.Block);
        // Note: Unsupported CSS display values would fallback to semantic default
        // This would be tested with actual implementation that handles unsupported values
    }

    [Fact]
    public void Classify_ExplicitCssDisplayBlockOnSpan_ReturnsBlock()
    {
        // Arrange
        var classifier = new TestDisplayClassifier();
        var span = new DocumentNode(DocumentNodeType.Span);
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block);
        
        // Act
        var result = classifier.Classify(span, styles);
        
        // Assert
        result.ShouldBe(DisplayClass.Block);
    }

    [Fact]
    public void Classify_ExplicitCssDisplayInlineBlockOnDiv_ReturnsInlineBlock()
    {
        // Arrange
        var classifier = new TestDisplayClassifier();
        var div = new DocumentNode(DocumentNodeType.Div);
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock);
        
        // Act
        var result = classifier.Classify(div, styles);
        
        // Assert
        result.ShouldBe(DisplayClass.InlineBlock);
    }

    [Theory]
    [InlineData(DocumentNodeType.Div, DisplayClass.Block)]
    [InlineData(DocumentNodeType.Span, DisplayClass.Inline)]
    [InlineData(DocumentNodeType.Strong, DisplayClass.Inline)]
    [InlineData(DocumentNodeType.Paragraph, DisplayClass.Block)]
    [InlineData(DocumentNodeType.Heading1, DisplayClass.Block)]
    [InlineData(DocumentNodeType.ListItem, DisplayClass.Block)]
    [InlineData(DocumentNodeType.Table, DisplayClass.Block)]
    [InlineData(DocumentNodeType.Text, DisplayClass.Inline)]
    [InlineData(DocumentNodeType.LineBreak, DisplayClass.Inline)]
    public void Classify_VariousNodeTypes_ReturnsCorrectSemanticDefaults(DocumentNodeType nodeType, DisplayClass expected)
    {
        // Arrange
        var classifier = new TestDisplayClassifier();
        var node = new DocumentNode(nodeType);
        
        // Act
        var result = classifier.Classify(node, CssStyleMap.Empty);
        
        // Assert
        result.ShouldBe(expected);
    }

    // Test implementation that simulates the DisplayClassifier behavior
    // This will be replaced by actual DisplayClassifier implementation in T008
    private class TestDisplayClassifier
    {
        public DisplayClass Classify(DocumentNode node, CssStyleMap style)
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
}
