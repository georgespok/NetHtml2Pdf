using NetHtml2Pdf.Core;
using QuestPDF.Fluent;

namespace NetHtml2Pdf.Renderer.Interfaces;

internal interface ITableComposer
{
    void Compose(ColumnDescriptor column, DocumentNode tableNode);
}