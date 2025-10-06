using NetHtml2Pdf.Core;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Renderer.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Shouldly;

namespace NetHtml2Pdf.Test.Renderer;

public class BlockComposerTests
{
    [Fact]
    public void Compose_Paragraph_DelegatesToInlineComposerForEachChild()
    {
        var inlineComposer = new RecordingInlineComposer();
        var listComposer = new RecordingListComposer();
        var spacingApplier = new PassthroughSpacingApplier();
        var sut = new BlockComposer(inlineComposer, listComposer, spacingApplier);

        var paragraph = new DocumentNode(DocumentNodeType.Paragraph);
        paragraph.AddChild(new DocumentNode(DocumentNodeType.Text, "Hello"));

        var strong = new DocumentNode(DocumentNodeType.Strong);
        strong.AddChild(new DocumentNode(DocumentNodeType.Text, "World"));
        paragraph.AddChild(strong);

        GenerateDocument(column => sut.Compose(column, paragraph));

        inlineComposer.Nodes.ShouldBe(new[]
        {
            DocumentNodeType.Text,
            DocumentNodeType.Strong
        });
        listComposer.Called.ShouldBeFalse();
    }

    [Fact]
    public void Compose_List_DelegatesToListComposer()
    {
        var inlineComposer = new RecordingInlineComposer();
        var listComposer = new RecordingListComposer();
        var spacingApplier = new PassthroughSpacingApplier();
        var sut = new BlockComposer(inlineComposer, listComposer, spacingApplier);

        var listNode = new DocumentNode(DocumentNodeType.List);
        listNode.AddChild(new DocumentNode(DocumentNodeType.ListItem));

        GenerateDocument(column => sut.Compose(column, listNode));

        listComposer.Called.ShouldBeTrue();
        listComposer.LastOrdered.ShouldBeFalse();
    }

    private static void GenerateDocument(Action<ColumnDescriptor> compose)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.UseEnvironmentFonts = false;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(10);
                page.Content().Column(compose);
            });
        });

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
    }

    private sealed class RecordingInlineComposer : IInlineComposer
    {
        public List<DocumentNodeType> Nodes { get; } = [];

        public void Compose(QuestPDF.Fluent.TextDescriptor text, DocumentNode node, InlineStyleState style)
        {
            Nodes.Add(node.NodeType);
            var content = node.TextContent ?? node.NodeType.ToString();
            text.Span(content);
        }
    }

    private sealed class RecordingListComposer : IListComposer
    {
        public bool Called { get; private set; }
        public bool LastOrdered { get; private set; }

        public void Compose(ColumnDescriptor column, DocumentNode listNode, bool ordered, Action<ColumnDescriptor, DocumentNode> composeBlock)
        {
            Called = true;
            LastOrdered = ordered;
            column.Item().Text("list");
        }
    }

    private sealed class PassthroughSpacingApplier : IBlockSpacingApplier
    {
        public IContainer ApplySpacing(IContainer container, CssStyleMap styles) => container;
    }
}
