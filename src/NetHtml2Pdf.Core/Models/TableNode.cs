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
        public List<TableRowNode> Rows { get; set; } = new List<TableRowNode>();

        /// <summary>
        /// Column definitions for this table
        /// </summary>
        public List<TableColumnDefinition> ColumnDefinitions { get; set; } = new List<TableColumnDefinition>();

        /// <summary>
        /// Border width for table cells
        /// </summary>
        public float BorderWidth { get; set; } = 1;

        /// <summary>
        /// Border color for table cells
        /// </summary>
        public string BorderColor { get; set; } = "#CCCCCC";
    }

    /// <summary>
    /// Represents a table row
    /// Pure POCO with no external dependencies
    /// </summary>
    public class TableRowNode
    {
        /// <summary>
        /// Cells in this row
        /// </summary>
        public List<TableCellNode> Cells { get; set; } = new List<TableCellNode>();

        /// <summary>
        /// Whether this is a header row
        /// </summary>
        public bool IsHeader { get; set; }
    }

    /// <summary>
    /// Represents a table cell
    /// Pure POCO with no external dependencies
    /// </summary>
    public class TableCellNode
    {
        /// <summary>
        /// Content of this cell
        /// </summary>
        public List<DocumentNode> Content { get; set; } = new List<DocumentNode>();

        /// <summary>
        /// Text alignment for this cell
        /// </summary>
        public TextAlignment Alignment { get; set; } = TextAlignment.Left;
    }

    /// <summary>
    /// Defines a table column
    /// Pure POCO with no external dependencies
    /// </summary>
    public class TableColumnDefinition
    {
        /// <summary>
        /// Column type
        /// </summary>
        public TableColumnType Type { get; set; } = TableColumnType.Relative;

        /// <summary>
        /// Fixed width for constant columns
        /// </summary>
        public float Width { get; set; }
    }

    /// <summary>
    /// Defines the type of table column
    /// </summary>
    public enum TableColumnType
    {
        Relative,
        Fixed
    }
}
