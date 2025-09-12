using QuestPDF.Infrastructure;
using NetHtml2Pdf.Core.Models;

namespace NetHtml2Pdf.Rendering.Interfaces
{
    /// <summary>
    /// Factory interface for creating document node mappers
    /// </summary>
    public interface IDocumentNodeMapperFactory
    {
        /// <summary>
        /// Gets the appropriate mapper for the specified document node
        /// </summary>
        /// <param name="node">The document node to map</param>
        /// <returns>The mapper that can handle the node</returns>
        IDocumentNodeMapper GetMapper(DocumentNode node);

        /// <summary>
        /// Registers a mapper for a specific document node type
        /// </summary>
        /// <param name="mapper">The mapper to register</param>
        void RegisterMapper(IDocumentNodeMapper mapper);
    }
}
