using Microsoft.Extensions.DependencyInjection.Extensions;
using NetHtml2Pdf;
using NetHtml2Pdf.Parser;
using NetHtml2Pdf.Parser.Interfaces;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Renderer.Interfaces;
using AngleSharpHtmlParser = AngleSharp.Html.Parser.HtmlParser;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class NetHtml2PdfServiceCollectionExtensions
{
    [Obsolete("AddNetHtml2Pdf is deprecated. Use AddPdfBuilder() instead. This method will be removed in a future version.")]
    public static IServiceCollection AddNetHtml2Pdf(
        this IServiceCollection services,
        Action<RendererOptions>? configureRenderer = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = services.AddOptions<RendererOptions>();
        if (configureRenderer is not null)
        {
            optionsBuilder.Configure(configureRenderer);
        }

        optionsBuilder.PostConfigure(options =>
        {
            if (string.IsNullOrWhiteSpace(options.FontPath))
            {
                options.FontPath = RendererOptions.DetermineDefaultFontPath();
            }
        });

        services.TryAddSingleton<IBlockSpacingApplier, BlockSpacingApplier>();
        services.TryAddSingleton<IInlineComposer, InlineComposer>();
        services.TryAddSingleton<IListComposer, ListComposer>();
        services.TryAddSingleton<IBlockComposer, BlockComposer>();
        services.TryAddSingleton<IPdfRendererFactory, PdfRendererFactory>();
        services.TryAddSingleton<ICssDeclarationParser, CssDeclarationParser>();
        services.TryAddSingleton<ICssDeclarationUpdater, CssStyleUpdater>();
        services.TryAddSingleton<ICssClassStyleExtractor, CssClassStyleExtractor>();
        services.TryAddSingleton<AngleSharpHtmlParser>();
        services.TryAddSingleton<IHtmlParser, HtmlParser>();
        services.TryAddSingleton<IHtmlConverter, HtmlConverter>();

        return services;
    }

    public static IServiceCollection AddPdfBuilder(
        this IServiceCollection services,
        Action<RendererOptions>? configureRenderer = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var optionsBuilder = services.AddOptions<RendererOptions>();
        if (configureRenderer is not null)
        {
            optionsBuilder.Configure(configureRenderer);
        }

        optionsBuilder.PostConfigure(options =>
        {
            if (string.IsNullOrWhiteSpace(options.FontPath))
            {
                options.FontPath = RendererOptions.DetermineDefaultFontPath();
            }
        });

        services.TryAddSingleton<IBlockSpacingApplier, BlockSpacingApplier>();
        services.TryAddSingleton<IInlineComposer, InlineComposer>();
        services.TryAddSingleton<IListComposer, ListComposer>();
        services.TryAddSingleton<ITableComposer, TableComposer>();
        services.TryAddSingleton<IBlockComposer, BlockComposer>();
        services.TryAddSingleton<IPdfRendererFactory, PdfRendererFactory>();
        services.TryAddSingleton<ICssDeclarationParser, CssDeclarationParser>();
        services.TryAddSingleton<ICssDeclarationUpdater, CssStyleUpdater>();
        services.TryAddSingleton<ICssClassStyleExtractor, CssClassStyleExtractor>();
        services.TryAddSingleton<AngleSharpHtmlParser>();
        services.TryAddSingleton<IHtmlParser, HtmlParser>();
        services.TryAddTransient<IPdfBuilder, PdfBuilder>();

        return services;
    }
}
