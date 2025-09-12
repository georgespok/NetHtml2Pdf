using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NetHtml2Pdf.RenderModel
{
    /// <summary>
    /// Represents a table element
    /// </summary>
    public class TableNode : RenderNode
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
        public string BorderColor { get; set; } = Colors.Grey.Lighten2;

        public override void Render(IContainer container)
        {
            if (Rows.Count == 0)
                return;

            container.Table(table =>
            {
                // Define columns
                table.ColumnsDefinition(columns =>
                {
                    foreach (var columnDef in ColumnDefinitions)
                    {
                        switch (columnDef.Type)
                        {
                            case TableColumnType.Relative:
                                columns.RelativeColumn();
                                break;
                            case TableColumnType.Fixed:
                                columns.ConstantColumn(columnDef.Width);
                                break;
                        }
                    }
                });

                // Render rows
                foreach (var row in Rows)
                {
                    row.Render(table, BorderWidth, BorderColor);
                }
            });
        }
    }

    /// <summary>
    /// Represents a table row
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

        public void Render(QuestPDF.Fluent.TableDescriptor table, float borderWidth, string borderColor)
        {
            foreach (var cell in Cells)
            {
                table.Cell().Element(container =>
                {
                    var styledContainer = container
                        .Padding(5)
                        .Border(borderWidth)
                        .BorderColor(borderColor);

                    if (IsHeader)
                    {
                        // Header styling would be applied here
                    }

                    cell.Render(styledContainer);
                });
            }
        }
    }

    /// <summary>
    /// Represents a table cell
    /// </summary>
    public class TableCellNode
    {
        /// <summary>
        /// Content of this cell
        /// </summary>
        public List<RenderNode> Content { get; set; } = new List<RenderNode>();

        /// <summary>
        /// Text alignment for this cell
        /// </summary>
        public HorizontalAlignment Alignment { get; set; } = HorizontalAlignment.Left;

        public void Render(IContainer container)
        {
            container.Column(column =>
            {
                foreach (var node in Content)
                {
                    column.Item().Element(nodeContainer =>
                    {
                        node.Render(nodeContainer);
                    });
                }
            });
        }
    }

    /// <summary>
    /// Defines a table column
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
