using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
    private readonly RendererOptions _rendererOptions;
    private readonly ILogger _logger;

    private string? _header;
    private string? _footer;
    private readonly List<string> _fallbackElements;

    public PdfBuilder() : this(NullLogger<PdfBuilder>.Instance)
    {
    }

    public PdfBuilder(ILogger logger) : this(RendererOptions.CreateDefault(), logger)
    {
    }

    public PdfBuilder(
        RendererOptions rendererOptions,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(rendererOptions);
        ArgumentNullException.ThrowIfNull(logger);

        _rendererOptions = rendererOptions;
        _logger = logger;
        _pages = new List<string>();
        _fallbackElements = new List<string>();

        // Centralized dependency instantiation
        var angleSharpParser = new AngleSharp.Html.Parser.HtmlParser();
        var cssDeclarationParser = new CssDeclarationParser();
        var cssDeclarationUpdater = new CssStyleUpdater();
        var classStyleExtractor = new CssClassStyleExtractor(cssDeclarationParser, cssDeclarationUpdater);

        _parser = new HtmlParser(
            angleSharpParser,
            classStyleExtractor,
            TrackFallbackElement);

        var blockComposer = PdfRenderer.CreateDefaultBlockComposer();
        _rendererFactory = new PdfRendererFactory(blockComposer);
    }

    internal PdfBuilder(
        IHtmlParser parser,
        IPdfRendererFactory rendererFactory,
        RendererOptions rendererOptions,
        ILogger logger)
    {
        _parser = parser;
        _rendererFactory = rendererFactory;
        _rendererOptions = rendererOptions;
        _logger = logger;
        _pages = new List<string>();
        _fallbackElements = new List<string>();
    }

    public IPdfBuilder Reset()
    {
        _pages.Clear();
        _header = null;
        _footer = null;
        _fallbackElements.Clear();
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
            headerNode = ParseWithWarnings(_header);
        }

        // Parse footer if set
        DocumentNode? footerNode = null;
        if (!string.IsNullOrEmpty(_footer))
        {
            footerNode = ParseWithWarnings(_footer);
        }

        // Parse all pages
        var documentNodes = new List<DocumentNode>();
        foreach (var html in _pages)
        {
            var documentNode = ParseWithWarnings(html);
            documentNodes.Add(documentNode);
        }

        var rendererOptions = options != null
            ? new RendererOptions { FontPath = options.FontPath }
            : _rendererOptions;

        var renderer = _rendererFactory.Create(rendererOptions);

        // Render multi-page PDF with headers and footers
        var pdfBytes = renderer.Render(documentNodes, headerNode, footerNode);

        return pdfBytes;
    }

    /// <summary>
    /// Parses HTML with warning tracking for fallback elements.
    /// </summary>
    /// <param name="html">The HTML content to parse.</param>
    /// <returns>The parsed document node.</returns>
    private DocumentNode ParseWithWarnings(string html)
    {
        return _parser.Parse(html, _logger);
    }

    /// <summary>
    /// Tracks a fallback element that was processed.
    /// </summary>
    /// <param name="elementName">The name of the fallback element.</param>
    internal void TrackFallbackElement(string elementName)
    {
        _logger.LogWarning("FallbackRenderer: Unsupported element '<{ElementName}>' processed with best-effort rendering", elementName);
        _fallbackElements.Add(elementName);
    }

    /// <summary>
    /// Gets the current fallback elements for telemetry purposes.
    /// </summary>
    /// <returns>A read-only list of fallback element names.</returns>
    internal IReadOnlyList<string> GetFallbackElements() => _fallbackElements.AsReadOnly();
}

