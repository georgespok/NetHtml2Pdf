namespace NetHtml2Pdf.Core
{
    public class ConverterOptions
    {
        public string FontPath { get; init; } = string.Empty;

        /// <summary>
        /// Enables the Phase 2 layout pipeline for paragraphs/headings/spans.
        /// </summary>
        public bool EnableNewLayoutForTextBlocks { get; init; }

        /// <summary>
        /// Enables diagnostic output for the layout engine (structured fragment logs).
        /// </summary>
        public bool EnableLayoutDiagnostics { get; init; }
    }
}
