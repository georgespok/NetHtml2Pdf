using NetHtml2Pdf.Core.Enums;

namespace NetHtml2Pdf.Core;

internal class DocumentNode
{
    private readonly List<DocumentNode> _children = [];

    public DocumentNode(DocumentNodeType nodeType, 
        string? textContent = null, 
        CssStyleMap? styles = null)
    {
        if (nodeType == DocumentNodeType.Text && textContent is null)
        {
            textContent = string.Empty;
        }

        NodeType = nodeType;
        TextContent = textContent;
        Styles = styles ?? CssStyleMap.Empty;
    }

    public DocumentNodeType NodeType { get; }

    public string? TextContent { get; private set; }

    public CssStyleMap Styles { get; }

    public IReadOnlyList<DocumentNode> Children => _children.AsReadOnly();

    public void AddChild(DocumentNode child)
    {
        ArgumentNullException.ThrowIfNull(child);

        if (child.NodeType == DocumentNodeType.Text && _children.Count > 0 && _children[^1].NodeType == DocumentNodeType.Text)
        {
            _children[^1].AppendText(child.TextContent ?? string.Empty);
            return;
        }

        _children.Add(child);
    }

    internal void AppendText(string value)
    {
        if (NodeType != DocumentNodeType.Text)
        {
            throw new InvalidOperationException("Can only append text to a text node.");
        }

        TextContent = (TextContent ?? string.Empty) + value;
    }
}
