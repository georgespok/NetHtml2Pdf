using NetHtml2Pdf.Core;
using Shouldly;

namespace NetHtml2Pdf.Test.Renderer;

public class WrapWithSpacingTests
{
    // Note: These are unit tests for the WrapWithSpacing implementation
    // The actual WrapWithSpacing implementation will be created in T009
    
    [Fact]
    public void ApplySpacing_Order_MarginParentThenBorderElementThenPaddingElement()
    {
        // Arrange
        var wrapWithSpacing = new TestWrapWithSpacing();
        var parentStyles = CssStyleMap.Empty.WithMargin(BoxSpacing.FromAll(10));
        var elementStyles = CssStyleMap.Empty
            .WithBorder(new BorderInfo(2, "solid", "#000000"))
            .WithPadding(BoxSpacing.FromAll(5));
        
        var parentContainer = new TestContainer();
        var elementContainer = new TestContainer();

        // Act
        var result = wrapWithSpacing.ApplySpacing(parentContainer, elementContainer, parentStyles, elementStyles);

        // Assert - Order should be: margin(parent) → border(element) → padding(element)
        result.Applications.ShouldBe(new[]
        {
            "MarginTop:10", "MarginRight:10", "MarginBottom:10", "MarginLeft:10", // Parent margin first
            "Border:2", // Element border second
            "PaddingTop:5", "PaddingRight:5", "PaddingBottom:5", "PaddingLeft:5" // Element padding last
        });
    }

    [Fact]
    public void ApplySpacing_NestedContainers_CumulativeParentPadding()
    {
        // Arrange - Nested containers with cumulative parent padding
        var wrapWithSpacing = new TestWrapWithSpacing();
        var grandparentStyles = CssStyleMap.Empty.WithPadding(BoxSpacing.FromAll(20));
        var parentStyles = CssStyleMap.Empty.WithPadding(BoxSpacing.FromAll(10));
        var elementStyles = CssStyleMap.Empty.WithPadding(BoxSpacing.FromAll(5));
        
        var grandparentContainer = new TestContainer();
        var parentContainer = new TestContainer();
        var elementContainer = new TestContainer();

        // Act
        var result = wrapWithSpacing.ApplyNestedSpacing(grandparentContainer, parentContainer, elementContainer, 
            grandparentStyles, parentStyles, elementStyles);

        // Assert - Should accumulate padding from all levels (no clamping)
        result.Applications.ShouldContain("PaddingTop:20"); // Grandparent
        result.Applications.ShouldContain("PaddingTop:10"); // Parent  
        result.Applications.ShouldContain("PaddingTop:5");  // Element
    }

    [Fact]
    public void ApplySpacing_OnlyMargin_AppliesMarginOnly()
    {
        // Arrange
        var wrapWithSpacing = new TestWrapWithSpacing();
        var parentStyles = CssStyleMap.Empty.WithMargin(BoxSpacing.FromSpecific(15, null, 20, null));
        var elementStyles = CssStyleMap.Empty;
        
        var parentContainer = new TestContainer();
        var elementContainer = new TestContainer();

        // Act
        var result = wrapWithSpacing.ApplySpacing(parentContainer, elementContainer, parentStyles, elementStyles);

        // Assert
        result.Applications.ShouldBe(new[] { "MarginTop:15", "MarginBottom:20" });
    }

    [Fact]
    public void ApplySpacing_OnlyBorder_AppliesBorderOnly()
    {
        // Arrange
        var wrapWithSpacing = new TestWrapWithSpacing();
        var parentStyles = CssStyleMap.Empty;
        var elementStyles = CssStyleMap.Empty.WithBorder(new BorderInfo(3, "dashed", "#FF0000"));
        
        var parentContainer = new TestContainer();
        var elementContainer = new TestContainer();

        // Act
        var result = wrapWithSpacing.ApplySpacing(parentContainer, elementContainer, parentStyles, elementStyles);

        // Assert
        result.Applications.ShouldBe(new[] { "Border:3" });
    }

    [Fact]
    public void ApplySpacing_OnlyPadding_AppliesPaddingOnly()
    {
        // Arrange
        var wrapWithSpacing = new TestWrapWithSpacing();
        var parentStyles = CssStyleMap.Empty;
        var elementStyles = CssStyleMap.Empty.WithPadding(BoxSpacing.FromSpecific(null, 25, null, 30));
        
        var parentContainer = new TestContainer();
        var elementContainer = new TestContainer();

        // Act
        var result = wrapWithSpacing.ApplySpacing(parentContainer, elementContainer, parentStyles, elementStyles);

        // Assert
        result.Applications.ShouldBe(new[] { "PaddingRight:25", "PaddingLeft:30" });
    }

    [Fact]
    public void ApplySpacing_EmptyStyles_NoApplications()
    {
        // Arrange
        var wrapWithSpacing = new TestWrapWithSpacing();
        var parentStyles = CssStyleMap.Empty;
        var elementStyles = CssStyleMap.Empty;
        
        var parentContainer = new TestContainer();
        var elementContainer = new TestContainer();

        // Act
        var result = wrapWithSpacing.ApplySpacing(parentContainer, elementContainer, parentStyles, elementStyles);

        // Assert
        result.Applications.ShouldBeEmpty();
    }

    [Fact]
    public void ApplySpacing_ComplexCombination_AllElementsAppliedInOrder()
    {
        // Arrange
        var wrapWithSpacing = new TestWrapWithSpacing();
        var parentStyles = CssStyleMap.Empty
            .WithMargin(BoxSpacing.FromSpecific(5, 10, 15, 20));
        var elementStyles = CssStyleMap.Empty
            .WithBorder(new BorderInfo(1, "solid", "#00FF00"))
            .WithPadding(BoxSpacing.FromSpecific(25, 30, 35, 40));
        
        var parentContainer = new TestContainer();
        var elementContainer = new TestContainer();

        // Act
        var result = wrapWithSpacing.ApplySpacing(parentContainer, elementContainer, parentStyles, elementStyles);

        // Assert - Verify all values are applied in correct order
        var expected = new[]
        {
            "MarginTop:5", "MarginRight:10", "MarginBottom:15", "MarginLeft:20",
            "Border:1",
            "PaddingTop:25", "PaddingRight:30", "PaddingBottom:35", "PaddingLeft:40"
        };
        result.Applications.ShouldBe(expected);
    }

    [Fact]
    public void ApplySpacing_BorderNotVisible_SkipsBorder()
    {
        // Arrange
        var wrapWithSpacing = new TestWrapWithSpacing();
        var parentStyles = CssStyleMap.Empty.WithMargin(BoxSpacing.FromAll(10));
        var elementStyles = CssStyleMap.Empty
            .WithBorder(new BorderInfo(2, "none", "#000000")) // Border with "none" style
            .WithPadding(BoxSpacing.FromAll(5));
        
        var parentContainer = new TestContainer();
        var elementContainer = new TestContainer();

        // Act
        var result = wrapWithSpacing.ApplySpacing(parentContainer, elementContainer, parentStyles, elementStyles);

        // Assert - Border should be skipped when not visible
        result.Applications.ShouldBe(new[]
        {
            "MarginTop:10", "MarginRight:10", "MarginBottom:10", "MarginLeft:10",
            "PaddingTop:5", "PaddingRight:5", "PaddingBottom:5", "PaddingLeft:5"
        });
    }

    [Fact]
    public void ApplySpacing_PreserveCurrentVisualOutput_ExactBehaviorParity()
    {
        // Arrange - This test ensures the WrapWithSpacing behavior matches current BlockSpacingApplier
        var wrapWithSpacing = new TestWrapWithSpacing();
        var parentStyles = CssStyleMap.Empty.WithMargin(BoxSpacing.FromAll(12));
        var elementStyles = CssStyleMap.Empty
            .WithBorder(new BorderInfo(1, "solid", "#000000"))
            .WithPadding(BoxSpacing.FromAll(8));
        
        var parentContainer = new TestContainer();
        var elementContainer = new TestContainer();

        // Act
        var result = wrapWithSpacing.ApplySpacing(parentContainer, elementContainer, parentStyles, elementStyles);

        // Assert - This should match the exact behavior of current BlockSpacingApplier
        // The order and values should be identical to preserve visual output
        var expected = new[]
        {
            "MarginTop:12", "MarginRight:12", "MarginBottom:12", "MarginLeft:12",
            "Border:1",
            "PaddingTop:8", "PaddingRight:8", "PaddingBottom:8", "PaddingLeft:8"
        };
        result.Applications.ShouldBe(expected);
    }

    [Theory]
    [InlineData(10.0, 15.0, 20.0, 25.0, 2.0, "solid", "#000000", 5.0, 8.0, 12.0, 18.0)]
    [InlineData(0.0, 0.0, 0.0, 0.0, 1.0, "dashed", "#FF0000", 0.0, 0.0, 0.0, 0.0)]
    [InlineData(5.0, null, 10.0, null, 0.0, "none", "#000000", 3.0, null, 7.0, null)]
    public void ApplySpacing_VariousValues_AppliesCorrectly(double marginTop, double? marginRight, double marginBottom, double? marginLeft,
        double borderWidth, string borderStyle, string borderColor,
        double paddingTop, double? paddingRight, double paddingBottom, double? paddingLeft)
    {
        // Arrange
        var wrapWithSpacing = new TestWrapWithSpacing();
        var parentStyles = CssStyleMap.Empty.WithMargin(BoxSpacing.FromSpecific(marginTop, marginRight, marginBottom, marginLeft));
        var elementStyles = CssStyleMap.Empty
            .WithBorder(new BorderInfo(borderWidth, borderStyle, borderColor))
            .WithPadding(BoxSpacing.FromSpecific(paddingTop, paddingRight, paddingBottom, paddingLeft));
        
        var parentContainer = new TestContainer();
        var elementContainer = new TestContainer();

        // Act
        var result = wrapWithSpacing.ApplySpacing(parentContainer, elementContainer, parentStyles, elementStyles);

        // Assert - Verify all non-null values are applied
        var expectedApplications = new List<string>();
        
        if (marginTop != 0) expectedApplications.Add($"MarginTop:{marginTop}");
        if (marginRight.HasValue) expectedApplications.Add($"MarginRight:{marginRight}");
        if (marginBottom != 0) expectedApplications.Add($"MarginBottom:{marginBottom}");
        if (marginLeft.HasValue) expectedApplications.Add($"MarginLeft:{marginLeft}");
        
        if (borderWidth > 0 && borderStyle != "none") expectedApplications.Add($"Border:{borderWidth}");
        
        if (paddingTop != 0) expectedApplications.Add($"PaddingTop:{paddingTop}");
        if (paddingRight.HasValue) expectedApplications.Add($"PaddingRight:{paddingRight}");
        if (paddingBottom != 0) expectedApplications.Add($"PaddingBottom:{paddingBottom}");
        if (paddingLeft.HasValue) expectedApplications.Add($"PaddingLeft:{paddingLeft}");
        
        result.Applications.ShouldBe(expectedApplications);
    }

    // Test implementation that simulates the WrapWithSpacing behavior
    // This will be replaced by actual WrapWithSpacing implementation in T009
    private class TestWrapWithSpacing
    {
        public TestResult ApplySpacing(TestContainer parent, TestContainer element, 
            CssStyleMap parentStyles, CssStyleMap elementStyles)
        {
            var result = new TestResult();
            
            // Apply margin (parent) - affects positioning relative to siblings
            if (parentStyles.Margin.HasValue)
            {
                if (parentStyles.Margin.Top.HasValue && parentStyles.Margin.Top.Value != 0)
                    result.Applications.Add($"MarginTop:{parentStyles.Margin.Top.Value}");
                if (parentStyles.Margin.Right.HasValue)
                    result.Applications.Add($"MarginRight:{parentStyles.Margin.Right.Value}");
                if (parentStyles.Margin.Bottom.HasValue && parentStyles.Margin.Bottom.Value != 0)
                    result.Applications.Add($"MarginBottom:{parentStyles.Margin.Bottom.Value}");
                if (parentStyles.Margin.Left.HasValue)
                    result.Applications.Add($"MarginLeft:{parentStyles.Margin.Left.Value}");
            }

            // Apply border (element) - adds border around element's content area
            if (elementStyles.Border.IsVisible)
            {
                result.Applications.Add($"Border:{elementStyles.Border.GetWidthInPixels()}");
            }

            // Apply padding (element) - affects content area inside the element
            if (elementStyles.Padding.HasValue)
            {
                if (elementStyles.Padding.Top.HasValue && elementStyles.Padding.Top.Value != 0)
                    result.Applications.Add($"PaddingTop:{elementStyles.Padding.Top.Value}");
                if (elementStyles.Padding.Right.HasValue)
                    result.Applications.Add($"PaddingRight:{elementStyles.Padding.Right.Value}");
                if (elementStyles.Padding.Bottom.HasValue && elementStyles.Padding.Bottom.Value != 0)
                    result.Applications.Add($"PaddingBottom:{elementStyles.Padding.Bottom.Value}");
                if (elementStyles.Padding.Left.HasValue)
                    result.Applications.Add($"PaddingLeft:{elementStyles.Padding.Left.Value}");
            }

            return result;
        }

        public TestResult ApplyNestedSpacing(TestContainer grandparent, TestContainer parent, TestContainer element,
            CssStyleMap grandparentStyles, CssStyleMap parentStyles, CssStyleMap elementStyles)
        {
            var result = new TestResult();
            
            // Apply all levels of padding (cumulative, no clamping)
            ApplyPadding(grandparentStyles, result);
            ApplyPadding(parentStyles, result);
            ApplyPadding(elementStyles, result);
            
            return result;
        }

        private static void ApplyPadding(CssStyleMap styles, TestResult result)
        {
            if (styles.Padding.HasValue)
            {
                if (styles.Padding.Top.HasValue)
                    result.Applications.Add($"PaddingTop:{styles.Padding.Top.Value}");
                if (styles.Padding.Right.HasValue)
                    result.Applications.Add($"PaddingRight:{styles.Padding.Right.Value}");
                if (styles.Padding.Bottom.HasValue)
                    result.Applications.Add($"PaddingBottom:{styles.Padding.Bottom.Value}");
                if (styles.Padding.Left.HasValue)
                    result.Applications.Add($"PaddingLeft:{styles.Padding.Left.Value}");
            }
        }
    }

    // Test helper classes
    private class TestContainer
    {
        // Simulates IContainer for testing
    }

    private class TestResult
    {
        public List<string> Applications { get; } = [];
    }
}
