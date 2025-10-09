using NetHtml2Pdf.Core;
using QuestPDF.Fluent;

namespace NetHtml2Pdf.Renderer.Interfaces;

internal interface IInlineComposer
{
    void Compose(TextDescriptor text, DocumentNode node, InlineStyleState style);
}