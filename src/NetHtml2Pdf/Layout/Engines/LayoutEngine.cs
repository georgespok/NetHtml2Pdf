using System.Text.Json;
using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Contexts;
using NetHtml2Pdf.Layout.Display;
using NetHtml2Pdf.Layout.Model;

namespace NetHtml2Pdf.Layout.Engines;

/// <summary>
/// Orchestrates layout by building a layout box tree and delegating to formatting contexts.
/// </summary>
internal sealed class LayoutEngine : ILayoutEngine
{
    private readonly IDisplayClassifier _displayClassifier;
    private readonly BlockFormattingContext _blockFormattingContext;
    private readonly IInlineFormattingContext _inlineFormattingContext;
    private readonly ILogger<LayoutEngine>? _logger;

    private static readonly HashSet<DocumentNodeType> SupportedBlockNodes = new()
    {
        DocumentNodeType.Paragraph,
        DocumentNodeType.Heading1,
        DocumentNodeType.Heading2,
        DocumentNodeType.Heading3,
        DocumentNodeType.Heading4,
        DocumentNodeType.Heading5,
        DocumentNodeType.Heading6
    };

    private static readonly HashSet<DocumentNodeType> SupportedInlineNodes = new()
    {
        DocumentNodeType.Span,
        DocumentNodeType.Strong,
        DocumentNodeType.Bold,
        DocumentNodeType.Italic,
        DocumentNodeType.Text,
        DocumentNodeType.LineBreak
    };

    public LayoutEngine(
        IDisplayClassifier displayClassifier,
        BlockFormattingContext blockFormattingContext,
        IInlineFormattingContext inlineFormattingContext,
        ILogger<LayoutEngine>? logger = null)
    {
        _displayClassifier = displayClassifier ?? throw new ArgumentNullException(nameof(displayClassifier));
        _blockFormattingContext = blockFormattingContext ?? throw new ArgumentNullException(nameof(blockFormattingContext));
        _inlineFormattingContext = inlineFormattingContext ?? throw new ArgumentNullException(nameof(inlineFormattingContext));
        _logger = logger;
    }

    public LayoutResult Layout(DocumentNode root, LayoutConstraints constraints, LayoutEngineOptions options)
    {
        ArgumentNullException.ThrowIfNull(root);
        options ??= LayoutEngineOptions.Disabled;

        if (!options.EnableNewLayoutForTextBlocks)
        {
            _logger?.LogDebug("Layout engine disabled via feature flag. Returning disabled result.");
            return LayoutResult.Disabled();
        }

        var layoutBox = BuildLayoutBox(root, CreateNodePath(root, 0), parentDisplay: null);
        if (layoutBox is null)
        {
            _logger?.LogDebug("Node {NodeType} not supported by layout engine. Falling back.", root.NodeType);
            return LayoutResult.Fallback($"Node '{root.NodeType}' is not supported by the layout engine yet.");
        }

        LayoutFragment fragment = layoutBox.Display switch
        {
            DisplayClass.Block => _blockFormattingContext.Layout(layoutBox, constraints),
            DisplayClass.Inline => _inlineFormattingContext.Layout(layoutBox, constraints),
            _ => throw new InvalidOperationException($"Unsupported display class {layoutBox.Display}.")
        };

        var fragments = new[] { fragment };

        if (options.EnableDiagnostics)
        {
            foreach (var topLevelFragment in fragments)
            {
                LogFragmentDiagnostics(topLevelFragment);
            }
        }

        return LayoutResult.Success(fragments);
    }

    private LayoutBox? BuildLayoutBox(DocumentNode node, string nodePath, DisplayClass? parentDisplay)
    {
        var display = _displayClassifier.Classify(node, node.Styles);

        if (!IsSupported(display, node))
        {
            return null;
        }

        if (parentDisplay == DisplayClass.Inline && display == DisplayClass.Block)
        {
            return null;
        }

        var children = new List<LayoutBox>();
        for (var i = 0; i < node.Children.Count; i++)
        {
            var child = node.Children[i];
            var childPath = $"{nodePath}/{CreateNodePath(child, i)}";
            var childBox = BuildLayoutBox(child, childPath, display);
            if (childBox is not null)
            {
                children.Add(childBox);
            }
        }

        var spacing = LayoutSpacing.FromStyles(node.Styles);
        return new LayoutBox(node, display, node.Styles, spacing, nodePath, children);
    }

    private void LogFragmentDiagnostics(LayoutFragment fragment)
    {
        if (_logger is null)
        {
            return;
        }

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

        foreach (var child in fragment.Children)
        {
            LogFragmentDiagnostics(child);
        }
    }

    private static string CreateNodePath(DocumentNode node, int index) => $"{node.NodeType}:{index}";

    private static bool IsSupported(DisplayClass display, DocumentNode node) =>
        display switch
        {
            DisplayClass.Block => SupportedBlockNodes.Contains(node.NodeType),
            DisplayClass.Inline => SupportedInlineNodes.Contains(node.NodeType),
            _ => false
        };
}
