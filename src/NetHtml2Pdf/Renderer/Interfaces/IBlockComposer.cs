using NetHtml2Pdf.Core;
using QuestPDF.Fluent;

namespace NetHtml2Pdf.Renderer.Interfaces;

internal interface IBlockComposer
{
    void Compose(ColumnDescriptor column, DocumentNode node);
}