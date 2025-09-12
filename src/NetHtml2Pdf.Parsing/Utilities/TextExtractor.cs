using AngleSharp.Dom;
using NetHtml2Pdf.Core.Models;

namespace NetHtml2Pdf.Parsing.Utilities
{
    /// <summary>
    /// Utility class for extracting text runs from HTML elements
    /// </summary>
    public static class TextExtractor
    {
        /// <summary>
        /// Extracts text runs from an HTML element, handling both text nodes and inline formatting elements
        /// </summary>
        /// <param name="element">The HTML element to extract text from</param>
        /// <returns>A list of extracted text runs</returns>
        public static List<TextRunNode> ExtractTextRuns(IElement element)
        {
            var textRuns = new List<TextRunNode>();
            
            foreach (var child in element.ChildNodes)
            {
                if (child.NodeType == NodeType.Text)
                {
                    var text = child.TextContent.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        textRuns.Add(new TextRunNode { Text = text });
                    }
                }
                else if (child is IElement childElement)
                {
                    var tagName = childElement.TagName.ToLowerInvariant();
                    var textRun = new TextRunNode();
                    
                    // Extract text content from child element
                    var childText = childElement.TextContent.Trim();
                    if (!string.IsNullOrEmpty(childText))
                    {
                        textRun.Text = childText;
                        
                        // Apply formatting based on tag
                        switch (tagName)
                        {
                            case "strong" or "b":
                                textRun.IsBold = true;
                                break;
                            case "em" or "i":
                                textRun.IsItalic = true;
                                break;
                        }
                        
                        textRuns.Add(textRun);
                    }
                }
            }
            
            return textRuns;
        }

        /// <summary>
        /// Extracts text runs from an HTML element and adds them to an existing list
        /// </summary>
        /// <param name="element">The HTML element to extract text from</param>
        /// <param name="textRuns">The list to add extracted text runs to</param>
        [Obsolete("Use the return-based overload instead: textRuns.AddRange(TextExtractor.ExtractTextRuns(element))")]
        public static void ExtractTextRuns(IElement element, List<TextRunNode> textRuns)
        {
            var extractedRuns = ExtractTextRuns(element);
            textRuns.AddRange(extractedRuns);
        }
    }
}
