using Microsoft.Extensions.Options;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Parser;
using NetHtml2Pdf.Parser.Interfaces;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Renderer.Interfaces;

namespace NetHtml2Pdf;

public class HtmlConverter : IHtmlConverter
{
    private readonly IHtmlParser _parser;
    private readonly IPdfRendererFactory _factory;
    private readonly IOptions<RendererOptions> _defaultRendererOptions;

    public HtmlConverter()
        : this(
            new HtmlParser(),
            new PdfRendererFactory(),
            Options.Create(RendererOptions.CreateDefault()))
    {
    }

    internal HtmlConverter(
        IHtmlParser htmlParser,
        IPdfRendererFactory rendererFactory,
        IOptions<RendererOptions> rendererOptions)
    {
        ArgumentNullException.ThrowIfNull(htmlParser);
        ArgumentNullException.ThrowIfNull(rendererFactory);
        ArgumentNullException.ThrowIfNull(rendererOptions);

        _parser = htmlParser;
        _factory = rendererFactory;
        _defaultRendererOptions = rendererOptions;
    }

    public byte[] ConvertToPdf(string html, ConverterOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            throw new ArgumentException("HTML content cannot be null or empty", nameof(html));
        }

        var document = _parser.Parse(html);
        var rendererOptions = ResolveRendererOptions(options);
        var renderer = _factory.Create(rendererOptions);

        return renderer.Render(document);
    }

    private RendererOptions ResolveRendererOptions(ConverterOptions? options)
    {
        if (!string.IsNullOrWhiteSpace(options?.FontPath))
        {
            return new RendererOptions { FontPath = options!.FontPath! };
        }

        var defaults = _defaultRendererOptions.Value;
        return new RendererOptions { FontPath = defaults.FontPath };
    }
}
