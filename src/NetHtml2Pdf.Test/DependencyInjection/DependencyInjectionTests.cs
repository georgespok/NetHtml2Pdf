using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetHtml2Pdf;
using NetHtml2Pdf.Parser;
using NetHtml2Pdf.Parser.Interfaces;
using NetHtml2Pdf.Renderer;
using Shouldly;
using AngleSharpHtmlParser = AngleSharp.Html.Parser.HtmlParser;

namespace NetHtml2Pdf.Test.DependencyInjection;

public class DependencyInjectionTests
{
    [Fact]
    public void AddNetHtml2Pdf_RegistersHtmlConverter()
    {
        var services = new ServiceCollection();

        services.AddNetHtml2Pdf();

        using var provider = services.BuildServiceProvider();
        var converter = provider.GetRequiredService<IHtmlConverter>();

        converter.ShouldNotBeNull();
    }

    [Fact]
    public void HtmlConverter_FromServiceProvider_ConvertsHtml()
    {
        var services = new ServiceCollection();
        services.AddNetHtml2Pdf();

        using var provider = services.BuildServiceProvider();
        var converter = provider.GetRequiredService<IHtmlConverter>();

        var result = converter.ConvertToPdf("<p>Hello</p>");
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void AddNetHtml2Pdf_AppliesRendererOptionsConfiguration()
    {
        var services = new ServiceCollection();
        var customFontPath = RendererOptions.CreateDefault().FontPath;

        services.AddNetHtml2Pdf(options => options.FontPath = customFontPath);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RendererOptions>>();

        options.Value.FontPath.ShouldBe(customFontPath);
    }

    [Fact]
    public void AddNetHtml2Pdf_RegistersParserDependencies()
    {
        var services = new ServiceCollection();
        services.AddNetHtml2Pdf();

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<ICssDeclarationParser>().ShouldNotBeNull();
        provider.GetRequiredService<ICssDeclarationUpdater>().ShouldNotBeNull();
        provider.GetRequiredService<ICssClassStyleExtractor>().ShouldNotBeNull();
        provider.GetRequiredService<AngleSharpHtmlParser>().ShouldNotBeNull();
    }

    [Fact]
    public void AddNetHtml2Pdf_AllowsParserCustomization()
    {
        var services = new ServiceCollection();
        services.AddNetHtml2Pdf();
        services.AddSingleton<ICssDeclarationParser, StubCssDeclarationParser>();

        using var provider = services.BuildServiceProvider();
        var parser = provider.GetRequiredService<ICssDeclarationParser>();

        parser.ShouldBeOfType<StubCssDeclarationParser>();
    }

    private sealed class StubCssDeclarationParser : ICssDeclarationParser
    {
        public IEnumerable<CssDeclaration> Parse(string declarations) =>
            Array.Empty<CssDeclaration>();
    }
}
