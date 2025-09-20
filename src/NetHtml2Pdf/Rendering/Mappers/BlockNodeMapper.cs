using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Rendering.Interfaces;

namespace NetHtml2Pdf.Rendering.Mappers
{
    /// <summary>
    /// Maps BlockNode to QuestPDF elements
    /// </summary>
    public class BlockNodeMapper(IDocumentNodeMapperFactory mapperFactory) : IDocumentNodeMapper<BlockNode>
    {
        private readonly IDocumentNodeMapperFactory _mapperFactory = mapperFactory ?? throw new ArgumentNullException(nameof(mapperFactory));

        public void Map(BlockNode blockNode, IContainer container)
        {
            container.Column(column =>
            {
                foreach (var child in blockNode.Children)
                {
                    column.Item().Element(childContainer =>
                    {
                        var styledContainer = ApplyBlockStyling(blockNode, childContainer);
                        var mapper = _mapperFactory.GetMapper(child);
                        mapper.Map(child, styledContainer);
                    });
                }
            });
        }

        private static IContainer ApplyBlockStyling(BlockNode blockNode, IContainer container)
        {
            var styledContainer = container;

            if (blockNode.Margins > 0)
            {
                styledContainer = styledContainer.Padding(blockNode.Margins);
            }

            if (blockNode.PaddingTop > 0 || blockNode.PaddingRight > 0 || blockNode.PaddingBottom > 0 || blockNode.PaddingLeft > 0)
            {
                styledContainer = styledContainer.PaddingTop(blockNode.PaddingTop)
                                                 .PaddingRight(blockNode.PaddingRight)
                                                 .PaddingBottom(blockNode.PaddingBottom)
                                                 .PaddingLeft(blockNode.PaddingLeft);
            }

            if (!string.IsNullOrEmpty(blockNode.BackgroundColor))
            {
                // Background color would be applied here
                // styledContainer = styledContainer.BackgroundColor(blockNode.BackgroundColor);
            }

            if (blockNode.BorderWidth > 0 && !string.IsNullOrEmpty(blockNode.BorderColor))
            {
                styledContainer = styledContainer.Border(blockNode.BorderWidth).BorderColor(blockNode.BorderColor);
            }

            return styledContainer;
        }
    }
}
