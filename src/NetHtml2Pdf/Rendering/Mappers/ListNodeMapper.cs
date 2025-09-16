using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Rendering.Interfaces;

namespace NetHtml2Pdf.Rendering.Mappers
{
    /// <summary>
    /// Maps ListNode to QuestPDF elements
    /// </summary>
    public class ListNodeMapper(IDocumentNodeMapperFactory mapperFactory) : IDocumentNodeMapper<ListNode>
    {
        private readonly IDocumentNodeMapperFactory _mapperFactory = mapperFactory ?? throw new ArgumentNullException(nameof(mapperFactory));

        public void Map(ListNode listNode, IContainer container)
        {
            container.Column(column =>
            {
                for (var i = 0; i < listNode.Items.Count; i++)
                {
                    var item = listNode.Items[i];
                    column.Item().Row(row =>
                    {
                        // Add bullet or number with minimal width
                        var bulletText = listNode.IsOrdered ? $"{i + 1}." : listNode.BulletCharacter;
                        row.AutoItem().Text(bulletText).FontSize(12);
                        
                        // Add small spacing
                        row.ConstantItem(8).Text(" ");
                        
                        // Add content with remaining space
                        row.RelativeItem().Element(contentContainer =>
                        {
                            MapListItem(item, contentContainer);
                        });
                    });
                }
            });
        }

        private void MapListItem(ListItemNode listItem, IContainer container)
        {
            container.Column(column =>
            {
                foreach (var node in listItem.Content)
                {
                    column.Item().Element(nodeContainer =>
                    {
                        var mapper = _mapperFactory.GetMapper(node);
                        mapper.Map(node, nodeContainer);
                    });
                }
            });
        }
    }
}
