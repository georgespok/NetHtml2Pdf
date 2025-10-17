using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Test.Support;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Renderer;

[Collection("PdfRendering")]
public class BlockComposerFragmentParityTests : PdfRenderTestBase
{
    private static readonly string[] SampleFiles =
    [
        "display-block.html",
        "display-inline-block.html",
        "display-mixed.html",
        "display-none.html",
        "display-unsupported.html"
    ];

    public BlockComposerFragmentParityTests(ITestOutputHelper output) : base(output)
    {
    }

    public static IEnumerable<object[]> SampleFileData() => SampleFiles.Select(file => new object[] { file });

    [Theory]
    [MemberData(nameof(SampleFileData))]
    public void LayoutFlag_ShouldMatchLegacyOutput(string sampleFileName)
    {
        var projectRoot = GetTestProjectRoot();
        var repoRoot = Path.GetFullPath(Path.Combine(projectRoot, "..", ".."));

        var samplePath = Path.Combine(repoRoot, "src", "NetHtml2Pdf.TestConsole", "samples", sampleFileName);
        File.Exists(samplePath).ShouldBeTrue($"Sample file not found: {samplePath}");

        var html = File.ReadAllText(samplePath);

        var legacy = RenderWords(html, enableNewLayout: false);
        var migrated = RenderWords(html, enableNewLayout: true);

        migrated.ShouldBe(legacy);
    }

    private static string[] RenderWords(string html, bool enableNewLayout)
    {
        var builder = new NetHtml2Pdf.PdfBuilder();
        builder.AddPage(html);

        var pdfBytes = builder.Build(new ConverterOptions
        {
            FontPath = RendererOptions.CreateDefault().FontPath,
            EnableNewLayoutForTextBlocks = enableNewLayout,
            EnableLayoutDiagnostics = false
        });

        return ExtractWords(pdfBytes);
    }

    private static string GetTestProjectRoot()
    {
        var assemblyDir = AppContext.BaseDirectory;
        return Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", ".."));
    }
}
