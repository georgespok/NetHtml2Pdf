using Shouldly;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
using Xunit.Abstractions;
using System.Runtime.CompilerServices;

namespace NetHtml2Pdf.Test
{
    public class HtmlConverterTests(ITestOutputHelper output)
    {
        private readonly HtmlConverter _converter = new();

        
        #region Basic Conversion Tests


        [Fact]
        public async Task Convert_ValidHtml_WithHeadings_ReturnsValidPdf()
        {
            // Arrange
            const string html = @"
            <h1>Header1</h1>
            <h2>Header2</h2>
            <h3>Header3</h3>
            <h4>Header4</h4>
            <h5>Header5</h5>
            <h6>Header6</h6>";

            // Act
            var result = await ConvertToPdfAsync(html);

            await SavePdfForInspectionAsync(result);

            // Assert
            AssertValidPdf(result);

            var words = GetPdfWords(result);
            words.Count.ShouldBe(6);
            words[0].Text.ShouldBe("Header1");
            words[1].Text.ShouldBe("Header2");
            words[2].Text.ShouldBe("Header3");
            words[3].Text.ShouldBe("Header4");
            words[4].Text.ShouldBe("Header5");
            words[5].Text.ShouldBe("Header6");    
        }
        
        [Fact]
        public async Task Performance_FivePages_CompletesUnderTwoSeconds()
        {
            // Arrange
            var builder = new PdfDocumentBuilder();
            for (int i = 1; i <= 5; i++)
            {
                builder.AddPdfPage($"<section><h1>Page {i}</h1><p>{new string('X', 2000)}</p></section>");
            }

            // Act
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var pdf = await builder.RenderAsync();
            sw.Stop();

            // Assert
            AssertValidPdf(pdf);
            output.WriteLine($"Elapsed: {sw.ElapsedMilliseconds} ms");
            sw.Elapsed.TotalSeconds.ShouldBeLessThan(2.0);
        }

        [Fact]
        public async Task Convert_Determinism_SameInputSameWords()
        {
            // Arrange
            const string html = "<section><h1>Title</h1><p>Hello World</p></section>";

            // Act
            var pdf1 = await ConvertToPdfAsync(html);
            var pdf2 = await ConvertToPdfAsync(html);

            // Assert
            AssertValidPdf(pdf1);
            AssertValidPdf(pdf2);
            var words1 = GetPdfWords(pdf1).Select(w => (w.Text, w.BoundingBox.Left, w.BoundingBox.Bottom)).ToArray();
            var words2 = GetPdfWords(pdf2).Select(w => (w.Text, w.BoundingBox.Left, w.BoundingBox.Bottom)).ToArray();
            words2.ShouldBe(words1);
        }

        [Fact]
        public async Task Convert_EmptyInput_ThrowsArgumentException()
        {
            await Should.ThrowAsync<ArgumentException>(() => _converter.ConvertToPdfBytes(""));
        }

        [Fact]
        public async Task Convert_CssSubset_AppliesSupportedIgnoresUnsupported()
        {
            // Arrange
            const string html = "<p style='color: red; font-size: 18px; padding-left: 50px; display: flex;'>Styled Text</p>";

            // Act
            var pdfBytes = await ConvertToPdfAsync(html);

            // Assert
            AssertValidPdf(pdfBytes);

            var words = GetPdfWords(pdfBytes);
            words[0].Text.ShouldBe("Styled");
            // Expect left position: 1" margin (72) + 50px padding = 122
            words[0].BoundingBox.Left.ShouldBe(122);
            // display:flex should not affect layout; text present as usual
        }

        [Fact]
        public async Task Convert_UnsupportedElements_IgnoresElementKeepsInnerText()
        {
            // Arrange: wrap text in unsupported tags like <marquee> and <blink>
            const string html = "<p>Start <marquee>Inner</marquee> <blink>Text</blink> End</p>";

            // Act
            var pdfBytes = await ConvertToPdfAsync(html);

            // Assert
            AssertValidPdf(pdfBytes);
            var text = ExtractTextFromPdf(pdfBytes);
            text.ShouldContain("Start");
            text.ShouldContain("Inner");
            text.ShouldContain("Text");
            text.ShouldContain("End");
        }

        [Fact]
        public async Task Convert_ValidTable_ReturnsValidPdfWithContent()
        {
            // Arrange
            var html = "<div style='padding-left: 200px'>" + CreateTableHtml(
                ["Name", "Age", "City"],
                [["Alice", "30", "New York"],
                ["Bob", "25", "London"]]
            ) + "</div>";

            // Act
            var result = await ConvertToPdfAsync(html);

            await SavePdfForInspectionAsync(result);

            // Assert
            AssertValidPdf(result);
            AssertPdfContainsText(result, "Name", "Age", "City", "Alice", "30", "New York", "Bob", "25", "London");
        }

        [Theory]
        [InlineData(new[] { "Name" }, new[] { "Alice" }, new[] { "Bob" })]
        [InlineData(new[] { "ID", "Name", "Age", "City", "Country", "Salary" }, 
                   new[] { "1", "Alice", "30", "New York", "USA", "50000" }, 
                   new[] { "2", "Bob", "25", "London", "UK", "45000" })]
        [InlineData(new[] { "Name", "Age", "City" }, 
                   new[] { "Alice", "", "New York" }, 
                   new[] { "", "25", "" })]
        public async Task Convert_TableVariations_ReturnsValidPdf(string[] headers, params string[][] rows)
        {
            // Arrange
            var html = CreateTableHtml(headers, rows);

            // Act
            var result = await ConvertToPdfAsync(html);

            // Assert
            AssertValidPdf(result);
        }

        #endregion

        #region Error Handling Tests

        [Theory]
        [InlineData("invalid html")]
        [InlineData("<p>Unclosed tag")]
        public async Task Convert_InvalidHtml_ReturnsValidPdf(string html)
        {
            // Act
            var result = await ConvertToPdfAsync(html);

            // Assert
            AssertValidPdf(result);
        }

        [Fact]
        public async Task Convert_NullHtml_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _converter.ConvertToPdfBytes(null!));
        }

        #endregion

        #region Content Verification Tests

        [Fact]
        public async Task Convert_TableWithSingleColumn_ContainsExpectedContent()
        {
            // Arrange
            var html = CreateTableHtmlNoBorders(["Name"], ["Alice"], ["Bob"]);

            // Act
            var result = await ConvertToPdfAsync(html);

            // Assert
            AssertValidPdf(result);
            AssertPdfContainsText(result, "Name", "Alice", "Bob");
        }

        [Fact]
        public async Task Convert_TableWithEmptyCells_HandlesEmptyContent()
        {
            // Arrange
            var html = CreateTableHtml(
                ["Name", "Age", "City"],
                ["Alice", "", "New York"],
                ["", "25", ""]
            );

            // Act
            var result = await ConvertToPdfAsync(html);

            // Assert
            AssertValidPdf(result);
            AssertPdfContainsText(result, "Name", "Age", "City", "Alice", "New York", "25");
        }

        #endregion

        #region Complex Integration Tests

        [Fact]
        public async Task Convert_ValidTable_PreservesContentStructure()
        {
            // Arrange
            var originalHtml = CreateTableHtml(
                ["Name", "Age", "City"],
                ["Alice", "30", "New York"],
                ["Bob", "25", "London"]
            );

            // Act
            var pdfBytes = await ConvertToPdfAsync(originalHtml);

            await SavePdfForInspectionAsync(pdfBytes);

            // Assert
            AssertValidPdf(pdfBytes);
            AssertPdfContainsText(pdfBytes, "Name", "Alice", "Bob");
        }

        [Fact]
        public async Task Convert_ValidTable_WithBorders()
        {
            // Arrange
            var originalHtml = CreateTableHtml(
                ["Name", "Age", "City"],
                ["Alice", "30", "New York"], ["Bob", "25", "London"]);

            // Act
            var pdfBytes = await ConvertToPdfAsync(originalHtml);

            await SavePdfForInspectionAsync(pdfBytes);

            // Assert
            AssertValidPdf(pdfBytes);
            AssertPdfContainsText(pdfBytes, "Name", "Alice", "Bob");
        }

        [Fact]
        public async Task Convert_ComplexTable_MaintainsDataIntegrity()
        {
            // Arrange
            var originalHtml = CreateTableHtml(
                ["ID", "Name", "Age", "City", "Country", "Salary"],
                ["1", "Alice Johnson", "30", "New York", "USA", "$50,000"],
                ["2", "Bob Smith", "25", "London", "UK", "£45,000"],
                ["3", "Charlie Brown", "28", "Berlin", "Germany", "€55,000"]
            );

            // Act
            var pdfBytes = await ConvertToPdfAsync(originalHtml);

            // Assert
            AssertValidPdf(pdfBytes);
            AssertPdfContainsText(pdfBytes, "Alice", "Bob", "Charlie", "New York", "London", "Berlin");
        }

        [Fact]
        public async Task Convert_LineBreaks_PreservesLineBreakStructure()
        {
            // Arrange
            var html = "<p>Line1<br>Line2<br>Line3</p>";

            // Act
            var pdfBytes = await ConvertToPdfAsync(html);

            // Assert
            AssertValidPdf(pdfBytes);
            
            await SavePdfForInspectionAsync(pdfBytes);

            // Verify words are on different lines (line breaks create separate lines)
            var words = GetPdfWords(pdfBytes);
            words[0].Text.ShouldBe("Line1");
            words[1].Text.ShouldBe("Line2");
            words[2].Text.ShouldBe("Line3");
            Assert.True(WordsInOneLine(words, false)); // Should NOT be in one line
        }

        [Fact]
        public async Task Convert_ListItems_RendersItemTexts()
        {
            // Arrange
            var html = "<ol><li>Item1</li><li>Item2</li><li>Item3</li></ol>";

            // Act
            var pdfBytes = await ConvertToPdfAsync(html);

            // Assert
            AssertValidPdf(pdfBytes);
            
            await SavePdfForInspectionAsync(pdfBytes);

            // Verify list items are rendered correctly
            var words = GetPdfWords(pdfBytes);
            words.Count.ShouldBe(6); // 3 bullets + 3 items
            words[0].Text.ShouldBe("1.");
            words[1].Text.ShouldBe("Item1");
            words[2].Text.ShouldBe("2.");
            words[3].Text.ShouldBe("Item2");
            words[4].Text.ShouldBe("3.");
            words[5].Text.ShouldBe("Item3");
        }

        [Fact]
        public async Task Convert_ParagraphWithPadding_RendersPadding()
        {
            // Arrange
            const string html = "<p style='padding: 100px;'>Padded Text</p>";

            // Act
            var pdfBytes = await ConvertToPdfAsync(html);

            // Assert
            AssertValidPdf(pdfBytes);
            
            await SavePdfForInspectionAsync(pdfBytes);

            // Verify list items are rendered correctly
            var words = GetPdfWords(pdfBytes);
            words[0].Text.ShouldBe("Padded");
            words[0].BoundingBox.Left.ShouldBe(172);
        }
        #endregion

        [Fact]
        public async Task Builder_AddPdfPage_RendersMultiplePages()
        {
            // Arrange
            var builder = new PdfDocumentBuilder();
            builder.AddPdfPage("<section><h1>PageOne</h1><p>Hello</p></section>");
            builder.AddPdfPage("<section><h1>PageTwo</h1><p>World</p></section>");

            // Act
            var pdf = await builder.RenderAsync();

            // Assert
            AssertValidPdf(pdf);
            using var doc = PdfDocument.Open(pdf);
            doc.NumberOfPages.ShouldBe(2);

            var page1Text = doc.GetPage(1).Text;
            var page2Text = doc.GetPage(2).Text;
            page1Text.ShouldContain("PageOne");
            page1Text.ShouldContain("Hello");
            page2Text.ShouldContain("PageTwo");
            page2Text.ShouldContain("World");
        }

        [Fact]
        public async Task Builder_HeaderFooter_RenderOnEachPage()
        {
            // Arrange
            var builder = new PdfDocumentBuilder();
            builder.SetHeaderHtml("<div><strong>HeaderText</strong></div>");
            builder.SetFooterHtml("<div><em>FooterText</em></div>");
            builder.AddPdfPage("<section><h1>Page 1</h1><p>Alpha</p></section>");
            builder.AddPdfPage("<section><h1>Page 2</h1><p>Beta</p></section>");

            // Act
            var pdf = await builder.RenderAsync();

            // Assert
            AssertValidPdf(pdf);
            using var doc = PdfDocument.Open(pdf);
            doc.NumberOfPages.ShouldBe(2);
            var p1 = doc.GetPage(1).Text;
            var p2 = doc.GetPage(2).Text;
            p1.ShouldContain("HeaderText");
            p1.ShouldContain("FooterText");
            p2.ShouldContain("HeaderText");
            p2.ShouldContain("FooterText");
        }

        #region Helper Methods

        /// <summary>
        /// Converts HTML to PDF and returns the PDF bytes
        /// </summary>
        private async Task<byte[]> ConvertToPdfAsync(string html)
        {
            return await _converter.ConvertToPdfBytes(html);
        }

        /// <summary>
        /// Asserts that PDF bytes are valid and not empty
        /// </summary>
        private static void AssertValidPdf(byte[] pdfBytes)
        {
            Assert.NotNull(pdfBytes);
            Assert.True(pdfBytes.Length > 0);
            Assert.Equal(0x25, pdfBytes[0]); // PDF file starts with %PDF
            Assert.Equal(0x50, pdfBytes[1]); // P
            Assert.Equal(0x44, pdfBytes[2]); // D
            Assert.Equal(0x46, pdfBytes[3]); // F
        }

        /// <summary>
        /// Asserts that PDF contains all specified text content
        /// </summary>
        private void AssertPdfContainsText(byte[] pdfBytes, params string[] expectedTexts)
        {
            var extractedText = ExtractTextFromPdf(pdfBytes);
            foreach (var expectedText in expectedTexts)
            {
                Assert.Contains(expectedText, extractedText);
            }
        }

		/// <summary>
		/// Saves PDF bytes to a temp file and logs the path for inspection.
		/// When no file name is provided, uses the calling test method's name.
		/// </summary>
		private async Task SavePdfForInspectionAsync(
			byte[] pdfBytes,
			string? fileName = null,
			[CallerMemberName] string? callerName = null)
		{
			var baseName = string.IsNullOrWhiteSpace(fileName) ? (callerName ?? "output") : fileName;
			var safeBase = MakeSafeFileName(baseName);
			var finalFileName = safeBase.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? safeBase : $"{safeBase}.pdf";
			var tempPath = Path.Combine(Path.GetTempPath(), finalFileName);
			await File.WriteAllBytesAsync(tempPath, pdfBytes);
			output.WriteLine($"PDF saved to: {tempPath}");
		}

		private static string MakeSafeFileName(string name)
		{
			var invalidChars = Path.GetInvalidFileNameChars();
			var builder = new System.Text.StringBuilder(name.Length);
			foreach (var ch in name)
			{
				builder.Append(invalidChars.Contains(ch) ? '_' : ch);
			}
			return builder.ToString().Trim();
		}

        private static string CreateTableHtml(string[] headers, params string[][] rows) => 
            CreateTableHtml(true, headers, rows);

        private static string CreateTableHtmlNoBorders(string[] headers, params string[][] rows) => 
            CreateTableHtml(false, headers, rows);

        private static string CreateTableHtml(bool withBorders, string[] headers, params string[][] rows)
        {
            if (rows == null || rows.Length == 0)
            {
                throw new ArgumentException("At least one row is required", nameof(rows));
            }

            var html = new System.Text.StringBuilder();
            html.AppendLine("<table>");
            if (withBorders)
            {
                html.AppendLine("<style>");
                html.AppendLine("table { border-collapse: collapse; }");
                html.AppendLine("th, td { border: 1px solid black; padding: 5px; }");
                html.AppendLine("</style>");
            }
            html.AppendLine("<thead>");
            html.AppendLine("<tr>");
            foreach (var header in headers)
            {
                html.AppendLine($"<th>{header}</th>");
            }
            html.AppendLine("</tr>");
            html.AppendLine("</thead>");
            html.AppendLine("<tbody>");
            foreach (var row in rows)
            {
                html.AppendLine("<tr>");
                foreach (var cell in row)
                {
                    html.AppendLine($"<td>{cell}</td>");
                }
                html.AppendLine("</tr>");
            }
            html.AppendLine("</tbody>");
            html.AppendLine("</table>");
            return html.ToString();
        }
        
        /// <summary>
        /// Extracts text content from PDF bytes for verification
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
        /// Checks if words are positioned on the same line based on their bottom Y-coordinates
        /// </summary>
        private static bool WordsInOneLine(List<Word> words, bool shouldBeInOneLine)
        {
            if (words.Count == 0)
                return true;

            var distinctBottomPositions = words
                .Select(word => word.BoundingBox.Bottom)
                .Distinct()
                .Count();

            return shouldBeInOneLine 
                ? distinctBottomPositions == 1 
                : distinctBottomPositions == words.Count;
        }

        /// <summary>
        /// Extracts all words from PDF pages for layout analysis
        /// </summary>
        private static List<Word> GetPdfWords(byte[] pdfBytes)
        {
            using var document = PdfDocument.Open(pdfBytes);
            
            var words = new List<Word>();
            
            for (var pageNumber = 1; pageNumber <= document.NumberOfPages; pageNumber++)
            {
                var page = document.GetPage(pageNumber);
                var pageWords = page.GetWords(NearestNeighbourWordExtractor.Instance);
                words.AddRange(pageWords);
            }

            return words;
        }

        private class TableData
        {
            public List<string> Headers { get; set; } = new();
            public List<List<string>> Rows { get; set; } = new();
        }

        #endregion
    }
}