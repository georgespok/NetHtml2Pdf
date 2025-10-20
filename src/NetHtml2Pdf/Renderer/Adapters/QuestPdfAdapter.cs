using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Model;
using NetHtml2Pdf.Layout.Pagination;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace NetHtml2Pdf.Renderer.Adapters;

internal sealed class QuestPdfAdapter : IRendererAdapter
{
    private readonly List<PageFragmentTree> _pendingPages = [];
    private RendererContext? _context;
    private bool _documentBegun;

    public void BeginDocument(PaginatedDocument document, RendererContext context)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(context);

        _pendingPages.Clear();
        _context = context;
        _documentBegun = true;

        ConfigureQuestPdf(context.RendererOptions);
    }

    public void Render(PageFragmentTree page, RendererContext context)
    {
        ArgumentNullException.ThrowIfNull(page);
        if (!_documentBegun)
        {
            throw new InvalidOperationException("BeginDocument must be called before Render.");
        }

        _pendingPages.Add(page);
    }

    public byte[] EndDocument(RendererContext context)
    {
        if (!_documentBegun)
        {
            throw new InvalidOperationException("BeginDocument must be called before EndDocument.");
        }

        var documentBytes = BuildDocument();

        _pendingPages.Clear();
        _documentBegun = false;
        _context = null;

        return documentBytes;
    }

    private byte[] BuildDocument()
    {
        if (_context is null)
        {
            throw new InvalidOperationException("Renderer context is not available.");
        }

        var document = Document.Create(container =>
        {
            foreach (var page in _pendingPages)
            {
                container.Page(pageDescriptor =>
                {
                    pageDescriptor.Margin(20);

                    if (_context.Header is not null)
                    {
                        pageDescriptor.Header().Element(header =>
                        {
                            RenderFragment(header, _context.Header, _context);
                        });
                    }

                    if (_context.Footer is not null)
                    {
                        pageDescriptor.Footer().Element(footer =>
                        {
                            RenderFragment(footer, _context.Footer, _context);
                        });
                    }

                    pageDescriptor.Content().Column(column =>
                    {
                        foreach (var slice in page.Fragments)
                        {
                            column.Item().Element(item =>
                            {
                                RenderFragment(item, slice.SourceFragment, _context);
                            });
                        }
                    });
                });
            }
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    private static void RenderFragment(IContainer container, LayoutFragment fragment, RendererContext context)
    {
        ArgumentNullException.ThrowIfNull(fragment);
        ArgumentNullException.ThrowIfNull(context);

        var textContent = ExtractText(fragment.Node);
        var displayText = string.IsNullOrWhiteSpace(textContent)
            ? fragment.Node.NodeType.ToString()
            : textContent.Trim();

        container
            .Border(0.25f)
            .Background(Colors.Grey.Lighten5)
            .Padding(6)
            .AlignLeft()
            .Text(text =>
            {
                text.DefaultTextStyle(x => x.FontSize(10));
                text.Span(displayText);
            });

        if (context.RendererOptions.EnablePaginationDiagnostics && context.Logger is not null)
        {
            context.Logger.LogDebug("QuestPdfAdapter rendered fragment at path {NodePath}", fragment.NodePath);
        }
    }

    private static string ExtractText(DocumentNode node)
    {
        if (node.NodeType == DocumentNodeType.Text)
        {
            return node.TextContent ?? string.Empty;
        }

        if (node.Children.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var child in node.Children)
        {
            builder.Append(ExtractText(child));
        }

        return builder.ToString();
    }

    private static void ConfigureQuestPdf(RendererOptions options)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var fontPath = options.FontPath;

        if (!string.IsNullOrWhiteSpace(fontPath) && File.Exists(fontPath))
        {
            QuestPDF.Settings.UseEnvironmentFonts = false;
            using var fontStream = File.OpenRead(fontPath);
            FontManager.RegisterFont(fontStream);
        }
        else
        {
            QuestPDF.Settings.UseEnvironmentFonts = true;
        }
    }
}
