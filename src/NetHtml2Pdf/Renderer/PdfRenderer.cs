using NetHtml2Pdf.Core;
using NetHtml2Pdf.Renderer.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NetHtml2Pdf.Renderer;

internal class PdfRenderer(RendererOptions? options = null,
    IBlockComposer? blockComposer = null) : IPdfRenderer
{
    private readonly RendererOptions _options = options ?? RendererOptions.CreateDefault();
    private readonly IBlockComposer _blockComposer = blockComposer ?? CreateDefaultBlockComposer();

    public byte[] Render(DocumentNode document)
    {
        ArgumentNullException.ThrowIfNull(document);

        ConfigureQuestPdf();
        var pdfDocument = CreateDocument(document);

        using var stream = new MemoryStream();
        pdfDocument.GeneratePdf(stream);
        return stream.ToArray();
    }

    internal static IBlockComposer CreateDefaultBlockComposer()
    {
        var spacingApplier = new BlockSpacingApplier();
        var inlineComposer = new InlineComposer();
        var listComposer = new ListComposer(inlineComposer, spacingApplier);

        return new BlockComposer(inlineComposer, listComposer, spacingApplier);
    }

    private void ConfigureQuestPdf()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.UseEnvironmentFonts = false;

        using var fontStream = File.OpenRead(_options.FontPath);
        QuestPDF.Drawing.FontManager.RegisterFont(fontStream);
    }

    private IDocument CreateDocument(DocumentNode document)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Content().Column(column =>
                {
                    foreach (var child in document.Children)
                    {
                        _blockComposer.Compose(column, child);
                    }
                });
            });
        });
    }
}
