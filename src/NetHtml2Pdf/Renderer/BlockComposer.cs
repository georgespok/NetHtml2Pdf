using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Renderer.Interfaces;
using QuestPDF.Fluent;

namespace NetHtml2Pdf.Renderer;

internal sealed class BlockComposer(
    IInlineComposer inlineComposer, IListComposer listComposer,
    ITableComposer tableComposer, IBlockSpacingApplier spacingApplier) : IBlockComposer
{
    public void Compose(ColumnDescriptor column, DocumentNode node)
    {
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
                
            case DocumentNodeType.ListItem:                
            case DocumentNodeType.Document:                
            case DocumentNodeType.Span:
            case DocumentNodeType.Strong:
            case DocumentNodeType.Bold:
            case DocumentNodeType.Italic:
            case DocumentNodeType.Text:
            case DocumentNodeType.LineBreak:
            case DocumentNodeType.TableHead:
            case DocumentNodeType.TableBody:
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
        // Apply margin at the parent level (affects positioning relative to siblings)
        var marginContainer = spacingApplier.ApplyMargin(column.Item(), node.Styles);
        
        // Apply border at the element level (wraps the content area)
        var borderedContainer = spacingApplier.ApplyBorder(marginContainer, node.Styles);
        
        // Apply padding at the element level (affects positioning of children within this element)
        var paddedContainer = spacingApplier.ApplySpacing(borderedContainer, node.Styles);

        paddedContainer.Text(text =>
        {
            foreach (var child in node.Children)
            {
                inlineComposer.Compose(text, child, InlineStyleState.Empty);
            }
        });
    }

    private void ComposeHeading(ColumnDescriptor column, DocumentNode node, double fontSize, bool bold)
    {
        // Apply margin at the parent level (affects positioning relative to siblings)
        var marginContainer = spacingApplier.ApplyMargin(column.Item(), node.Styles);
        
        // Apply border at the element level (wraps the content area)
        var borderedContainer = spacingApplier.ApplyBorder(marginContainer, node.Styles);
        
        // Apply padding at the element level (affects positioning of children within this element)
        var paddedContainer = spacingApplier.ApplySpacing(borderedContainer, node.Styles);

        paddedContainer.Text(text =>
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
        // Apply margin at the parent level (affects positioning relative to siblings)
        var marginContainer = spacingApplier.ApplyMargin(column.Item(), node.Styles);
        
        // Apply border at the element level (wraps the content area)
        var borderedContainer = spacingApplier.ApplyBorder(marginContainer, node.Styles);
        
        // Apply padding at the element level (affects positioning of children within this element)
        var paddedContainer = spacingApplier.ApplySpacing(borderedContainer, node.Styles);
        
        paddedContainer.Column(containerColumn =>
        {
            foreach (var child in node.Children)
            {
                Compose(containerColumn, child);
            }
        });
    }

    private void ComposeInlineContainer(ColumnDescriptor column, DocumentNode node)
    {
        // Apply margin at the parent level (affects positioning relative to siblings)
        var marginContainer = spacingApplier.ApplyMargin(column.Item(), node.Styles);
        
        // Apply border at the element level (wraps the content area)
        var borderedContainer = spacingApplier.ApplyBorder(marginContainer, node.Styles);
        
        // Apply padding at the element level (affects positioning of children within this element)
        var paddedContainer = spacingApplier.ApplySpacing(borderedContainer, node.Styles);
        
        paddedContainer.Text(text => inlineComposer.Compose(text, node, InlineStyleState.Empty));
    }
}
