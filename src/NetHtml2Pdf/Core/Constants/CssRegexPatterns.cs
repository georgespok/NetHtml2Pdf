namespace NetHtml2Pdf.Core.Constants;

/// <summary>
/// Provides CSS regex pattern constants for use throughout the application.
/// </summary>
public static class CssRegexPatterns
{
    // CSS class rule pattern: .className { declarations }
    public const string ClassRule = @"\.(?<name>[A-Za-z0-9_-]+)\s*\{(?<body>[^}]*)\}";
    
    // HTML tag name validation pattern
    public const string HtmlTagName = @"^[a-zA-Z][a-zA-Z0-9-]*$";
    
    // CSS property name pattern (for validation)
    public const string CssPropertyName = @"^[a-zA-Z][a-zA-Z0-9-]*$";
    
    // CSS value pattern (basic validation)
    public const string CssValue = @"^[^;{}]+$";
}
