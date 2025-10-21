namespace NetHtml2Pdf.Layout.Pagination;

/// <summary>
///     Non-fatal pagination issues that should be surfaced as warnings.
/// </summary>
internal enum PaginationWarningCode
{
    Unknown = 0,
    KeepTogetherOverflow,
    UnsupportedFragment,
    NonRenderableFragment,
    HeaderFooterOverflow,
    DiagnosticsSuppressed
}