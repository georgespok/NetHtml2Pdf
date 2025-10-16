using NetHtml2Pdf.Core;
using NetHtml2Pdf.Renderer.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace NetHtml2Pdf.Renderer.Spacing;

/// <summary>
/// Internal helper class that applies spacing (margin, border, padding) in the correct order
/// to preserve current visual output exactly. Delegates to existing BlockSpacingApplier.
/// </summary>
internal static class WrapWithSpacing
{
    /// <summary>
    /// Applies spacing to a container in the correct order: margin(parent) → border(element) → padding(element).
    /// This preserves current visual output exactly by delegating to existing BlockSpacingApplier methods.
    /// </summary>
    /// <param name="container">The QuestPDF container to apply spacing to</param>
    /// <param name="parentStyles">CSS styles from the parent container (for margin)</param>
    /// <param name="elementStyles">CSS styles from the element (for border and padding)</param>
    /// <param name="spacingApplier">The BlockSpacingApplier instance to delegate to</param>
    /// <returns>The container with all spacing applied in the correct order</returns>
    public static IContainer ApplySpacing(IContainer container, 
        CssStyleMap parentStyles, 
        CssStyleMap elementStyles, 
        IBlockSpacingApplier spacingApplier)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(parentStyles);
        ArgumentNullException.ThrowIfNull(elementStyles);
        ArgumentNullException.ThrowIfNull(spacingApplier);

        // Order: Margin (parent) → Border (element) → Padding (element)
        
        // 1. Apply margin from parent styles (affects positioning relative to siblings)
        container = spacingApplier.ApplyMargin(container, parentStyles);
        
        // 2. Apply border from element styles (adds border around element's content area)
        container = spacingApplier.ApplyBorder(container, elementStyles);
        
        // 3. Apply padding from element styles (affects content area inside the element)
        container = spacingApplier.ApplySpacing(container, elementStyles);

        return container;
    }

    /// <summary>
    /// Applies spacing to a container using only element styles (no parent margin).
    /// This is a convenience method for cases where parent margin is not needed.
    /// </summary>
    /// <param name="container">The QuestPDF container to apply spacing to</param>
    /// <param name="elementStyles">CSS styles from the element (for border and padding)</param>
    /// <param name="spacingApplier">The BlockSpacingApplier instance to delegate to</param>
    /// <returns>The container with border and padding applied</returns>
    public static IContainer ApplyElementSpacing(IContainer container, 
        CssStyleMap elementStyles, 
        IBlockSpacingApplier spacingApplier)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(elementStyles);
        ArgumentNullException.ThrowIfNull(spacingApplier);

        // Apply border and padding only (no parent margin)
        container = spacingApplier.ApplyBorder(container, elementStyles);
        container = spacingApplier.ApplySpacing(container, elementStyles);

        return container;
    }

    /// <summary>
    /// Applies spacing to nested containers with cumulative parent padding simulation.
    /// This preserves the current behavior where nested containers accumulate padding
    /// without clamping, maintaining visual output exactly.
    /// </summary>
    /// <param name="container">The QuestPDF container to apply spacing to</param>
    /// <param name="parentStyles">CSS styles from the parent container</param>
    /// <param name="elementStyles">CSS styles from the element</param>
    /// <param name="spacingApplier">The BlockSpacingApplier instance to delegate to</param>
    /// <returns>The container with cumulative spacing applied</returns>
    public static IContainer ApplyNestedSpacing(IContainer container,
        CssStyleMap parentStyles,
        CssStyleMap elementStyles,
        IBlockSpacingApplier spacingApplier)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(parentStyles);
        ArgumentNullException.ThrowIfNull(elementStyles);
        ArgumentNullException.ThrowIfNull(spacingApplier);

        // Apply parent padding first (cumulative, no clamping)
        container = spacingApplier.ApplySpacing(container, parentStyles);
        
        // Then apply element border and padding
        container = spacingApplier.ApplyBorder(container, elementStyles);
        container = spacingApplier.ApplySpacing(container, elementStyles);

        return container;
    }

    /// <summary>
    /// Creates a new BlockSpacingApplier instance for use with WrapWithSpacing methods.
    /// This is a convenience method for creating the default spacing applier.
    /// </summary>
    /// <returns>A new BlockSpacingApplier instance</returns>
    public static IBlockSpacingApplier CreateSpacingApplier()
    {
        return new BlockSpacingApplier();
    }
}
