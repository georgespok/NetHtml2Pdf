using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Parsing;
using Shouldly;

namespace NetHtml2Pdf.Test
{
    /// <summary>
    /// Unit tests for the HtmlParser class
    /// </summary>
    public class HtmlParserTests
    {
        private readonly HtmlParser _htmlParser = new();

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
        private static TextRunNode CreateTextRun(string text, bool isBold = false, bool isItalic = false, string color = null, float? fontSize = null)
        {
            return new TextRunNode { Text = text, IsBold = isBold, IsItalic = isItalic, Color = color, FontSize = fontSize };
        }

        /// Helper method to assert text run properties
        /// </summary>
        private static void AssertTextRun(TextRunNode textRun, TextRunNode expectedTextRun)
        {
            textRun.Text.ShouldBe(expectedTextRun.Text);
            textRun.IsBold.ShouldBe(expectedTextRun.IsBold);
            textRun.IsItalic.ShouldBe(expectedTextRun.IsItalic);
            textRun.Color.ShouldBe(expectedTextRun.Color);
            textRun.FontSize.ShouldBe(expectedTextRun.FontSize);
        }

        /// <summary>
        /// Helper method to assert multiple text runs
        /// </summary>
        private static void AssertTextRuns(List<TextRunNode> actualRuns, params TextRunNode[] expectedRuns)
        {
            Assert.Equal(expectedRuns.Length, actualRuns.Count);
            for (var i = 0; i < expectedRuns.Length; i++)
            {
                AssertTextRun(actualRuns[i], expectedRuns[i]);
            }
        }

        [Fact]
        public async Task ParseAsync_WithLineBreaks_CreatesParagraphWithLineBreakElements()
        {
            // Arrange
            const string html = "<p>Line1<br>Line2</p>";
            var expectedRuns = new[]
            {
                CreateTextRun("Line1"),
                CreateTextRun("\n"),
                CreateTextRun("Line2")
            };

            // Act
            var paragraphNode = await ParseParagraphAsync(html);

            // Assert
            AssertTextRuns(paragraphNode.TextRuns, expectedRuns);
        }

        [Fact]
        public async Task ParseAsync_WithListItems_CreatesListNodeElements()
        {
            // Arrange
            const string html = "<ol><li>Line1</li><li>Line2</li></ol>";
            var expectedRuns = new[]
            {
                CreateListItem("Line1"),
                CreateListItem("Line2")
            };

            // Act
            var listItemNodes = await _htmlParser.ParseAsync(html);

            // Assert
            AssertListItems(listItemNodes, expectedRuns);
        }

        private void AssertListItems(List<DocumentNode> documentNodes, ListItemNode[] expectedRuns)
        {
            Assert.Equal(1, documentNodes.Count);

            for (var i = 0; i < expectedRuns.Length; i++)
            {
                var listItem = ((ListNode)documentNodes[0]).Items[i];
                var listItemNode = Assert.IsType<ListItemNode>(listItem);
                
                var textRuns = listItemNode.Content.OfType<TextRunNode>().ToList();
                
                AssertTextRuns(textRuns, expectedRuns[i].Content.OfType<TextRunNode>().ToArray());
            }
        }

        private static ListItemNode CreateListItem(string text, bool isBold = false, bool isItalic = false, string color = null, float? fontSize = null)
        {
            return new ListItemNode { Content = new List<DocumentNode> { new TextRunNode { Text = text, IsBold = isBold, IsItalic = isItalic, Color = color, FontSize = fontSize } } };
        }

        [Fact]
        public async Task ParseAsync_WithMixedInlineElements_CreatesParagraphWithAllElements()
        {
            // Arrange
            const string html = "<p>Start <strong>bold</strong> and <em>italic</em> with <br>line break</p>";
            var expectedRuns = new[]
            {
                CreateTextRun("Start "),
                CreateTextRun("bold", isBold: true),
                CreateTextRun(" and "),
                CreateTextRun("italic", isItalic: true),
                CreateTextRun(" with "),
                CreateTextRun("\n"),
                CreateTextRun("line break")
            };

            // Act
            var paragraphNode = await ParseParagraphAsync(html);

            // Assert
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
            AssertTextRun(paragraphNode.TextRuns[0], new TextRunNode { Text = expectedText, IsBold = expectedBold, IsItalic = expectedItalic });
        }

        [Theory]
        [InlineData("<p></p>")]
        [InlineData("<p><span></span></p>")]
        public async Task ParseAsync_WithEmptyContent_ReturnsEmptyTextRuns(string html)
        {
            // Act
            var paragraphNode = await ParseParagraphAsync(html);

            // Assert
            Assert.Empty(paragraphNode.TextRuns);
        }

        [Fact]
        public async Task ParseAsync_WithStyleAttributes_CreatesParagraphWithStyleAttributes()
        {
            // Arrange
            const string html = "<p style='color: red; font-size: 16px;'>Red text</p>";
            var expectedRuns = new[]
            {
                CreateTextRun("Red text", color: "#FF0000", fontSize: 16)
            };

            // Act
            var paragraphNode = await ParseParagraphAsync(html);

            // Assert
            AssertTextRuns(paragraphNode.TextRuns, expectedRuns);
        }
    }
}
