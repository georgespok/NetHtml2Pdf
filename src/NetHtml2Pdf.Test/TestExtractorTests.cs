using AngleSharp;
using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Parsing.Utilities;

namespace NetHtml2Pdf.Test
{
    /// <summary>
    /// Unit tests for the TextExtractor utility class
    /// </summary>
    public class TestExtractorTests
    {
        private readonly IBrowsingContext _browsingContext;

        public TestExtractorTests()
        {
            var config = Configuration.Default;
            _browsingContext = BrowsingContext.New(config);
        }

        /// <summary>
        /// Helper method to extract text runs from HTML
        /// </summary>
        private List<TextRunNode> ExtractTextRunsFromHtml(string html)
        {
            var document = _browsingContext.OpenAsync(req => req.Content(html)).Result;
            var element = document.QuerySelector("p")!;
            return TextExtractor.ExtractTextRuns(element);
        }

        /// <summary>
        /// Helper method to create a text run with specified properties
        /// </summary>
        private static TextRunNode CreateTextRun(string text, bool isBold = false, bool isItalic = false)
        {
            return new TextRunNode { Text = text, IsBold = isBold, IsItalic = isItalic };
        }

        /// <summary>
        /// Helper method to assert text run properties
        /// </summary>
        private static void AssertTextRun(TextRunNode textRun, string expectedText, bool expectedBold = false, bool expectedItalic = false)
        {
            Assert.Equal(expectedText, textRun.Text);
            Assert.Equal(expectedBold, textRun.IsBold);
            Assert.Equal(expectedItalic, textRun.IsItalic);
        }

        [Theory]
        [InlineData("<p>Hello World</p>", "Hello World", false, false)]
        [InlineData("<p><strong>Bold Text</strong></p>", "Bold Text", true, false)]
        [InlineData("<p><b>Bold Text</b></p>", "Bold Text", true, false)]
        [InlineData("<p><em>Italic Text</em></p>", "Italic Text", false, true)]
        [InlineData("<p><i>Italic Text</i></p>", "Italic Text", false, true)]
        public void ExtractTextRuns_SingleTextRun_ReturnsCorrectFormatting(string html, string expectedText, bool expectedBold, bool expectedItalic)
        {
            // Act
            var result = ExtractTextRunsFromHtml(html);

            // Assert
            Assert.Single(result);
            AssertTextRun(result[0], expectedText, expectedBold, expectedItalic);
        }

        [Fact]
        public void ExtractTextRuns_MixedFormatting_ReturnsMultipleTextRuns()
        {
            // Arrange
            var html = "<p>Normal <strong>bold</strong> and <em>italic</em> text</p>";
            var expectedRuns = new[]
            {
                CreateTextRun("Normal"),
                CreateTextRun("bold", isBold: true),
                CreateTextRun("and"),
                CreateTextRun("italic", isItalic: true),
                CreateTextRun("text")
            };

            // Act
            var result = ExtractTextRunsFromHtml(html);

            // Assert
            Assert.Equal(expectedRuns.Length, result.Count);
            for (int i = 0; i < expectedRuns.Length; i++)
            {
                AssertTextRun(result[i], expectedRuns[i].Text, expectedRuns[i].IsBold, expectedRuns[i].IsItalic);
            }
        }

        [Theory]
        [InlineData("<p></p>")]
        [InlineData("<p>   \n\t  </p>")]
        [InlineData("<p><strong></strong><em></em></p>")]
        public void ExtractTextRuns_EmptyOrWhitespaceContent_ReturnsEmptyList(string html)
        {
            // Act
            var result = ExtractTextRunsFromHtml(html);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void ExtractTextRuns_ComplexMixedContent_ReturnsCorrectTextRuns()
        {
            // Arrange
            var html = "<p>Start <strong>bold text</strong> middle <em>italic text</em> end</p>";
            var expectedRuns = new[]
            {
                CreateTextRun("Start"),
                CreateTextRun("bold text", isBold: true),
                CreateTextRun("middle"),
                CreateTextRun("italic text", isItalic: true),
                CreateTextRun("end")
            };

            // Act
            var result = ExtractTextRunsFromHtml(html);

            // Assert
            Assert.Equal(expectedRuns.Length, result.Count);
            for (int i = 0; i < expectedRuns.Length; i++)
            {
                AssertTextRun(result[i], expectedRuns[i].Text, expectedRuns[i].IsBold, expectedRuns[i].IsItalic);
            }
        }

        [Fact]
        public void ExtractTextRuns_WithLineBreaks_IgnoresBrTagsAndReturnsOnlyTextContent()
        {
            // Arrange
            var html = "<p>Line1<br> Line2</p>";
            
            // Act
            var result = ExtractTextRunsFromHtml(html);

            // Assert
            // Note: ExtractTextRuns only extracts text content and formatting from elements
            // Line breaks (<br> tags) are handled at a higher level in the parsing pipeline
            // by the LineBreakElementConverter, not in the text extraction phase
            Assert.Equal(2, result.Count);
            AssertTextRun(result[0], "Line1");
            AssertTextRun(result[1], "Line2");
        }
    }
}
