using System.Runtime.CompilerServices;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Support;

/// <summary>
///     Base class for PDF rendering tests providing common PDF analysis and assertion helpers.
///     Disables parallelization to prevent QuestPDF memory access violations during concurrent rendering.
/// </summary>
[CollectionDefinition("PdfRendering", DisableParallelization = true)]
public abstract class PdfRenderTestBase(ITestOutputHelper output) : PdfValidationTestBase
{
    protected ITestOutputHelper Output => output;

    #region PDF Word Extraction

    /// <summary>
    ///     Extracts all words from a PDF with their styling attributes.
    /// </summary>
    protected static IReadOnlyList<PdfWord> GetPdfWords(byte[] pdfBytes)
    {
        return PdfWordParser.GetStyledWords(pdfBytes);
    }

    /// <summary>
    ///     Extracts all words from a PDF as strings for simple text validation.
    /// </summary>
    protected static string[] ExtractWords(byte[] pdfBytes)
    {
        return PdfWordParser.GetTextWords(pdfBytes);
    }

    #endregion

    #region DocumentNode Builders

    internal static DocumentNode Document(params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.Document), children);
    }

    internal static DocumentNode Div(params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.Div), children);
    }

    internal static DocumentNode Div(CssStyleMap styles, params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.Div, styles: styles), children);
    }

    internal static DocumentNode Section(params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.Section), children);
    }

    internal static DocumentNode Paragraph(params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.Paragraph), children);
    }

    internal static DocumentNode Paragraph(CssStyleMap styles, params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.Paragraph, styles: styles), children);
    }

    internal static DocumentNode Span(CssStyleMap styles, params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.Span, styles: styles), children);
    }

    internal static DocumentNode Strong(params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.Strong), children);
    }

    internal static DocumentNode Italic(params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.Italic), children);
    }

    internal static DocumentNode UnorderedList(params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.UnorderedList), children);
    }

    internal static DocumentNode OrderedList(params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.OrderedList), children);
    }

    internal static DocumentNode ListItem(params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.ListItem), children);
    }

    internal static DocumentNode Heading(DocumentNodeType type, params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(type), children);
    }

    internal static DocumentNode Table(params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.Table), children);
    }

    internal static DocumentNode Table(CssStyleMap styles, params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.Table, styles: styles), children);
    }

    internal static DocumentNode TableHead(params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.TableHead), children);
    }

    internal static DocumentNode TableBody(params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.TableBody), children);
    }

    internal static DocumentNode TableRow(params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.TableRow), children);
    }

    internal static DocumentNode TableHeaderCell(params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.TableHeaderCell), children);
    }

    internal static DocumentNode TableHeaderCell(CssStyleMap styles, params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.TableHeaderCell, styles: styles), children);
    }

    internal static DocumentNode TableCell(params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.TableCell), children);
    }

    internal static DocumentNode TableCell(CssStyleMap styles, params DocumentNode[] children)
    {
        return AddChildren(new DocumentNode(DocumentNodeType.TableCell, styles: styles), children);
    }

    internal static DocumentNode Text(string text, CssStyleMap? styles = null)
    {
        return new DocumentNode(DocumentNodeType.Text, text, styles ?? CssStyleMap.Empty);
    }

    internal static DocumentNode LineBreak()
    {
        return new DocumentNode(DocumentNodeType.LineBreak);
    }

    private static DocumentNode AddChildren(DocumentNode node, params DocumentNode[] children)
    {
        foreach (var child in children) node.AddChild(child);

        return node;
    }

    #endregion

    #region Test Helpers

    /// <summary>
    ///     Saves PDF bytes to a temp file and logs the path for inspection.
    ///     Uses the calling test method's name if no filename is provided.
    /// </summary>
    protected async Task SavePdfForInspectionAsync(
        byte[] pdfBytes,
        string? fileName = null,
        [CallerMemberName] string? callerName = null)
    {
        var baseName = string.IsNullOrWhiteSpace(fileName)
            ? callerName ?? "output"
            : fileName;

        var safeFileName = MakeSafeFileName(baseName);
        var finalFileName = EnsurePdfExtension(safeFileName);
        var tempPath = Path.Combine(Path.GetTempPath(), finalFileName);

        await File.WriteAllBytesAsync(tempPath, pdfBytes);
        output.WriteLine($"PDF saved to: {tempPath}");
    }

    private static string MakeSafeFileName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(ch => invalidChars.Contains(ch) ? '_' : ch)).Trim();
    }

    private static string EnsurePdfExtension(string fileName)
    {
        return fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
            ? fileName
            : $"{fileName}.pdf";
    }

    #endregion
}