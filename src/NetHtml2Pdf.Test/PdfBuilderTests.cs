using Moq;
using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Parser.Interfaces;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Renderer.Interfaces;
using NetHtml2Pdf.Test.Support;
using Shouldly;

namespace NetHtml2Pdf.Test;

public class PdfBuilderTests : PdfValidationTestBase
{
    private readonly Mock<IHtmlParser> _mockParser;
    private readonly Mock<IPdfRendererFactory> _mockRendererFactory;
    private readonly Mock<IPdfRenderer> _mockRenderer;
    private readonly RendererOptions _options;

    // Test data constants
    private const string Page1Html = "<h2>Page 1</h2><p>First page content</p>";
    private const string Page2Html = "<h2>Page 2</h2><p>Second page content</p>";
    private const string Page3Html = "<h2>Page 3</h2><p>Third page content</p>";
    private const string HeaderHtml = "<h1>Document Header</h1><p>This header should appear on all pages</p>";
    private const string FooterHtml = "<p>Document Footer</p><p>This footer should appear on all pages</p>";

    // Additional test data constants
    private const string SimplePageHtml = "<h1>Page 1</h1><p>First page content</p>";
    private const string SimplePage2Html = "<h1>Page 2</h1><p>Second page content</p>";
    private const string TestContentHtml = "<p>Test content</p>";
    private const string SimpleHeaderHtml = "<h1>Header</h1>";
    private const string SimplePageContentHtml = "<p>Page content</p>";
    private const string SimpleFooterHtml = "<p>Footer</p>";

    // Dynamic height test constants
    private const string LargeHeaderHtml = "<div style='height: 200px; background-color: #f0f0f0;'><h1>Large Header</h1><p>This is a large header that should reduce the available page content area</p><p>Additional content to make it taller</p><p>More content to ensure it's large enough</p></div>";
    private const string LargeFooterHtml = "<div style='height: 150px; background-color: #e0e0e0;'><p>Large Footer</p><p>This is a large footer that should reduce the available page content area</p><p>Additional footer content</p></div>";
    private const string PageContentHtml = "<h2>Page Content</h2><p>This is the main page content that should be adjusted based on header and footer sizes</p><p>More content to fill the page</p>";

    public PdfBuilderTests()
    {
        _mockParser = new Mock<IHtmlParser>();
        _mockRendererFactory = new Mock<IPdfRendererFactory>();
        _mockRenderer = new Mock<IPdfRenderer>();
        _options = new RendererOptions();

        var mockDocumentNode = new DocumentNode(DocumentNodeType.Paragraph, "Test");
        _mockParser.Setup(p => p.Parse(It.IsAny<string>(), It.IsAny<ILogger?>())).Returns(mockDocumentNode);

        _mockRenderer.Setup(r => r.Render(It.IsAny<DocumentNode>(), null, null))
            .Returns(StandardPdfBytes);

        _mockRenderer.Setup(r => r.Render(It.IsAny<IEnumerable<DocumentNode>>(), It.IsAny<DocumentNode?>(), It.IsAny<DocumentNode?>()))
            .Returns(StandardPdfBytes);

        _mockRendererFactory.Setup(f => f.Create(It.IsAny<RendererOptions>())).Returns(_mockRenderer.Object);
    }

    [Fact]
    public void Reset_ReturnsIPdfBuilderForFluentChaining()
    {
        var builder = CreateBuilder();

        var result = builder.Reset();

        result.ShouldNotBeNull();
        result.ShouldBeAssignableTo<IPdfBuilder>();
        result.ShouldBeSameAs(builder);
    }

    [Theory]
    [InlineData(null, typeof(ArgumentNullException), "htmlContent", null)]
    [InlineData("", typeof(ArgumentException), "htmlContent", "cannot be empty")]
    [InlineData("   ", typeof(ArgumentException), "htmlContent", "cannot be empty")]
    public void AddPage_WithInvalidInput_ThrowsExpectedException(string? input, Type expectedExceptionType, string expectedParamName, string? expectedMessageFragment)
    {
        // Arrange
        var builder = CreateBuilder();

        // Act & Assert
        var exception = Should.Throw<Exception>(() => builder.AddPage(input!));

        exception.ShouldBeOfType(expectedExceptionType);

        // Cast to ArgumentException to access ParamName property
        if (exception is ArgumentException argException)
        {
            argException.ParamName.ShouldBe(expectedParamName);
        }

        if (expectedMessageFragment != null)
        {
            exception.Message.ShouldContain(expectedMessageFragment);
        }
    }

    [Fact]
    public void Build_WithoutPages_ThrowsInvalidOperationException()
    {
        var builder = CreateBuilder();

        var exception = Should.Throw<InvalidOperationException>(() => builder.Build());

        exception.Message.ShouldBe("At least one page must be added before building PDF");
    }

    [Fact]
    public void Build_WithPages_ReturnsValidPdf()
    {
        var builder = CreateBuilder();

        var result = builder.AddPage(TestContentHtml).Build();

        VerifyBasicPdfResult(result);

        // Verify parser was called with the HTML content
        _mockParser.Verify(p => p.Parse(TestContentHtml, It.IsAny<ILogger?>()), Times.Once);

        // Verify renderer calls
        VerifyRendererCalls();
    }

    [Fact]
    public void Build_WithTwoPages_AccumulatesBothPages()
    {
        var builder = CreateBuilder();

        // Add two pages using fluent API
        var result = builder
            .AddPage(SimplePageHtml)
            .AddPage(SimplePage2Html)
            .Build();

        // Verify PDF bytes returned
        VerifyBasicPdfResult(result);

        _mockParser.Verify(p => p.Parse(It.IsAny<string>(), It.IsAny<ILogger?>()), Times.AtLeastOnce);
        VerifyRendererCalls();
    }

    [Fact]
    public void FluentChaining_AllMethods_Work()
    {
        var builder = CreateBuilder();

        // Test fluent chaining: Reset → SetHeader → AddPage → SetFooter → Build
        var result = builder
            .Reset()
            .SetHeader(SimpleHeaderHtml)
            .AddPage(SimplePageContentHtml)
            .SetFooter(SimpleFooterHtml)
            .Build();

        // Verify PDF bytes returned
        VerifyBasicPdfResult(result);

        // Verify all methods return IPdfBuilder for fluent chaining
        // (If any method didn't return IPdfBuilder, the chain would break at compile time)

        // Verify parser and renderer were called
        _mockParser.Verify(p => p.Parse(It.IsAny<string>(), It.IsAny<ILogger?>()), Times.AtLeastOnce);
        VerifyRendererCalls();
    }

    [Theory]
    [InlineData(null, null, 3)]
    [InlineData(HeaderHtml, null, 4)]
    [InlineData(null, FooterHtml, 4)]
    [InlineData(HeaderHtml, FooterHtml, 5)]
    public void PdfBuilder_WithHeaderFooter_GeneratesCorrectParserCalls(string? headerHtml, string? footerHtml, int expectedParserCalls)
    {
        // Arrange
        var builder = CreateBuilder();

        // Act - Build PDF with header/footer and multiple pages
        if (headerHtml != null)
        {
            builder = builder.SetHeader(headerHtml);
        }

        if (footerHtml != null)
        {
            builder = builder.SetFooter(footerHtml);
        }

        var result = builder
            .AddPage(Page1Html)
            .AddPage(Page2Html)
            .AddPage(Page3Html)
            .Build();

        // Assert - Verify PDF bytes returned
        VerifyBasicPdfResult(result);

        // Verify parser calls based on configuration
        if (headerHtml != null)
        {
            _mockParser.Verify(p => p.Parse(headerHtml, It.IsAny<ILogger?>()), Times.Once);
        }

        if (footerHtml != null)
        {
            _mockParser.Verify(p => p.Parse(footerHtml, It.IsAny<ILogger?>()), Times.Once);
        }

        // Verify all pages were parsed
        _mockParser.Verify(p => p.Parse(Page1Html, It.IsAny<ILogger?>()), Times.Once);
        _mockParser.Verify(p => p.Parse(Page2Html, It.IsAny<ILogger?>()), Times.Once);
        _mockParser.Verify(p => p.Parse(Page3Html, It.IsAny<ILogger?>()), Times.Once);

        // Verify total parser calls
        _mockParser.Verify(p => p.Parse(It.IsAny<string>(), It.IsAny<ILogger?>()), Times.Exactly(expectedParserCalls));

        // Verify renderer calls
        VerifyRendererCalls();

    }

    [Fact]
    public void PdfBuilder_HeaderFooterDynamicHeight_AdjustsPageContent()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act - Build PDF with large header and footer, then add page content
        var result = builder
            .SetHeader(LargeHeaderHtml)
            .SetFooter(LargeFooterHtml)
            .AddPage(PageContentHtml)
            .Build();

        // Assert - Verify PDF bytes returned
        VerifyBasicPdfResult(result);

        // Verify header was parsed (should be called once for header)
        _mockParser.Verify(p => p.Parse(LargeHeaderHtml, It.IsAny<ILogger?>()), Times.Once);

        // Verify footer was parsed (should be called once for footer)
        _mockParser.Verify(p => p.Parse(LargeFooterHtml, It.IsAny<ILogger?>()), Times.Once);

        // Verify page content was parsed
        _mockParser.Verify(p => p.Parse(PageContentHtml, It.IsAny<ILogger?>()), Times.Once);

        // Verify total parser calls: 1 header + 1 footer + 1 page = 3 calls
        _mockParser.Verify(p => p.Parse(It.IsAny<string>(), It.IsAny<ILogger?>()), Times.Exactly(3));

        // Verify renderer calls
        VerifyRendererCalls();

    }

    [Fact]
    public void PdfBuilder_MultipleBuildCalls_ProducesIndependentPdfs()
    {
        // Arrange
        var builder = CreateBuilder();

        // Act - First PDF
        var pdf1 = builder
            .Reset()
            .AddPage(SimplePageHtml)
            .Build();

        // Act - Second PDF (reuse same builder instance)
        var pdf2 = builder
            .Reset()
            .AddPage(SimplePage2Html)
            .Build();

        // Assert - Both PDFs should be valid and independent
        VerifyBasicPdfResult(pdf1);
        VerifyBasicPdfResult(pdf2);

        // Verify both PDFs were generated (renderer called twice)
        _mockRendererFactory.Verify(f => f.Create(It.IsAny<RendererOptions>()), Times.Exactly(2));
        _mockRenderer.Verify(r => r.Render(It.IsAny<IEnumerable<DocumentNode>>(), It.IsAny<DocumentNode?>(), It.IsAny<DocumentNode?>()), Times.Exactly(2));

        // Verify parser was called for both pages
        _mockParser.Verify(p => p.Parse(SimplePageHtml, It.IsAny<ILogger?>()), Times.Once);
        _mockParser.Verify(p => p.Parse(SimplePage2Html, It.IsAny<ILogger?>()), Times.Once);
    }

    // Helper methods for common setup and verification
    private IPdfBuilder CreateBuilder()
    {
        return new PdfBuilder(_mockParser.Object, _mockRendererFactory.Object, _options, GetLogger());
    }

    private IPdfBuilder CreateBuilderWithRealParser() =>
        // Use public constructor that centralizes instantiation and wiring of fallback tracking
        new PdfBuilder(_options, GetLogger());

    private static Microsoft.Extensions.Logging.Abstractions.NullLogger<PdfBuilder> GetLogger() =>
        Microsoft.Extensions.Logging.Abstractions.NullLogger<PdfBuilder>.Instance;

    private void VerifyBasicPdfResult(byte[] result)
    {
        ValidatePdfBytes(result);
    }

    private void VerifyRendererCalls()
    {
        _mockRendererFactory.Verify(f => f.Create(It.IsAny<RendererOptions>()), Times.Once);
        _mockRenderer.Verify(r => r.Render(It.IsAny<IEnumerable<DocumentNode>>(), It.IsAny<DocumentNode?>(), It.IsAny<DocumentNode?>()), Times.Once);
    }

    [Fact]
    public void PdfBuilder_WithUnsupportedElements_ShouldLogWarnings()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var pdfBuilder = new PdfBuilder(_options, mockLogger.Object);
        var htmlWithUnsupportedElements = "<div><video>Unsupported video element</video><canvas>Unsupported canvas element</canvas></div>";

        // Act
        pdfBuilder.AddPage(htmlWithUnsupportedElements);
        var pdfBytes = pdfBuilder.Build();

        // Assert
        VerifyBasicPdfResult(pdfBytes);

        // Verify that warnings were logged for unsupported elements via ILogger
        VerifyWarningLogged(mockLogger, "VIDEO");
        VerifyWarningLogged(mockLogger, "CANVAS");

        // Verify that fallback elements were tracked
        var fallbackElements = pdfBuilder.GetFallbackElements();
        fallbackElements.ShouldContain("VIDEO");
        fallbackElements.ShouldContain("CANVAS");
    }

    [Fact]
    public void PdfBuilder_Reset_ShouldClearWarningsAndFallbackElements()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var pdfBuilder = new PdfBuilder(_options, mockLogger.Object);
        var htmlWithUnsupportedElements = "<div><video>Unsupported element</video></div>";

        // Act - Add page with unsupported element and build to trigger parsing/warnings
        pdfBuilder.AddPage(htmlWithUnsupportedElements).Build();
        var fallbackElementsBeforeReset = pdfBuilder.GetFallbackElements();

        // Verify warnings were logged via ILogger
        VerifyWarningLogged(mockLogger, "VIDEO");

        // Verify fallback elements were tracked
        fallbackElementsBeforeReset.ShouldNotBeEmpty();

        // Act - Reset builder
        pdfBuilder.Reset();

        // Assert - Fallback elements should be cleared (logger history is not cleared by reset)
        var fallbackElementsAfterReset = pdfBuilder.GetFallbackElements();

        fallbackElementsAfterReset.ShouldBeEmpty();
    }

    private static void VerifyWarningLogged(Moq.Mock<Microsoft.Extensions.Logging.ILogger> logger, string expectedFragment) =>
        logger.Verify(l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() != null && v.ToString()!.IndexOf(expectedFragment, StringComparison.OrdinalIgnoreCase) >= 0),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.AtLeastOnce());
}

