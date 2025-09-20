namespace NetHtml2Pdf.Core.Models
{
    /// <summary>
    /// Represents a table element
    /// Pure POCO with no external dependencies
    /// </summary>
    public class TableNode : DocumentNode
    {
        /// <summary>
        /// Table rows
        /// </summary>
        public List<TableRowNode> Rows { get; } = [];

        /// <summary>
        /// Column definitions for this table
        /// </summary>
        public List<TableColumnDefinition> ColumnDefinitions { get; } = [];

    }
}
