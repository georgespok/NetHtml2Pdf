using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Renderer.Interfaces;

namespace NetHtml2Pdf.Renderer.Mappers
{
    /// <summary>
    /// Default mapper for unknown document node types
    /// </summary>
    public class DefaultNodeMapper(IDocumentNodeMapperFactory mapperFactory) : IDocumentNodeMapper
    {
        private readonly IDocumentNodeMapperFactory _mapperFactory = mapperFactory ?? throw new ArgumentNullException(nameof(mapperFactory));

        public void Map(DocumentNode node, IContainer container)
        {
            // For unknown node types, try to render children
            MapChildren(node, container);
        }

        public bool CanMap(DocumentNode node)
        {
            // This mapper can handle any node type as a fallback
            return true;
        }

        private void MapChildren(DocumentNode node, IContainer container)
        {
            container.Column(column =>
            {
                foreach (var child in node.Children)
                {
                    column.Item().Element(childContainer =>
                    {
                        var mapper = _mapperFactory.GetMapper(child);
                        mapper.Map(child, childContainer);
                    });
                }
            });
        }
    }
}
