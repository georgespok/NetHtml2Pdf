using NetHtml2Pdf.Core;
using Xunit;

namespace NetHtml2Pdf.Test.Layout;

public class LayoutEngineContractTests
{
    [Fact]
    public void LayoutEngine_ShouldImplementInterface()
    {
        var engineInterface = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Engines.ILayoutEngine");
        var engineType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Engines.LayoutEngine");

        Assert.True(engineInterface.IsAssignableFrom(engineType),
            "LayoutEngine should implement ILayoutEngine");
    }

    [Fact]
    public void LayoutEngine_ShouldExposeLayoutMethodWithExpectedSignature()
    {
        var engineType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Engines.LayoutEngine");
        var layoutResultType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Engines.LayoutResult");
        var constraintsType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Model.LayoutConstraints");
        var optionsType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Engines.LayoutEngineOptions");

        var method = LayoutTestHelper.RequireMethod(engineType, "Layout", typeof(DocumentNode), constraintsType, optionsType);
        Assert.Equal(layoutResultType, method.ReturnType);
    }

    [Fact]
    public void LayoutResult_ShouldExposeStatusAndFragments()
    {
        var layoutResultType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Engines.LayoutResult");
        var fragmentType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Model.LayoutFragment");
        var fragmentsType = LayoutTestHelper.MakeReadOnlyListType(fragmentType);

        Assert.Equal(typeof(bool), LayoutTestHelper.RequireProperty(layoutResultType, "IsSuccess").PropertyType);
        Assert.Equal(typeof(bool), LayoutTestHelper.RequireProperty(layoutResultType, "IsFallback").PropertyType);
        Assert.Equal(typeof(bool), LayoutTestHelper.RequireProperty(layoutResultType, "IsDisabled").PropertyType);
        Assert.Equal(typeof(string), LayoutTestHelper.RequireProperty(layoutResultType, "FallbackReason").PropertyType);
        Assert.Equal(fragmentsType, LayoutTestHelper.RequireProperty(layoutResultType, "Fragments").PropertyType);
    }

    [Fact]
    public void LayoutEngineOptions_ShouldExposeFeatureFlags()
    {
        var optionsType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Engines.LayoutEngineOptions");

        Assert.Equal(typeof(bool), LayoutTestHelper.RequireProperty(optionsType, "EnableNewLayoutForTextBlocks").PropertyType);
        Assert.Equal(typeof(bool), LayoutTestHelper.RequireProperty(optionsType, "EnableDiagnostics").PropertyType);
    }
}
