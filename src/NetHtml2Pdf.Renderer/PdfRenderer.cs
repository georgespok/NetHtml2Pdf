using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using NetHtml2Pdf.Core.Models;

namespace NetHtml2Pdf.Renderer
{
    /// <summary>
    /// Renders the intermediate document model to QuestPDF elements using a mapper
    /// </summary>
    public class PdfRenderer
    {
        private readonly DocumentModelMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the PdfRenderer class
        /// </summary>
        public PdfRenderer()
        {
            _mapper = new DocumentModelMapper();
        }

        /// <summary>
        /// Renders a list of document nodes to a QuestPDF document
        /// </summary>
        /// <param name="documentNodes">The document nodes to render</param>
        /// <returns>PDF bytes</returns>
        public byte[] RenderToPdf(List<DocumentNode> documentNodes)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Defaults per spec: Letter portrait, 1 inch margins, Inter font
                    page.Size(PageSizes.Letter);
                    page.Margin(72); // 72 pt = 1 inch
                    page.DefaultTextStyle(style => style.FontFamily("Inter"));
                    page.Content().Column(column =>
                    {
                        foreach (var node in documentNodes)
                        {
                            column.Item().Element(nodeContainer =>
                            {
                                _mapper.MapNode(node, nodeContainer);
                            });
                        }
                    });
                });
            }).GeneratePdf();
        }

        /// <summary>
        /// Renders multiple pages (each as a list of document nodes) into a single PDF.
        /// </summary>
        public byte[] RenderPagesToPdf(
            IReadOnlyList<List<DocumentNode>> pages,
            List<DocumentNode>? headerNodes = null,
            List<DocumentNode>? footerNodes = null)
        {
            return Document.Create(container =>
            {
                foreach (var nodes in pages)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.Letter);
                        page.Margin(72);
                        page.DefaultTextStyle(style => style.FontFamily("Inter"));
                        if (headerNodes != null && headerNodes.Count > 0)
                        {
                            page.Header().Element(headerContainer =>
                            {
                                headerContainer.Column(col =>
                                {
                                    foreach (var h in headerNodes)
                                        col.Item().Element(c => _mapper.MapNode(h, c));
                                });
                            });
                        }
                        page.Content().Column(column =>
                        {
                            foreach (var node in nodes)
                            {
                                column.Item().Element(nodeContainer =>
                                {
                                    _mapper.MapNode(node, nodeContainer);
                                });
                            }
                        });
                        if (footerNodes != null && footerNodes.Count > 0)
                        {
                            page.Footer().Element(footerContainer =>
                            {
                                footerContainer.Column(col =>
                                {
                                    foreach (var f in footerNodes)
                                        col.Item().Element(c => _mapper.MapNode(f, c));
                                });
                            });
                        }
                    });
                }
            }).GeneratePdf();
        }

        /// <summary>
        /// Renders a single document node to a container using the mapper
        /// </summary>
        /// <param name="node">The document node to render</param>
        /// <param name="container">The container to render to</param>
        public void RenderNode(DocumentNode node, IContainer container)
        {
            _mapper.MapNode(node, container);
        }

        public void Setup(string fontPath)
        {
            if (!File.Exists(fontPath))
            {
                throw new ApplicationException($"Missing font at {fontPath}. Ensure it's copied to output.");
            }

            QuestPDF.Settings.UseEnvironmentFonts = false;
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Drawing.FontManager.RegisterFont(File.OpenRead(fontPath));
        }
    }
}
