using NetHtml2Pdf.Core;
using NetHtml2Pdf.Renderer.Interfaces;
using QuestPDF.Fluent;

namespace NetHtml2Pdf.Renderer;

internal sealed class BlockComposer(
    IInlineComposer inlineComposer, IListComposer listComposer,
    IBlockSpacingApplier spacingApplier) : IBlockComposer
{
    public void Compose(ColumnDescriptor column, DocumentNode node)
    {
        switch (node.NodeType)
        {
            case DocumentNodeType.Div:
            case DocumentNodeType.Section:
                foreach (var child in node.Children)
                {
                    Compose(column, child);
                }
                break;
            case DocumentNodeType.Paragraph:
                ComposeParagraph(column, node);
                break;
            case DocumentNodeType.List:
                listComposer.Compose(column, node, ordered: false, Compose);
                break;
            case DocumentNodeType.OrderedList:
                listComposer.Compose(column, node, ordered: true, Compose);
                break;
                
            case DocumentNodeType.ListItem:                
            case DocumentNodeType.Document:                
            case DocumentNodeType.Span:
            case DocumentNodeType.Strong:
            case DocumentNodeType.Text:
            case DocumentNodeType.LineBreak:
            case DocumentNodeType.Table:
            case DocumentNodeType.TableSection:
            case DocumentNodeType.TableRow:
            case DocumentNodeType.TableHeaderCell:
            case DocumentNodeType.TableCell:
            case DocumentNodeType.Fallback:
            case DocumentNodeType.Generic:
            default:
                ComposeInlineContainer(column, node);
                break;
        }
    }

    private void ComposeParagraph(ColumnDescriptor column, DocumentNode node)
    {
        var container = spacingApplier.ApplySpacing(column.Item(), node.Styles);

        container.Text(text =>
        {
            foreach (var child in node.Children)
            {
                inlineComposer.Compose(text, child, InlineStyleState.Empty);
            }
        });
    }

    private void ComposeInlineContainer(ColumnDescriptor column, DocumentNode node)
    {
        var container = spacingApplier.ApplySpacing(column.Item(), node.Styles);
        container.Text(text => inlineComposer.Compose(text, node, InlineStyleState.Empty));
    }
}
