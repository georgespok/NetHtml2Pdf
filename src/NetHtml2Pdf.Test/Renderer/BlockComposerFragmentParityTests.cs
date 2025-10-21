using NetHtml2Pdf.Core;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Test.Support;
using Shouldly;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Renderer;

[Collection("PdfRendering")]
public class BlockComposerFragmentParityTests(ITestOutputHelper output) : PdfRenderTestBase(output)
{
    private static readonly string[] SampleFiles =
    [
        "display-block.html",
        "display-inline-block.html",
        "display-mixed.html",
        "display-none.html",
        "display-unsupported.html"
    ];

    public static IEnumerable<object[]> SampleFileData()
    {
        return SampleFiles.Select(file => new object[] { file });
    }

    [Theory]
    [MemberData(nameof(SampleFileData))]
    public void LayoutFlag_ShouldMatchLegacyOutput(string sampleFileName)
    {
        var projectRoot = GetTestProjectRoot();
        var repoRoot = Path.GetFullPath(Path.Combine(projectRoot, "..", ".."));

        var samplePath = Path.Combine(repoRoot, "src", "NetHtml2Pdf.TestConsole", "samples", sampleFileName);
        File.Exists(samplePath).ShouldBeTrue($"Sample file not found: {samplePath}");

        var html = File.ReadAllText(samplePath);

        var legacy = RenderWords(html, false);
        var migrated = RenderWords(html, true);

        migrated.ShouldBe(legacy);
    }

    private static string[] RenderWords(string html, bool enableNewLayout)
    {
        var builder = new PdfBuilder();
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