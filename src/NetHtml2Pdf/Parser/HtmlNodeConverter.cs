using AngleSharp.Dom;
using NetHtml2Pdf.Core;

namespace NetHtml2Pdf.Parser;

internal sealed class HtmlNodeConverter(CssStyleResolver styleResolver)
{
    private static readonly IReadOnlyDictionary<string, DocumentNodeType> ElementTypeMap = new Dictionary<string, DocumentNodeType>(StringComparer.OrdinalIgnoreCase)
    {
        ["DIV"] = DocumentNodeType.Div,
        ["SECTION"] = DocumentNodeType.Section,
        ["P"] = DocumentNodeType.Paragraph,
        ["SPAN"] = DocumentNodeType.Span,
        ["STRONG"] = DocumentNodeType.Strong,
        ["B"] = DocumentNodeType.Bold,
        ["I"] = DocumentNodeType.Italic,
        ["UL"] = DocumentNodeType.UnorderedList,
        ["OL"] = DocumentNodeType.OrderedList,
        ["LI"] = DocumentNodeType.ListItem,
        ["TABLE"] = DocumentNodeType.Table,
        ["THEAD"] = DocumentNodeType.TableHead,
        ["TBODY"] = DocumentNodeType.TableBody,
        ["TR"] = DocumentNodeType.TableRow,
        ["TH"] = DocumentNodeType.TableHeaderCell,
        ["TD"] = DocumentNodeType.TableCell,
        ["H1"] = DocumentNodeType.Heading1,
        ["H2"] = DocumentNodeType.Heading2,
        ["H3"] = DocumentNodeType.Heading3,
        ["H4"] = DocumentNodeType.Heading4,
        ["H5"] = DocumentNodeType.Heading5,
        ["H6"] = DocumentNodeType.Heading6
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
        if (string.Equals(element.TagName, "BR", StringComparison.OrdinalIgnoreCase))
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
