using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Renderer.Interfaces;
using QuestPDF.Fluent;

namespace NetHtml2Pdf.Renderer;

internal sealed class InlineComposer : IInlineComposer
{
    public void Compose(TextDescriptor text, DocumentNode node, InlineStyleState style)
    {
        // Skip rendering for nodes with display:none
        if (node.Styles.DisplaySet && node.Styles.Display == CssDisplay.None)
            return;

        var currentStyle = style.ApplyCss(node.Styles);

        // Check CSS display property first - it overrides HTML element semantics
        if (node.Styles.DisplaySet && node.Styles.Display == CssDisplay.InlineBlock)
        {
            ComposeInlineBlock(text, node, currentStyle);
            return;
        }

        switch (node.NodeType)
        {
            case DocumentNodeType.Text:
                ComposeTextNode(text, node, currentStyle);
                break;
            case DocumentNodeType.LineBreak:
                text.EmptyLine();
                break;
            case DocumentNodeType.Strong:
            case DocumentNodeType.Bold:
                var boldStyle = currentStyle.WithBold();
                foreach (var child in node.Children)
                {
                    if (child.Styles.DisplaySet && child.Styles.Display == CssDisplay.None)
                        continue;
                    Compose(text, child, boldStyle);
                }
                break;
            case DocumentNodeType.Italic:
                var italicStyle = currentStyle.WithItalic();
                foreach (var child in node.Children)
                {
                    if (child.Styles.DisplaySet && child.Styles.Display == CssDisplay.None)
                        continue;
                    Compose(text, child, italicStyle);
                }
                break;
            default:
                foreach (var child in node.Children)
                {
                    if (child.Styles.DisplaySet && child.Styles.Display == CssDisplay.None)
                        continue;
                    Compose(text, child, currentStyle);
                }
                break;
        }
    }

    private static void ComposeTextNode(TextDescriptor text,
        DocumentNode node, InlineStyleState style)
    {
        var span = text.Span(node.TextContent ?? string.Empty);

        if (style.Bold)
        {
            span.SemiBold();
        }

        if (style.Italic)
        {
            span.Italic();
        }

        if (style.Underline)
        {
            span.Underline();
        }

        if (style.FontSize.HasValue)
        {
            span.FontSize((float)style.FontSize.Value);
        }

        if (!string.IsNullOrEmpty(style.Color))
        {
            span.FontColor(style.Color);
        }

        if (!string.IsNullOrEmpty(style.BackgroundColor))
        {
            span.BackgroundColor(style.BackgroundColor);
        }
    }

    private void ComposeInlineBlock(TextDescriptor text, DocumentNode node, InlineStyleState style)
    {
        // Inline-block behavior: participates in inline flow as an atomic box
        // Validate inline-block sizing and wrapping constraints before rendering

        // Validate inline-block element sizing constraints
        ValidateInlineBlockSizing(node);

        // Validate inline-block wrapping behavior
        ValidateInlineBlockWrapping(node);

        if (node.Children.Count > 0)
        {
            // If node has children, compose them as inline elements
            foreach (var child in node.Children)
            {
                if (child.Styles.DisplaySet && child.Styles.Display == CssDisplay.None)
                    continue;
                Compose(text, child, style);
            }
        }
        else if (!string.IsNullOrEmpty(node.TextContent))
        {
            // If it's a leaf node with text content, render as text
            ComposeTextNode(text, node, style);
        }
        else
        {
            // Empty inline-block element - render as empty space
            text.Span(" ");
        }
    }

    #region Inline-Block Sizing and Wrapping Validation

    /// <summary>
    /// Validates inline-block element sizing constraints according to CSS box model rules.
    /// </summary>
    private static void ValidateInlineBlockSizing(DocumentNode node)
    {
        // Inline-block elements should behave like inline elements but with block-like properties
        // This validation ensures proper CSS box model behavior for inline-block elements

        // Check for conflicting sizing constraints
        ValidateInlineBlockWidthConstraints(node);

        // Validate margin constraints for inline-block elements
        ValidateInlineBlockMarginConstraints(node);

        // Validate padding constraints for inline-block elements
        ValidateInlineBlockPaddingConstraints(node);

        // Validate border constraints for inline-block elements
        ValidateInlineBlockBorderConstraints(node);
    }

    /// <summary>
    /// Validates width constraints for inline-block elements.
    /// </summary>
    private static void ValidateInlineBlockWidthConstraints(DocumentNode node)
    {
        // Inline-block elements can have explicit width constraints
        // Unlike inline elements, they can have width, height, margins, padding, and borders

        // Note: Currently, the CSS parser doesn't support width/height properties
        // This validation is prepared for future width/height support

        // Future implementation would check:
        // - width: auto (default - fits content)
        // - width: specific values (fixed width)
        // - max-width constraints
        // - min-width constraints
        // - height constraints
        // - max-height constraints
        // - min-height constraints
    }

    /// <summary>
    /// Validates margin constraints for inline-block elements.
    /// </summary>
    private static void ValidateInlineBlockMarginConstraints(DocumentNode node)
    {
        if (!node.Styles.Margin.HasValue)
            return;

        var margin = node.Styles.Margin;

        // Validate margin values are non-negative
        if (margin.Top.HasValue && margin.Top.Value < 0)
            throw new InvalidOperationException($"Negative top margin ({margin.Top.Value}) is not allowed for inline-block element {node.NodeType}");

        if (margin.Right.HasValue && margin.Right.Value < 0)
            throw new InvalidOperationException($"Negative right margin ({margin.Right.Value}) is not allowed for inline-block element {node.NodeType}");

        if (margin.Bottom.HasValue && margin.Bottom.Value < 0)
            throw new InvalidOperationException($"Negative bottom margin ({margin.Bottom.Value}) is not allowed for inline-block element {node.NodeType}");

        if (margin.Left.HasValue && margin.Left.Value < 0)
            throw new InvalidOperationException($"Negative left margin ({margin.Left.Value}) is not allowed for inline-block element {node.NodeType}");
    }

    /// <summary>
    /// Validates padding constraints for inline-block elements.
    /// </summary>
    private static void ValidateInlineBlockPaddingConstraints(DocumentNode node)
    {
        if (!node.Styles.Padding.HasValue)
            return;

        var padding = node.Styles.Padding;

        // Validate padding values are non-negative
        if (padding.Top.HasValue && padding.Top.Value < 0)
            throw new InvalidOperationException($"Negative top padding ({padding.Top.Value}) is not allowed for inline-block element {node.NodeType}");

        if (padding.Right.HasValue && padding.Right.Value < 0)
            throw new InvalidOperationException($"Negative right padding ({padding.Right.Value}) is not allowed for inline-block element {node.NodeType}");

        if (padding.Bottom.HasValue && padding.Bottom.Value < 0)
            throw new InvalidOperationException($"Negative bottom padding ({padding.Bottom.Value}) is not allowed for inline-block element {node.NodeType}");

        if (padding.Left.HasValue && padding.Left.Value < 0)
            throw new InvalidOperationException($"Negative left padding ({padding.Left.Value}) is not allowed for inline-block element {node.NodeType}");
    }

    /// <summary>
    /// Validates border constraints for inline-block elements.
    /// </summary>
    private static void ValidateInlineBlockBorderConstraints(DocumentNode node)
    {
        if (!node.Styles.Border.IsVisible)
            return;

        var border = node.Styles.Border;

        // Validate border width is non-negative
        if (border.Width.HasValue && border.Width.Value < 0)
            throw new InvalidOperationException($"Negative border width ({border.Width.Value}) is not allowed for inline-block element {node.NodeType}");
    }

    /// <summary>
    /// Validates inline-block wrapping behavior according to CSS rules.
    /// </summary>
    private static void ValidateInlineBlockWrapping(DocumentNode node)
    {
        // Inline-block elements participate in inline flow but can have block-like properties
        // This validation ensures proper wrapping behavior

        // Check for content that might affect wrapping
        ValidateInlineBlockContentConstraints(node);

        // Validate that inline-block elements don't have conflicting display properties
        ValidateInlineBlockDisplayConstraints(node);
    }

    /// <summary>
    /// Validates content constraints for inline-block elements.
    /// </summary>
    private static void ValidateInlineBlockContentConstraints(DocumentNode node)
    {
        // Inline-block elements can contain both inline and block content
        // This validation ensures the content is appropriate for inline-block context

        // Check for nested block elements that might cause layout issues
        foreach (var child in node.Children)
        {
            if (IsBlockElement(child))
            {
                // Inline-block elements can contain block elements, but this might affect wrapping
                // Log a warning or validate the specific case
                ValidateNestedBlockInInlineBlock(child, node);
            }
        }
    }

    /// <summary>
    /// Validates nested block elements within inline-block elements.
    /// </summary>
    private static void ValidateNestedBlockInInlineBlock(DocumentNode blockChild, DocumentNode inlineBlockParent)
    {
        // Nested block elements in inline-block can cause layout issues
        // This validation ensures proper handling of such cases

        // Check if the nested block element has conflicting properties
        if (blockChild.Styles.Margin.HasValue || blockChild.Styles.Padding.HasValue || blockChild.Styles.Border.IsVisible)
        {
            // Nested block with spacing properties might affect inline-block wrapping
            // This is generally allowed but should be validated
            ValidateNestedBlockSpacing(blockChild, inlineBlockParent);
        }
    }

    /// <summary>
    /// Validates spacing properties of nested block elements in inline-block.
    /// </summary>
    private static void ValidateNestedBlockSpacing(DocumentNode blockChild, DocumentNode inlineBlockParent)
    {
        // Validate that nested block spacing doesn't conflict with inline-block behavior
        var blockMargin = blockChild.Styles.Margin;
        var blockPadding = blockChild.Styles.Padding;

        // Check for excessive margins that might break inline flow
        if (blockMargin.HasValue)
        {
            var totalVerticalMargin = (blockMargin.Top ?? 0) + (blockMargin.Bottom ?? 0);
            if (totalVerticalMargin > 50) // Arbitrary threshold for inline-block context
            {
                // Large vertical margins in nested blocks might affect inline-block wrapping
                // This is a warning case rather than an error
                // In a full implementation, this might be logged as a warning
            }
        }
    }

    /// <summary>
    /// Validates display constraints for inline-block elements.
    /// </summary>
    private static void ValidateInlineBlockDisplayConstraints(DocumentNode node)
    {
        // Ensure the element is actually set to inline-block
        if (!node.Styles.DisplaySet || node.Styles.Display != CssDisplay.InlineBlock)
        {
            throw new InvalidOperationException($"Element {node.NodeType} is not set to display: inline-block");
        }

        // Validate that inline-block elements don't have conflicting display properties
        // This is more of a consistency check
        ValidateInlineBlockConsistency(node);
    }

    /// <summary>
    /// Validates consistency of inline-block element properties.
    /// </summary>
    private static void ValidateInlineBlockConsistency(DocumentNode node)
    {
        // Inline-block elements should have consistent properties
        // This validation ensures the element behaves correctly as inline-block

        // Check for properties that might conflict with inline-block behavior
        // For now, this is a placeholder for future validation logic

        // Future implementation might check:
        // - float property conflicts
        // - position property conflicts
        // - overflow property interactions
        // - z-index considerations
    }

    /// <summary>
    /// Determines if a node is a block element.
    /// </summary>
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

    #endregion
}
