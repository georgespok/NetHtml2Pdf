using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Display;
using NetHtml2Pdf.Renderer.Inline;
using NetHtml2Pdf.Renderer.Interfaces;
using QuestPDF.Fluent;

namespace NetHtml2Pdf.Renderer;

/// <summary>
/// Thin orchestration layer that classifies nodes and delegates inline rendering
/// to <see cref="InlineFlowLayoutEngine"/> while preserving behavior parity.
/// </summary>
internal sealed class InlineComposer(
    IDisplayClassifier? displayClassifier = null,
    InlineFlowLayoutEngine? inlineFlowLayoutEngine = null) : IInlineComposer
{
    private readonly IDisplayClassifier _displayClassifier = displayClassifier ?? new DisplayClassifier();
    private readonly InlineFlowLayoutEngine _inlineFlowLayoutEngine = inlineFlowLayoutEngine ?? new InlineFlowLayoutEngine();

    public void Compose(TextDescriptor text, DocumentNode node, InlineStyleState style)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(style);

        var displayClass = _displayClassifier.Classify(node, node.Styles);

        if (displayClass == DisplayClass.None)
            return;

        // Inline and inline-block traversal now lives in the shared flow engine.
        _inlineFlowLayoutEngine.ProcessInlineContent(text, node, style);
    }
}
