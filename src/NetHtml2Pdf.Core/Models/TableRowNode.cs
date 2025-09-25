namespace NetHtml2Pdf.Core.Models;

/// <summary>
/// Represents a table row
/// Pure POCO with no external dependencies
/// </summary>
public class TableRowNode
{
    /// <summary>
    /// Cells in this row
    /// </summary>
    public List<TableCellNode> Cells { get; } = [];

    /// <summary>
    /// Whether this is a header row
    /// </summary>
    public bool IsHeader { get; set; }
}