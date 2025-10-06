using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace NetHtml2Pdf.Test.Support;

internal static class PdfParser
{
    public static string[] ExtractWords(byte[] pdfBytes)
    {
        if (pdfBytes is null)
        {
            throw new ArgumentNullException(nameof(pdfBytes));
        }

        using var stream = new MemoryStream(pdfBytes);
        using var pdf = PdfDocument.Open(stream);

        var words = new List<string>();
        foreach (var page in pdf.GetPages())
        {
            words.AddRange(
                page.GetWords().Select(word => new string([.. word.Text.Where(ch => ch != 0)]))
                    .Where(cleaned => !string.IsNullOrWhiteSpace(cleaned)));
        }

        return [.. words];
    }

    public static string GetMostCommonTextColor(Word? word)
        {
            if (word == null)
            {
                return "#000000";
            }

            // Get all letters in the word
            var letters = word.Letters;
            if (letters.Count == 0)
            {
                return "#000000";
            }

            // Count colors by their hex representation
            var colorCounts = new Dictionary<string, int>();
            
            foreach (var letter in letters)
            {
                var colorHex = GetLetterColorHex(letter);
                colorCounts[colorHex] = colorCounts.GetValueOrDefault(colorHex, 0) + 1;
            }

            // Find the most common color
            var mostCommonColor = colorCounts
                .OrderByDescending(kvp => kvp.Value)
                .ThenBy(kvp => kvp.Key) // For tie-breaking, use first alphabetically
                .FirstOrDefault();

            return mostCommonColor.Key ?? "#000000";
        }

        /// <summary>
        /// Gets the color of a single letter as a hex string
        /// </summary>
        /// <param name="letter">The PdfPig Letter to analyze</param>
        /// <returns>Hex color string</returns>
        private static string GetLetterColorHex(Letter letter)
        {
            try
            {
                // Check FillColor first (most common for text)
                var hex = ConvertIColorToHex(letter.FillColor);
                if (hex != "#000000")
                    return hex;

                // Check Color property
                hex = ConvertIColorToHex(letter.Color);
                if (hex != "#000000")
                {
                    return hex;
                }

                // Check StrokeColor as fallback
                hex = ConvertIColorToHex(letter.StrokeColor);
                if (hex != "#000000")
                {
                    return hex;
                }
            }
            catch
            {
                // If any error occurs, return black
                return "#000000";
            }

            return "#000000"; // Default to black
        }

        /// <summary>
        /// Converts PdfPig IColor to hex string
        /// </summary>
        /// <param name="color">The IColor from PdfPig</param>
        /// <returns>Hex color string</returns>
        private static string ConvertIColorToHex(object color)
        {
            try
            {
                // Get the ToString representation which shows RGB values
                var colorString = color.ToString();
                
                // Parse RGB values from string like "RGB: (1, 0, 0)"
                if (colorString != null && colorString.StartsWith("RGB:"))
                {
                    var rgbPart = colorString.Substring(4).Trim(); // Remove "RGB:" and trim
                    rgbPart = rgbPart.Trim('(', ')'); // Remove parentheses
                    var values = rgbPart.Split(',');
                    
                    if (values.Length >= 3)
                    {
                        if (double.TryParse(values[0].Trim(), out var r) &&
                            double.TryParse(values[1].Trim(), out var g) &&
                            double.TryParse(values[2].Trim(), out var b))
                        {
                            // Convert from 0-1 range to 0-255 range
                            var red = (int)(r * 255);
                            var green = (int)(g * 255);
                            var blue = (int)(b * 255);
                            
                            return $"#{red:X2}{green:X2}{blue:X2}";
                        }
                    }
                }
            }
            catch
            {
                // If parsing fails, return black
            }

            return "#000000";
        }

        /// <summary>
        /// Attempts to convert various color representations to hex
        /// </summary>
        /// <param name="value">The color value to convert</param>
        /// <returns>Hex color string</returns>
        private static string ConvertToHex(object value)
        {
            try
            {
                // Handle different color representations
                if (value is System.Drawing.Color color)
                {
                    return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                }

                if (value is float[] rgbArray && rgbArray.Length >= 3)
                {
                    var r = (int)(rgbArray[0] * 255);
                    var g = (int)(rgbArray[1] * 255);
                    var b = (int)(rgbArray[2] * 255);
                    return $"#{r:X2}{g:X2}{b:X2}";
                }

                if (value is double[] rgbArrayD && rgbArrayD.Length >= 3)
                {
                    var r = (int)(rgbArrayD[0] * 255);
                    var g = (int)(rgbArrayD[1] * 255);
                    var b = (int)(rgbArrayD[2] * 255);
                    return $"#{r:X2}{g:X2}{b:X2}";
                }

                // Try to parse as string
                if (value is string str)
                {
                    if (str.StartsWith("#"))
                        return str;
                    
                    // Try to parse common color names
                    return str.ToLowerInvariant() switch
                    {
                        "red" => "#FF0000",
                        "green" => "#00FF00",
                        "blue" => "#0000FF",
                        "black" => "#000000",
                        "white" => "#FFFFFF",
                        "yellow" => "#FFFF00",
                        "cyan" => "#00FFFF",
                        "magenta" => "#FF00FF",
                        _ => "#000000"
                    };
                }
            }
            catch
            {
                // If conversion fails, return black
            }

            return "#000000";
        }
}
