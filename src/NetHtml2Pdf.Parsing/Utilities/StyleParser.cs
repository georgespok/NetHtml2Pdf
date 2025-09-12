using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;
using NetHtml2Pdf.Parsing.Interfaces;

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
            
            if (styles.TryGetValue("padding", out var padding))
            {
                var paddingValue = ParseSize(padding);
                if (paddingValue.HasValue)
                {
                    node.Padding = paddingValue.Value;
                }
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
