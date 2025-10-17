namespace NetHtml2Pdf.Layout.Model;

/// <summary>
/// Carries optional diagnostic details for observability.
/// </summary>
internal readonly struct LayoutDiagnostics
{
    public LayoutDiagnostics(
        string contextName,
        LayoutConstraints appliedConstraints,
        float width,
        float height,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ContextName = contextName;
        AppliedConstraints = appliedConstraints;
        Width = width;
        Height = height;
        Metadata = metadata ?? EmptyMetadata;
    }

    private static IReadOnlyDictionary<string, string> EmptyMetadata { get; } = new Dictionary<string, string>();

    public string ContextName { get; }

    public LayoutConstraints AppliedConstraints { get; }

    public float Width { get; }

    public float Height { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }

    public static LayoutDiagnostics Empty(LayoutConstraints constraints)
    {
        return new LayoutDiagnostics("None", constraints, 0, 0);
    }
}
