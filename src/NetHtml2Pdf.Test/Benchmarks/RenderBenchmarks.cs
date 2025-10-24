using System.Diagnostics;
using System.Text.Json;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Test.Support;

namespace NetHtml2Pdf.Test.Benchmarks;

public class RenderBenchmarks
{
    [Fact]
    public void GenerateBenchmarkReport()
    {
        var outDir = Environment.GetEnvironmentVariable("RENDER_BENCH_OUTDIR");
        if (string.IsNullOrWhiteSpace(outDir))
            // Default to a local artifacts directory relative to repo root if not provided
            outDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../..",
                "scripts/benchmarks/artifacts"));

        Directory.CreateDirectory(outDir!);

        var results = new Dictionary<string, object?>();

        // Inline-block document
        var inlineBlockDoc = CreateInlineBlockDocument();
        Measure(inlineBlockDoc,
            () => CreateOptions(false, false),
            () => CreateOptions(true, false),
            "InlineBlock_",
            results);

        // Table document
        var tableDoc = CreateSimpleTableDocument();
        Measure(tableDoc,
            () => CreateOptions(false, false),
            () => CreateOptions(false, true),
            "Table_",
            results);

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var filePath = Path.Combine(outDir!, $"bench_{timestamp}.json");
        var json = JsonSerializer.Serialize(results, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(filePath, json);
    }

    private static void Measure(
        DocumentNode document,
        Func<RendererOptions> createOptionsOff,
        Func<RendererOptions> createOptionsOn,
        string prefix,
        IDictionary<string, object?> results)
    {
        // Off
        var offOptions = createOptionsOff();
        var offRenderer = (PdfRenderer)RendererComposition.CreateRenderer(offOptions);
        var sw = Stopwatch.StartNew();
        var offBytes = offRenderer.Render(document);
        sw.Stop();
        var offMs = sw.Elapsed.TotalMilliseconds;
        var offWords = PdfWordParser.GetTextWords(offBytes);

        // On
        var onOptions = createOptionsOn();
        var onRenderer = (PdfRenderer)RendererComposition.CreateRenderer(onOptions);
        sw.Restart();
        var onBytes = onRenderer.Render(document);
        sw.Stop();
        var onMs = sw.Elapsed.TotalMilliseconds;
        var onWords = PdfWordParser.GetTextWords(onBytes);

        results[$"{prefix}Off_TimeMs"] = Math.Round(offMs);
        results[$"{prefix}On_TimeMs"] = Math.Round(onMs);
        results[$"{prefix}DeltaWords"] = onWords.Length - offWords.Length;
    }

    private static RendererOptions CreateOptions(bool enableInlineBlock, bool enableTable)
    {
        return new RendererOptions
        {
            EnablePagination = true,
            EnableQuestPdfAdapter = true,
            EnableNewLayoutForTextBlocks = true,
            EnableInlineBlockContext = enableInlineBlock,
            EnableTableContext = enableTable,
            EnableLayoutDiagnostics = false,
            EnablePaginationDiagnostics = false,
            FontPath = string.Empty
        };
    }

    private static DocumentNode CreateInlineBlockDocument()
    {
        var inlineBlock = new DocumentNode(DocumentNodeType.Span,
            styles: CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock));
        inlineBlock.AddChild(new DocumentNode(DocumentNodeType.Text, "Badge"));

        var paragraph = new DocumentNode(DocumentNodeType.Paragraph);
        paragraph.AddChild(new DocumentNode(DocumentNodeType.Text, "Prefix "));
        paragraph.AddChild(inlineBlock);
        paragraph.AddChild(new DocumentNode(DocumentNodeType.Text, " Suffix"));

        var root = new DocumentNode(DocumentNodeType.Div);
        root.AddChild(paragraph);
        return root;
    }

    private static DocumentNode CreateSimpleTableDocument()
    {
        var table = new DocumentNode(DocumentNodeType.Table);
        var head = new DocumentNode(DocumentNodeType.TableHead);
        head.AddChild(Row(DocumentNodeType.TableHeaderCell, "Header"));
        table.AddChild(head);

        var body = new DocumentNode(DocumentNodeType.TableBody);
        body.AddChild(Row(DocumentNodeType.TableCell, "Value 1"));
        body.AddChild(Row(DocumentNodeType.TableCell, "Value 2"));
        table.AddChild(body);

        var root = new DocumentNode(DocumentNodeType.Div);
        root.AddChild(table);
        return root;
    }

    private static DocumentNode Row(DocumentNodeType cellType, params string[] cells)
    {
        var row = new DocumentNode(DocumentNodeType.TableRow);
        foreach (var text in cells)
        {
            var cell = new DocumentNode(cellType);
            cell.AddChild(new DocumentNode(DocumentNodeType.Text, text));
            row.AddChild(cell);
        }

        return row;
    }
}