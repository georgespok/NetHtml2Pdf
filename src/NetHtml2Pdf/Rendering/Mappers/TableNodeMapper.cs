using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Rendering.Interfaces;

namespace NetHtml2Pdf.Rendering.Mappers
{
    /// <summary>
    /// Maps TableNode to QuestPDF elements
    /// </summary>
    public class TableNodeMapper(IDocumentNodeMapperFactory mapperFactory) : IDocumentNodeMapper<TableNode>
    {
        private readonly IDocumentNodeMapperFactory _mapperFactory = mapperFactory ?? throw new ArgumentNullException(nameof(mapperFactory));

		public void Map(TableNode tableNode, IContainer container)
        {
            if (tableNode.Rows.Count == 0)
                return;

            container.Table(table =>
            {
                // Define columns
                table.ColumnsDefinition(columns =>
                {
                    foreach (var columnDef in tableNode.ColumnDefinitions)
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
				var totalRows = tableNode.Rows.Count;
				var totalCols = tableNode.ColumnDefinitions.Count;
				for (var rowIndex = 0; rowIndex < totalRows; rowIndex++)
				{
					var row = tableNode.Rows[rowIndex];
					MapTableRow(row, table, tableNode.BorderWidth, tableNode.BorderColor, rowIndex, totalRows, totalCols);
				}
            });
        }

		private void MapTableRow(TableRowNode row, TableDescriptor table, float borderWidth, string borderColor, int rowIndex, int totalRows, int totalCols)
        {
			for (var colIndex = 0; colIndex < row.Cells.Count; colIndex++)
			{
				var cell = row.Cells[colIndex];
				var isLastRow = rowIndex == totalRows - 1;
				var isLastCol = colIndex == totalCols - 1;

				table.Cell().Element(container =>
				{
					var styledContainer = container
						// Draw internal borders in a collapsed manner:
						// - use left and top borders for internal grid lines
						// - draw right and bottom only on outer edges
						.BorderLeft(borderWidth)
						.BorderTop(borderWidth);

					if (isLastCol)
						styledContainer = styledContainer.BorderRight(borderWidth);
					if (isLastRow)
						styledContainer = styledContainer.BorderBottom(borderWidth);

					styledContainer = styledContainer
						.BorderColor(borderColor)
						.Padding(5);

					if (row.IsHeader)
					{
						// Header styling would be applied here
						// styledContainer = styledContainer.BackgroundColor(Colors.Grey.Lighten3);
					}

					MapTableCell(cell, styledContainer);
				});
			}
        }

        private void MapTableCell(TableCellNode cell, IContainer container)
        {
            container.Column(column =>
            {
                foreach (var node in cell.Content)
                {
                    column.Item().Element(nodeContainer =>
                    {
                        var mapper = _mapperFactory.GetMapper(node);
                        mapper.Map(node, nodeContainer);
                    });
                }
            });
        }
    }
}
