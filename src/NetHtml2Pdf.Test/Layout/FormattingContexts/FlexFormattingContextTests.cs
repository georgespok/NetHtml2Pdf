using NetHtml2Pdf.Core;
using NetHtml2Pdf.Layout.Display;
using NetHtml2Pdf.Layout.Engines;
using NetHtml2Pdf.Layout.FormattingContexts;
using NetHtml2Pdf.Layout.Model;
using NetHtml2Pdf.Renderer;
using NetHtml2Pdf.Test.Support;
using Shouldly;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Layout.FormattingContexts;

[Collection("PdfRendering")]
public class FlexFormattingContextTests(ITestOutputHelper output) : PdfRenderTestBase(output)
{
    [Fact]
    public void Layout_WithFlexContainer_ShouldEmitAlignmentMetadata_WhenFlagEnabled()
    {
        var (engine, layoutOptions) = CreateEngine();
        var constraints = new LayoutConstraints(0, 400, 0, 200, 200, true);

        var root = BuildFlexContainer();

        var result = engine.Layout(root, constraints, layoutOptions);

        result.IsSuccess.ShouldBeTrue(
            "Flex containers should be processed by FlexFormattingContext when the preview flag is enabled.");
        result.Fragments.ShouldNotBeEmpty();
        var fragment = result.Fragments[0];
        fragment.Diagnostics.ContextName.ShouldBe("FlexFormattingContext");
        fragment.Diagnostics.Metadata.ShouldNotBeNull();
        fragment.Diagnostics.Metadata.ContainsKey("flex:direction").ShouldBeTrue();
        fragment.Diagnostics.Metadata.ContainsKey("flex:justify").ShouldBeTrue();
        fragment.Diagnostics.Metadata.ContainsKey("flex:align").ShouldBeTrue();
        fragment.Diagnostics.Metadata.ContainsKey("flex:wrap").ShouldBeTrue();
    }

    [Fact]
    public void Layout_WithFlexFlagDisabled_ShouldNotUseFlexContext()
    {
        var options = new RendererOptions
        {
            EnableNewLayoutForTextBlocks = true,
            EnableFlexContext = false
        };

        var (engine, layoutOptions) = CreateEngine(options);
        var constraints = new LayoutConstraints(0, 400, 0, 200, 200, true);

        var root = BuildFlexContainer();

        var result = engine.Layout(root, constraints, layoutOptions);

        result.IsSuccess.ShouldBeTrue("Engine should fall back to non-flex handling when flag is disabled.");
        result.Fragments.ShouldNotBeEmpty();
        result.Fragments[0].Diagnostics.ContextName.ShouldNotBe("FlexFormattingContext");
    }

    private static (LayoutEngine engine, LayoutEngineOptions options) CreateEngine(
        RendererOptions? optionsOverride = null)
    {
        var rendererOptions = optionsOverride ?? new RendererOptions
        {
            EnableNewLayoutForTextBlocks = true,
            EnableFlexContext = true
        };

        var layoutOptions = LayoutEngineOptions.FromRendererOptions(rendererOptions);
        var factory = FormattingContextFactory.CreateDefault(layoutOptions);
        var classifier = new DisplayClassifier(options: rendererOptions);

        var engine = new LayoutEngine(
            classifier,
            factory.GetBlockFormattingContext(),
            factory.GetInlineFormattingContext(),
            factory.GetInlineBlockFormattingContext(),
            factory.GetTableFormattingContext(),
            factory.GetFlexFormattingContext(),
            factory.Options);

        return (engine, layoutOptions);
    }

    private static DocumentNode BuildFlexContainer()
    {
        var item1 = Div(Text("Item 1"));
        var item2 = Div(Text("Item 2"));
        return Div(item1, item2);
    }
}