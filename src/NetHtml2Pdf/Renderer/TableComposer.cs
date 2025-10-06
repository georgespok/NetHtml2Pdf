using NetHtml2Pdf.Core;
using NetHtml2Pdf.Renderer.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NetHtml2Pdf.Renderer;

internal sealed class TableComposer(IInlineComposer inlineComposer, IBlockSpacingApplier spacingApplier) : ITableComposer
{
    public void Compose(ColumnDescriptor column, DocumentNode tableNode)
    {
        var container = spacingApplier.ApplySpacing(column.Item(), tableNode.Styles);

        container.Table(table =>
        {
            // Collect all rows from thead and tbody
            var allRows = new List<DocumentNode>();
            
            foreach (var section in tableNode.Children)
            {
                if (section.NodeType == DocumentNodeType.TableHead || 
                    section.NodeType == DocumentNodeType.TableBody)
                {
                    foreach (var row in section.Children)
                    {
                        if (row.NodeType == DocumentNodeType.TableRow)
                        {
                            allRows.Add(row);
                        }
                    }
                }
                else if (section.NodeType == DocumentNodeType.TableRow)
                {
                    // Handle direct tr children (without tbody/thead wrapper)
                    allRows.Add(section);
                }
            }

            if (allRows.Count == 0)
            {
                return;
            }

            // Determine column count from first row
            var firstRow = allRows[0];
            var columnCount = firstRow.Children.Count;

            // Define columns with equal width (call ColumnsDefinition only once)
            table.ColumnsDefinition(columns =>
            {
                for (int i = 0; i < columnCount; i++)
                {
                    columns.RelativeColumn();
                }
            });

            // Render header rows and data rows
            foreach (var row in allRows)
            {
                foreach (var cell in row.Children)
                {
                    if (cell.NodeType == DocumentNodeType.TableHeaderCell ||
                        cell.NodeType == DocumentNodeType.TableCell)
                    {
                        table.Cell().Element(cellContainer =>
                        {
                            RenderCell(cellContainer, cell);
                        });
                    }
                }
            }
        });
    }

    private void RenderCell(IContainer cellContainer, DocumentNode cell)
    {
        // Apply border first (outside)
        var borderedContainer = cellContainer;
        if (!string.IsNullOrEmpty(cell.Styles.Border))
        {
            borderedContainer = borderedContainer.Border(1).BorderColor(Colors.Gray);
        }

        // Apply background color
        if (!string.IsNullOrEmpty(cell.Styles.BackgroundColor))
        {
            var bgColor = RenderingHelpers.ConvertToHexColor(cell.Styles.BackgroundColor);
            if (bgColor != null)
            {
                borderedContainer = borderedContainer.Background(bgColor);
            }
        }

        // Apply cell spacing (padding) inside the border
        var paddedContainer = spacingApplier.ApplySpacing(borderedContainer, cell.Styles);

        // Apply text alignment
        var alignedContainer = RenderingHelpers.ApplyTextAlignment(paddedContainer, cell.Styles.TextAlign);

        // Render cell content
        alignedContainer.Text(text =>
        {
            // Apply bold formatting for header cells
            var initialState = cell.NodeType == DocumentNodeType.TableHeaderCell
                ? InlineStyleState.Empty.WithBold()
                : InlineStyleState.Empty;

            foreach (var child in cell.Children)
            {
                inlineComposer.Compose(text, child, initialState);
            }
        });
    }
}

