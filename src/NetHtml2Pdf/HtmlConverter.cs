using AngleSharp;
using AngleSharp.Dom;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Document = QuestPDF.Fluent.Document;

namespace NetHtml2Pdf
{
    public class HtmlConverter
    {
        public async Task<byte[]> Convert(string html)
        {
            var context = BrowsingContext.New(Configuration.Default);
            var document = await context.OpenAsync(req => req.Content(html));

            var table = document.QuerySelector("table");

            QuestPDF.Settings.License = LicenseType.Community;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Content().Element(c =>
                    {
                        c.Table(tableDescriptor =>
                        {
                            var headers = table.QuerySelectorAll("thead tr th").Select(th => th.TextContent.Trim()).ToList();
                            
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
                            var rows = table.QuerySelectorAll("tbody tr");
                            foreach (var row in rows)
                            {
                                var cells = row.QuerySelectorAll("td").Select(td => td.TextContent.Trim());
                                foreach (var cell in cells)
                                {
                                    tableDescriptor.Cell().Element(CellStyle).Text(cell);
                                }
                            }

                            static IContainer CellStyle(IContainer container) =>
                                container.Padding(5).Border(1).BorderColor(Colors.Grey.Lighten2);
                        });
                    });
                });
            }).GeneratePdf();
        
        }

    }
}
