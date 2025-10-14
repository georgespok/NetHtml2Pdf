using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Renderer.Interfaces;
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
            case DocumentNodeType.Bold:
                var boldStyle = currentStyle.WithBold();
                foreach (var child in node.Children)
                {
                    Compose(text, child, boldStyle);
                }
                break;
            case DocumentNodeType.Italic:
                var italicStyle = currentStyle.WithItalic();
                foreach (var child in node.Children)
                {
                    Compose(text, child, italicStyle);
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

        if (style.FontSize.HasValue)
        {
            span.FontSize((float)style.FontSize.Value);
        }

        if (!string.IsNullOrEmpty(style.Color))
        {
            span.FontColor(style.Color);
        }

        if (!string.IsNullOrEmpty(style.BackgroundColor))
        {
            span.BackgroundColor(style.BackgroundColor);
        }
    }
}
