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

        /// <summary>
        /// Enables the pagination pass (Phase 3), slicing layout fragments into page-scoped trees.
        /// </summary>
        public bool EnablePagination { get; init; }

        /// <summary>
        /// Routes rendering through the QuestPDF adapter instead of legacy composers. Requires pagination.
        /// </summary>
        public bool EnableQuestPdfAdapter { get; init; }

        /// <summary>
        /// Enables detailed pagination diagnostics (per-page and fragment logging).
        /// </summary>
        public bool EnablePaginationDiagnostics { get; init; }
    }
}
