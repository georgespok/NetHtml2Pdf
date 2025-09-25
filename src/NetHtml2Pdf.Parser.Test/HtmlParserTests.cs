using NetHtml2Pdf.Core.Models;
using Shouldly;

namespace NetHtml2Pdf.Parser.Test
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
        private static TextRunNode CreateTextRun(string text, bool isBold = false, bool isItalic = false, string? color = null, float? fontSize = null, float? padding = null)
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
            // Padding is no longer a property of TextRunNode
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
        public async Task ParseAsync_WithHeadings_CreatesParagraphsWithDefaults()
        {
            // Arrange
            const string html = @"<section>
                <h1>H1</h1>
                <h2>H2</h2>
                <h3>H3</h3>
                <h4>H4</h4>
                <h5>H5</h5>
                <h6>H6</h6>
                </section>";

            // Act
            var nodes = await _htmlParser.ParseAsync(html);

            // Expect a single BlockNode (for <section>) containing 6 ParagraphNode children
            nodes.Count.ShouldBe(1);
            var block = nodes[0].ShouldBeOfType<BlockNode>();
            block.Children.Count.ShouldBe(6);
            var paragraphs = block.Children.Select(Assert.IsType<ParagraphNode>).ToList();

            var labels = new[] { "H1", "H2", "H3", "H4", "H5", "H6" };
            for (var i = 0; i < paragraphs.Count; i++)
            {
                paragraphs[i].TextRuns.Count.ShouldBe(1);
                paragraphs[i].TextRuns[0].Text.ShouldBe(labels[i]);
                paragraphs[i].TextRuns[0].IsBold.ShouldBeTrue();
            }

            // Font sizes should be descending according to defaults
            var sizes = paragraphs.Select(p => p.Style.Text.FontSize.GetValueOrDefault()).ToList();
            sizes.Count.ShouldBe(6);
            for (var i = 0; i < sizes.Count - 1; i++)
            {
                sizes[i].ShouldBeGreaterThan(sizes[i + 1]);
            }
            
            foreach (var p in paragraphs)
            {
                p.Style.Box.MarginTop.GetValueOrDefault().ShouldBeGreaterThan(0);
                p.Style.Box.MarginBottom.GetValueOrDefault().ShouldBeGreaterThan(0);
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

        private static ListItemNode CreateListItem(string text, bool isBold = false, bool isItalic = false, string? color = null, float? fontSize = null)
        {
            return new ListItemNode
            {
                Content =
                [
                    new TextRunNode
                        { Text = text, IsBold = isBold, IsItalic = isItalic, Color = color, FontSize = fontSize }
                ]
            };
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

        [Fact]
        public async Task ParseAsync_WithStylePadding_CreatesParagraphWithStylePadding()
        {
            // Arrange
            const string html = "<p style='padding: 10px;'>Padded text</p>";
            var expectedRuns = new[]
            {
                CreateTextRun("Padded text", padding: 10)
            };

            // Act
            var paragraphNode = await ParseParagraphAsync(html);

            // Assert
            paragraphNode.Style.Box.PaddingLeft.ShouldBe(10);
            paragraphNode.Style.Box.PaddingRight.ShouldBe(10);
            paragraphNode.Style.Box.PaddingTop.ShouldBe(10);
            paragraphNode.Style.Box.PaddingBottom.ShouldBe(10);
        }

        [Fact]
        public async Task ParseParagraph_WithIndividualPadding_ShouldSetCorrectValues()
        {
            // Arrange
            var html = "<p style='padding-left: 15px; padding-right: 20px; padding-top: 5px; padding-bottom: 25px;'>Text with individual padding</p>";

            // Act
            var paragraphNode = await ParseParagraphAsync(html);

            // Assert
            paragraphNode.Style.Box.PaddingLeft.ShouldBe(15);
            paragraphNode.Style.Box.PaddingRight.ShouldBe(20);
            paragraphNode.Style.Box.PaddingTop.ShouldBe(5);
            paragraphNode.Style.Box.PaddingBottom.ShouldBe(25);
        }

        [Fact]
        public async Task ParseAsync_WithTable_CreatesTableNode()
        {
            // Arrange
            var html = "<table tyle=\"border-collapse: collapse; width: 100%;\">" +
                       "<tr>" +
                       "<td style=\"border: 1px solid black; padding: 8px; text-align: left;\">Cell1</td>" +
                       "<td style=\"border: 1px solid black; padding: 8px; text-align: left;\">Cell2</td>" +
                       "</tr>" +
                       "</table>";

            // Act
            var tableNode = await ParseTableAsync(html);
            // Assert
            tableNode.Rows.Count.ShouldBe(1);
            tableNode.Rows[0].Cells.Count.ShouldBe(2);
            tableNode.Rows[0].Cells[0].Content.Count.ShouldBe(1);
            tableNode.Rows[0].Cells[0].Content[0].ShouldBeOfType<TextRunNode>();
            ((TextRunNode)tableNode.Rows[0].Cells[0].Content[0]).Text.ShouldBe("Cell1");
            tableNode.Rows[0].Cells[1].Content.Count.ShouldBe(1);
            tableNode.Rows[0].Cells[1].Content[0].ShouldBeOfType<TextRunNode>();
            ((TextRunNode)tableNode.Rows[0].Cells[1].Content[0]).Text.ShouldBe("Cell2");

        }

        private async Task<TableNode> ParseTableAsync(string html)
        {
            var result = await _htmlParser.ParseAsync(html);
            Assert.Single(result);
            return Assert.IsType<TableNode>(result[0]);
        }


    }
}
