using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using QuestPDF.Fluent;

namespace NetHtml2Pdf.Renderer.Inline;

/// <summary>
///     Internal engine for processing inline flow layout.
///     Extracts traversal and validation logic from InlineComposer while preserving current behavior.
/// </summary>
internal sealed class InlineFlowLayoutEngine
{
    /// <summary>
    ///     Processes inline content by traversing nodes and applying style inheritance.
    ///     Moves traversal/validation logic from InlineComposer without changing behavior.
    /// </summary>
    /// <param name="textDescriptor">The QuestPDF TextDescriptor for rendering</param>
    /// <param name="node">The document node to process</param>
    /// <param name="style">The current inline style state</param>
    public void ProcessInlineContent(TextDescriptor textDescriptor, DocumentNode node, InlineStyleState style)
    {
        ArgumentNullException.ThrowIfNull(textDescriptor);
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(style);

        // Skip rendering for nodes with display:none
        if (node.Styles.DisplaySet && node.Styles.Display == CssDisplay.None)
            return;

        var currentStyle = style.ApplyCss(node.Styles);

        // Check CSS display property first - it overrides HTML element semantics
        if (node.Styles.DisplaySet && node.Styles.Display == CssDisplay.InlineBlock)
        {
            ProcessInlineBlock(textDescriptor, node, currentStyle);
            return;
        }

        switch (node.NodeType)
        {
            case DocumentNodeType.Text:
                ProcessTextNode(textDescriptor, node, currentStyle);
                break;
            case DocumentNodeType.LineBreak:
                textDescriptor.EmptyLine();
                break;
            case DocumentNodeType.Strong:
            case DocumentNodeType.Bold:
                var boldStyle = currentStyle.WithBold();
                if (node.Children.Count > 0)
                    foreach (var child in node.Children)
                    {
                        if (child.Styles.DisplaySet && child.Styles.Display == CssDisplay.None)
                            continue;
                        ProcessInlineContent(textDescriptor, child, boldStyle);
                    }
                else if (!string.IsNullOrEmpty(node.TextContent))
                    // If no children but has text content, process as text with bold style
                    ProcessTextNode(textDescriptor, node, boldStyle);

                break;
            case DocumentNodeType.Italic:
                var italicStyle = currentStyle.WithItalic();
                if (node.Children.Count > 0)
                    foreach (var child in node.Children)
                    {
                        if (child.Styles.DisplaySet && child.Styles.Display == CssDisplay.None)
                            continue;
                        ProcessInlineContent(textDescriptor, child, italicStyle);
                    }
                else if (!string.IsNullOrEmpty(node.TextContent))
                    // If no children but has text content, process as text with italic style
                    ProcessTextNode(textDescriptor, node, italicStyle);

                break;
            default:
                foreach (var child in node.Children)
                {
                    if (child.Styles.DisplaySet && child.Styles.Display == CssDisplay.None)
                        continue;
                    ProcessInlineContent(textDescriptor, child, currentStyle);
                }

                break;
        }
    }

    /// <summary>
    ///     Processes a text node by applying the current style to the text content.
    /// </summary>
    private static void ProcessTextNode(TextDescriptor textDescriptor, DocumentNode node, InlineStyleState style)
    {
        var span = textDescriptor.Span(node.TextContent ?? string.Empty);

        if (style.Bold) span.SemiBold();

        if (style.Italic) span.Italic();

        if (style.Underline) span.Underline();

        if (style.FontSize.HasValue) span.FontSize((float)style.FontSize.Value);

        if (!string.IsNullOrEmpty(style.Color)) span.FontColor(style.Color);

        if (!string.IsNullOrEmpty(style.BackgroundColor)) span.BackgroundColor(style.BackgroundColor);
    }

    /// <summary>
    ///     Processes inline-block nodes using simplified validation checks.
    ///     Keeps the current simplified inline-block handling behavior.
    /// </summary>
    private void ProcessInlineBlock(TextDescriptor textDescriptor, DocumentNode node, InlineStyleState style)
    {
        // Simplified inline-block processing (current behavior)
        // Validate inline-block sizing and wrapping constraints before rendering
        ValidateInlineBlockSizing(node);
        ValidateInlineBlockWrapping(node);

        if (node.Children.Count > 0)
            // If node has children, compose them as inline elements
            foreach (var child in node.Children)
            {
                if (child.Styles.DisplaySet && child.Styles.Display == CssDisplay.None)
                    continue;
                ProcessInlineContent(textDescriptor, child, style);
            }
        else if (!string.IsNullOrEmpty(node.TextContent))
            // If it's a leaf node with text content, render as text
            ProcessTextNode(textDescriptor, node, style);
        else
            // Empty inline-block element - render as empty space
            textDescriptor.Span(" ");
    }

    #region Inline-Block Validation (Simplified)

    /// <summary>
    ///     Validates inline-block element sizing constraints according to CSS box model rules.
    ///     Keeps simplified inline-block checks as per current behavior.
    /// </summary>
    private static void ValidateInlineBlockSizing(DocumentNode node)
    {
        // Simplified validation - check for conflicting sizing constraints
        ValidateInlineBlockWidthConstraints(node);
        ValidateInlineBlockMarginConstraints(node);
        ValidateInlineBlockPaddingConstraints(node);
        ValidateInlineBlockBorderConstraints(node);
    }

    /// <summary>
    ///     Validates width constraints for inline-block elements.
    /// </summary>
    private static void ValidateInlineBlockWidthConstraints(DocumentNode node)
    {
        // Inline-block elements can have explicit width constraints
        // Currently simplified - prepared for future width/height support
    }

    /// <summary>
    ///     Validates margin constraints for inline-block elements.
    /// </summary>
    private static void ValidateInlineBlockMarginConstraints(DocumentNode node)
    {
        if (!node.Styles.Margin.HasValue)
            return;

        var margin = node.Styles.Margin;

        // Validate margin values are non-negative
        if (margin.Top.HasValue && margin.Top.Value < 0)
            throw new InvalidOperationException(
                $"Negative top margin ({margin.Top.Value}) is not allowed for inline-block element {node.NodeType}");

        if (margin.Right.HasValue && margin.Right.Value < 0)
            throw new InvalidOperationException(
                $"Negative right margin ({margin.Right.Value}) is not allowed for inline-block element {node.NodeType}");

        if (margin.Bottom.HasValue && margin.Bottom.Value < 0)
            throw new InvalidOperationException(
                $"Negative bottom margin ({margin.Bottom.Value}) is not allowed for inline-block element {node.NodeType}");

        if (margin.Left.HasValue && margin.Left.Value < 0)
            throw new InvalidOperationException(
                $"Negative left margin ({margin.Left.Value}) is not allowed for inline-block element {node.NodeType}");
    }

    /// <summary>
    ///     Validates padding constraints for inline-block elements.
    /// </summary>
    private static void ValidateInlineBlockPaddingConstraints(DocumentNode node)
    {
        if (!node.Styles.Padding.HasValue)
            return;

        var padding = node.Styles.Padding;

        // Validate padding values are non-negative
        if (padding.Top.HasValue && padding.Top.Value < 0)
            throw new InvalidOperationException(
                $"Negative top padding ({padding.Top.Value}) is not allowed for inline-block element {node.NodeType}");

        if (padding.Right.HasValue && padding.Right.Value < 0)
            throw new InvalidOperationException(
                $"Negative right padding ({padding.Right.Value}) is not allowed for inline-block element {node.NodeType}");

        if (padding.Bottom.HasValue && padding.Bottom.Value < 0)
            throw new InvalidOperationException(
                $"Negative bottom padding ({padding.Bottom.Value}) is not allowed for inline-block element {node.NodeType}");

        if (padding.Left.HasValue && padding.Left.Value < 0)
            throw new InvalidOperationException(
                $"Negative left padding ({padding.Left.Value}) is not allowed for inline-block element {node.NodeType}");
    }

    /// <summary>
    ///     Validates border constraints for inline-block elements.
    /// </summary>
    private static void ValidateInlineBlockBorderConstraints(DocumentNode node)
    {
        if (!node.Styles.Border.IsVisible)
            return;

        var border = node.Styles.Border;

        // Validate border width is non-negative
        if (border.Width.HasValue && border.Width.Value < 0)
            throw new InvalidOperationException(
                $"Negative border width ({border.Width.Value}) is not allowed for inline-block element {node.NodeType}");
    }

    /// <summary>
    ///     Validates inline-block wrapping behavior according to CSS rules.
    /// </summary>
    private static void ValidateInlineBlockWrapping(DocumentNode node)
    {
        // Simplified validation - check for content that might affect wrapping
        ValidateInlineBlockContentConstraints(node);
        ValidateInlineBlockDisplayConstraints(node);
    }

    /// <summary>
    ///     Validates content constraints for inline-block elements.
    /// </summary>
    private static void ValidateInlineBlockContentConstraints(DocumentNode node)
    {
        // Check for nested block elements that might cause layout issues
        foreach (var child in node.Children)
            if (IsBlockElement(child))
                ValidateNestedBlockInInlineBlock(child, node);
    }

    /// <summary>
    ///     Validates nested block elements within inline-block elements.
    /// </summary>
    private static void ValidateNestedBlockInInlineBlock(DocumentNode blockChild, DocumentNode inlineBlockParent)
    {
        // Check if the nested block element has conflicting properties
        if (blockChild.Styles.Margin.HasValue || blockChild.Styles.Padding.HasValue ||
            blockChild.Styles.Border.IsVisible) ValidateNestedBlockSpacing(blockChild, inlineBlockParent);
    }

    /// <summary>
    ///     Validates spacing properties of nested block elements in inline-block.
    /// </summary>
    private static void ValidateNestedBlockSpacing(DocumentNode blockChild, DocumentNode inlineBlockParent)
    {
        // Validate that nested block spacing doesn't conflict with inline-block behavior
        var blockMargin = blockChild.Styles.Margin;
        var blockPadding = blockChild.Styles.Padding;

        // Check for negative margins in nested blocks
        if (blockMargin.HasValue)
        {
            if (blockMargin.Top.HasValue && blockMargin.Top.Value < 0)
                throw new InvalidOperationException(
                    $"Negative top margin ({blockMargin.Top.Value}) in nested block element {blockChild.NodeType} within inline-block {inlineBlockParent.NodeType}");

            if (blockMargin.Right.HasValue && blockMargin.Right.Value < 0)
                throw new InvalidOperationException(
                    $"Negative right margin ({blockMargin.Right.Value}) in nested block element {blockChild.NodeType} within inline-block {inlineBlockParent.NodeType}");

            if (blockMargin.Bottom.HasValue && blockMargin.Bottom.Value < 0)
                throw new InvalidOperationException(
                    $"Negative bottom margin ({blockMargin.Bottom.Value}) in nested block element {blockChild.NodeType} within inline-block {inlineBlockParent.NodeType}");

            if (blockMargin.Left.HasValue && blockMargin.Left.Value < 0)
                throw new InvalidOperationException(
                    $"Negative left margin ({blockMargin.Left.Value}) in nested block element {blockChild.NodeType} within inline-block {inlineBlockParent.NodeType}");
        }

        // Check for negative padding in nested blocks
        if (blockPadding.HasValue)
        {
            if (blockPadding.Top.HasValue && blockPadding.Top.Value < 0)
                throw new InvalidOperationException(
                    $"Negative top padding ({blockPadding.Top.Value}) in nested block element {blockChild.NodeType} within inline-block {inlineBlockParent.NodeType}");

            if (blockPadding.Right.HasValue && blockPadding.Right.Value < 0)
                throw new InvalidOperationException(
                    $"Negative right padding ({blockPadding.Right.Value}) in nested block element {blockChild.NodeType} within inline-block {inlineBlockParent.NodeType}");

            if (blockPadding.Bottom.HasValue && blockPadding.Bottom.Value < 0)
                throw new InvalidOperationException(
                    $"Negative bottom padding ({blockPadding.Bottom.Value}) in nested block element {blockChild.NodeType} within inline-block {inlineBlockParent.NodeType}");

            if (blockPadding.Left.HasValue && blockPadding.Left.Value < 0)
                throw new InvalidOperationException(
                    $"Negative left padding ({blockPadding.Left.Value}) in nested block element {blockChild.NodeType} within inline-block {inlineBlockParent.NodeType}");
        }
    }

    /// <summary>
    ///     Validates display constraints for inline-block elements.
    /// </summary>
    private static void ValidateInlineBlockDisplayConstraints(DocumentNode node)
    {
        // Validate that inline-block elements don't have conflicting display properties
        // This is simplified validation - more complex validation can be added later
    }

    /// <summary>
    ///     Determines if a node represents a block element.
    /// </summary>
    private static bool IsBlockElement(DocumentNode node)
    {
        return node.NodeType switch
        {
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
                DocumentNodeType.Table => true,
            _ => false
        };
    }

    #endregion
}