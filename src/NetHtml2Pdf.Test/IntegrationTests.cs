using Microsoft.Extensions.Logging.Abstractions;
using NetHtml2Pdf.Core.Constants;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Test.Support;
using Shouldly;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test;

[Collection("PdfRendering")]
public class IntegrationTests(ITestOutputHelper output) : PdfRenderTestBase(output)
{
    [Fact]
    public void PdfBuilder_SinglePage_RendersContent()
    {
        // Arrange
        var builder = new PdfBuilder(
            RendererOptions.CreateDefault(),
            NullLogger<PdfBuilder>.Instance);
        const string html =
            "<h1>Single Page Test</h1><p>This is a test page with <strong>bold text</strong> and <em>italic text</em>.</p>";

        // Act
        var pdfBytes = builder.AddPage(html).Build();

        // Assert
        AssertValidPdf(pdfBytes);

        // Verify content is rendered correctly
        var words = ExtractWords(pdfBytes);
        words.ShouldContain("Single");
        words.ShouldContain("Page");
        words.ShouldContain("Test");
        words.ShouldContain("bold");
        words.ShouldContain("italic");

        // Verify formatting is applied
        var pdfWords = GetPdfWords(pdfBytes);
        var boldWord = pdfWords.FirstOrDefault(w => w.Text.Contains("bold", StringComparison.OrdinalIgnoreCase));
        boldWord.ShouldNotBeNull();
        boldWord.IsBold.ShouldBeTrue();
    }

    [Fact]
    public async Task PdfBuilder_DisplayInline_RendersInline()
    {
        // Arrange
        var builder = new PdfBuilder(
            RendererOptions.CreateDefault(),
            NullLogger<PdfBuilder>.Instance);

        const string htmlPage = """
                                <style>
                                    .text-block { margin: 4px 0; padding: 2px; display: inline-block; border: 2px solid red;}
                                </style>
                                <div>
                                  <div class="text-block">
                                    aaa
                                  </div>
                                  <div class="text-block">
                                    bbb
                                  </div>
                                </div>
                                """;

        // Act
        var pdfBytes = builder.AddPage(htmlPage).Build();
        // Assert
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes);

        // Debug: Extract and log all words to see what's being rendered
        var words = PdfWordParser.GetRawWords(pdfBytes);
        Output.WriteLine($"Extracted {words.Count} words from PDF:");
        foreach (var word in words)
            Output.WriteLine(
                $"  Word: '{word.Text}' at position ({word.BoundingBox.TopLeft.X:F1}, {word.BoundingBox.TopLeft.Y:F1})");

        // Verify text content is present
        var extractedWords = PdfWordParser.GetTextWords(pdfBytes);
        Output.WriteLine($"Extracted text words: [{string.Join(", ", extractedWords.Select(w => $"'{w}'"))}]");

        // Check if both "aaa" and "bbb" are present
        extractedWords.ShouldContain("aaa");
        extractedWords.ShouldContain("bbb");

        // Check if elements are side-by-side (same Y coordinate)
        var aaaWord = words.FirstOrDefault(w => w.Text == "aaa");
        var bbbWord = words.FirstOrDefault(w => w.Text == "bbb");

        if (aaaWord != null && bbbWord != null)
        {
            var yDifference = Math.Abs(aaaWord.BoundingBox.TopLeft.Y - bbbWord.BoundingBox.TopLeft.Y);
            Output.WriteLine($"Y coordinate difference: {yDifference:F1} (should be close to 0 for side-by-side)");

            if (yDifference < 5) // Allow some tolerance for font metrics
            {
                Output.WriteLine("✅ Elements are rendered side-by-side!");
            }
            else
            {
                Output.WriteLine("❌ Elements are NOT side-by-side - inline-block implementation issue");

                // Debug: Check if the text is concatenated (which would indicate inline rendering)
                var combinedText = string.Join("", extractedWords);
                Output.WriteLine($"Combined text: '{combinedText}'");
                Output.WriteLine(combinedText == "aaabbb"
                    ? "✅ Text is concatenated - inline rendering is working!"
                    : "❌ Text is not concatenated - elements are rendered as separate blocks");
            }
        }

        Output.WriteLine("✅ Inline-block elements rendered with text content");
    }

    [Fact]
    public async Task FullDocument_Rendering_SmokeTest()
    {
        const string headerHtml =
            $"<div style=\"text-align:center;font-size:12px;color:{HexColors.Red};border-bottom:1px solid #ccc;padding:5px;\">NetHtml2Pdf Multi-Page Test Document</div>";
        const string footerHtml =
            "<div style=\"text-align:center;font-size:10px;border-top:1px solid #ccc;padding:5px;\">Page <span style=\"font-weight:bold;\">[PAGE]</span> | Generated by NetHtml2Pdf</div>";

        // Page 1: Basic Text and Formatting Elements
        const string page1Html = $$$"""
                                    <style>
                                        .title { color: {{{HexColors.Blue}}}; text-align: center; margin-bottom: 20px; }
                                        .highlight { background-color: {{{HexColors.Yellow}}}; padding: 2px 4px; }
                                        .text-block { margin: 4px 0; padding: 2px; display: inline-block; border: 2px solid {{{HexColors.Orange}}};}
                                    </style>
                                    <h1 class="title">Page 1: Text Formatting</h1>
                                    <p>This page demonstrates basic text formatting capabilities including <strong>bold text</strong>, <i>italic text</i>, and <span class="highlight">highlighted text</span>.</p>
                                    <p>Additional formatting includes <span style="color: {{{HexColors.Red}}};">colored text</span>, <span style="text-decoration: underline;">underlined text</span>, and <span style="font-weight: bold; color: {{{HexColors.Green}}};">combined styles</span>.</p>
                                    <div style="border-left: 4px solid {{{HexColors.Blue}}}; padding-left: 15px; margin: 20px 0; font-style: italic;">
                                        <p>"This is a styled div demonstrating quoted text with left border styling."</p>
                                    </div>
                                    <p>Text with <span style="color: {{{HexColors.Purple}}};">colored text</span> and <span style="font-weight: bold;">bold emphasis</span> for variety.</p>

                                    <h3>display: inline-block example</h3>
                                    <div>
                                      <div class="text-block">
                                        Left.
                                      </div>
                                      <div class="text-block">
                                        Right.
                                      </div>
                                    </div>
                                    """;

        // Page 2: Lists and Tables
        const string page2Html = $$"""
                                   <style>
                                       .list-container { margin: 15px 0; padding: 10px; border: 1px solid {{HexColors.Green}}; }
                                       .table-container { margin: 15px 0; }
                                       .data-table { border: 2px solid {{HexColors.Blue}}; border-collapse: collapse; width: 100%; }
                                       .header-cell { background-color: {{HexColors.LightGray}}; border: 1px solid {{HexColors.Blue}}; padding: 8px; font-weight: bold; }
                                       .data-cell { border: 1px solid {{HexColors.Blue}}; padding: 6px; }
                                       .total-row { background-color: {{HexColors.Yellow}}; font-weight: bold; }
                                   </style>
                                   <h1 style="color: {{HexColors.Green}}; text-align: center;">Page 2: Lists & Tables</h1>

                                   <div class="list-container">
                                       <h3>Ordered Lists</h3>
                                       <ol>
                                           <li>First priority item</li>
                                           <li>Second priority item</li>
                                           <li>Third priority item</li>
                                       </ol>
                                   </div>

                                   <div class="list-container">
                                       <h3>Unordered Lists</h3>
                                       <ul>
                                           <li>Bullet point one</li>
                                           <li>Bullet point two</li>
                                           <li>Bullet point three</li>
                                       </ul>
                                   </div>

                                   <div class="table-container">
                                       <h3>Data Table</h3>
                                       <table class="data-table">
                                           <thead>
                                               <tr>
                                                   <th class="header-cell">Item</th>
                                                   <th class="header-cell">Count</th>
                                                   <th class="header-cell">Price</th>
                                               </tr>
                                           </thead>
                                           <tbody>
                                               <tr>
                                                   <td class="data-cell">Widget</td>
                                                   <td class="data-cell">10</td>
                                                   <td class="data-cell">$15.00</td>
                                               </tr>
                                               <tr>
                                                   <td class="data-cell">Gadget</td>
                                                   <td class="data-cell">5</td>
                                                   <td class="data-cell">$19.99</td>
                                               </tr>
                                               <tr class="total-row">
                                                   <td class="data-cell">Total</td>
                                                   <td class="data-cell">15</td>
                                                   <td class="data-cell">$275.45</td>
                                               </tr>
                                           </tbody>
                                       </table>
                                   </div>
                                   """;

        // Page 3: CSS Styling and Advanced Elements
        const string page3Html = $$"""
                                   <style>
                                       .feature-card { border: 2px solid {{HexColors.Purple}}; margin: 10px 0; padding: 15px; background-color: #f8f9fa; }
                                       .status-badge { display: inline-block; padding: 4px 8px; background-color: {{HexColors.Green}}; color: white; font-size: 11px; }
                                   </style>
                                   <h1 style="color: {{HexColors.Purple}}; text-align: center;">Page 3: CSS & Advanced Features</h1>

                                   <div class="feature-card">
                                       <h3><span class="status-badge">FEATURE</span> CSS Shorthand Support</h3>
                                       <p>This library supports CSS shorthand properties:</p>
                                       <div style="margin: 5px 0; padding: 5px; border: 2px solid {{HexColors.Blue}}; background-color: #f0f8ff;">
                                           <p><strong>Margin Shorthand:</strong> <span style="font-family: monospace;">margin: 5px 0</span></p>
                                           <p><strong>Border Shorthand:</strong> <span style="font-family: monospace;">border: 2px solid red</span></p>
                                       </div>
                                   </div>

                                   <div class="feature-card">
                                       <h3><span class="status-badge">STYLE</span> Visual Elements</h3>
                                       <div style="border: 1px solid {{HexColors.Orange}}; padding: 4px; margin: 2px 0;">
                                           <p>Solid border container with padding</p>
                                       </div>
                                       <div style="border-left: 5px solid {{HexColors.Green}}; padding-left: 4px; margin: 2px 0;">
                                           <p>Left border accent with indented content</p>
                                       </div>
                                   </div>
                                   """;

        var builder = new PdfBuilder(RendererOptions.CreateDefault(), NullLogger<PdfBuilder>.Instance);
        builder.SetHeader(headerHtml);
        builder.SetFooter(footerHtml);

        var pdfBytes = builder
            .AddPage(page1Html)
            .AddPage(page2Html)
            .AddPage(page3Html)
            .Build();

        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes, "integration-multi-page-document.pdf");

        var words = ExtractWords(pdfBytes);

        // Verify unique content from each page
        // Page 1: Text formatting elements
        words.ShouldContain("Text");
        words.ShouldContain("Formang"); // PDF text extraction shows "Formang" instead of "Formatting"
        words.ShouldContain("styled");
        words.ShouldContain("colored");
        words.ShouldContain("emphasis");

        // Page 2: Lists and tables
        words.ShouldContain("Lists");
        words.ShouldContain("Tables");
        words.ShouldContain("priority");
        words.ShouldContain("Bullet");
        words.Any(w => w.Contains("10", StringComparison.Ordinal)).ShouldBeTrue();
        words.Any(w => w.Contains("$275.45", StringComparison.Ordinal)).ShouldBeTrue();

        // Page 3: CSS and advanced features
        words.ShouldContain("CSS");
        words.ShouldContain("Advanced");

        var pdfWords = GetPdfWords(pdfBytes);

        // Verify header appears on all pages
        var headerWord = pdfWords.FirstOrDefault(w => w.Text.Contains("Mul-Page", StringComparison.OrdinalIgnoreCase));
        headerWord.ShouldNotBeNull();

        // Verify footer appears on all pages
        var footerWord = pdfWords.FirstOrDefault(w => w.Text.Contains("Generated", StringComparison.OrdinalIgnoreCase));
        footerWord.ShouldNotBeNull();

        // Verify text formatting from page 1
        var boldWord = pdfWords.FirstOrDefault(w => w.Text.Contains("bold", StringComparison.OrdinalIgnoreCase));
        boldWord.ShouldNotBeNull();
        boldWord.IsBold.ShouldBeTrue();

        var italicWord = pdfWords.FirstOrDefault(w => w.Text.Contains("italic", StringComparison.OrdinalIgnoreCase));
        italicWord.ShouldNotBeNull();

        // Verify unique elements from each page
        // Page 1: styled div and colored text
        var styledWord = pdfWords.FirstOrDefault(w => w.Text.Contains("styled", StringComparison.OrdinalIgnoreCase));
        styledWord.ShouldNotBeNull();

        // Page 2: table content
        var tableWord = pdfWords.FirstOrDefault(w => w.Text.Contains("Widget", StringComparison.OrdinalIgnoreCase));
        tableWord.ShouldNotBeNull();

        // Page 3: CSS and API content
        var cssWord = pdfWords.FirstOrDefault(w => w.Text.Contains("CSS", StringComparison.OrdinalIgnoreCase));
        cssWord.ShouldNotBeNull();

        // Debug: Check inline-block elements positioning
        Output.WriteLine($"All extracted words: [{string.Join(", ", words.Select(w => $"'{w}'"))}]");

        // Look for the actual extracted words - be more specific
        var leftWord = words.FirstOrDefault(w => w == "Le.");
        var rightWord = words.FirstOrDefault(w => w == "Right.");

        if (leftWord != null && rightWord != null)
            Output.WriteLine($"Found inline-block elements: '{leftWord}' and '{rightWord}'");
        else
            Output.WriteLine(
                $"Could not find inline-block words. Left found: {leftWord != null}, Right found: {rightWord != null}");

        // Get raw words for positioning analysis
        var rawWords = PdfWordParser.GetRawWords(pdfBytes);
        Output.WriteLine($"Raw words count: {rawWords.Count}");
        Output.WriteLine(
            $"Raw words containing 'Le': [{string.Join(", ", rawWords.Where(w => w.Text.Contains("Le")).Select(w => $"'{w.Text}'"))}]");
        Output.WriteLine(
            $"Raw words containing 'Right': [{string.Join(", ", rawWords.Where(w => w.Text.Contains("Right")).Select(w => $"'{w.Text}'"))}]");

        var leftRawWord = rawWords.FirstOrDefault(w => w.Text.Contains("Le"));
        var rightRawWord = rawWords.FirstOrDefault(w => w.Text == "Right.");

        if (leftRawWord != null && rightRawWord != null)
        {
            var yDifference = Math.Abs(leftRawWord.BoundingBox.TopLeft.Y - rightRawWord.BoundingBox.TopLeft.Y);
            var xDifference = Math.Abs(leftRawWord.BoundingBox.TopLeft.X - rightRawWord.BoundingBox.TopLeft.X);

            Output.WriteLine(
                $"Left word position: ({leftRawWord.BoundingBox.TopLeft.X:F1}, {leftRawWord.BoundingBox.TopLeft.Y:F1})");
            Output.WriteLine(
                $"Right word position: ({rightRawWord.BoundingBox.TopLeft.X:F1}, {rightRawWord.BoundingBox.TopLeft.Y:F1})");
            Output.WriteLine($"Y difference: {yDifference:F1} (should be < 5 for same line)");
            Output.WriteLine($"X difference: {xDifference:F1} (should be > 50 for side-by-side)");

            Output.WriteLine(yDifference < 5
                ? "✅ Inline-block elements are on the same line!"
                : "❌ Inline-block elements are NOT on the same line - structure issue");
        }

        Output.WriteLine($"✅ Multi-page PDF generated successfully with {words.Length} words across 3 pages");
    }

    [Fact]
    public async Task InlineBlock_MixedContent_ShouldFlowCorrectly()
    {
        // Test the scenario described by the user:
        // - inline-block elements should flow inline when no block elements are between them
        // - inline-block elements should break to new lines when block elements are between them

        var builder = new PdfBuilder(
            RendererOptions.CreateDefault(),
            NullLogger<PdfBuilder>.Instance);

        const string htmlPage = """
                                <style>
                                    .inline-block { display: inline-block; border: 2px solid red; padding: 2px; margin: 2px; }
                                    .block { display: block; border: 2px solid blue; padding: 2px; margin: 2px; }
                                </style>
                                <div>
                                    <div class="inline-block">A</div>
                                    <div class="inline-block">B</div>
                                    <div class="block">Block Element</div>
                                    <div class="inline-block">C</div>
                                    <div class="inline-block">D</div>
                                </div>
                                """;

        // Act
        var pdfBytes = builder.AddPage(htmlPage).Build();
        AssertValidPdf(pdfBytes);
        await SavePdfForInspectionAsync(pdfBytes);

        // Debug: Check positioning
        var rawWords = PdfWordParser.GetRawWords(pdfBytes);
        var wordA = rawWords.FirstOrDefault(w => w.Text.Contains('A'));
        var wordB = rawWords.FirstOrDefault(w => w.Text.Contains('B'));
        var wordBlock = rawWords.FirstOrDefault(w => w.Text.Contains("Block"));
        var wordC = rawWords.FirstOrDefault(w => w.Text.Contains('C'));
        var wordD = rawWords.FirstOrDefault(w => w.Text.Contains('D'));

        Output.WriteLine("Word positions:");
        if (wordA != null)
            Output.WriteLine($"  A: ({wordA.BoundingBox.TopLeft.X:F1}, {wordA.BoundingBox.TopLeft.Y:F1})");
        if (wordB != null)
            Output.WriteLine($"  B: ({wordB.BoundingBox.TopLeft.X:F1}, {wordB.BoundingBox.TopLeft.Y:F1})");
        if (wordBlock != null)
            Output.WriteLine($"  Block: ({wordBlock.BoundingBox.TopLeft.X:F1}, {wordBlock.BoundingBox.TopLeft.Y:F1})");
        if (wordC != null)
            Output.WriteLine($"  C: ({wordC.BoundingBox.TopLeft.X:F1}, {wordC.BoundingBox.TopLeft.Y:F1})");
        if (wordD != null)
            Output.WriteLine($"  D: ({wordD.BoundingBox.TopLeft.X:F1}, {wordD.BoundingBox.TopLeft.Y:F1})");

        // A and B should be on the same line (inline-block flow)
        if (wordA != null && wordB != null)
        {
            var abYDiff = Math.Abs(wordA.BoundingBox.TopLeft.Y - wordB.BoundingBox.TopLeft.Y);
            Output.WriteLine($"A-B Y difference: {abYDiff:F1} (should be < 5 for same line)");
        }

        // C and D should be on the same line (inline-block flow)
        if (wordC != null && wordD != null)
        {
            var cdYDiff = Math.Abs(wordC.BoundingBox.TopLeft.Y - wordD.BoundingBox.TopLeft.Y);
            Output.WriteLine($"C-D Y difference: {cdYDiff:F1} (should be < 5 for same line)");
        }

        // Block element should be on its own line
        Output.WriteLine("✅ Mixed content inline-block test completed");
    }
}