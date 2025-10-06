using NetHtml2Pdf.Core;
using NetHtml2Pdf.Renderer;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Shouldly;

namespace NetHtml2Pdf.Test.Renderer;

public class ListComposerTests
{
    [Fact]
    public void Compose_ListItemWithInlineAndBlockContent_FlushesInlineBuffer()
    {
        var inlineComposer = new RecordingInlineComposer();
        var spacingApplier = new PassthroughSpacingApplier();
        var sut = new ListComposer(inlineComposer, spacingApplier);

        var listNode = new DocumentNode(DocumentNodeType.List);
        var listItem = new DocumentNode(DocumentNodeType.ListItem);
        listItem.AddChild(new DocumentNode(DocumentNodeType.Text, "First"));

        var paragraph = new DocumentNode(DocumentNodeType.Paragraph);
        paragraph.AddChild(new DocumentNode(DocumentNodeType.Text, "Nested"));
        listItem.AddChild(paragraph);

        listItem.AddChild(new DocumentNode(DocumentNodeType.Text, "Second"));
        listNode.AddChild(listItem);

        var forwarded = new List<DocumentNode>();

        GenerateDocument(column => sut.Compose(column, listNode, false, (c, node) =>
        {
            forwarded.Add(node);
            c.Item().Text("block");
        }));

        inlineComposer.Nodes.ShouldBe(new[]
        {
            DocumentNodeType.Text,
            DocumentNodeType.Text
        });
        forwarded.ShouldHaveSingleItem().NodeType.ShouldBe(DocumentNodeType.Paragraph);
    }

    [Fact]
    public void Compose_NonListItemChild_DelegatesToComposeBlock()
    {
        var inlineComposer = new RecordingInlineComposer();
        var spacingApplier = new PassthroughSpacingApplier();
        var sut = new ListComposer(inlineComposer, spacingApplier);

        var listNode = new DocumentNode(DocumentNodeType.List);
        var paragraph = new DocumentNode(DocumentNodeType.Paragraph);
        listNode.AddChild(paragraph);

        var forwarded = new List<DocumentNode>();

        GenerateDocument(column => sut.Compose(column, listNode, false, (c, node) =>
        {
            forwarded.Add(node);
            c.Item().Text("forwarded");
        }));

        forwarded.ShouldHaveSingleItem().ShouldBe(paragraph);
        inlineComposer.Nodes.ShouldBeEmpty();
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

        public void Compose(TextDescriptor text, DocumentNode node, InlineStyleState style)
        {
            Nodes.Add(node.NodeType);
            text.Span(node.TextContent ?? node.NodeType.ToString());
        }
    }

    private sealed class PassthroughSpacingApplier : IBlockSpacingApplier
    {
        public IContainer ApplySpacing(IContainer container, CssStyleMap styles) => container;
    }
}
