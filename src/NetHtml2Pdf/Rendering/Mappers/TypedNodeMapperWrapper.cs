using QuestPDF.Infrastructure;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Rendering.Interfaces;

namespace NetHtml2Pdf.Rendering.Mappers
{
    /// <summary>
    /// Wrapper class to adapt generic typed mappers to the non-generic interface
    /// </summary>
    /// <typeparam name="T">The document node type</typeparam>
    public class TypedNodeMapperWrapper<T>(IDocumentNodeMapper<T> typedMapper) : IDocumentNodeMapper
        where T : DocumentNode
    {
        private readonly IDocumentNodeMapper<T> _typedMapper = typedMapper ?? throw new ArgumentNullException(nameof(typedMapper));

        public void Map(DocumentNode node, IContainer container)
        {
            if (node is T typedNode)
            {
                _typedMapper.Map(typedNode, container);
            }
        }

        public bool CanMap(DocumentNode node)
        {
            return node is T;
        }
    }
}
