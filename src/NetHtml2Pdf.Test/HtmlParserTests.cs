using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Parsing;

namespace NetHtml2Pdf.Test
{
    /// <summary>
    /// Unit tests for the HtmlParser class
    /// </summary>
    public class HtmlParserTests
    {
        private readonly HtmlParser _htmlParser;

        public HtmlParserTests()
        {
            _htmlParser = new HtmlParser();
        }

        /// <summary>
        /// Helper method to parse HTML and return the first paragraph node
        /// </summary>
        private async Task<ParagraphNode> ParseParagraphAsync(string html)
        {
            var result = await _htmlParser.ParseAsync(html);
            Assert.Single(result);
            return Assert.IsType<ParagraphNode>(result[0]);
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

        /// <summary>
        /// Helper method to assert multiple text runs
        /// </summary>
        private static void AssertTextRuns(List<TextRunNode> actualRuns, params TextRunNode[] expectedRuns)
        {
            Assert.Equal(expectedRuns.Length, actualRuns.Count);
            for (int i = 0; i < expectedRuns.Length; i++)
            {
                AssertTextRun(actualRuns[i], expectedRuns[i].Text, expectedRuns[i].IsBold, expectedRuns[i].IsItalic);
            }
        }

        [Fact]
        public async Task ParseAsync_WithLineBreaks_CreatesParagraphWithLineBreakElements()
        {
            // Arrange
            var html = "<p>Line1<br>Line2</p>";
            var expectedRuns = new[]
            {
                CreateTextRun("Line1"),
                CreateTextRun("\n"),
                CreateTextRun("Line2")
            };

            // Act
            var paragraphNode = await ParseParagraphAsync(html);

            // Assert
            // NEW BEHAVIOR: The paragraph now contains 3 text runs including the <br> tag
            // The enhanced ParagraphElementConverter now properly handles nested elements
            AssertTextRuns(paragraphNode.TextRuns, expectedRuns);
        }

        [Fact]
        public async Task ParseAsync_WithMixedInlineElements_CreatesParagraphWithAllElements()
        {
            // Arrange
            var html = "<p>Start <strong>bold</strong> and <em>italic</em> with <br>line break</p>";
            var expectedRuns = new[]
            {
                CreateTextRun("Start"),
                CreateTextRun("bold", isBold: true),
                CreateTextRun("and"),
                CreateTextRun("italic", isItalic: true),
                CreateTextRun("with"),
                CreateTextRun("\n"),
                CreateTextRun("line break")
            };

            // Act
            var paragraphNode = await ParseParagraphAsync(html);

            // Assert
            // The text extraction trims whitespace, so we get: "Start", "bold", "and", "italic", "with", "\n", "line break"
            AssertTextRuns(paragraphNode.TextRuns, expectedRuns);
        }

        [Theory]
        [InlineData("<p>Plain text</p>", "Plain text")]
        [InlineData("<p><strong>Bold text</strong></p>", "Bold text", true, false)]
        [InlineData("<p><em>Italic text</em></p>", "Italic text", false, true)]
        [InlineData("<p><strong><em>Bold italic</em></strong></p>", "Bold italic", true, true)]
        public async Task ParseAsync_WithSingleInlineElement_CreatesCorrectFormatting(string html, string expectedText, bool expectedBold = false, bool expectedItalic = false)
        {
            // Act
            var paragraphNode = await ParseParagraphAsync(html);

            // Assert
            Assert.Single(paragraphNode.TextRuns);
            AssertTextRun(paragraphNode.TextRuns[0], expectedText, expectedBold, expectedItalic);
        }

        [Theory]
        [InlineData("<p></p>")]
        [InlineData("<p>   </p>")]
        [InlineData("<p><span></span></p>")]
        public async Task ParseAsync_WithEmptyContent_ReturnsEmptyTextRuns(string html)
        {
            // Act
            var paragraphNode = await ParseParagraphAsync(html);

            // Assert
            Assert.Empty(paragraphNode.TextRuns);
        }
    }
}
