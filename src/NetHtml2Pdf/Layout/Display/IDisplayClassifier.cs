using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;

namespace NetHtml2Pdf.Layout.Display;

/// <summary>
///     Internal interface for classifying document nodes into display contexts.
///     Centralizes display classification logic for consistent behavior across composers.
/// </summary>
internal interface IDisplayClassifier
{
    /// <summary>
    ///     Classifies a document node into a display class based on CSS display property and semantic defaults.
    /// </summary>
    /// <param name="node">The document node to classify</param>
    /// <param name="style">The CSS style map for the node</param>
    /// <returns>The display class for the node</returns>
    DisplayClass Classify(DocumentNode node, CssStyleMap style);
}