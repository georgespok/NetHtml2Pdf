using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Parsing.Interfaces;
using NetHtml2Pdf.Parsing.Utilities;

namespace NetHtml2Pdf.Parsing.Converters
{
    /// <summary>
    /// Converts HTML table elements to TableNode
    /// </summary>
    public class TableElementConverter(IStyleParser styleParser, IHtmlElementConverterFactory converterFactory)
        : IHtmlElementConverter<TableNode>
    {
        private readonly IStyleParser _styleParser = styleParser ?? throw new ArgumentNullException(nameof(styleParser));
        private readonly IHtmlElementConverterFactory _converterFactory = converterFactory ?? throw new ArgumentNullException(nameof(converterFactory));

        public TableNode? Convert(IElement element)
        {
            var table = new TableNode();
            
            // Find header row
            var headerRow = element.QuerySelector("thead tr") ?? element.QuerySelector("tr");
            if (headerRow != null)
            {
                var headers = headerRow.QuerySelectorAll("th, td").Select(cell => cell.TextContent).ToList();
                
                // Define columns
                foreach (var _ in headers)
                {
                    table.ColumnDefinitions.Add(new TableColumnDefinition { Type = TableColumnType.Relative });
                }
                
                // Add header row
                var headerRowNode = new TableRowNode { IsHeader = true };
                foreach (var header in headers)
                {
                    var cell = new TableCellNode();
                    cell.Content.Add(new ParagraphNode 
                    { 
                        TextRuns = new List<TextRunNode> 
                        { 
                            new TextRunNode { Text = header, IsBold = true } 
                        } 
                    });
                    headerRowNode.Cells.Add(cell);
                }
                table.Rows.Add(headerRowNode);
            }
            
            // Add data rows
            var dataRows = element.QuerySelectorAll("tbody tr").Concat(
                element.QuerySelectorAll("tr").Skip(headerRow != null ? 1 : 0));
                
            foreach (var row in dataRows)
            {
                var rowNode = new TableRowNode();
                var cells = row.QuerySelectorAll("td, th");
                
                foreach (var cell in cells)
                {
                    var cellNode = new TableCellNode();
                    var cellParagraph = new ParagraphNode();
                    cellParagraph.TextRuns = TextExtractor.ExtractTextRuns(cell);
                    cellNode.Content.Add(cellParagraph);
                    rowNode.Cells.Add(cellNode);
                }
                
                table.Rows.Add(rowNode);
            }
            
            _styleParser.ApplyInlineStyles(element, table);
            return table;
        }

    }
}
