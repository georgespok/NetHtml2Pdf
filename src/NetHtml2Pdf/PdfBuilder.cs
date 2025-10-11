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
        
        // For now, render only the first page (multi-page support in T044-T045)
        var html = _pages[0];
        var documentNode = _parser.Parse(html);
        
        var rendererOptions = options != null 
            ? new RendererOptions { FontPath = options.FontPath }
            : _rendererOptions.Value;
            
        var renderer = _rendererFactory.Create(rendererOptions);
        var pdfBytes = renderer.Render(documentNode);
        
        return pdfBytes;
    }
}

