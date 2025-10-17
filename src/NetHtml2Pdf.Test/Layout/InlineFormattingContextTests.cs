using NetHtml2Pdf.Renderer.Inline;
using Xunit;

namespace NetHtml2Pdf.Test.Layout;

public class InlineFormattingContextTests
{
    [Fact]
    public void InlineFormattingContext_ShouldImplementContractInterface()
    {
        var interfaceType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Contexts.IInlineFormattingContext");
        var inlineContextType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Contexts.InlineFormattingContext");

        Assert.True(interfaceType.IsAssignableFrom(inlineContextType),
            "InlineFormattingContext should implement IInlineFormattingContext");
    }

    [Fact]
    public void InlineFormattingContext_ShouldAcceptInlineFlowEngine()
    {
        var inlineContextType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Contexts.InlineFormattingContext");
        var engineType = typeof(InlineFlowLayoutEngine);

        var hasCtor = inlineContextType
            .GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Any(ctor =>
            {
                var parameters = ctor.GetParameters();
                return parameters.Length > 0 && parameters[0].ParameterType == engineType;
            });

        Assert.True(hasCtor, "InlineFormattingContext constructor with InlineFlowLayoutEngine dependency not available yet.");
    }

    [Fact]
    public void InlineFormattingContext_LayoutMethod_ShouldReturnLayoutFragment()
    {
        var inlineContextType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Contexts.InlineFormattingContext");
        var layoutBoxType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Model.LayoutBox");
        var constraintsType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Model.LayoutConstraints");
        var fragmentType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Model.LayoutFragment");

        var layoutMethod = LayoutTestHelper.RequireMethod(inlineContextType, "Layout", layoutBoxType, constraintsType);
        Assert.Equal(fragmentType, layoutMethod.ReturnType);
    }
}
