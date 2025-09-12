using QuestPDF.Fluent;
using QuestPDF.Helpers;
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
                foreach (var row in tableNode.Rows)
                {
                    MapTableRow(row, table, tableNode.BorderWidth, tableNode.BorderColor);
                }
            });
        }

        private void MapTableRow(TableRowNode row, QuestPDF.Fluent.TableDescriptor table, float borderWidth, string borderColor)
        {
            foreach (var cell in row.Cells)
            {
                table.Cell().Element(container =>
                {
                    var styledContainer = container
                        .Padding(5)
                        .Border(borderWidth)
                        .BorderColor(borderColor);

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
