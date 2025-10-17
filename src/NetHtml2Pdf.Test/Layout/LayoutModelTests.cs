using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using Xunit;

namespace NetHtml2Pdf.Test.Layout;

public class LayoutModelTests
{
    [Fact]
    public void LayoutBox_ShouldExpose_CoreProperties()
    {
        var layoutBoxType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Model.LayoutBox");
        var spacingType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Model.LayoutSpacing");
        var childrenType = LayoutTestHelper.MakeReadOnlyListType(layoutBoxType);

        LayoutTestHelper.RequireConstructor(
            layoutBoxType,
            typeof(DocumentNode),
            typeof(DisplayClass),
            typeof(CssStyleMap),
            spacingType,
            typeof(string),
            childrenType);

        Assert.Equal(typeof(DocumentNode), LayoutTestHelper.RequireProperty(layoutBoxType, "Node").PropertyType);
        Assert.Equal(typeof(DisplayClass), LayoutTestHelper.RequireProperty(layoutBoxType, "Display").PropertyType);
        Assert.Equal(typeof(CssStyleMap), LayoutTestHelper.RequireProperty(layoutBoxType, "Style").PropertyType);
        Assert.Equal(spacingType, LayoutTestHelper.RequireProperty(layoutBoxType, "Spacing").PropertyType);
        Assert.Equal(typeof(string), LayoutTestHelper.RequireProperty(layoutBoxType, "NodePath").PropertyType);
        Assert.Equal(childrenType, LayoutTestHelper.RequireProperty(layoutBoxType, "Children").PropertyType);

        var node = new DocumentNode(DocumentNodeType.Paragraph);
        var instance = LayoutTestHelper.CreateLayoutBox(node, DisplayClass.Block, "Paragraph:0");

        Assert.Same(node, LayoutTestHelper.RequireProperty(layoutBoxType, "Node").GetValue(instance));
        Assert.Equal(DisplayClass.Block, LayoutTestHelper.RequireProperty(layoutBoxType, "Display").GetValue(instance));
        Assert.Equal("Paragraph:0", LayoutTestHelper.RequireProperty(layoutBoxType, "NodePath").GetValue(instance));
    }

    [Fact]
    public void LayoutConstraints_ShouldCaptureInlineAndBlockRanges()
    {
        var constraintsType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Model.LayoutConstraints");

        LayoutTestHelper.RequireConstructor(
            constraintsType,
            typeof(float),
            typeof(float),
            typeof(float),
            typeof(float),
            typeof(float),
            typeof(bool));

        Assert.Equal(typeof(float), LayoutTestHelper.RequireProperty(constraintsType, "InlineMin").PropertyType);
        Assert.Equal(typeof(float), LayoutTestHelper.RequireProperty(constraintsType, "InlineMax").PropertyType);
        Assert.Equal(typeof(float), LayoutTestHelper.RequireProperty(constraintsType, "BlockMin").PropertyType);
        Assert.Equal(typeof(float), LayoutTestHelper.RequireProperty(constraintsType, "BlockMax").PropertyType);
        Assert.Equal(typeof(float), LayoutTestHelper.RequireProperty(constraintsType, "PageRemainingBlockSize").PropertyType);
        Assert.Equal(typeof(bool), LayoutTestHelper.RequireProperty(constraintsType, "AllowBreaks").PropertyType);

        var instance = LayoutTestHelper.CreateLayoutConstraints(100, 300, 0, 1000, 900, allowBreaks: true);

        Assert.Equal(100f, LayoutTestHelper.RequireProperty(constraintsType, "InlineMin").GetValue(instance));
        Assert.Equal(300f, LayoutTestHelper.RequireProperty(constraintsType, "InlineMax").GetValue(instance));
        Assert.Equal(0f, LayoutTestHelper.RequireProperty(constraintsType, "BlockMin").GetValue(instance));
        Assert.Equal(1000f, LayoutTestHelper.RequireProperty(constraintsType, "BlockMax").GetValue(instance));
        Assert.Equal(900f, LayoutTestHelper.RequireProperty(constraintsType, "PageRemainingBlockSize").GetValue(instance));
        Assert.Equal(true, LayoutTestHelper.RequireProperty(constraintsType, "AllowBreaks").GetValue(instance));
    }

    [Fact]
    public void LayoutFragment_ShouldExposeShapeBaselineAndDiagnostics()
    {
        var fragmentType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Model.LayoutFragment");
        var diagnosticsType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Model.LayoutDiagnostics");
        var layoutBoxType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Model.LayoutBox");
        var childrenType = LayoutTestHelper.MakeReadOnlyListType(fragmentType);
        var kindType = LayoutTestHelper.RequireType("NetHtml2Pdf.Layout.Model.LayoutFragmentKind");

        LayoutTestHelper.RequireConstructor(
            fragmentType,
            kindType,
            layoutBoxType,
            typeof(float),
            typeof(float),
            typeof(float?),
            childrenType,
            diagnosticsType);

        Assert.Equal(typeof(float), LayoutTestHelper.RequireProperty(fragmentType, "Width").PropertyType);
        Assert.Equal(typeof(float), LayoutTestHelper.RequireProperty(fragmentType, "Height").PropertyType);
        Assert.Equal(typeof(float?), LayoutTestHelper.RequireProperty(fragmentType, "Baseline").PropertyType);
        Assert.Equal(typeof(string), LayoutTestHelper.RequireProperty(fragmentType, "NodePath").PropertyType);
        Assert.Equal(childrenType, LayoutTestHelper.RequireProperty(fragmentType, "Children").PropertyType);
        Assert.Equal(diagnosticsType, LayoutTestHelper.RequireProperty(fragmentType, "Diagnostics").PropertyType);

        var fragment = LayoutTestHelper.CreateLayoutFragment(120, 40, 12, "Paragraph:0");

        Assert.Equal(120f, LayoutTestHelper.RequireProperty(fragmentType, "Width").GetValue(fragment));
        Assert.Equal(40f, LayoutTestHelper.RequireProperty(fragmentType, "Height").GetValue(fragment));
        Assert.Equal(12f, LayoutTestHelper.RequireProperty(fragmentType, "Baseline").GetValue(fragment));
        Assert.Equal("Paragraph:0", LayoutTestHelper.RequireProperty(fragmentType, "NodePath").GetValue(fragment));
    }
}
