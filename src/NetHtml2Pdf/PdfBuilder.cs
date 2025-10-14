using Microsoft.Extensions.Options;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Parser;
using NetHtml2Pdf.Parser.Interfaces;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Renderer.Interfaces;

namespace NetHtml2Pdf;

public class PdfBuilder : IPdfBuilder
{
    private readonly List<string> _pages;
    private readonly IHtmlParser _parser;
    private readonly IPdfRendererFactory _rendererFactory;
    private readonly IOptions<RendererOptions> _rendererOptions;
    
    private string? _header;
    private string? _footer;

    public PdfBuilder()
        : this(
            new HtmlParser(),
            new PdfRendererFactory(),
            Options.Create(RendererOptions.CreateDefault()))
    {
    }

    internal PdfBuilder(
        IHtmlParser parser,
        IPdfRendererFactory rendererFactory,
        IOptions<RendererOptions> rendererOptions)
    {
        _parser = parser;
        _rendererFactory = rendererFactory;
        _rendererOptions = rendererOptions;
        _pages = new List<string>();
    }

    public IPdfBuilder Reset()
    {
        _pages.Clear();
        _header = null;
        _footer = null;
        return this;
    }

    public IPdfBuilder SetHeader(string html)
    {
        ArgumentNullException.ThrowIfNull(html);
        
        if (string.IsNullOrWhiteSpace(html))
        {
            throw new ArgumentException("Header HTML cannot be empty or whitespace.", nameof(html));
        }
        
        _header = html;
        return this;
    }

    public IPdfBuilder SetFooter(string html)
    {
        ArgumentNullException.ThrowIfNull(html);
        
        if (string.IsNullOrWhiteSpace(html))
        {
            throw new ArgumentException("Footer HTML cannot be empty or whitespace.", nameof(html));
        }
        
        _footer = html;
        return this;
    }

    public IPdfBuilder AddPage(string htmlContent)
    {
        ArgumentNullException.ThrowIfNull(htmlContent);
        
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            throw new ArgumentException("HTML content cannot be empty or whitespace.", nameof(htmlContent));
        }
        
        _pages.Add(htmlContent);
        return this;
    }

    public byte[] Build(ConverterOptions? options = null)
    {
        if (_pages.Count == 0)
        {
            throw new InvalidOperationException("At least one page must be added before building PDF");
        }
        
        // Parse header if set
        DocumentNode? headerNode = null;
        if (!string.IsNullOrEmpty(_header))
        {
            headerNode = _parser.Parse(_header);
        }
        
        // Parse footer if set
        DocumentNode? footerNode = null;
        if (!string.IsNullOrEmpty(_footer))
        {
            footerNode = _parser.Parse(_footer);
        }
        
        // Parse all pages
        var documentNodes = new List<DocumentNode>();
        foreach (var html in _pages)
        {
            var documentNode = _parser.Parse(html);
            documentNodes.Add(documentNode);
        }
        
        var rendererOptions = options != null 
            ? new RendererOptions { FontPath = options.FontPath }
            : _rendererOptions.Value;
            
        var renderer = _rendererFactory.Create(rendererOptions);
        
        // Render multi-page PDF with headers and footers
        var pdfBytes = renderer.Render(documentNodes, headerNode, footerNode);
        
        return pdfBytes;
    }
}

