using NetHtml2Pdf.Core;
using QuestPDF.Fluent;

namespace NetHtml2Pdf.Renderer.Interfaces;

internal interface IListComposer
{
    void Compose(ColumnDescriptor column, DocumentNode listNode, bool ordered,
        Action<ColumnDescriptor, DocumentNode> composeBlock);
}