using NetHtml2Pdf.Test.Support;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Renderer;

[Collection("PdfRendering")]
public class Phase1ParityTests : PdfRenderTestBase
{
    private static readonly string[] SampleFiles =
    [
        "display-block.html",
        "display-inline-block.html",
        "display-mixed.html",
        "display-none.html",
        // display-spacing.html uses oversized containers that currently trigger QuestPDF constraint errors for single-page renders.
        "display-unsupported.html"
    ];

    public Phase1ParityTests(ITestOutputHelper output) : base(output)
    {
    }

    public static IEnumerable<object[]> SampleFileData() => SampleFiles.Select(file => new object[] { file });

    [Theory]
    [MemberData(nameof(SampleFileData))]
    public void SampleHtml_ShouldMatchBaselineTextRuns(string sampleFileName)
    {
        var projectRoot = GetTestProjectRoot();
        var repoRoot = Path.GetFullPath(Path.Combine(projectRoot, "..", ".."));

        var samplePath = Path.Combine(repoRoot, "src", "NetHtml2Pdf.TestConsole", "samples", sampleFileName);
        File.Exists(samplePath).ShouldBeTrue($"Sample file not found: {samplePath}");

        var html = File.ReadAllText(samplePath);

        var builder = new NetHtml2Pdf.PdfBuilder();
        builder.AddPage(html);
        var pdfBytes = builder.Build();

        AssertValidPdf(pdfBytes);

        var words = ExtractWords(pdfBytes);

        var baselineFileName = Path.GetFileNameWithoutExtension(sampleFileName) + ".words.txt";
        var baselinePath = Path.Combine(projectRoot, "Renderer", "Baselines", baselineFileName);

        var updateFlag = Environment.GetEnvironmentVariable("UPDATE_PHASE1_BASELINE");
        if (!string.IsNullOrWhiteSpace(updateFlag) &&
            (string.Equals(updateFlag, "1", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(updateFlag, "true", StringComparison.OrdinalIgnoreCase)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(baselinePath)!);
            File.WriteAllLines(baselinePath, words);
            Output.WriteLine($"Updated baseline: {baselinePath}");
            return;
        }

        File.Exists(baselinePath).ShouldBeTrue($"Baseline not found for {sampleFileName}. Set UPDATE_PHASE1_BASELINE=1 to generate.");

        var expectedWords = File.ReadAllLines(baselinePath);
        words.ShouldBe(expectedWords);
    }

    private static string GetTestProjectRoot()
    {
        var assemblyDir = AppContext.BaseDirectory;
        return Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", ".."));
    }
}
