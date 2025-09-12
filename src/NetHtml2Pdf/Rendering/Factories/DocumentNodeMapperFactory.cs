using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Rendering.Mappers;
using NetHtml2Pdf.Rendering.Interfaces;

namespace NetHtml2Pdf.Rendering.Factories
{
    /// <summary>
    /// Factory for creating document node mappers
    /// </summary>
    public class DocumentNodeMapperFactory : IDocumentNodeMapperFactory
    {
        private readonly List<IDocumentNodeMapper> _mappers;
        private readonly IDocumentNodeMapper _defaultMapper;

        public DocumentNodeMapperFactory()
        {
            _mappers = new List<IDocumentNodeMapper>();
            _defaultMapper = new DefaultNodeMapper(this);
            RegisterDefaultMappers();
        }

        /// <summary>
        /// Gets the appropriate mapper for the specified document node
        /// </summary>
        /// <param name="node">The document node to map</param>
        /// <returns>The mapper that can handle the node</returns>
        public IDocumentNodeMapper GetMapper(DocumentNode node)
        {
            var mapper = _mappers.FirstOrDefault(m => m.CanMap(node));
            return mapper ?? _defaultMapper;
        }

        /// <summary>
        /// Registers a mapper for a specific document node type
        /// </summary>
        /// <param name="mapper">The mapper to register</param>
        public void RegisterMapper(IDocumentNodeMapper mapper)
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            _mappers.Add(mapper);
        }

        /// <summary>
        /// Registers the default mappers for common document node types
        /// </summary>
        private void RegisterDefaultMappers()
        {
            RegisterMapper(new TypedNodeMapperWrapper<BlockNode>(new BlockNodeMapper(this)));
            RegisterMapper(new TypedNodeMapperWrapper<ParagraphNode>(new ParagraphNodeMapper(this)));
            RegisterMapper(new TypedNodeMapperWrapper<TableNode>(new TableNodeMapper(this)));
            RegisterMapper(new TypedNodeMapperWrapper<ListNode>(new ListNodeMapper(this)));
            RegisterMapper(new TypedNodeMapperWrapper<ImageNode>(new ImageNodeMapper()));
        }
    }
}
