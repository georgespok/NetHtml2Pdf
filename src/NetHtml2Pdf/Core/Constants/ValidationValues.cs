namespace NetHtml2Pdf.Core.Constants;

/// <summary>
///     Provides validation value constants for use throughout the application.
/// </summary>
public static class ValidationValues
{
    // File size limits
    public const long MinOutputSize = 1024; // 1KB
    public const long MaxOutputSize = 100L * 1024 * 1024; // 100MB

    // Memory constraints
    public const long MinMemoryUsage = 0;
    public const long MaxMemoryUsage = 1024L * 1024 * 1024; // 1GB (reasonable limit)

    // Element count constraints
    public const int MinElementCount = 0;
    public const int MaxElementCount = 10000; // Reasonable limit for HTML elements

    // CSS property count constraints
    public const int MinCssPropertyCount = 0;

    public const int MaxCssPropertyCount = 1000; // Reasonable limit for CSS properties

    // Platform names
    public static readonly HashSet<string> ValidPlatforms = ["Windows", "Linux"];

    // Validation result types
    public static readonly HashSet<string> ValidValidationResults = ["IDENTICAL", "DIFFERENT", "ERROR"];

    // Date constraints
    public static readonly DateTime MinDate = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
}