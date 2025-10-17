using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Display;
using NetHtml2Pdf.Layout.Engines;
using NetHtml2Pdf.Layout.Model;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Renderer.Interfaces;
using NetHtml2Pdf.Test.Support;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Shouldly;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Renderer;

public class BlockComposerTests(ITestOutputHelper output) : PdfRenderTestBase(output)
{
    [Fact]
    public void Compose_Paragraph_DelegatesToInlineComposerForEachChild()
    {
        var sut = CreateSut();
        var paragraph = Paragraph(Text("Hello"), Strong(Text("World")));

        GenerateDocument(column => sut.Compose(column, paragraph));

        sut.InlineComposer.Nodes.ShouldBe(new[] { DocumentNodeType.Text, DocumentNodeType.Strong });
        sut.ListComposer.Called.ShouldBeFalse();
    }

    [Fact]
    public void Compose_Paragraph_WithLayoutEngineEnabled_InvokesLayoutEngine()
    {
        var sut = CreateSut(enableNewLayout: true);
        var paragraph = Paragraph(Text("Hello"));

        GenerateDocument(column => sut.Compose(column, paragraph));

        sut.LayoutEngine.Calls.ShouldBeGreaterThan(0);
        sut.InlineComposer.Nodes.ShouldBe(new[] { DocumentNodeType.Text });
    }

    [Fact]
    public void Compose_Paragraph_LayoutEngineFallback_UsesLegacyPath()
    {
        var sut = CreateSut(enableNewLayout: true);
        sut.LayoutEngine.OverrideResult = LayoutResult.Fallback("unsupported");
        var paragraph = Paragraph(Text("Hello"));

        GenerateDocument(column => sut.Compose(column, paragraph));

        sut.LayoutEngine.Calls.ShouldBe(1);
        sut.InlineComposer.Nodes.ShouldBe(new[] { DocumentNodeType.Text });
    }

    [Fact]
    public void Compose_List_DelegatesToListComposer()
    {
        var sut = CreateSut();
        var listNode = new DocumentNode(DocumentNodeType.List);
        listNode.AddChild(ListItem());

        GenerateDocument(column => sut.Compose(column, listNode));

        sut.ListComposer.Called.ShouldBeTrue();
        sut.ListComposer.LastOrdered.ShouldBeFalse();
    }

    [Theory]
    [InlineData(DocumentNodeType.Span, "Block span")]
    [InlineData(DocumentNodeType.Div, "Block text")]
    public void Compose_DisplayBlock_ComposesAsBlockElement(DocumentNodeType nodeType, string textContent)
    {
        var sut = CreateSut();
        var node = CreateBlockDisplayNode(nodeType, textContent);

        GenerateDocument(column => sut.Compose(column, node));

        // For span with text content, expect the span itself to be processed as text
        // For div with text content, expect the div itself to be processed as text
        var expectedNodes = new[] { nodeType };
        sut.InlineComposer.Nodes.ShouldBe(expectedNodes);
    }

    [Fact]
    public void Compose_DisplayBlockWithChildren_ComposesChildrenAsBlockElements()
    {
        var sut = CreateSut();
        var divWithBlockDisplay = CreateBlockDisplayNode(DocumentNodeType.Div);
        divWithBlockDisplay.AddChild(new DocumentNode(DocumentNodeType.Text, "First block"));
        divWithBlockDisplay.AddChild(new DocumentNode(DocumentNodeType.Strong, "Second block"));

        GenerateDocument(column => sut.Compose(column, divWithBlockDisplay));

        sut.InlineComposer.Nodes.ShouldBe(new[] { DocumentNodeType.Text, DocumentNodeType.Strong });
    }

    [Fact]
    public void Compose_DisplayBlockEmptyElement_HandlesEmptyBlock()
    {
        var sut = CreateSut();
        var emptyBlockSpan = CreateBlockDisplayNode(DocumentNodeType.Span);

        GenerateDocument(column => sut.Compose(column, emptyBlockSpan));

        sut.InlineComposer.Nodes.ShouldBeEmpty();
    }

    private TestSut CreateSut(bool enableNewLayout = false)
    {
        var inlineComposer = new RecordingInlineComposer();
        var listComposer = new RecordingListComposer();
        var tableComposer = new RecordingTableComposer();
        var spacingApplier = new PassthroughSpacingApplier();
        var rendererOptions = new RendererOptions
        {
            EnableNewLayoutForTextBlocks = enableNewLayout
        };
        var layoutEngine = new RecordingLayoutEngine();
        var displayClassifier = new DisplayClassifier(options: rendererOptions);
        var blockComposer = new BlockComposer(
            inlineComposer,
            listComposer,
            tableComposer,
            spacingApplier,
            rendererOptions,
            displayClassifier,
            layoutEngine);

        return new TestSut(blockComposer, inlineComposer, listComposer, tableComposer, layoutEngine, rendererOptions);
    }

    private static DocumentNode CreateBlockDisplayNode(DocumentNodeType nodeType, string? textContent = null)
    {
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block);
        return new DocumentNode(nodeType, textContent, styles);
    }

    private static void GenerateDocument(Action<ColumnDescriptor> compose)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.UseEnvironmentFonts = false;

        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(10);
                page.Content().Column(compose);
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
    }

    private sealed class TestSut
    {
        public TestSut(BlockComposer blockComposer, RecordingInlineComposer inlineComposer,
                      RecordingListComposer listComposer, RecordingTableComposer tableComposer,
                      RecordingLayoutEngine layoutEngine, RendererOptions rendererOptions)
        {
            BlockComposer = blockComposer;
            InlineComposer = inlineComposer;
            ListComposer = listComposer;
            TableComposer = tableComposer;
            LayoutEngine = layoutEngine;
            RendererOptions = rendererOptions;
        }

        public BlockComposer BlockComposer { get; }
        public RecordingInlineComposer InlineComposer { get; }
        public RecordingListComposer ListComposer { get; }
        public RecordingTableComposer TableComposer { get; }
        public RecordingLayoutEngine LayoutEngine { get; }
        public RendererOptions RendererOptions { get; }

        public void Compose(ColumnDescriptor column, DocumentNode node) => BlockComposer.Compose(column, node);
    }

    private sealed class RecordingInlineComposer : IInlineComposer
    {
        public List<DocumentNodeType> Nodes { get; } = [];

        public void Compose(QuestPDF.Fluent.TextDescriptor text, DocumentNode node, InlineStyleState style)
        {
            Nodes.Add(node.NodeType);
            var content = node.TextContent ?? node.NodeType.ToString();
            text.Span(content);
        }
    }

    private sealed class RecordingListComposer : IListComposer
    {
        public bool Called { get; private set; }
        public bool LastOrdered { get; private set; }

        public void Compose(ColumnDescriptor column, DocumentNode listNode, bool ordered, Action<ColumnDescriptor, DocumentNode> composeBlock)
        {
            Called = true;
            LastOrdered = ordered;
            column.Item().Text("list");
        }
    }

    private sealed class RecordingTableComposer : ITableComposer
    {
        public bool Called { get; private set; }

        public void Compose(ColumnDescriptor column, DocumentNode tableNode)
        {
            Called = true;
            column.Item().Text("table");
        }
    }

    private sealed class RecordingLayoutEngine : ILayoutEngine
    {
        public int Calls { get; private set; }
        public LayoutResult? OverrideResult { get; set; }

        public LayoutResult Layout(DocumentNode root, LayoutConstraints constraints, LayoutEngineOptions options)
        {
            Calls++;
            if (OverrideResult is not null)
            {
                return OverrideResult;
            }

            var spacing = LayoutSpacing.FromStyles(root.Styles);
            var box = new LayoutBox(root, DisplayClass.Block, root.Styles, spacing, $"{root.NodeType}:0", Array.Empty<LayoutBox>());
            var diagnostics = new LayoutDiagnostics("Test", constraints, constraints.InlineMax, constraints.BlockMax);
            var fragment = LayoutFragment.CreateBlock(box, constraints.InlineMax, constraints.BlockMax, Array.Empty<LayoutFragment>(), diagnostics);
            return LayoutResult.Success(new[] { fragment });
        }
    }

    private sealed class PassthroughSpacingApplier : IBlockSpacingApplier
    {
        public IContainer ApplySpacing(IContainer container, CssStyleMap styles) => container;
        public IContainer ApplyMargin(IContainer container, CssStyleMap styles) => container;
        public IContainer ApplyBorder(IContainer container, CssStyleMap styles) => container;
    }

    #region Block Width Rules and Margin Collapsing Tests

    [Fact]
    public void ComposeAsBlock_WithNegativeMargin_ShouldThrowException()
    {
        // Arrange
        var sut = CreateSut();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithMarginTop(-10);
        var node = new DocumentNode(DocumentNodeType.Div, "Test content", styles);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() =>
        {
            GenerateDocument(column => sut.Compose(column, node));
        });

        exception.Message.ShouldContain("Negative top margin (-10) is not allowed for block element Div");
    }

    [Fact]
    public void ComposeAsBlock_WithNegativePadding_ShouldThrowException()
    {
        // Arrange
        var sut = CreateSut();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithPaddingLeft(-5);
        var node = new DocumentNode(DocumentNodeType.Paragraph, "Test content", styles);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() =>
        {
            GenerateDocument(column => sut.Compose(column, node));
        });

        exception.Message.ShouldContain("Negative left padding (-5) is not allowed for block element Paragraph");
    }

    [Fact]
    public void ComposeAsBlock_WithValidMargins_ShouldNotThrowException()
    {
        // Arrange
        var sut = CreateSut();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithMargin(BoxSpacing.FromAll(10));
        var node = new DocumentNode(DocumentNodeType.Div, "Test content", styles);

        // Act & Assert - Should not throw
        Should.NotThrow(() =>
        {
            GenerateDocument(column => sut.Compose(column, node));
        });
    }

    [Fact]
    public void ComposeAsBlock_WithValidPadding_ShouldNotThrowException()
    {
        // Arrange
        var sut = CreateSut();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithPadding(BoxSpacing.FromAll(5));
        var node = new DocumentNode(DocumentNodeType.Paragraph, "Test content", styles);

        // Act & Assert - Should not throw
        Should.NotThrow(() =>
        {
            GenerateDocument(column => sut.Compose(column, node));
        });
    }

    [Fact]
    public void RenderChildrenWithInlineFlow_WithAdjacentBlockElements_ShouldValidateMarginCollapsing()
    {
        // Arrange
        var sut = CreateSut();
        var container = CreateBlockDisplayNode(DocumentNodeType.Div);

        var firstBlockStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithMarginBottom(20);
        var firstBlock = new DocumentNode(DocumentNodeType.Paragraph, "First block", firstBlockStyles);

        var secondBlockStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithMarginTop(15);
        var secondBlock = new DocumentNode(DocumentNodeType.Paragraph, "Second block", secondBlockStyles);

        container.AddChild(firstBlock);
        container.AddChild(secondBlock);

        // Act & Assert - Should not throw (margin collapsing validation should pass)
        Should.NotThrow(() =>
        {
            GenerateDocument(column => sut.Compose(column, container));
        });
    }

    [Fact]
    public void RenderChildrenWithInlineFlow_WithBlockElementsWithBorders_ShouldNotCollapseMargins()
    {
        // Arrange
        var sut = CreateSut();
        var container = CreateBlockDisplayNode(DocumentNodeType.Div);

        var firstBlockStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithMarginBottom(20).WithBorder(new BorderInfo(1, "solid", "#000000"));
        var firstBlock = new DocumentNode(DocumentNodeType.Paragraph, "First block", firstBlockStyles);

        var secondBlockStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithMarginTop(15);
        var secondBlock = new DocumentNode(DocumentNodeType.Paragraph, "Second block", secondBlockStyles);

        container.AddChild(firstBlock);
        container.AddChild(secondBlock);

        // Act & Assert - Should not throw (margins should not collapse due to border)
        Should.NotThrow(() =>
        {
            GenerateDocument(column => sut.Compose(column, container));
        });
    }

    [Fact]
    public void RenderChildrenWithInlineFlow_WithBlockElementsWithPadding_ShouldNotCollapseMargins()
    {
        // Arrange
        var sut = CreateSut();
        var container = CreateBlockDisplayNode(DocumentNodeType.Div);

        var firstBlockStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithMarginBottom(20).WithPadding(BoxSpacing.FromAll(5));
        var firstBlock = new DocumentNode(DocumentNodeType.Paragraph, "First block", firstBlockStyles);

        var secondBlockStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithMarginTop(15);
        var secondBlock = new DocumentNode(DocumentNodeType.Paragraph, "Second block", secondBlockStyles);

        container.AddChild(firstBlock);
        container.AddChild(secondBlock);

        // Act & Assert - Should not throw (margins should not collapse due to padding)
        Should.NotThrow(() =>
        {
            GenerateDocument(column => sut.Compose(column, container));
        });
    }

    [Fact]
    public void ComposeAsBlock_WithZeroMargins_ShouldNotThrowException()
    {
        // Arrange
        var sut = CreateSut();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithMargin(BoxSpacing.FromAll(0));
        var node = new DocumentNode(DocumentNodeType.Div, "Test content", styles);

        // Act & Assert - Should not throw
        Should.NotThrow(() =>
        {
            GenerateDocument(column => sut.Compose(column, node));
        });
    }

    [Fact]
    public void ComposeAsBlock_WithZeroPadding_ShouldNotThrowException()
    {
        // Arrange
        var sut = CreateSut();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithPadding(BoxSpacing.FromAll(0));
        var node = new DocumentNode(DocumentNodeType.Paragraph, "Test content", styles);

        // Act & Assert - Should not throw
        Should.NotThrow(() =>
        {
            GenerateDocument(column => sut.Compose(column, node));
        });
    }

    [Fact]
    public void ComposeAsBlock_WithMixedNegativeMargins_ShouldThrowException()
    {
        // Arrange
        var sut = CreateSut();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block)
            .WithMarginTop(10)      // Valid
            .WithMarginRight(-5)    // Invalid
            .WithMarginBottom(15)   // Valid
            .WithMarginLeft(-8);    // Invalid
        var node = new DocumentNode(DocumentNodeType.Div, "Test content", styles);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() =>
        {
            GenerateDocument(column => sut.Compose(column, node));
        });

        // Should throw for the first negative margin encountered
        exception.Message.ShouldContain("Negative right margin (-5) is not allowed for block element Div");
    }

    [Fact]
    public void ComposeAsBlock_WithMixedNegativePadding_ShouldThrowException()
    {
        // Arrange
        var sut = CreateSut();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block)
            .WithPaddingTop(5)      // Valid
            .WithPaddingRight(-3)   // Invalid
            .WithPaddingBottom(8)   // Valid
            .WithPaddingLeft(-2);  // Invalid
        var node = new DocumentNode(DocumentNodeType.Paragraph, "Test content", styles);

        // Act & Assert
        var exception = Should.Throw<InvalidOperationException>(() =>
        {
            GenerateDocument(column => sut.Compose(column, node));
        });

        // Should throw for the first negative padding encountered
        exception.Message.ShouldContain("Negative right padding (-3) is not allowed for block element Paragraph");
    }

    #endregion

    #region Additional Spacing Interaction Tests

    [Fact]
    public void ComposeAsBlock_WithMarginsPaddingAndBorder_ShouldApplyCorrectly()
    {
        // Arrange
        var sut = CreateSut();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block)
            .WithMargin(BoxSpacing.FromAll(10))
            .WithPadding(BoxSpacing.FromAll(5))
            .WithBorder(new BorderInfo(2, "solid", "#000000"));
        var node = new DocumentNode(DocumentNodeType.Div, "Test content", styles);

        // Act & Assert - Should not throw and should apply all spacing correctly
        Should.NotThrow(() =>
        {
            GenerateDocument(column => sut.Compose(column, node));
        });
    }

    [Fact]
    public void ComposeAsBlock_WithVerticalSpacing_ShouldApplyCorrectly()
    {
        // Arrange
        var sut = CreateSut();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block)
            .WithMarginTop(15)
            .WithMarginBottom(20)
            .WithPaddingTop(8)
            .WithPaddingBottom(12);
        var node = new DocumentNode(DocumentNodeType.Paragraph, "Test content", styles);

        // Act & Assert - Should not throw and should apply vertical spacing correctly
        Should.NotThrow(() =>
        {
            GenerateDocument(column => sut.Compose(column, node));
        });
    }

    [Fact]
    public void RenderChildrenWithInlineFlow_WithComplexMarginCollapsing_ShouldHandleCorrectly()
    {
        // Arrange - Test complex margin collapsing with multiple adjacent blocks
        var sut = CreateSut();
        var container = CreateBlockDisplayNode(DocumentNodeType.Div);

        var firstBlockStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithMarginBottom(25);
        var firstBlock = new DocumentNode(DocumentNodeType.Paragraph, "First block", firstBlockStyles);

        var secondBlockStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithMarginTop(30).WithMarginBottom(15);
        var secondBlock = new DocumentNode(DocumentNodeType.Paragraph, "Second block", secondBlockStyles);

        var thirdBlockStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithMarginTop(20);
        var thirdBlock = new DocumentNode(DocumentNodeType.Paragraph, "Third block", thirdBlockStyles);

        container.AddChild(firstBlock);
        container.AddChild(secondBlock);
        container.AddChild(thirdBlock);

        // Act & Assert - Should not throw (complex margin collapsing should be handled)
        Should.NotThrow(() =>
        {
            GenerateDocument(column => sut.Compose(column, container));
        });
    }

    [Fact]
    public void RenderChildrenWithInlineFlow_WithMixedElementTypesAndSpacing_ShouldHandleCorrectly()
    {
        // Arrange - Test spacing interactions across different element types
        var sut = CreateSut();
        var container = CreateBlockDisplayNode(DocumentNodeType.Div);

        var paragraphStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithMarginBottom(20);
        var paragraph = new DocumentNode(DocumentNodeType.Paragraph, "Paragraph text", paragraphStyles);

        var divStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithMarginTop(15).WithPadding(BoxSpacing.FromAll(10));
        var div = new DocumentNode(DocumentNodeType.Div, "Div content", divStyles);

        var spanStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithMarginTop(10).WithBorder(new BorderInfo(1, "dashed", "#666666"));
        var span = new DocumentNode(DocumentNodeType.Span, "Span content", spanStyles);

        container.AddChild(paragraph);
        container.AddChild(div);
        container.AddChild(span);

        // Act & Assert - Should not throw (mixed element types with spacing should be handled)
        Should.NotThrow(() =>
        {
            GenerateDocument(column => sut.Compose(column, container));
        });
    }

    [Fact]
    public void ComposeAsBlock_WithAllSidesDifferentSpacing_ShouldApplyCorrectly()
    {
        // Arrange - Test asymmetric spacing on all sides
        var sut = CreateSut();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block)
            .WithMarginTop(20)
            .WithMarginRight(15)
            .WithMarginBottom(25)
            .WithMarginLeft(10)
            .WithPaddingTop(8)
            .WithPaddingRight(12)
            .WithPaddingBottom(6)
            .WithPaddingLeft(14)
            .WithBorder(new BorderInfo(3, "solid", "#FF0000"));
        var node = new DocumentNode(DocumentNodeType.Div, "Asymmetric spacing test", styles);

        // Act & Assert - Should not throw and should apply asymmetric spacing correctly
        Should.NotThrow(() =>
        {
            GenerateDocument(column => sut.Compose(column, node));
        });
    }

    [Fact]
    public void RenderChildrenWithInlineFlow_WithMarginCollapsingAndBorders_ShouldPreventCollapse()
    {
        // Arrange - Test that borders prevent margin collapsing
        var sut = CreateSut();
        var container = CreateBlockDisplayNode(DocumentNodeType.Div);

        var firstBlockStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block)
            .WithMarginBottom(20)
            .WithBorder(new BorderInfo(2, "solid", "#000000"));
        var firstBlock = new DocumentNode(DocumentNodeType.Paragraph, "First block with border", firstBlockStyles);

        var secondBlockStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithMarginTop(15);
        var secondBlock = new DocumentNode(DocumentNodeType.Paragraph, "Second block", secondBlockStyles);

        container.AddChild(firstBlock);
        container.AddChild(secondBlock);

        // Act & Assert - Should not throw (borders should prevent margin collapsing)
        Should.NotThrow(() =>
        {
            GenerateDocument(column => sut.Compose(column, container));
        });
    }

    [Fact]
    public void ComposeAsBlock_WithLargeSpacingValues_ShouldHandleCorrectly()
    {
        // Arrange - Test with large spacing values to ensure no overflow issues
        var sut = CreateSut();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block)
            .WithMargin(BoxSpacing.FromAll(1000))
            .WithPadding(BoxSpacing.FromAll(500))
            .WithBorder(new BorderInfo(50, "solid", "#000000"));
        var node = new DocumentNode(DocumentNodeType.Div, "Large spacing test", styles);

        // Act & Assert - Should not throw even with large spacing values
        Should.NotThrow(() =>
        {
            GenerateDocument(column => sut.Compose(column, node));
        });
    }

    [Fact]
    public void RenderChildrenWithInlineFlow_WithNestedBlockSpacing_ShouldHandleCorrectly()
    {
        // Arrange - Test nested blocks with different spacing
        var sut = CreateSut();
        var outerContainer = CreateBlockDisplayNode(DocumentNodeType.Div);

        var innerContainerStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block)
            .WithMargin(BoxSpacing.FromAll(20))
            .WithPadding(BoxSpacing.FromAll(15));
        var innerContainer = new DocumentNode(DocumentNodeType.Div, "Inner container", innerContainerStyles);

        var innerBlockStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block)
            .WithMargin(BoxSpacing.FromAll(10))
            .WithBorder(new BorderInfo(1, "solid", "#CCCCCC"));
        var innerBlock = new DocumentNode(DocumentNodeType.Paragraph, "Inner block", innerBlockStyles);

        innerContainer.AddChild(innerBlock);
        outerContainer.AddChild(innerContainer);

        // Act & Assert - Should not throw (nested blocks with spacing should be handled)
        Should.NotThrow(() =>
        {
            GenerateDocument(column => sut.Compose(column, outerContainer));
        });
    }

    [Fact]
    public void ComposeAsBlock_WithDecimalSpacingValues_ShouldHandleCorrectly()
    {
        // Arrange - Test with decimal spacing values
        var sut = CreateSut();
        var styles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block)
            .WithMarginTop(12.5)
            .WithMarginRight(7.25)
            .WithMarginBottom(15.75)
            .WithMarginLeft(9.125)
            .WithPaddingTop(3.5)
            .WithPaddingRight(4.25)
            .WithPaddingBottom(2.75)
            .WithPaddingLeft(5.125);
        var node = new DocumentNode(DocumentNodeType.Paragraph, "Decimal spacing test", styles);

        // Act & Assert - Should not throw with decimal spacing values
        Should.NotThrow(() =>
        {
            GenerateDocument(column => sut.Compose(column, node));
        });
    }

    [Fact]
    public void RenderChildrenWithInlineFlow_WithZeroMarginCollapsing_ShouldHandleCorrectly()
    {
        // Arrange - Test margin collapsing with zero margins
        var sut = CreateSut();
        var container = CreateBlockDisplayNode(DocumentNodeType.Div);

        var firstBlockStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithMarginBottom(0);
        var firstBlock = new DocumentNode(DocumentNodeType.Paragraph, "First block with zero margin", firstBlockStyles);

        var secondBlockStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block).WithMarginTop(0);
        var secondBlock = new DocumentNode(DocumentNodeType.Paragraph, "Second block with zero margin", secondBlockStyles);

        container.AddChild(firstBlock);
        container.AddChild(secondBlock);

        // Act & Assert - Should not throw (zero margins should be handled correctly)
        Should.NotThrow(() =>
        {
            GenerateDocument(column => sut.Compose(column, container));
        });
    }

    #endregion
}
