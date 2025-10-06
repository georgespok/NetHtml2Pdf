using NetHtml2Pdf.Core;
using QuestPDF.Fluent;

namespace NetHtml2Pdf.Renderer;

internal sealed class InlineComposer : IInlineComposer
{
    public void Compose(TextDescriptor text, DocumentNode node, InlineStyleState style)
    {
        var currentStyle = style.ApplyCss(node.Styles);

        switch (node.NodeType)
        {
            case DocumentNodeType.Text:
                ComposeTextNode(text, node, currentStyle);
                break;
            case DocumentNodeType.LineBreak:
                text.EmptyLine();
                break;
            case DocumentNodeType.Strong:
                var strongStyle = currentStyle.WithBold();
                foreach (var child in node.Children)
                {
                    Compose(text, child, strongStyle);
                }
                break;
            default:
                foreach (var child in node.Children)
                {
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
    }
}
