using NetHtml2Pdf.Core;
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
            var hexColor = ConvertToHexColor(style.Color);
            if (hexColor != null)
            {
                span.FontColor(hexColor);
            }
        }

        if (!string.IsNullOrEmpty(style.BackgroundColor))
        {
            var hexColor = ConvertToHexColor(style.BackgroundColor);
            if (hexColor != null)
            {
                span.BackgroundColor(hexColor);
            }
        }
    }

    private static string? ConvertToHexColor(string color)
    {
        if (color.StartsWith("#"))
            return color;

        // Convert common named colors to hex
        return color.ToLowerInvariant() switch
        {
            "red" => "#FF0000",
            "blue" => "#0000FF",
            "green" => "#008000",
            "yellow" => "#FFFF00",
            "black" => "#000000",
            "white" => "#FFFFFF",
            "gray" or "grey" => "#808080",
            "orange" => "#FFA500",
            "purple" => "#800080",
            "pink" => "#FFC0CB",
            _ => color.StartsWith("#") ? color : null
        };
    }
}
