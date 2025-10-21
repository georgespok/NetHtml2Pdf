namespace NetHtml2Pdf.Core.Enums;

/// <summary>
///     Indicates the source of CSS styling.
/// </summary>
public enum CssStyleSource
{
    /// <summary>
    ///     Applied via style attribute
    /// </summary>
    Inline,

    /// <summary>
    ///     Applied via CSS class
    /// </summary>
    Class,

    /// <summary>
    ///     Inherited from parent element
    /// </summary>
    Inherited,

    /// <summary>
    ///     Browser default styling
    /// </summary>
    Default
}