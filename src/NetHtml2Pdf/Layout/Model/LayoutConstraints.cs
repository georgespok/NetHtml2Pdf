namespace NetHtml2Pdf.Layout.Model;

/// <summary>
///     Describes the available geometry for a node during layout.
/// </summary>
internal readonly struct LayoutConstraints
{
    public LayoutConstraints(
        float inlineMin,
        float inlineMax,
        float blockMin,
        float blockMax,
        float pageRemainingBlockSize,
        bool allowBreaks)
    {
        if (inlineMin < 0 || inlineMax < 0 || blockMin < 0 || blockMax < 0 || pageRemainingBlockSize < 0)
            throw new ArgumentOutOfRangeException(nameof(inlineMin), "Constraint values cannot be negative.");

        if (inlineMin > inlineMax) throw new ArgumentException("inlineMin cannot be greater than inlineMax.");

        if (blockMin > blockMax) throw new ArgumentException("blockMin cannot be greater than blockMax.");

        InlineMin = inlineMin;
        InlineMax = inlineMax;
        BlockMin = blockMin;
        BlockMax = blockMax;
        PageRemainingBlockSize = pageRemainingBlockSize;
        AllowBreaks = allowBreaks;
    }

    public float InlineMin { get; }

    public float InlineMax { get; }

    public float BlockMin { get; }

    public float BlockMax { get; }

    public float PageRemainingBlockSize { get; }

    public bool AllowBreaks { get; }

    public LayoutConstraints ForInlineChild()
    {
        return new LayoutConstraints(InlineMin, InlineMax, 0, BlockMax, PageRemainingBlockSize, AllowBreaks);
    }

    public LayoutConstraints ForBlockChild()
    {
        return new LayoutConstraints(InlineMin, InlineMax, BlockMin, BlockMax, PageRemainingBlockSize, AllowBreaks);
    }
}