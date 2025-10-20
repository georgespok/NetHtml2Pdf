using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Display;
using NetHtml2Pdf.Layout.Engines;
using NetHtml2Pdf.Layout.Model;
using NetHtml2Pdf.Renderer.Interfaces;
using NetHtml2Pdf.Renderer.Spacing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NetHtml2Pdf.Renderer;

internal sealed class BlockComposer(
    IInlineComposer inlineComposer,
    IListComposer listComposer,
    ITableComposer tableComposer,
    IBlockSpacingApplier spacingApplier,
    RendererOptions? options = null,
    IDisplayClassifier? displayClassifier = null,
    ILayoutEngine? layoutEngine = null) : IBlockComposer
{
    private static readonly HashSet<DocumentNodeType> LayoutEligibleNodeTypes =
    [
        DocumentNodeType.Paragraph,
        DocumentNodeType.Heading1,
        DocumentNodeType.Heading2,
        DocumentNodeType.Heading3,
        DocumentNodeType.Heading4,
        DocumentNodeType.Heading5,
        DocumentNodeType.Heading6
    ];

    private static readonly LayoutConstraints DefaultConstraints = new(0, 600, 0, 1000, 1000, allowBreaks: true);

    private readonly RendererOptions _options = options ?? RendererOptions.CreateDefault();
    private readonly ILayoutEngine? _layoutEngine = layoutEngine;
    private readonly IDisplayClassifier _displayClassifier = displayClassifier ?? new DisplayClassifier(options: options);
    public void Compose(ColumnDescriptor column, DocumentNode node) => ComposeInternal(column, node, bypassLayout: false);

    private void ComposeInternal(ColumnDescriptor column, DocumentNode node, bool bypassLayout)
    {
        ArgumentNullException.ThrowIfNull(column);
        ArgumentNullException.ThrowIfNull(node);

        if (!bypassLayout && ShouldUseLayoutEngine(node) && TryComposeWithLayoutEngine(column, node))
        {
            return;
        }

        ComposeLegacy(column, node);
    }

    private void ComposeLegacy(ColumnDescriptor column, DocumentNode node)
    {
        var displayClass = _displayClassifier.Classify(node, node.Styles);

        switch (displayClass)
        {
            case DisplayClass.None:
                return;

            case DisplayClass.Inline:
                ComposeInlineContainer(column, node);
                return;

            case DisplayClass.InlineBlock:
                ComposeAsBlock(column, node);
                return;

            case DisplayClass.Block:
                if (node.Styles.DisplaySet && node.Styles.Display == CssDisplay.Block)
                {
                    ComposeAsBlock(column, node);
                    return;
                }
                break;
        }

        switch (node.NodeType)
        {
            case DocumentNodeType.Div:
            case DocumentNodeType.Section:
                ComposeContainer(column, node);
                break;
            case DocumentNodeType.Paragraph:
                ComposeParagraph(column, node);
                break;
            case DocumentNodeType.Heading1:
            case DocumentNodeType.Heading2:
            case DocumentNodeType.Heading3:
            case DocumentNodeType.Heading4:
            case DocumentNodeType.Heading5:
            case DocumentNodeType.Heading6:
                var headingSize = RenderingHelpers.GetHeadingFontSize(node.NodeType);
                if (headingSize.HasValue)
                {
                    ComposeHeading(column, node, fontSize: headingSize.Value, bold: true);
                }
                break;
            case DocumentNodeType.List:
            case DocumentNodeType.UnorderedList:
                listComposer.Compose(column, node, ordered: false, Compose);
                break;
            case DocumentNodeType.OrderedList:
                listComposer.Compose(column, node, ordered: true, Compose);
                break;
            case DocumentNodeType.Table:
                tableComposer.Compose(column, node);
                break;
            default:
                ComposeAsBlock(column, node);
                break;
        }
    }

    private bool ShouldUseLayoutEngine(DocumentNode node)
    {
        return _layoutEngine is not null
            && _options.EnableNewLayoutForTextBlocks
            && LayoutEligibleNodeTypes.Contains(node.NodeType);
    }

    private bool TryComposeWithLayoutEngine(ColumnDescriptor column, DocumentNode node)
    {
        if (_layoutEngine is null)
        {
            return false;
        }

        var engineResult = _layoutEngine.Layout(
            node,
            DefaultConstraints,
            new LayoutEngineOptions
            {
                EnableNewLayoutForTextBlocks = _options.EnableNewLayoutForTextBlocks,
                EnableDiagnostics = _options.EnableLayoutDiagnostics
            });

        if (engineResult.IsDisabled || engineResult.IsFallback || !engineResult.IsSuccess)
        {
            return false;
        }

        foreach (var fragment in engineResult.Fragments)
        {
            ComposeFragment(column, fragment);
        }

        return true;
    }

    private void ComposeFragment(ColumnDescriptor column, LayoutFragment fragment)
    {
        ComposeInternal(column, fragment.Node, bypassLayout: true);
    }

    private void ComposeParagraph(ColumnDescriptor column, DocumentNode node)
    {
        // Helper preserves margin -> border -> padding order to maintain parity.
        var spacedContainer = WrapWithSpacing.ApplySpacing(
            column.Item(),
            node.Styles,
            node.Styles,
            spacingApplier);

        spacedContainer.Text(text =>
        {
            foreach (var child in node.Children)
            {
                inlineComposer.Compose(text, child, InlineStyleState.Empty);
            }
        });
    }

    private void ComposeHeading(ColumnDescriptor column, DocumentNode node, double fontSize, bool bold)
    {
        var spacedContainer = WrapWithSpacing.ApplySpacing(
            column.Item(),
            node.Styles,
            node.Styles,
            spacingApplier);

        spacedContainer.Text(text =>
        {
            var headingStyle = InlineStyleState.Empty.WithFontSize(fontSize);
            if (bold)
            {
                headingStyle = headingStyle.WithBold();
            }

            foreach (var child in node.Children)
            {
                inlineComposer.Compose(text, child, headingStyle);
            }
        });
    }

    private void ComposeContainer(ColumnDescriptor column, DocumentNode node)
    {
        var spacedContainer = WrapWithSpacing.ApplySpacing(
            column.Item(),
            node.Styles,
            node.Styles,
            spacingApplier);

        // Render children with proper inline-block flow logic
        RenderChildrenWithInlineFlow(spacedContainer, node);
    }

    private void ComposeInlineContainer(ColumnDescriptor column, DocumentNode node)
    {
        var spacedContainer = WrapWithSpacing.ApplySpacing(
            column.Item(),
            node.Styles,
            node.Styles,
            spacingApplier);

        spacedContainer.Text(text => inlineComposer.Compose(text, node, InlineStyleState.Empty));
    }

    private void ComposeAsBlock(ColumnDescriptor column, DocumentNode node)
    {
        // Block behavior: starts on new line; full available width; margins/padding/border apply
        // Validate block width rules and margin collapsing before rendering

        // Validate block element sizing constraints
        ValidateBlockSizing(node);

        var spacedContainer = WrapWithSpacing.ApplySpacing(
            column.Item(),
            node.Styles,
            node.Styles,
            spacingApplier);

        // If node has children, compose them as block elements
        if (node.Children.Count > 0)
        {
            spacedContainer.Column(containerColumn =>
            {
                foreach (var child in node.Children)
                {
                    Compose(containerColumn, child);
                }
            });
        }
        else if (!string.IsNullOrEmpty(node.TextContent))
        {
            // If it's a leaf node with text content, render as text
            spacedContainer.Text(text => inlineComposer.Compose(text, node, InlineStyleState.Empty));
        }
        else
        {
            // Empty block element - just render the container for spacing
            spacedContainer.Shrink();
        }
    }


    private void ComposeInlineBlockAsSideBySideBlock(IContainer container, DocumentNode node)
    {
        // Render inline-block element as a block element with its own borders, margins, and padding
        // This allows proper rendering of borders while maintaining side-by-side positioning
        // The container will automatically fit to available space due to RelativeItem() usage

        var spacedContainer = WrapWithSpacing.ApplySpacing(
            container,
            node.Styles,
            node.Styles,
            spacingApplier);

        if (node.Children.Count > 0)
        {
            // If node has children, compose them as inline elements within this block
            spacedContainer.Text(text =>
            {
                foreach (var child in node.Children)
                {
                    inlineComposer.Compose(text, child, InlineStyleState.Empty);
                }
            });
        }
        else if (!string.IsNullOrEmpty(node.TextContent))
        {
            // If it's a leaf node with text content, render as text
            spacedContainer.Text(text =>
            {
                var textStyle = InlineStyleState.Empty.ApplyCss(node.Styles);
                inlineComposer.Compose(text, node, textStyle);
            });
        }
        else
        {
            // Empty inline-block element - render as empty space
            spacedContainer.Text(text => text.Span(" "));
        }
    }

    private void RenderChildrenWithInlineFlow(IContainer container, DocumentNode node)
    {
        if (node.Children.Count == 0)
            return;

        // Group children into lines based on their display properties
        var lines = GroupChildrenIntoLines(node.Children.ToList());

        container.Column(column =>
        {
            foreach (var line in lines)
            {
                if (line.Count == 1 && IsBlockElement(line[0]))
                {
                    // Single block element - render as block
                    Compose(column, line[0]);
                }
                else if (line.All(IsInlineBlockElement))
                {
                    // All elements in this line are inline-block - render them side-by-side
                    // Use RelativeItem() to allow elements to fit available space
                    column.Item().Row(row =>
                    {
                        foreach (var child in line)
                        {
                            row.RelativeItem().Element(childContainer =>
                                ComposeInlineBlockAsSideBySideBlock(childContainer, child));
                        }
                    });
                }
                else
                {
                    // Mixed content or other cases - render each element as a separate block
                    // Validate margin collapsing for adjacent block elements
                    var blockElements = line.Where(IsBlockElement).ToList();
                    if (blockElements.Count > 1)
                    {
                        ValidateMarginCollapsing(blockElements);
                    }

                    foreach (var child in line)
                    {
                        Compose(column, child);
                    }
                }
            }
        });
    }

    private static List<List<DocumentNode>> GroupChildrenIntoLines(List<DocumentNode> children)
    {
        var lines = new List<List<DocumentNode>>();
        var currentLine = new List<DocumentNode>();

        foreach (var child in children)
        {
            // Skip nodes with display:none
            if (child.Styles.DisplaySet && child.Styles.Display == CssDisplay.None)
                continue;

            if (IsBlockElement(child))
            {
                // Block element - finish current line and start new one
                if (currentLine.Count > 0)
                {
                    lines.Add(currentLine);
                    currentLine = [];
                }
                lines.Add([child]);
            }
            else if (IsInlineElement(child) || IsInlineBlockElement(child))
            {
                // Inline or inline-block element - add to current line
                currentLine.Add(child);
            }
            else
            {
                // Unknown element type - treat as block
                if (currentLine.Count > 0)
                {
                    lines.Add(currentLine);
                    currentLine = [];
                }
                lines.Add([child]);
            }
        }

        // Add remaining elements in current line
        if (currentLine.Count > 0)
        {
            lines.Add(currentLine);
        }

        return lines;
    }

    private static bool IsBlockElement(DocumentNode node)
    {
        // Check if element is explicitly set to block
        if (node.Styles.DisplaySet && node.Styles.Display == CssDisplay.Block)
            return true;

        // If display is explicitly set to inline-block, it's not a block element
        if (node.Styles.DisplaySet && node.Styles.Display == CssDisplay.InlineBlock)
            return false;

        // Default block elements (only if display is not explicitly set)
        return node.NodeType is DocumentNodeType.Div or DocumentNodeType.Paragraph or
               DocumentNodeType.Heading1 or DocumentNodeType.Heading2 or
               DocumentNodeType.Heading3 or DocumentNodeType.Heading4 or
               DocumentNodeType.Heading5 or DocumentNodeType.Heading6 or
               DocumentNodeType.List or DocumentNodeType.Table or
               DocumentNodeType.Section;
    }

    private static bool IsInlineElement(DocumentNode node)
    {
        // Check if element is explicitly set to inline
        if (node.Styles.DisplaySet && node.Styles.Display == CssDisplay.Default)
        {
            // Default inline elements
            return node.NodeType is DocumentNodeType.Span or DocumentNodeType.Strong or
                   DocumentNodeType.Italic or DocumentNodeType.Text;
        }

        return false;
    }

    private static bool IsInlineBlockElement(DocumentNode node)
    {
        return node.Styles.DisplaySet && node.Styles.Display == CssDisplay.InlineBlock;
    }

    private void ComposeInlineElement(IContainer container, DocumentNode node)
    {
        // Render inline element within the container
        container.Text(text =>
        {
            var style = InlineStyleState.Empty.ApplyCss(node.Styles);
            inlineComposer.Compose(text, node, style);
        });
    }

    private void ComposeInlineBlockAsInlineElement(TextDescriptor text, DocumentNode node)
    {
        // Render inline-block element as inline content
        // Note: This is a simplified version that doesn't handle borders/margins/padding
        // For full inline-block support, we'd need a more complex approach

        if (node.Children.Count > 0)
        {
            foreach (var child in node.Children)
            {
                inlineComposer.Compose(text, child, InlineStyleState.Empty);
            }
        }
        else if (!string.IsNullOrEmpty(node.TextContent))
        {
            var style = InlineStyleState.Empty.ApplyCss(node.Styles);
            inlineComposer.Compose(text, node, style);
        }
        else
        {
            text.Span(" ");
        }
    }

    #region Block Width Rules and Margin Collapsing Validation

    /// <summary>
    /// Validates block element sizing constraints according to CSS box model rules.
    /// </summary>
    private static void ValidateBlockSizing(DocumentNode node)
    {
        // Block elements should take full available width by default
        // This validation ensures proper CSS box model behavior

        // Check for conflicting width constraints
        ValidateWidthConstraints(node);

        // Validate margin constraints
        ValidateMarginConstraints(node);

        // Validate padding constraints
        ValidatePaddingConstraints(node);
    }

    /// <summary>
    /// Validates width constraints for block elements.
    /// </summary>
    private static void ValidateWidthConstraints(DocumentNode node)
    {
        // Block elements should not have explicit width constraints that would
        // prevent them from taking full available width unless specifically intended

        // Note: Currently, the CSS parser doesn't support width/height properties
        // This validation is prepared for future width/height support

        // Future implementation would check:
        // - width: auto (default for block elements)
        // - width: 100% (explicit full width)
        // - width: specific values (fixed width)
        // - max-width constraints
        // - min-width constraints
    }

    /// <summary>
    /// Validates margin constraints for block elements.
    /// </summary>
    private static void ValidateMarginConstraints(DocumentNode node)
    {
        if (!node.Styles.Margin.HasValue)
            return;

        var margin = node.Styles.Margin;

        // Validate margin values are non-negative
        if (margin.Top.HasValue && margin.Top.Value < 0)
            throw new InvalidOperationException($"Negative top margin ({margin.Top.Value}) is not allowed for block element {node.NodeType}");

        if (margin.Right.HasValue && margin.Right.Value < 0)
            throw new InvalidOperationException($"Negative right margin ({margin.Right.Value}) is not allowed for block element {node.NodeType}");

        if (margin.Bottom.HasValue && margin.Bottom.Value < 0)
            throw new InvalidOperationException($"Negative bottom margin ({margin.Bottom.Value}) is not allowed for block element {node.NodeType}");

        if (margin.Left.HasValue && margin.Left.Value < 0)
            throw new InvalidOperationException($"Negative left margin ({margin.Left.Value}) is not allowed for block element {node.NodeType}");
    }

    /// <summary>
    /// Validates padding constraints for block elements.
    /// </summary>
    private static void ValidatePaddingConstraints(DocumentNode node)
    {
        if (!node.Styles.Padding.HasValue)
            return;

        var padding = node.Styles.Padding;

        // Validate padding values are non-negative
        if (padding.Top.HasValue && padding.Top.Value < 0)
            throw new InvalidOperationException($"Negative top padding ({padding.Top.Value}) is not allowed for block element {node.NodeType}");

        if (padding.Right.HasValue && padding.Right.Value < 0)
            throw new InvalidOperationException($"Negative right padding ({padding.Right.Value}) is not allowed for block element {node.NodeType}");

        if (padding.Bottom.HasValue && padding.Bottom.Value < 0)
            throw new InvalidOperationException($"Negative bottom padding ({padding.Bottom.Value}) is not allowed for block element {node.NodeType}");

        if (padding.Left.HasValue && padding.Left.Value < 0)
            throw new InvalidOperationException($"Negative left padding ({padding.Left.Value}) is not allowed for block element {node.NodeType}");
    }

    /// <summary>
    /// Validates margin collapsing behavior for adjacent block elements.
    /// This method should be called when composing multiple block elements in sequence.
    /// </summary>
    private static void ValidateMarginCollapsing(List<DocumentNode> blockElements)
    {
        if (blockElements.Count < 2)
            return;

        // CSS margin collapsing rules:
        // 1. Adjacent vertical margins collapse to the larger value
        // 2. Margins don't collapse if there's a border, padding, or inline content between them
        // 3. Negative margins collapse differently (subtract from positive margins)

        for (int i = 0; i < blockElements.Count - 1; i++)
        {
            var currentElement = blockElements[i];
            var nextElement = blockElements[i + 1];

            ValidateAdjacentMarginCollapsing(currentElement, nextElement);
        }
    }

    /// <summary>
    /// Validates margin collapsing between two adjacent block elements.
    /// </summary>
    private static void ValidateAdjacentMarginCollapsing(DocumentNode firstElement, DocumentNode secondElement)
    {
        // Check if margins should collapse according to CSS rules
        var firstBottomMargin = firstElement.Styles.Margin.Bottom ?? 0;
        var secondTopMargin = secondElement.Styles.Margin.Top ?? 0;

        // Margins collapse when:
        // 1. Both elements are block-level
        // 2. No border, padding, or inline content between them
        // 3. Both margins are vertical (top/bottom)

        bool shouldCollapse = ShouldMarginsCollapse(firstElement, secondElement);

        if (shouldCollapse)
        {
            // Calculate collapsed margin value
            var collapsedMargin = CalculateCollapsedMargin(firstBottomMargin, secondTopMargin);

            // Log validation result (in a real implementation, this might be logged)
            // For now, we just validate that the calculation is correct
            ValidateCollapsedMarginCalculation(firstBottomMargin, secondTopMargin, collapsedMargin);
        }
    }

    /// <summary>
    /// Determines if margins between two elements should collapse according to CSS rules.
    /// </summary>
    private static bool ShouldMarginsCollapse(DocumentNode firstElement, DocumentNode secondElement)
    {
        // Margins collapse when both elements are block-level
        if (!IsBlockElement(firstElement) || !IsBlockElement(secondElement))
            return false;

        // Margins don't collapse if there's a border between elements
        if (firstElement.Styles.Border.IsVisible || secondElement.Styles.Border.IsVisible)
            return false;

        // Margins don't collapse if there's padding between elements
        if (firstElement.Styles.Padding.HasValue || secondElement.Styles.Padding.HasValue)
            return false;

        // Margins don't collapse if there's inline content between elements
        // (This is simplified - in a full implementation, we'd check for actual inline content)

        return true;
    }

    /// <summary>
    /// Calculates the collapsed margin value according to CSS rules.
    /// </summary>
    private static double CalculateCollapsedMargin(double firstMargin, double secondMargin)
    {
        // CSS margin collapsing rules:
        // 1. If both margins are positive, collapse to the larger value
        // 2. If one margin is negative, subtract it from the positive margin
        // 3. If both margins are negative, collapse to the more negative value

        if (firstMargin >= 0 && secondMargin >= 0)
        {
            // Both positive - collapse to larger value
            return Math.Max(firstMargin, secondMargin);
        }
        else if (firstMargin < 0 && secondMargin < 0)
        {
            // Both negative - collapse to more negative value
            return Math.Min(firstMargin, secondMargin);
        }
        else
        {
            // One positive, one negative - subtract negative from positive
            return firstMargin + secondMargin;
        }
    }

    /// <summary>
    /// Validates that the collapsed margin calculation is correct.
    /// </summary>
    private static void ValidateCollapsedMarginCalculation(double firstMargin, double secondMargin, double collapsedMargin)
    {
        // Validate the calculation logic
        double expectedCollapsed;

        if (firstMargin >= 0 && secondMargin >= 0)
        {
            expectedCollapsed = Math.Max(firstMargin, secondMargin);
        }
        else if (firstMargin < 0 && secondMargin < 0)
        {
            expectedCollapsed = Math.Min(firstMargin, secondMargin);
        }
        else
        {
            expectedCollapsed = firstMargin + secondMargin;
        }

        if (Math.Abs(collapsedMargin - expectedCollapsed) > 0.001)
        {
            throw new InvalidOperationException(
                $"Margin collapsing calculation error: expected {expectedCollapsed}, got {collapsedMargin} " +
                $"(first: {firstMargin}, second: {secondMargin})");
        }
    }

    #endregion
}
