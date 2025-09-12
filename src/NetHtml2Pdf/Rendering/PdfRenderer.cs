using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using NetHtml2Pdf.Core.Models;

namespace NetHtml2Pdf.Rendering
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
                    page.Margin(30);
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
        /// Renders a single document node to a container using the mapper
        /// </summary>
        /// <param name="node">The document node to render</param>
        /// <param name="container">The container to render to</param>
        public void RenderNode(DocumentNode node, IContainer container)
        {
            _mapper.MapNode(node, container);
        }
    }
}
