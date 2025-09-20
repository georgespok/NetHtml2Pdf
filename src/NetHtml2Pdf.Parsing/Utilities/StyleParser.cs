using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Parsing.Interfaces;
using System.Xml.Linq;

namespace NetHtml2Pdf.Parsing.Utilities
{
    /// <summary>
    /// Parses CSS styles from HTML elements and applies them to document nodes
    /// </summary>
    public class StyleParser : IStyleParser
    {
        public void ApplyInlineStyles(IElement element, DocumentNode node)
        {
            var style = element.GetAttribute("style");
            if (string.IsNullOrEmpty(style))
                return;

            // Parse basic inline styles
            var styles = ParseInlineStyles(style);
            
            // Apply text alignment
            if (styles.TryGetValue("text-align", out var textAlign))
            {
                node.Alignment = textAlign.ToLowerInvariant() switch
                {
                    "center" => TextAlignment.Center,
                    "right" => TextAlignment.Right,
                    "justify" => TextAlignment.Justify,
                    _ => TextAlignment.Left
                };
            }
            
            // Apply margins and padding (simplified parsing)
            if (styles.TryGetValue("margin", out var margin))
            {
                var marginValue = ParseSize(margin);
                if (marginValue.HasValue)
                {
                    node.Margins = marginValue.Value;
                }
            }
            
            // Handle padding (all sides) - set all individual padding values
            if (styles.TryGetValue("padding", out var padding))
            {
                var paddingValue = ParseSize(padding);
                if (paddingValue.HasValue)
                {
                    node.PaddingLeft = paddingValue.Value;
                    node.PaddingRight = paddingValue.Value;
                    node.PaddingTop = paddingValue.Value;
                    node.PaddingBottom = paddingValue.Value;
                }
            }

            // Handle individual padding properties
            if (styles.TryGetValue("padding-left", out var paddingLeft))
            {
                var paddingLeftValue = ParseSize(paddingLeft);
                if (paddingLeftValue.HasValue)
                {
                    node.PaddingLeft = paddingLeftValue.Value;
                }
            }

            if (styles.TryGetValue("padding-right", out var paddingRight))
            {
                var paddingRightValue = ParseSize(paddingRight);
                if (paddingRightValue.HasValue)
                {
                    node.PaddingRight = paddingRightValue.Value;
                }
            }

            if (styles.TryGetValue("padding-top", out var paddingTop))
            {
                var paddingTopValue = ParseSize(paddingTop);
                if (paddingTopValue.HasValue)
                {
                    node.PaddingTop = paddingTopValue.Value;
                }
            }

            if (styles.TryGetValue("padding-bottom", out var paddingBottom))
            {
                var paddingBottomValue = ParseSize(paddingBottom);
                if (paddingBottomValue.HasValue)
                {
                    node.PaddingBottom = paddingBottomValue.Value;
                }
            }

            // Apply text-level styles to paragraph text runs
            if (node is ParagraphNode paragraphNode)
            {
                ApplyTextStylesToParagraph(styles, paragraphNode);
            }
        }

        public float? ParseSize(string size)
        {
            if (string.IsNullOrEmpty(size))
                return null;
                
            var cleanSize = size.Replace("px", "").Replace("pt", "").Trim();
            if (float.TryParse(cleanSize, out var result))
            {
                return result;
            }
            
            return null;
        }

        private void ApplyTextStylesToParagraph(Dictionary<string, string> styles, ParagraphNode paragraphNode)
        {
            // Apply color to all text runs
            if (styles.TryGetValue("color", out var color))
            {
                var hexColor = ConvertColorToHex(color);
                foreach (var textRun in paragraphNode.TextRuns)
                {
                    textRun.Color = hexColor;
                }
            }

            // Apply font-size to all text runs
            if (styles.TryGetValue("font-size", out var fontSize))
            {
                var fontSizeValue = ParseSize(fontSize);
                if (fontSizeValue.HasValue)
                {
                    foreach (var textRun in paragraphNode.TextRuns)
                    {
                        textRun.FontSize = fontSizeValue.Value;
                    }
                }
            }

            // Apply font-weight to all text runs
            if (styles.TryGetValue("font-weight", out var fontWeight))
            {
                var isBold = fontWeight.ToLowerInvariant() switch
                {
                    "bold" or "bolder" or "700" or "800" or "900" => true,
                    _ => false
                };

                foreach (var textRun in paragraphNode.TextRuns)
                {
                    textRun.IsBold = isBold;
                }
            }

            // Apply font-style to all text runs
            if (styles.TryGetValue("font-style", out var fontStyle))
            {
                var isItalic = fontStyle.ToLowerInvariant() switch
                {
                    "italic" or "oblique" => true,
                    _ => false
                };

                foreach (var textRun in paragraphNode.TextRuns)
                {
                    textRun.IsItalic = isItalic;
                }
            }
            
        }

        private string ConvertColorToHex(string color)
        {
            if (string.IsNullOrEmpty(color))
                return color;

            // If it's already a hex color, return as-is
            if (color.StartsWith("#") || IsHexColor(color))
                return color;

            // Convert CSS color names to hex values
            var colorName = color.ToLowerInvariant().Trim();
            return colorName switch
            {
                "red" => "#FF0000",
                "green" => "#008000",
                "blue" => "#0000FF",
                "yellow" => "#FFFF00",
                "orange" => "#FFA500",
                "purple" => "#800080",
                "pink" => "#FFC0CB",
                "brown" => "#A52A2A",
                "black" => "#000000",
                "white" => "#FFFFFF",
                "gray" or "grey" => "#808080",
                "lightgray" or "lightgrey" => "#D3D3D3",
                "darkgray" or "darkgrey" => "#A9A9A9",
                "cyan" => "#00FFFF",
                "magenta" => "#FF00FF",
                "lime" => "#00FF00",
                "navy" => "#000080",
                "olive" => "#808000",
                "teal" => "#008080",
                "silver" => "#C0C0C0",
                "maroon" => "#800000",
                _ => color // Return original if not recognized
            };
        }

        private bool IsHexColor(string color)
        {
            // Check if it's a valid hex color format (3, 4, 6, or 8 characters)
            return color.Length is 3 or 4 or 6 or 8 && 
                   color.All(c => char.IsDigit(c) || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'));
        }

        private Dictionary<string, string> ParseInlineStyles(string style)
        {
            var styles = new Dictionary<string, string>();
            var declarations = style.Split(';', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var declaration in declarations)
            {
                var parts = declaration.Split(':', 2);
                if (parts.Length == 2)
                {
                    var property = parts[0].Trim().ToLowerInvariant();
                    var value = parts[1].Trim();
                    styles[property] = value;
                }
            }
            
            return styles;
        }
    }
}
