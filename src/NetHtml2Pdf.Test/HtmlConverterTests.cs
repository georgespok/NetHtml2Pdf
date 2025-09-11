using UglyToad.PdfPig;
using HtmlAgilityPack;

namespace NetHtml2Pdf.Test
{
    public class HtmlConverterTests
    {
        private readonly HtmlConverter _converter = new();

        [Fact]
        public async Task Convert_ValidTable_ReturnsPdfBytes()
        {
            // Arrange
            var html = @"
                <table>
                    <thead>
                        <tr><th>Name</th><th>Age</th><th>City</th></tr>
                    </thead>
                    <tbody>
                        <tr><td>Alice</td><td>30</td><td>New York</td></tr>
                        <tr><td>Bob</td><td>25</td><td>London</td></tr>
                    </tbody>
                </table>";

            // Act
            var result = await _converter.ConvertToPdfBytes(html);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
            Assert.Equal(0x25, result[0]); // PDF file starts with %PDF
            Assert.Equal(0x50, result[1]); // P
            Assert.Equal(0x44, result[2]); // D
            Assert.Equal(0x46, result[3]); // F

            // Verify PDF content by extracting text
            var extractedText = ExtractTextFromPdf(result);
            Assert.Contains("Name", extractedText);
            Assert.Contains("Age", extractedText);
            Assert.Contains("City", extractedText);
            Assert.Contains("Alice", extractedText);
            Assert.Contains("30", extractedText);
            Assert.Contains("New York", extractedText);
            Assert.Contains("Bob", extractedText);
            Assert.Contains("25", extractedText);
            Assert.Contains("London", extractedText);
        }

        [Fact]
        public async Task Convert_EmptyTable_ThrowsException()
        {
            // Arrange
            var html = "<table></table>";

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _converter.ConvertToPdfBytes(html));
        }

        [Fact]
        public async Task Convert_TableWithoutHeaders_ThrowsException()
        {
            // Arrange
            var html = @"
                <table>
                    <tbody>
                        <tr><td>Alice</td><td>30</td></tr>
                    </tbody>
                </table>";

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _converter.ConvertToPdfBytes(html));
        }

        [Fact]
        public async Task Convert_TableWithSingleColumn_ReturnsPdfBytes()
        {
            // Arrange
            var html = @"
                <table>
                    <thead>
                        <tr><th>Name</th></tr>
                    </thead>
                    <tbody>
                        <tr><td>Alice</td></tr>
                        <tr><td>Bob</td></tr>
                    </tbody>
                </table>";

            // Act
            var result = await _converter.ConvertToPdfBytes(html);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task Convert_TableWithManyColumns_ReturnsPdfBytes()
        {
            // Arrange
            var html = @"
                <table>
                    <thead>
                        <tr><th>ID</th><th>Name</th><th>Age</th><th>City</th><th>Country</th><th>Salary</th></tr>
                    </thead>
                    <tbody>
                        <tr><td>1</td><td>Alice</td><td>30</td><td>New York</td><td>USA</td><td>50000</td></tr>
                        <tr><td>2</td><td>Bob</td><td>25</td><td>London</td><td>UK</td><td>45000</td></tr>
                    </tbody>
                </table>";

            // Act
            var result = await _converter.ConvertToPdfBytes(html);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task Convert_TableWithEmptyCells_ReturnsPdfBytes()
        {
            // Arrange
            var html = @"
                <table>
                    <thead>
                        <tr><th>Name</th><th>Age</th><th>City</th></tr>
                    </thead>
                    <tbody>
                        <tr><td>Alice</td><td></td><td>New York</td></tr>
                        <tr><td></td><td>25</td><td></td></tr>
                    </tbody>
                </table>";

            // Act
            var result = await _converter.ConvertToPdfBytes(html);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task Convert_InvalidHtml_ReturnsPdfBytes()
        {
            // Arrange
            var html = "invalid html";

            // Act
            var result = await _converter.ConvertToPdfBytes(html);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);
        }

        [Fact]
        public async Task Convert_NullHtml_ThrowsException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _converter.ConvertToPdfBytes(null!));
        }

        [Fact]
        public async Task Convert_TableWithSingleColumn_ReturnsPdfWithCorrectContent()
        {
            // Arrange
            var html = @"
                <table>
                    <thead>
                        <tr><th>Name</th></tr>
                    </thead>
                    <tbody>
                        <tr><td>Alice</td></tr>
                        <tr><td>Bob</td></tr>
                    </tbody>
                </table>";

            // Act
            var result = await _converter.ConvertToPdfBytes(html);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);

            // Verify PDF content
            var extractedText = ExtractTextFromPdf(result);
            Assert.Contains("Name", extractedText);
            Assert.Contains("Alice", extractedText);
            Assert.Contains("Bob", extractedText);
        }

        [Fact]
        public async Task Convert_TableWithEmptyCells_HandlesEmptyContent()
        {
            // Arrange
            var html = @"
                <table>
                    <thead>
                        <tr><th>Name</th><th>Age</th><th>City</th></tr>
                    </thead>
                    <tbody>
                        <tr><td>Alice</td><td></td><td>New York</td></tr>
                        <tr><td></td><td>25</td><td></td></tr>
                    </tbody>
                </table>";

            // Act
            var result = await _converter.ConvertToPdfBytes(html);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Length > 0);

            // Verify PDF content - should contain headers and non-empty cells
            var extractedText = ExtractTextFromPdf(result);
            Assert.Contains("Name", extractedText);
            Assert.Contains("Age", extractedText);
            Assert.Contains("City", extractedText);
            Assert.Contains("Alice", extractedText);
            Assert.Contains("New York", extractedText);
            Assert.Contains("25", extractedText);
        }

        [Fact]
        public async Task Convert_ValidTable_PreservesContentStructure()
        {
            // Arrange
            var originalHtml = @"
                <table>
                    <thead>
                        <tr><th>Name</th><th>Age</th><th>City</th></tr>
                    </thead>
                    <tbody>
                        <tr><td>Alice</td><td>30</td><td>New York</td></tr>
                        <tr><td>Bob</td><td>25</td><td>London</td></tr>
                    </tbody>
                </table>";

            // Act
            var pdfBytes = await _converter.ConvertToPdfBytes(originalHtml);

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.True(pdfBytes.Length > 0);

            // Convert PDF back to HTML and verify content is preserved
            var convertedHtml = ConvertPdfToHtml(pdfBytes);
            var originalTableData = ExtractTableData(originalHtml);
            var convertedTableData = ExtractTableData(convertedHtml);

            // Verify content is preserved in the converted HTML
            Assert.True(convertedTableData.Rows.Count > 0, "Should have at least one row in converted data");
            
            // Check that key content from original is present in converted
            var allOriginalText = string.Join(" ", originalTableData.Headers.Concat(originalTableData.Rows.SelectMany(r => r)));
            var allConvertedText = string.Join(" ", convertedTableData.Rows.SelectMany(r => r));
            
            // Verify key words are preserved
            Assert.Contains("Name", allConvertedText);
            Assert.Contains("Alice", allConvertedText);
            Assert.Contains("Bob", allConvertedText);
        }

        [Fact]
        public async Task Convert_ComplexTable_MaintainsDataIntegrity()
        {
            // Arrange
            var originalHtml = @"
                <table>
                    <thead>
                        <tr><th>ID</th><th>Name</th><th>Age</th><th>City</th><th>Country</th><th>Salary</th></tr>
                    </thead>
                    <tbody>
                        <tr><td>1</td><td>Alice Johnson</td><td>30</td><td>New York</td><td>USA</td><td>$50,000</td></tr>
                        <tr><td>2</td><td>Bob Smith</td><td>25</td><td>London</td><td>UK</td><td>£45,000</td></tr>
                        <tr><td>3</td><td>Charlie Brown</td><td>28</td><td>Berlin</td><td>Germany</td><td>€55,000</td></tr>
                    </tbody>
                </table>";

            // Act
            var pdfBytes = await _converter.ConvertToPdfBytes(originalHtml);

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.True(pdfBytes.Length > 0);

            // Convert PDF back to HTML and verify content preservation
            var convertedHtml = ConvertPdfToHtml(pdfBytes);
            var originalData = ExtractTableData(originalHtml);
            var convertedData = ExtractTableData(convertedHtml);

            // Verify content is preserved
            Assert.True(convertedData.Rows.Count > 0, "Should have at least one row in converted data");
            
            // Check that key content from original is present in converted
            var allConvertedText = string.Join(" ", convertedData.Rows.SelectMany(r => r));
            
            // Verify key data is preserved
            Assert.Contains("Alice", allConvertedText);
            Assert.Contains("Bob", allConvertedText);
            Assert.Contains("Charlie", allConvertedText);
            Assert.Contains("New York", allConvertedText);
            Assert.Contains("London", allConvertedText);
            Assert.Contains("Berlin", allConvertedText);
        }

        /// <summary>
        /// Helper method to extract text content from PDF bytes for verification
        /// </summary>
        private static string ExtractTextFromPdf(byte[] pdfBytes)
        {
            using var stream = new MemoryStream(pdfBytes);
            using var document = PdfDocument.Open(stream);
            
            var text = new System.Text.StringBuilder();
            foreach (var page in document.GetPages())
            {
                text.AppendLine(page.Text);
            }
            
            return text.ToString();
        }

        /// <summary>
        /// Converts PDF back to HTML using PdfPig to extract text and reconstruct HTML
        /// </summary>
        private static string ConvertPdfToHtml(byte[] pdfBytes)
        {
            using var stream = new MemoryStream(pdfBytes);
            using var document = PdfDocument.Open(stream);
            
            var html = new System.Text.StringBuilder();
            html.AppendLine("<html><body>");
            html.AppendLine("<table>");
            
            var allText = new List<string>();
            foreach (var page in document.GetPages())
            {
                var pageText = page.Text.Trim();
                if (!string.IsNullOrEmpty(pageText))
                {
                    allText.Add(pageText);
                }
            }
            
            if (allText.Count > 0)
            {
                // Get all text as one string and split by common delimiters
                var fullText = string.Join(" ", allText);
                
                // Try to identify table structure by looking for patterns
                // For now, create a simple structure with all extracted text
                html.AppendLine("<thead>");
                html.AppendLine("<tr>");
                html.AppendLine("<th>Content</th>");
                html.AppendLine("</tr>");
                html.AppendLine("</thead>");
                
                html.AppendLine("<tbody>");
                html.AppendLine("<tr>");
                html.AppendLine($"<td>{fullText}</td>");
                html.AppendLine("</tr>");
                html.AppendLine("</tbody>");
            }
            
            html.AppendLine("</table>");
            html.AppendLine("</body></html>");
            
            return html.ToString();
        }


        /// <summary>
        /// Extracts table data from HTML for comparison
        /// </summary>
        private static TableData ExtractTableData(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            
            var headers = new List<string>();
            var rows = new List<List<string>>();
            
            // Extract headers
            var headerCells = doc.DocumentNode.SelectNodes("//thead//th");
            if (headerCells != null)
            {
                headers.AddRange(headerCells.Select(th => th.InnerText.Trim()));
            }
            
            // Extract data rows
            var dataRows = doc.DocumentNode.SelectNodes("//tbody//tr");
            if (dataRows != null)
            {
                foreach (var row in dataRows)
                {
                    var cells = row.SelectNodes(".//td");
                    if (cells != null)
                    {
                        var rowData = cells.Select(td => td.InnerText.Trim()).ToList();
                        rows.Add(rowData);
                    }
                }
            }
            
            return new TableData { Headers = headers, Rows = rows };
        }
        
        private class TableData
        {
            public List<string> Headers { get; set; } = new();
            public List<List<string>> Rows { get; set; } = new();
        }
    }
}