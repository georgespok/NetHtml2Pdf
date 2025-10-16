using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Renderer.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NetHtml2Pdf.Renderer;

internal class PdfRenderer(RendererOptions? options = null,
    IBlockComposer? blockComposer = null) : IPdfRenderer
{
    private readonly RendererOptions _options = options ?? RendererOptions.CreateDefault();
    private readonly IBlockComposer _blockComposer = blockComposer ?? CreateDefaultBlockComposer(options ?? RendererOptions.CreateDefault());

    public byte[] Render(DocumentNode document, DocumentNode? header = null, DocumentNode? footer = null) =>
        Render([document], header, footer);

    public byte[] Render(IEnumerable<DocumentNode> pages,
        DocumentNode? header = null, DocumentNode? footer = null)
    {
        ArgumentNullException.ThrowIfNull(pages);

        ConfigureQuestPdf();
        var pdfDocument = CreateMultiPageDocument(pages, header, footer);

        using var stream = new MemoryStream();
        pdfDocument.GeneratePdf(stream);
        return stream.ToArray();
    }

    internal static IBlockComposer CreateDefaultBlockComposer(RendererOptions? options = null)
    {
        var spacingApplier = new BlockSpacingApplier();
        var inlineComposer = new InlineComposer();
        var listComposer = new ListComposer(inlineComposer, spacingApplier);
        var tableComposer = new TableComposer(inlineComposer, spacingApplier);

        return new BlockComposer(inlineComposer, listComposer, tableComposer, spacingApplier, options);
    }

    private void ConfigureQuestPdf()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.UseEnvironmentFonts = false;

        using var fontStream = File.OpenRead(_options.FontPath);
        QuestPDF.Drawing.FontManager.RegisterFont(fontStream);
    }

    private IDocument CreateMultiPageDocument(IEnumerable<DocumentNode> pages, DocumentNode? header, DocumentNode? footer)
    {
        return Document.Create(container =>
        {
            foreach (var pageDocument in pages)
            {
                container.Page(page =>
                {
                    page.Margin(40);

                    // Add header if provided
                    if (header != null)
                    {
                        page.Header().Column(column =>
                        {
                            foreach (var child in header.Children)
                            {
                                if (IsDisplayNone(child))
                                    continue;

                                _blockComposer.Compose(column, child);
                            }
                        });
                    }

                    // Add footer if provided
                    if (footer != null)
                    {
                        page.Footer().Column(column =>
                        {
                            foreach (var child in footer.Children)
                            {
                                if (IsDisplayNone(child))
                                    continue;

                                _blockComposer.Compose(column, child);
                            }
                        });
                    }

                    // Add page content
                    page.Content().Column(column =>
                    {
                        foreach (var child in pageDocument.Children)
                        {
                            if (IsDisplayNone(child))
                                continue;

                            _blockComposer.Compose(column, child);
                        }
                    });
                });
            }
        });
    }

    private static bool IsDisplayNone(DocumentNode node)
    {
        return node.Styles.DisplaySet && node.Styles.Display == CssDisplay.None;
    }
}
