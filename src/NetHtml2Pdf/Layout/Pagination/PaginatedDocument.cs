using System;
using System.Collections.Generic;

namespace NetHtml2Pdf.Layout.Pagination;

internal sealed class PaginatedDocument
{
    public PaginatedDocument(
        PageConstraints constraints,
        IReadOnlyList<PageFragmentTree> pages,
        IReadOnlyList<PaginationWarning>? warnings = null)
    {
        PageConstraints = constraints ?? throw new ArgumentNullException(nameof(constraints));
        Pages = pages ?? throw new ArgumentNullException(nameof(pages));
        if (pages.Count == 0)
        {
            throw new ArgumentException("Paginated document must contain at least one page.", nameof(pages));
        }

        Warnings = warnings ?? [];
    }

    public PageConstraints PageConstraints { get; }

    public IReadOnlyList<PageFragmentTree> Pages { get; }

    public IReadOnlyList<PaginationWarning> Warnings { get; }
}
