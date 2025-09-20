namespace NetHtml2Pdf.Core.Models;

/// <summary>
/// Represents a table cell
/// Pure POCO with no external dependencies
/// </summary>
public class TableCellNode
{
    /// <summary>
    /// Content of this cell
    /// </summary>
    public List<DocumentNode> Content { get; } = [];

    /// <summary>
    /// Common style attributes for this cell
    /// </summary>
    public NodeStyle Style { get; } = new NodeStyle();

    // Passthroughs removed; use Style directly
}