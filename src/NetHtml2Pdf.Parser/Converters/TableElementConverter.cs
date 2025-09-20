using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Core.Models.Styles.Text;
using NetHtml2Pdf.Parser.Interfaces;
using NetHtml2Pdf.Parser.Utilities;

namespace NetHtml2Pdf.Parser.Converters
{
    /// <summary>
    /// Converts HTML table elements to TableNode
    /// </summary>
    public class TableElementConverter(IStyleParser styleParser)
        : IHtmlElementConverter<TableNode>
    {
        private readonly IStyleParser _styleParser = styleParser ?? throw new ArgumentNullException(nameof(styleParser));

        public TableNode Convert(IElement element)
        {
            var table = new TableNode();
            
            // Read simple <style> rules inside the table element for th/td defaults
            var thDefaultStyles = new Dictionary<string, string>();
            var tdDefaultStyles = new Dictionary<string, string>();
            var styleElement = element.QuerySelector("style");
            if (styleElement != null)
            {
                var css = styleElement.TextContent;
                ExtractSelectors(css, out var thStyles, out var tdStyles, out var bothStyles);
                MergeStyles(thDefaultStyles, bothStyles);
                MergeStyles(tdDefaultStyles, bothStyles);
                MergeStyles(thDefaultStyles, thStyles);
                MergeStyles(tdDefaultStyles, tdStyles);
            }

            // Detect header row only if explicitly present or using <th> in first row
            var explicitHeaderRow = element.QuerySelector("thead tr");
            var headerRow = explicitHeaderRow;
            if (headerRow == null)
            {
                var firstRow = element.QuerySelector("tr");
                if (firstRow != null && firstRow.QuerySelectorAll("th").Any())
                    headerRow = firstRow;
            }

            // Determine number of columns
            var referenceRow = headerRow ?? element.QuerySelector("tr");
            var columnCount = referenceRow?.QuerySelectorAll("th, td").Length ?? 0;
            for (var i = 0; i < columnCount; i++)
                table.ColumnDefinitions.Add(new TableColumnDefinition { Type = TableColumnType.Relative });

            // Add header row if present (use only <th> cells)
            if (headerRow != null)
            {
                var headerCells = headerRow.QuerySelectorAll("th");
                if (headerCells.Length > 0)
                {
                    var headerRowNode = new TableRowNode { IsHeader = true };
                    foreach (var cell in headerCells)
                    {
                        var cellNode = new TableCellNode();
                        // Apply default header cell styles from <style>
                        if (thDefaultStyles.Count > 0)
                            ApplyStylesFromDictionary(thDefaultStyles, cellNode);
                        // Add text runs directly as content, bolded
                        var textRuns = TextExtractor.ExtractTextRuns(cell);
                        foreach (var run in textRuns)
                        {
                            run.IsBold = true;
                            cellNode.Content.Add(run);
                        }
                        headerRowNode.Cells.Add(cellNode);
                    }
                    table.Rows.Add(headerRowNode);
                }
            }

            // Add data rows
            var bodyRows = element.QuerySelectorAll("tbody tr");
            IEnumerable<IElement> dataRows = bodyRows;
            if (!dataRows.Any())
            {
                var allRows = element.QuerySelectorAll("tr");
                dataRows = headerRow != null ? allRows.Where(r => r != headerRow) : allRows;
            }
            
            foreach (var row in dataRows)
            {
                var rowNode = new TableRowNode();
                var cells = row.QuerySelectorAll("td, th");
                foreach (var cell in cells)
                {
                    var cellNode = new TableCellNode();
                    // Apply inline styles to the cell itself (padding, border, alignment)
                    _styleParser.ApplyInlineStyles(cell, cellNode);
                    // Apply table-level default td styles when present and not set by inline
                    if (tdDefaultStyles.Count > 0)
                        ApplyStylesFromDictionary(tdDefaultStyles, cellNode);

                    var textRuns = TextExtractor.ExtractTextRuns(cell);
                    foreach (var run in textRuns)
                        cellNode.Content.Add(run);
                    rowNode.Cells.Add(cellNode);
                }
                table.Rows.Add(rowNode);
            }
            
            _styleParser.ApplyInlineStyles(element, table);
            return table;
        }

        private static void MergeStyles(Dictionary<string, string> target, Dictionary<string, string> source)
        {
            foreach (var kv in source)
                target[kv.Key] = kv.Value;
        }

        private static void ExtractSelectors(string css, out Dictionary<string, string> th, out Dictionary<string, string> td, out Dictionary<string, string> both)
        {
            th = new();
            td = new();
            both = new();
            if (string.IsNullOrWhiteSpace(css)) return;

            // naive CSS block parser for rules like: th, td { ... }  | th { ... } | td { ... }
            var blocks = css.Split('}', StringSplitOptions.RemoveEmptyEntries);
            foreach (var block in blocks)
            {
                var parts = block.Split('{', 2);
                if (parts.Length != 2) continue;
                var selectors = parts[0].Trim();
                var declarations = parts[1].Trim();
                var styles = ParseDeclarations(declarations);
                var selectorList = selectors.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                            .Select(s => s.Trim().ToLowerInvariant());
                var set = selectorList.ToHashSet();
                var appliesTh = set.Contains("th");
                var appliesTd = set.Contains("td");
                if (appliesTh && appliesTd)
                    MergeStyles(both, styles);
                else if (appliesTh)
                    MergeStyles(th, styles);
                else if (appliesTd)
                    MergeStyles(td, styles);
            }
        }

        private static Dictionary<string, string> ParseDeclarations(string declarations)
        {
            var map = new Dictionary<string, string>();
            var decls = declarations.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var d in decls)
            {
                var kv = d.Split(':', 2);
                if (kv.Length != 2) continue;
                var prop = kv[0].Trim().ToLowerInvariant();
                var val = kv[1].Trim();
                map[prop] = val;
            }
            return map;
        }

        private void ApplyStylesFromDictionary(Dictionary<string, string> styles, TableCellNode cell)
        {
            if (styles.Count == 0) return;
            // text-align
            if (styles.TryGetValue("text-align", out var ta))
            {
                cell.Style.Alignment = ta.ToLowerInvariant() switch
                {
                    "center" => TextAlignment.Center,
                    "right" => TextAlignment.Right,
                    "justify" => TextAlignment.Justify,
                    _ => TextAlignment.Left
                };
            }

            // padding
            if (styles.TryGetValue("padding", out var padding))
            {
                var pv = _styleParser.ParseSize(padding);
                if (pv.HasValue)
                {
                    cell.Style.Box.PaddingLeft ??= pv.Value;
                    cell.Style.Box.PaddingRight ??= pv.Value;
                    cell.Style.Box.PaddingTop ??= pv.Value;
                    cell.Style.Box.PaddingBottom ??= pv.Value;
                }
            }
            if (styles.TryGetValue("padding-left", out var pl)) cell.Style.Box.PaddingLeft ??= _styleParser.ParseSize(pl);
            if (styles.TryGetValue("padding-right", out var pr)) cell.Style.Box.PaddingRight ??= _styleParser.ParseSize(pr);
            if (styles.TryGetValue("padding-top", out var pt)) cell.Style.Box.PaddingTop ??= _styleParser.ParseSize(pt);
            if (styles.TryGetValue("padding-bottom", out var pb)) cell.Style.Box.PaddingBottom ??= _styleParser.ParseSize(pb);

            // border
            _styleParser.ParseBorder(styles, out var bw, out var bc);
            if (bw.HasValue && !cell.Style.Border.Left.Width.HasValue) cell.Style.Border.Left.Width = bw.Value;
            if (!string.IsNullOrEmpty(bc) && string.IsNullOrWhiteSpace(cell.Style.Border.Left.ColorHex)) cell.Style.Border.Left.ColorHex = _styleParser.ConvertColorToHex(bc);
        }

    }
}
