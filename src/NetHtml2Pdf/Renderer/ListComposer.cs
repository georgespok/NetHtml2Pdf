using NetHtml2Pdf.Core;
using NetHtml2Pdf.Renderer.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NetHtml2Pdf.Renderer;

internal sealed class ListComposer(IInlineComposer inlineComposer, IBlockSpacingApplier spacingApplier)
    : IListComposer
{
    private const float BulletMarkerWidth = 14f;

    public void Compose(ColumnDescriptor column, DocumentNode listNode, bool ordered, Action<ColumnDescriptor, DocumentNode> composeBlock)
    {
        var container = spacingApplier.ApplySpacing(column.Item(), listNode.Styles);

        container.Column(listColumn =>
        {
            var index = 1;

            foreach (var child in listNode.Children)
            {
                if (child.NodeType != DocumentNodeType.ListItem)
                {
                    composeBlock(listColumn, child);
                    continue;
                }

                // Each list item should be a separate vertical item
                listColumn.Item().Row(row =>
                {
                    var marker = ordered ? $"{index}." : "\u2022";
                    row.ConstantItem(BulletMarkerWidth).Text(marker);
                    row.RelativeItem().Column(itemColumn => ComposeListItemContent(itemColumn, child, composeBlock));
                });

                index++;
            }
        });
    }

    private void ComposeListItemContent(ColumnDescriptor column, DocumentNode itemNode, Action<ColumnDescriptor, DocumentNode> composeBlock)
    {
        if (itemNode.Children.Count == 0)
        {
            column.Item().Text(string.Empty);
            return;
        }

        var inlineBuffer = new List<DocumentNode>();

        foreach (var child in itemNode.Children)
        {
            if (IsInlineNode(child))
            {
                inlineBuffer.Add(child);
                continue;
            }

            FlushInlineBuffer(inlineBuffer, column, inlineComposer);

            composeBlock(column, child);
        }

        FlushInlineBuffer(inlineBuffer, column, inlineComposer);
    }

    private static void FlushInlineBuffer(List<DocumentNode> inlineBuffer, 
        ColumnDescriptor column, IInlineComposer inlineComposer)
    {
        if (inlineBuffer.Count == 0)
        {
            return;
        }

        column.Item().Text(text =>
        {
            foreach (var inline in inlineBuffer)
            {
                inlineComposer.Compose(text, inline, InlineStyleState.Empty);
            }
        });

        inlineBuffer.Clear();
    }

    

    private static bool IsInlineNode(DocumentNode node) =>
        node.NodeType is DocumentNodeType.Text or DocumentNodeType.Span or DocumentNodeType.Strong 
        or DocumentNodeType.Bold or DocumentNodeType.Italic or DocumentNodeType.LineBreak;
}
