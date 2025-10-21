using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Renderer;

namespace NetHtml2Pdf.Layout.Display;

/// <summary>
///     Internal implementation of display classification logic.
///     Centralizes display decisions while preserving current behavior parity.
/// </summary>
internal sealed class DisplayClassifier(ILogger<DisplayClassifier>? logger = null, RendererOptions? options = null)
    : IDisplayClassifier
{
    private static readonly HashSet<DocumentNodeType> WarnedNodeTypes = [];
    private static readonly HashSet<string> WarnedDisplayValues = [];
    private readonly bool _enableTraceLogging = options?.EnableClassifierTraceLogging ?? false;
    private readonly ILogger<DisplayClassifier>? _logger = logger;

    /// <summary>
    ///     Classifies a document node into a display class based on CSS display property and semantic defaults.
    /// </summary>
    /// <param name="node">The document node to classify</param>
    /// <param name="style">The CSS style map for the node</param>
    /// <returns>The display class for the node</returns>
    public DisplayClass Classify(DocumentNode node, CssStyleMap style)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(style);

        DisplayClass result;

        // Explicit CSS display wins over semantic defaults
        if (style.DisplaySet && style.Display != CssDisplay.Default)
        {
            result = ClassifyByCssDisplay(style.Display, node.NodeType);

            if (_enableTraceLogging)
                _logger?.LogDebug(
                    "DisplayClassifier: Node {NodeType} classified as {DisplayClass} via CSS display {CssDisplay}",
                    node.NodeType, result, style.Display);
        }
        else
        {
            // Use semantic defaults
            result = GetSemanticDefault(node.NodeType);

            if (_enableTraceLogging)
                _logger?.LogDebug(
                    "DisplayClassifier: Node {NodeType} classified as {DisplayClass} via semantic default",
                    node.NodeType, result);
        }

        return result;
    }

    /// <summary>
    ///     Classifies based on explicit CSS display value.
    /// </summary>
    private DisplayClass ClassifyByCssDisplay(CssDisplay cssDisplay, DocumentNodeType nodeType)
    {
        return cssDisplay switch
        {
            CssDisplay.Block => DisplayClass.Block,
            CssDisplay.InlineBlock => DisplayClass.InlineBlock,
            CssDisplay.Flex => DisplayClass.Flex,
            CssDisplay.None => DisplayClass.None,
            _ => HandleUnsupportedDisplayValue(cssDisplay, nodeType)
        };
    }

    /// <summary>
    ///     Handles unsupported CSS display values by warning once per value and falling back to semantic default.
    /// </summary>
    private DisplayClass HandleUnsupportedDisplayValue(CssDisplay cssDisplay, DocumentNodeType nodeType)
    {
        var displayValue = cssDisplay.ToString();

        // Warn once per unsupported display value
        if (WarnedDisplayValues.Add(displayValue))
            _logger?.LogWarning(
                "Unsupported CSS display value '{DisplayValue}' for node type '{NodeType}'. Falling back to semantic default.",
                displayValue, nodeType);

        return GetSemanticDefault(nodeType);
    }

    /// <summary>
    ///     Gets the semantic default display class for a node type.
    /// </summary>
    private DisplayClass GetSemanticDefault(DocumentNodeType nodeType)
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

            // Table sub-elements (handled via table context in current implementation)
            // These default to Block but are processed via table context rather than explicit classification
            DocumentNodeType.TableHead or
                DocumentNodeType.TableBody or
                DocumentNodeType.TableSection or
                DocumentNodeType.TableRow or
                DocumentNodeType.TableHeaderCell or
                DocumentNodeType.TableCell => DisplayClass.Block,

            // Unknown/unsupported types default to Block with warning
            _ => HandleUnknownNodeType(nodeType)
        };
    }

    /// <summary>
    ///     Handles unknown node types by warning once per type and defaulting to Block.
    /// </summary>
    private DisplayClass HandleUnknownNodeType(DocumentNodeType nodeType)
    {
        // Warn once per unknown node type
        if (WarnedNodeTypes.Add(nodeType))
            _logger?.LogWarning("Unknown node type '{NodeType}'. Treating as Block element.", nodeType);

        return DisplayClass.Block;
    }
}