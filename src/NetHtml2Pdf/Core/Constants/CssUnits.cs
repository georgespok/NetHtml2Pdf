namespace NetHtml2Pdf.Core.Constants;

/// <summary>
/// Provides CSS unit constants for use throughout the application.
/// </summary>
public static class CssUnits
{
    // Length units
    public const string Pixels = "px";
    public const string Rem = "rem";
    public const string Em = "em";
    public const string Points = "pt";

    // RGB function
    public const string RgbFunction = "rgb(";
    public const string RgbFunctionEnd = ")";

    // Hex color prefix
    public const string HexPrefix = "#";

    // Border width keyword values (in pixels)
    public const double ThinWidth = 1.0;
    public const double MediumWidth = 3.0;
    public const double ThickWidth = 5.0;

    // Default border values
    public const double DefaultBorderWidth = 1.0;
    public const string DefaultBorderStyle = "solid";
    public const string DefaultBorderColor = "#000000";
}
