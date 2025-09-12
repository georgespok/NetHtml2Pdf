using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using NetHtml2Pdf.RenderModel;

namespace NetHtml2Pdf.PdfRendering
{
    /// <summary>
    /// Renders the intermediate render model to QuestPDF elements
    /// </summary>
    public class PdfRenderer
    {
        /// <summary>
        /// Renders a list of render nodes to a QuestPDF document
        /// </summary>
        /// <param name="renderNodes">The render nodes to render</param>
        /// <returns>PDF bytes</returns>
        public byte[] RenderToPdf(List<RenderNode> renderNodes)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Content().Column(column =>
                    {
                        foreach (var node in renderNodes)
                        {
                            column.Item().Element(nodeContainer =>
                            {
                                node.Render(nodeContainer);
                            });
                        }
                    });
                });
            }).GeneratePdf();
        }

        /// <summary>
        /// Renders a single render node to a container
        /// </summary>
        /// <param name="node">The render node to render</param>
        /// <param name="container">The container to render to</param>
        public void RenderNode(RenderNode node, IContainer container)
        {
            node.Render(container);
        }
    }
}
