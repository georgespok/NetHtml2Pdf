using Xunit;

namespace NetHtml2Pdf.Test.Layout;

public class LegacyFallbackTests
{
    [Fact]
    public void LayoutResult_Fallback_ShouldFlagLegacyPath()
    {
        var layoutResultType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Engines.LayoutResult");

        var fallbackMethod = layoutResultType.GetMethod(
            "Fallback",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
            binder: null,
            types: new[] { typeof(string) },
            modifiers: null);

        Assert.NotNull(fallbackMethod);

        var result = fallbackMethod!.Invoke(null, new object?[] { "Unsupported element" })!;

        Assert.True((bool)LayoutTestHelper.RequireProperty(layoutResultType, "IsFallback").GetValue(result)!);
        Assert.Equal("Unsupported element", LayoutTestHelper.RequireProperty(layoutResultType, "FallbackReason").GetValue(result));
    }

    [Fact]
    public void LayoutResult_Disabled_ShouldReportFlagOff()
    {
        var layoutResultType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Engines.LayoutResult");

        var disabledMethod = layoutResultType.GetMethod(
            "Disabled",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static,
            binder: null,
            types: Type.EmptyTypes,
            modifiers: null);

        Assert.NotNull(disabledMethod);

        var result = disabledMethod!.Invoke(null, Array.Empty<object>())!;

        Assert.True((bool)LayoutTestHelper.RequireProperty(layoutResultType, "IsDisabled").GetValue(result)!);
        Assert.False((bool)LayoutTestHelper.RequireProperty(layoutResultType, "IsSuccess").GetValue(result)!);
    }
}

