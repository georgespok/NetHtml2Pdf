using NetHtml2Pdf.Core.Models;
using QuestPDF.Infrastructure;
using NetHtml2Pdf.Rendering.Factories;
using NetHtml2Pdf.Rendering.Interfaces;

namespace NetHtml2Pdf.Rendering
{
    /// <summary>
    /// Maps pure POCO document model nodes to QuestPDF elements using a factory pattern
    /// </summary>
    public class DocumentModelMapper(IDocumentNodeMapperFactory? mapperFactory = null)
    {
        private readonly IDocumentNodeMapperFactory _mapperFactory = mapperFactory ?? new DocumentNodeMapperFactory();

        /// <summary>
        /// Maps a document node to QuestPDF elements
        /// </summary>
        /// <param name="node">The document node to map</param>
        /// <param name="container">The QuestPDF container to add elements to</param>
        public void MapNode(DocumentNode node, IContainer container)
        {
            var mapper = _mapperFactory.GetMapper(node);
            mapper.Map(node, container);
        }

        /// <summary>
        /// Registers a custom mapper for specific document node types
        /// </summary>
        /// <param name="mapper">The mapper to register</param>
        public void RegisterMapper(IDocumentNodeMapper mapper)
        {
            _mapperFactory.RegisterMapper(mapper);
        }

    }
}
