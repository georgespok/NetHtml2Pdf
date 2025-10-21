namespace NetHtml2Pdf.Layout.Model;

/// <summary>
///     Carries optional diagnostic details for observability.
/// </summary>
internal readonly struct LayoutDiagnostics(
    string contextName,
    LayoutConstraints appliedConstraints,
    float width,
    float height,
    IReadOnlyDictionary<string, string>? metadata = null)
{
    private static IReadOnlyDictionary<string, string> EmptyMetadata { get; } = new Dictionary<string, string>();

    public string ContextName { get; } = contextName;

    public LayoutConstraints AppliedConstraints { get; } = appliedConstraints;

    public float Width { get; } = width;

    public float Height { get; } = height;

    public IReadOnlyDictionary<string, string> Metadata { get; } = metadata ?? EmptyMetadata;

    public static LayoutDiagnostics Empty(LayoutConstraints constraints)
    {
        return new LayoutDiagnostics("None", constraints, 0, 0);
    }
}