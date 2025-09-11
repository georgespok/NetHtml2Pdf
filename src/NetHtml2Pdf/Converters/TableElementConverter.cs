using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using HtmlElement = AngleSharp.Dom.IElement;

namespace NetHtml2Pdf.Converters
{
    /// <summary>
    /// Converter for HTML table elements
    /// </summary>
    public class TableElementConverter : BaseHtmlElementConverter
    {
        protected override string[] SupportedTags => ["table"];

        public override void Convert(HtmlElement element, IContainer container)
        {
            container.Table(tableDescriptor =>
            {
                var headers = element.QuerySelectorAll("thead tr th").Select(th => th.TextContent.Trim()).ToList();
                
                if (headers.Count == 0)
                {
                    throw new InvalidOperationException("Table must have headers in thead tr th elements");
                }

                // Define columns based on header count
                tableDescriptor.ColumnsDefinition(columns =>
                {
                    foreach (var _ in headers)
                    {
                        columns.RelativeColumn();
                    }
                });

                // Add header row
                foreach (var header in headers)
                {
                    tableDescriptor.Cell().Element(CellStyle).Text(header).Bold();
                }

                // Add data rows
                var rows = element.QuerySelectorAll("tbody tr");
                foreach (var row in rows)
                {
                    var cells = row.QuerySelectorAll("td").Select(td => td.TextContent.Trim());
                    foreach (var cell in cells)
                    {
                        tableDescriptor.Cell().Element(CellStyle).Text(cell);
                    }
                }
            });
        }

        private static IContainer CellStyle(IContainer container) =>
            container.Padding(5).Border(1).BorderColor(Colors.Grey.Lighten2);
    }
}
