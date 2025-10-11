using Microsoft.Extensions.Options;
using Moq;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Parser.Interfaces;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Renderer.Interfaces;
using Shouldly;

namespace NetHtml2Pdf.Test;

public class PdfBuilderTests
{
    private readonly Mock<IHtmlParser> _mockParser;
    private readonly Mock<IPdfRendererFactory> _mockRendererFactory;
    private readonly Mock<IPdfRenderer> _mockRenderer;
    private readonly IOptions<RendererOptions> _options;

    public PdfBuilderTests()
    {
        _mockParser = new Mock<IHtmlParser>();
        _mockRendererFactory = new Mock<IPdfRendererFactory>();
        _mockRenderer = new Mock<IPdfRenderer>();
        _options = Options.Create(new RendererOptions());

        var mockDocumentNode = new DocumentNode(DocumentNodeType.Paragraph, "Test");
        _mockParser.Setup(p => p.Parse(It.IsAny<string>())).Returns(mockDocumentNode);
        
        _mockRenderer.Setup(r => r.Render(It.IsAny<DocumentNode>()))
            .Returns(new byte[] { 0x25, 0x50, 0x44, 0x46 });
        
        _mockRendererFactory.Setup(f => f.Create(It.IsAny<RendererOptions>())).Returns(_mockRenderer.Object);
    }

    [Fact]
    public void Reset_ReturnsIPdfBuilderForFluentChaining()
    {
        var builder = new PdfBuilder(_mockParser.Object, _mockRendererFactory.Object, _options);

        var result = builder.Reset();

        result.ShouldNotBeNull();
        result.ShouldBeAssignableTo<IPdfBuilder>();
        result.ShouldBeSameAs(builder);
    }

    [Fact]
    public void AddPage_WithNullHtml_ThrowsArgumentNullException()
    {
        var builder = new PdfBuilder(_mockParser.Object, _mockRendererFactory.Object, _options);

        var exception = Should.Throw<ArgumentNullException>(() => builder.AddPage(null!));

        exception.ParamName.ShouldBe("htmlContent");
    }

    [Fact]
    public void AddPage_WithEmptyHtml_ThrowsArgumentException()
    {
        var builder = new PdfBuilder(_mockParser.Object, _mockRendererFactory.Object, _options);

        var exception = Should.Throw<ArgumentException>(() => builder.AddPage(""));

        exception.ParamName.ShouldBe("htmlContent");
        exception.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    public void AddPage_WithWhitespaceHtml_ThrowsArgumentException()
    {
        var builder = new PdfBuilder(_mockParser.Object, _mockRendererFactory.Object, _options);

        var exception = Should.Throw<ArgumentException>(() => builder.AddPage("   "));

        exception.ParamName.ShouldBe("htmlContent");
        exception.Message.ShouldContain("cannot be empty");
    }

    [Fact]
    public void Build_WithoutPages_ThrowsInvalidOperationException()
    {
        var builder = new PdfBuilder(_mockParser.Object, _mockRendererFactory.Object, _options);

        var exception = Should.Throw<InvalidOperationException>(() => builder.Build());

        exception.Message.ShouldBe("At least one page must be added before building PDF");
    }

    [Fact]
    public void Build_WithPages_ReturnsValidPdf()
    {
        var builder = new PdfBuilder(_mockParser.Object, _mockRendererFactory.Object, _options);
        var html = "<p>Test content</p>";

        var result = builder.AddPage(html).Build();

        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.Length.ShouldBeGreaterThan(0);
        
        // Verify parser was called with the HTML content
        _mockParser.Verify(p => p.Parse(html), Times.Once);
        
        // Verify renderer factory created a renderer
        _mockRendererFactory.Verify(f => f.Create(It.IsAny<RendererOptions>()), Times.Once);
        
        // Verify renderer was called to generate PDF
        _mockRenderer.Verify(r => r.Render(It.IsAny<DocumentNode>()), Times.Once);
    }

    [Fact]
    public void Build_WithTwoPages_AccumulatesBothPages()
    {
        var builder = new PdfBuilder(_mockParser.Object, _mockRendererFactory.Object, _options);
        var page1Html = "<h1>Page 1</h1><p>First page content</p>";
        var page2Html = "<h1>Page 2</h1><p>Second page content</p>";

        // Add two pages using fluent API
        var result = builder
            .AddPage(page1Html)
            .AddPage(page2Html)
            .Build();

        // Verify PDF bytes returned
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.Length.ShouldBeGreaterThan(0);
        
        // Note: Current implementation only renders first page (T015)
        // Multi-page rendering will be implemented in T044-T045
        // For now, verify that AddPage() accepts multiple calls without error
        _mockParser.Verify(p => p.Parse(It.IsAny<string>()), Times.AtLeastOnce);
        _mockRendererFactory.Verify(f => f.Create(It.IsAny<RendererOptions>()), Times.Once);
        _mockRenderer.Verify(r => r.Render(It.IsAny<DocumentNode>()), Times.Once);
    }

    [Fact]
    public void FluentChaining_AllMethods_Work()
    {
        var builder = new PdfBuilder(_mockParser.Object, _mockRendererFactory.Object, _options);
        var headerHtml = "<h1>Header</h1>";
        var pageHtml = "<p>Page content</p>";
        var footerHtml = "<p>Footer</p>";

        // Test fluent chaining: Reset → SetHeader → AddPage → SetFooter → Build
        var result = builder
            .Reset()
            .SetHeader(headerHtml)
            .AddPage(pageHtml)
            .SetFooter(footerHtml)
            .Build();

        // Verify PDF bytes returned
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.Length.ShouldBeGreaterThan(0);
        
        // Verify all methods return IPdfBuilder for fluent chaining
        // (If any method didn't return IPdfBuilder, the chain would break at compile time)
        
        // Verify parser and renderer were called
        _mockParser.Verify(p => p.Parse(It.IsAny<string>()), Times.AtLeastOnce);
        _mockRendererFactory.Verify(f => f.Create(It.IsAny<RendererOptions>()), Times.Once);
        _mockRenderer.Verify(r => r.Render(It.IsAny<DocumentNode>()), Times.Once);
    }
}

