using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Display;
using NetHtml2Pdf.Layout.Engines;
using NetHtml2Pdf.Layout.FormattingContexts;
using NetHtml2Pdf.Layout.Model;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Test.Support;
using Shouldly;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Layout.FormattingContexts;

public class FormattingContextsContractTests(ITestOutputHelper output) : PdfRenderTestBase(output)
{
    [Fact]
    public void InlineBlockContext_ShouldProduceFragments_WhenFlagEnabled()
    {
        var options = new RendererOptions
        {
            EnableNewLayoutForTextBlocks = true,
            EnableInlineBlockContext = true,
            EnableLayoutDiagnostics = true
        };

        var layoutEngine = CreateLayoutEngine(options);
        var layoutOptions = LayoutEngineOptions.FromRendererOptions(options);
        var constraints = DefaultConstraints();

        var inlineBlockNode = Span(CssStyleMap.Empty.WithDisplay(CssDisplay.InlineBlock),
            Text("badge"));

        var root = Paragraph(inlineBlockNode);

        var result = layoutEngine.Layout(root, constraints, layoutOptions);

        result.IsSuccess.ShouldBeTrue(
            "inline-block nodes should be handled by InlineBlockFormattingContext when the flag is enabled.");
        result.Fragments.ShouldNotBeEmpty();
        result.Fragments[0].Diagnostics.ContextName.ShouldBe("InlineBlockFormattingContext");
        result.Fragments[0].Display.ShouldBe(DisplayClass.InlineBlock);
    }

    [Fact]
    public void TableContext_ShouldRepeatHeaders_WhenFlagEnabled()
    {
        var options = new RendererOptions
        {
            EnableNewLayoutForTextBlocks = true,
            EnableTableContext = true,
            EnableLayoutDiagnostics = true
        };

        var layoutEngine = CreateLayoutEngine(options);
        var layoutOptions = LayoutEngineOptions.FromRendererOptions(options);
        var constraints = DefaultConstraints();

        var table = BuildSimpleTable();

        var result = layoutEngine.Layout(table, constraints, layoutOptions);

        result.IsSuccess.ShouldBeTrue(
            "table nodes should be handled by TableFormattingContext when the flag is enabled.");
        result.Fragments.ShouldNotBeEmpty();
        result.Fragments[0].Diagnostics.ContextName.ShouldBe("TableFormattingContext");
    }

    [Fact]
    public void FlexContext_ShouldEmitAlignmentMetadata_WhenFlagEnabled()
    {
        var options = new RendererOptions
        {
            EnableNewLayoutForTextBlocks = true,
            EnableFlexContext = true,
            EnableLayoutDiagnostics = true
        };

        var layoutEngine = CreateLayoutEngine(options);
        var layoutOptions = LayoutEngineOptions.FromRendererOptions(options);
        var constraints = DefaultConstraints();

        var flexContainer = BuildFlexContainer();

        var result = layoutEngine.Layout(flexContainer, constraints, layoutOptions);

        result.IsSuccess.ShouldBeTrue(
            "flex containers should be processed by FlexFormattingContext when the preview flag is enabled.");
        result.Fragments.ShouldNotBeEmpty();
        result.Fragments[0].Diagnostics.ContextName.ShouldBe("FlexFormattingContext");
        result.Fragments[0].Diagnostics.Metadata.ShouldNotBeNull();
    }

    private static LayoutEngine CreateLayoutEngine(RendererOptions options)
    {
        var layoutOptions = LayoutEngineOptions.FromRendererOptions(options);
        var formattingContextFactory = FormattingContextFactory.CreateDefault(layoutOptions);
        var displayClassifier = new DisplayClassifier(options: options);

        return new LayoutEngine(
            displayClassifier,
            formattingContextFactory.GetBlockFormattingContext(),
            formattingContextFactory.GetInlineFormattingContext(),
            formattingContextFactory.GetInlineBlockFormattingContext(),
            formattingContextFactory.GetTableFormattingContext(),
            formattingContextFactory.GetFlexFormattingContext(),
            formattingContextFactory.Options);
    }

    private static LayoutConstraints DefaultConstraints()
    {
        return new LayoutConstraints(0, 400, 0, 400, 400, true);
    }

    private static DocumentNode BuildSimpleTable()
    {
        var tableHead = TableHead(
            TableRow(TableHeaderCell(Text("Header"))));

        var tableBody = TableBody(
            TableRow(TableCell(Text("Value"))));

        return Table(tableHead, tableBody);
    }

    private static DocumentNode BuildFlexContainer()
    {
        var item1 = Div(Text("Item 1"));
        var item2 = Div(Text("Item 2"));

        var flexStyles = CssStyleMap.Empty.WithDisplay(CssDisplay.Block);
        return Div(flexStyles, item1, item2);
    }
}