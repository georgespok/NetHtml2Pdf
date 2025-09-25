using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Renderer.Interfaces;

namespace NetHtml2Pdf.Renderer.Mappers
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

            if (blockNode.Style.Box.MarginTop.GetValueOrDefault() > 0 ||
                blockNode.Style.Box.MarginRight.GetValueOrDefault() > 0 ||
                blockNode.Style.Box.MarginBottom.GetValueOrDefault() > 0 ||
                blockNode.Style.Box.MarginLeft.GetValueOrDefault() > 0)
            {
                // QuestPDF lacks margin API on container; emulate margins via empty padding wrapper if needed
                styledContainer = styledContainer.PaddingTop(blockNode.Style.Box.MarginTop.GetValueOrDefault())
                                                 .PaddingRight(blockNode.Style.Box.MarginRight.GetValueOrDefault())
                                                 .PaddingBottom(blockNode.Style.Box.MarginBottom.GetValueOrDefault())
                                                 .PaddingLeft(blockNode.Style.Box.MarginLeft.GetValueOrDefault());
            }

            if (blockNode.Style.Box.PaddingTop.GetValueOrDefault() > 0 || blockNode.Style.Box.PaddingRight.GetValueOrDefault() > 0 || blockNode.Style.Box.PaddingBottom.GetValueOrDefault() > 0 || blockNode.Style.Box.PaddingLeft.GetValueOrDefault() > 0)
            {
                styledContainer = styledContainer.PaddingTop(blockNode.Style.Box.PaddingTop.GetValueOrDefault())
                                                 .PaddingRight(blockNode.Style.Box.PaddingRight.GetValueOrDefault())
                                                 .PaddingBottom(blockNode.Style.Box.PaddingBottom.GetValueOrDefault())
                                                 .PaddingLeft(blockNode.Style.Box.PaddingLeft.GetValueOrDefault());
            }

            if (!string.IsNullOrEmpty(blockNode.Style.Background.ColorHex))
            {
                // Background color would be applied here
                // styledContainer = styledContainer.BackgroundColor(blockNode.Style.Background.ColorHex);
            }

            var bw = blockNode.Style.Border.Left.Width.GetValueOrDefault();
            var bc = blockNode.Style.Border.Left.ColorHex;
            if (bw > 0 && !string.IsNullOrEmpty(bc))
            {
                styledContainer = styledContainer.Border(bw).BorderColor(bc);
            }

            return styledContainer;
        }
    }
}
