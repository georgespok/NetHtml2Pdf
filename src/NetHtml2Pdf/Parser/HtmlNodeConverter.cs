using AngleSharp.Dom;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Constants;
using NetHtml2Pdf.Core.Enums;

namespace NetHtml2Pdf.Parser;

internal sealed class HtmlNodeConverter(CssStyleResolver styleResolver)
{
    private static readonly IReadOnlyDictionary<string, DocumentNodeType> ElementTypeMap = new Dictionary<string, DocumentNodeType>(StringComparer.OrdinalIgnoreCase)
    {
        [HtmlTagNames.Div] = DocumentNodeType.Div,
        [HtmlTagNames.Section] = DocumentNodeType.Section,
        [HtmlTagNames.Paragraph] = DocumentNodeType.Paragraph,
        [HtmlTagNames.Span] = DocumentNodeType.Span,
        [HtmlTagNames.Strong] = DocumentNodeType.Strong,
        [HtmlTagNames.Bold] = DocumentNodeType.Bold,
        [HtmlTagNames.Italic] = DocumentNodeType.Italic,
        [HtmlTagNames.UnorderedList] = DocumentNodeType.UnorderedList,
        [HtmlTagNames.OrderedList] = DocumentNodeType.OrderedList,
        [HtmlTagNames.ListItem] = DocumentNodeType.ListItem,
        [HtmlTagNames.Table] = DocumentNodeType.Table,
        [HtmlTagNames.TableHead] = DocumentNodeType.TableHead,
        [HtmlTagNames.TableBody] = DocumentNodeType.TableBody,
        [HtmlTagNames.TableRow] = DocumentNodeType.TableRow,
        [HtmlTagNames.TableHeaderCell] = DocumentNodeType.TableHeaderCell,
        [HtmlTagNames.TableCell] = DocumentNodeType.TableCell,
        [HtmlTagNames.Header1] = DocumentNodeType.Heading1,
        [HtmlTagNames.Header2] = DocumentNodeType.Heading2,
        [HtmlTagNames.Header3] = DocumentNodeType.Heading3,
        [HtmlTagNames.Header4] = DocumentNodeType.Heading4,
        [HtmlTagNames.Header5] = DocumentNodeType.Heading5,
        [HtmlTagNames.Header6] = DocumentNodeType.Heading6
    };

    public DocumentNode? Convert(INode node, CssStyleMap inheritedStyles)
    {
        return node switch
        {
            IText textNode => CreateTextNode(textNode, inheritedStyles),
            IElement element => CreateElementNode(element, inheritedStyles),
            _ => null
        };
    }

    private static DocumentNode? CreateTextNode(IText textNode, CssStyleMap styles)
    {
        var content = textNode.Text;
        return string.IsNullOrWhiteSpace(content) 
            ? null 
            : new DocumentNode(DocumentNodeType.Text, content, styles);
    }

    private DocumentNode CreateElementNode(IElement element, CssStyleMap inheritedStyles)
    {
        if (string.Equals(element.TagName, HtmlTagNames.LineBreak, StringComparison.OrdinalIgnoreCase))
        {
            return new DocumentNode(DocumentNodeType.LineBreak, styles: inheritedStyles);
        }

        var styles = styleResolver.Resolve(element, inheritedStyles);
        var nodeType = MapElementType(element.TagName);
        var documentNode = new DocumentNode(nodeType, styles: styles);

        foreach (var child in element.ChildNodes)
        {
            var childNode = Convert(child, styles);
            if (childNode == null)
            {
                continue;
            }

            documentNode.AddChild(childNode);
        }

        return documentNode;
    }

    private static DocumentNodeType MapElementType(string tagName) => 
        ElementTypeMap.GetValueOrDefault(tagName, DocumentNodeType.Generic);
}
