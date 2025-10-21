namespace NetHtml2Pdf.Layout.Pagination;

internal sealed class PaginationWarning
{
    public PaginationWarning(PaginationWarningCode code, string message, string? nodePath = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Warning message cannot be null or whitespace.", nameof(message));

        Code = code;
        Message = message;
        NodePath = nodePath;
    }

    public PaginationWarningCode Code { get; }

    public string Message { get; }

    public string? NodePath { get; }
}