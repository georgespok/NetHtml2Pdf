using System.Linq;
using Xunit;

namespace NetHtml2Pdf.Test.Layout;

public class BlockFormattingContextTests
{
    [Fact]
    public void BlockFormattingContext_ShouldAcceptInlineFormattingDependency()
    {
        var inlineContract = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Contexts.IInlineFormattingContext");
        var blockContextType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Contexts.BlockFormattingContext");

        var hasExpectedCtor = blockContextType
            .GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Any(ctor =>
            {
                var parameters = ctor.GetParameters();
                return parameters.Length > 0 && parameters[0].ParameterType == inlineContract;
            });

        Assert.True(hasExpectedCtor, "BlockFormattingContext constructor with inline formatting dependency not available yet.");
    }

    [Fact]
    public void BlockFormattingContext_ShouldExposeLayoutMethod()
    {
        var layoutBoxType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Model.LayoutBox");
        var constraintsType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Model.LayoutConstraints");
        var fragmentType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Model.LayoutFragment");
        var blockContextType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Contexts.BlockFormattingContext");

        var layoutMethod = LayoutTestHelper.RequireMethod(blockContextType, "Layout", layoutBoxType, constraintsType);
        Assert.Equal(fragmentType, layoutMethod.ReturnType);
    }
}
