using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
#pragma warning disable CS0618 // Type or member is obsolete
        var services = new ServiceCollection();

        services.AddNetHtml2Pdf();

        using var provider = services.BuildServiceProvider();
        var converter = provider.GetRequiredService<IHtmlConverter>();

        converter.ShouldNotBeNull();
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Fact]
    public void HtmlConverter_FromServiceProvider_ConvertsHtml()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var services = new ServiceCollection();
        services.AddNetHtml2Pdf();

        using var provider = services.BuildServiceProvider();
        var converter = provider.GetRequiredService<IHtmlConverter>();

        var result = converter.ConvertToPdf("<p>Hello</p>");
        result.ShouldNotBeNull();
        result.Length.ShouldBeGreaterThan(0);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Fact]
    public void AddNetHtml2Pdf_AppliesRendererOptionsConfiguration()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var services = new ServiceCollection();
        var customFontPath = RendererOptions.CreateDefault().FontPath;

        services.AddNetHtml2Pdf(options => options.FontPath = customFontPath);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<RendererOptions>>();

        options.Value.FontPath.ShouldBe(customFontPath);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Fact]
    public void AddNetHtml2Pdf_RegistersParserDependencies()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var services = new ServiceCollection();
        services.AddNetHtml2Pdf();

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<ICssDeclarationParser>().ShouldNotBeNull();
        provider.GetRequiredService<ICssDeclarationUpdater>().ShouldNotBeNull();
        provider.GetRequiredService<ICssClassStyleExtractor>().ShouldNotBeNull();
        provider.GetRequiredService<AngleSharpHtmlParser>().ShouldNotBeNull();
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Fact]
    public void AddNetHtml2Pdf_AllowsParserCustomization()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var services = new ServiceCollection();
        services.AddNetHtml2Pdf();
        services.AddSingleton<ICssDeclarationParser, StubCssDeclarationParser>();

        using var provider = services.BuildServiceProvider();
        var parser = provider.GetRequiredService<ICssDeclarationParser>();

        parser.ShouldBeOfType<StubCssDeclarationParser>();
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Fact]
    public void AddPdfBuilder_RegistersIPdfBuilder()
    {
        var services = new ServiceCollection();

        services.AddPdfBuilder();

        using var provider = services.BuildServiceProvider();
        var builder = provider.GetRequiredService<IPdfBuilder>();

        builder.ShouldNotBeNull();
        builder.ShouldBeOfType<PdfBuilder>();
    }

    [Fact]
    public void AddPdfBuilder_RegistersAsTransient()
    {
        var services = new ServiceCollection();

        services.AddPdfBuilder();

        using var provider = services.BuildServiceProvider();
        var builder1 = provider.GetRequiredService<IPdfBuilder>();
        var builder2 = provider.GetRequiredService<IPdfBuilder>();

        // Transient lifetime means each resolution gets a new instance
        builder1.ShouldNotBeSameAs(builder2);
    }

    private sealed class StubCssDeclarationParser : ICssDeclarationParser
    {
        public IEnumerable<CssDeclaration> Parse(string declarations) =>
            [];
    }
}
