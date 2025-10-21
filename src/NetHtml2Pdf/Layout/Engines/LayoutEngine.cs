using System.Text.Json;
using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Contexts;
using NetHtml2Pdf.Layout.Diagnostics;
using NetHtml2Pdf.Layout.Display;
using NetHtml2Pdf.Layout.FormattingContexts;
using NetHtml2Pdf.Layout.Model;

namespace NetHtml2Pdf.Layout.Engines;

/// <summary>
///     Orchestrates layout by building a layout box tree and delegating to formatting contexts.
/// </summary>
internal sealed class LayoutEngine(
    IDisplayClassifier displayClassifier,
    BlockFormattingContext blockFormattingContext,
    IInlineFormattingContext inlineFormattingContext,
    InlineBlockFormattingContext? inlineBlockFormattingContext,
    TableFormattingContext? tableFormattingContext,
    FlexFormattingContext? flexFormattingContext,
    FormattingContextOptions formattingOptions,
    ILogger<LayoutEngine>? logger = null) : ILayoutEngine
{
    private static readonly HashSet<DocumentNodeType> SupportedBlockNodes =
    [
        DocumentNodeType.Div,
        DocumentNodeType.Paragraph,
        DocumentNodeType.Heading1,
        DocumentNodeType.Heading2,
        DocumentNodeType.Heading3,
        DocumentNodeType.Heading4,
        DocumentNodeType.Heading5,
        DocumentNodeType.Heading6,
        DocumentNodeType.Section,
        DocumentNodeType.Table,
        DocumentNodeType.TableHead,
        DocumentNodeType.TableBody,
        DocumentNodeType.TableSection,
        DocumentNodeType.TableRow,
        DocumentNodeType.TableHeaderCell,
        DocumentNodeType.TableCell
    ];

    private static readonly HashSet<DocumentNodeType> SupportedInlineNodes =
    [
        DocumentNodeType.Span,
        DocumentNodeType.Strong,
        DocumentNodeType.Bold,
        DocumentNodeType.Italic,
        DocumentNodeType.Text,
        DocumentNodeType.LineBreak
    ];

    private readonly BlockFormattingContext _blockFormattingContext =
        blockFormattingContext ?? throw new ArgumentNullException(nameof(blockFormattingContext));

    private readonly IDisplayClassifier _displayClassifier =
        displayClassifier ?? throw new ArgumentNullException(nameof(displayClassifier));

    private readonly FlexFormattingContext? _flexFormattingContext = flexFormattingContext;

    private readonly FormattingContextOptions _formattingOptions =
        formattingOptions ?? FormattingContextOptions.Disabled;

    private readonly InlineBlockFormattingContext? _inlineBlockFormattingContext = inlineBlockFormattingContext;

    private readonly IInlineFormattingContext _inlineFormattingContext =
        inlineFormattingContext ?? throw new ArgumentNullException(nameof(inlineFormattingContext));

    private readonly ILogger<LayoutEngine>? _logger = logger;
    private readonly TableFormattingContext? _tableFormattingContext = tableFormattingContext;

    public LayoutResult Layout(DocumentNode root, LayoutConstraints constraints, LayoutEngineOptions options)
    {
        ArgumentNullException.ThrowIfNull(root);
        options ??= LayoutEngineOptions.Disabled;

        if (!options.EnableNewLayoutForTextBlocks)
        {
            _logger?.LogDebug("Layout engine disabled via feature flag. Returning disabled result.");
            return LayoutResult.Disabled();
        }

        var layoutBox = BuildLayoutBox(root, CreateNodePath(root, 0), null);
        if (layoutBox is null)
        {
            _logger?.LogDebug("Node {NodeType} not supported by layout engine. Falling back.", root.NodeType);
            return LayoutResult.Fallback($"Node '{root.NodeType}' is not supported by the layout engine yet.");
        }

        // Telemetry hook for CSS display:flex: if present (parsed as unsupported display),
        // route to flex context when flag enabled; otherwise log downgrade and continue.
        var unsupportedDisplay = layoutBox.Node.Styles.UnsupportedDisplay;
        if (!string.IsNullOrWhiteSpace(unsupportedDisplay) &&
            string.Equals(unsupportedDisplay, "flex", StringComparison.OrdinalIgnoreCase))
        {
            if (options.EnableFlexContext && _flexFormattingContext is not null && _formattingOptions.EnableFlexContext)
            {
                var flexFragment = _flexFormattingContext.Layout(layoutBox, constraints, _logger);
                return ProduceResult(options, [flexFragment]);
            }

            FlexDiagnostics.LogDowngrade(_logger, layoutBox.NodePath, "FlagDisabled");
        }

        if (layoutBox.Node.NodeType == DocumentNodeType.Table)
        {
            if (!options.EnableTableContext || _tableFormattingContext is null ||
                !_formattingOptions.EnableTableContext)
            {
                _logger?.LogDebug("Table context disabled. Falling back for node {NodePath}.", layoutBox.NodePath);
                return LayoutResult.Fallback("Table context disabled.");
            }

            if (!options.EnableTableBorderCollapse &&
                string.Equals(layoutBox.Node.Styles.BorderCollapse, "collapse", StringComparison.OrdinalIgnoreCase))
                TableDiagnostics.LogBorderCollapseDowngrade(
                    _logger,
                    layoutBox.NodePath,
                    layoutBox.Node.Styles.BorderCollapse ?? "collapse",
                    "separate");

            var tableFragment = TableFormattingContext.Layout(layoutBox, constraints, _formattingOptions);
            return ProduceResult(options, [tableFragment]);
        }

        if (!options.EnableInlineBlockContext || _inlineBlockFormattingContext is null ||
            !_formattingOptions.EnableInlineBlockContext)
        {
            var inlineBlockNode = FindInlineBlockNode(layoutBox);
            if (inlineBlockNode is not null)
            {
                if (options.EnableDiagnostics)
                    FormattingContextDiagnostics.LogInlineBlockFallback(_logger, inlineBlockNode.NodePath,
                        "InlineBlockContextDisabled");

                _logger?.LogDebug("Inline-block context disabled. Falling back for node {NodePath}.",
                    inlineBlockNode.NodePath);
                return LayoutResult.Fallback("Inline-block context disabled.");
            }
        }

        // Flex container handling: preview, gated by flag and presence of context
        if (options.EnableFlexContext && _flexFormattingContext is not null && _formattingOptions.EnableFlexContext)
            // Treat any Div with children as potential flex container for preview purposes
            if (layoutBox.Node.NodeType == DocumentNodeType.Div)
            {
                var flexFragment = _flexFormattingContext.Layout(layoutBox, constraints, _logger);
                return ProduceResult(options, [flexFragment]);
            }

        if (layoutBox.Display == DisplayClass.InlineBlock)
            if (!options.EnableInlineBlockContext || _inlineBlockFormattingContext is null ||
                !_formattingOptions.EnableInlineBlockContext)
            {
                if (options.EnableDiagnostics)
                    FormattingContextDiagnostics.LogInlineBlockFallback(_logger, layoutBox.NodePath,
                        "InlineBlockContextDisabled");
                _logger?.LogDebug("Inline-block context disabled. Falling back for node {NodePath}.",
                    layoutBox.NodePath);
                return LayoutResult.Fallback("Inline-block context disabled.");
            }

        var fragment = layoutBox.Display switch
        {
            DisplayClass.Block => _blockFormattingContext.Layout(layoutBox, constraints),
            DisplayClass.Inline => _inlineFormattingContext.Layout(layoutBox, constraints),
            DisplayClass.InlineBlock => _inlineBlockFormattingContext!.Layout(layoutBox, constraints),
            _ => throw new InvalidOperationException($"Unsupported display class {layoutBox.Display}.")
        };

        return ProduceResult(options, NormalizeFragments(fragment));
    }

    private LayoutResult ProduceResult(LayoutEngineOptions options, IReadOnlyList<LayoutFragment> fragments)
    {
        if (options.EnableDiagnostics)
            foreach (var fragment in fragments)
                LogFragmentDiagnostics(fragment);

        return LayoutResult.Success(fragments);
    }

    private LayoutBox? BuildLayoutBox(DocumentNode node, string nodePath, DisplayClass? parentDisplay)
    {
        var display = _displayClassifier.Classify(node, node.Styles);

        if (!IsSupported(display, node)) return null;

        if (parentDisplay == DisplayClass.Inline && display == DisplayClass.Block) return null;

        var children = new List<LayoutBox>();
        for (var i = 0; i < node.Children.Count; i++)
        {
            var child = node.Children[i];
            var childPath = $"{nodePath}/{CreateNodePath(child, i)}";
            var childBox = BuildLayoutBox(child, childPath, display);
            if (childBox is not null) children.Add(childBox);
        }

        var spacing = LayoutSpacing.FromStyles(node.Styles);
        return new LayoutBox(node, display, node.Styles, spacing, nodePath, children);
    }

    private void LogFragmentDiagnostics(LayoutFragment fragment)
    {
        if (_logger is null) return;

        var diagnostics = fragment.Diagnostics;
        var constraints = diagnostics.AppliedConstraints;

        var payload = new
        {
            nodePath = fragment.NodePath,
            display = fragment.Display.ToString(),
            diagnostics.ContextName,
            size = new { fragment.Width, fragment.Height, fragment.Baseline },
            constraints = new
            {
                constraints.InlineMin,
                constraints.InlineMax,
                constraints.BlockMin,
                constraints.BlockMax,
                constraints.PageRemainingBlockSize,
                constraints.AllowBreaks
            },
            metadata = diagnostics.Metadata
        };

        var json = JsonSerializer.Serialize(payload);
        _logger.LogInformation("LayoutEngine.FragmentMeasured {Payload}", json);

        FormattingContextDiagnostics.LogInlineBlock(_logger, fragment);

        foreach (var child in fragment.Children) LogFragmentDiagnostics(child);
    }

    private static IReadOnlyList<LayoutFragment> NormalizeFragments(LayoutFragment fragment)
    {
        if (fragment.Kind == LayoutFragmentKind.Block &&
            fragment.Children.Count == 1 &&
            fragment.Children[0].Display == DisplayClass.InlineBlock)
            return [fragment.Children[0]];

        return [fragment];
    }

    private static LayoutBox? FindInlineBlockNode(LayoutBox box)
    {
        if (box.Display == DisplayClass.InlineBlock) return box;

        foreach (var child in box.Children)
        {
            var result = FindInlineBlockNode(child);
            if (result is not null) return result;
        }

        return null;
    }

    private static string CreateNodePath(DocumentNode node, int index)
    {
        return $"{node.NodeType}:{index}";
    }

    private static bool IsSupported(DisplayClass display, DocumentNode node)
    {
        return display switch
        {
            DisplayClass.Block => SupportedBlockNodes.Contains(node.NodeType),
            DisplayClass.Inline => SupportedInlineNodes.Contains(node.NodeType),
            DisplayClass.InlineBlock => SupportedInlineNodes.Contains(node.NodeType) ||
                                        SupportedBlockNodes.Contains(node.NodeType),
            _ => false
        };
    }
}