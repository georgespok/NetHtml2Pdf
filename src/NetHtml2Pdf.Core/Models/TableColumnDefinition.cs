namespace NetHtml2Pdf.Core.Models;

/// <summary>
/// Defines a table column
/// Pure POCO with no external dependencies
/// </summary>
public class TableColumnDefinition
{
    /// <summary>
    /// Column type
    /// </summary>
    public TableColumnType Type { get; init; } = TableColumnType.Relative;

    /// <summary>
    /// Fixed width for constant columns
    /// </summary>
    public float Width { get; set; }
}