using NetHtml2Pdf.Layout.Contexts;
using NetHtml2Pdf.Layout.Engines;
using NetHtml2Pdf.Renderer.Inline;

namespace NetHtml2Pdf.Layout.FormattingContexts;

internal sealed class FormattingContextFactory
{
    private readonly BlockFormattingContext _blockFormattingContext;
    private readonly FlexFormattingContext? _flexFormattingContext;
    private readonly InlineBlockFormattingContext? _inlineBlockFormattingContext;
    private readonly InlineFormattingContext _inlineFormattingContext;
    private readonly TableFormattingContext? _tableFormattingContext;

    private FormattingContextFactory(
        FormattingContextOptions options,
        InlineFormattingContext inlineFormattingContext,
        BlockFormattingContext blockFormattingContext,
        InlineBlockFormattingContext? inlineBlockFormattingContext,
        TableFormattingContext? tableFormattingContext,
        FlexFormattingContext? flexFormattingContext)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        _inlineFormattingContext =
            inlineFormattingContext ?? throw new ArgumentNullException(nameof(inlineFormattingContext));
        _blockFormattingContext =
            blockFormattingContext ?? throw new ArgumentNullException(nameof(blockFormattingContext));
        _inlineBlockFormattingContext = inlineBlockFormattingContext;
        _blockFormattingContext.SetInlineBlockContext(_inlineBlockFormattingContext);
        _tableFormattingContext = tableFormattingContext;
        _flexFormattingContext = flexFormattingContext;
    }

    public FormattingContextOptions Options { get; }

    public InlineFormattingContext GetInlineFormattingContext()
    {
        return _inlineFormattingContext;
    }

    public BlockFormattingContext GetBlockFormattingContext()
    {
        return _blockFormattingContext;
    }

    public InlineBlockFormattingContext? GetInlineBlockFormattingContext()
    {
        return _inlineBlockFormattingContext;
    }

    public TableFormattingContext? GetTableFormattingContext()
    {
        return _tableFormattingContext;
    }

    public FlexFormattingContext? GetFlexFormattingContext()
    {
        return _flexFormattingContext;
    }

    public static FormattingContextFactory CreateDefault(LayoutEngineOptions layoutOptions,
        InlineFlowLayoutEngine? inlineFlowLayoutEngine = null)
    {
        ArgumentNullException.ThrowIfNull(layoutOptions);

        var options = FormattingContextOptions.FromLayoutOptions(layoutOptions);
        inlineFlowLayoutEngine ??= new InlineFlowLayoutEngine();
        var inlineFormattingContext = new InlineFormattingContext(inlineFlowLayoutEngine);
        var blockFormattingContext = new BlockFormattingContext(inlineFormattingContext);
        var inlineBlockFormattingContext = options.EnableInlineBlockContext
            ? new InlineBlockFormattingContext(blockFormattingContext, inlineFormattingContext)
            : null;

        var tableFormattingContext = options.EnableTableContext
            ? new TableFormattingContext()
            : null;

        var flexFormattingContext = options.EnableFlexContext
            ? new FlexFormattingContext(blockFormattingContext, inlineFormattingContext)
            : null;

        return new FormattingContextFactory(
            options,
            inlineFormattingContext,
            blockFormattingContext,
            inlineBlockFormattingContext,
            tableFormattingContext,
            flexFormattingContext);
    }
}