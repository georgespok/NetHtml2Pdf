using System;
using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core;

namespace NetHtml2Pdf.Renderer.Adapters;

internal sealed class RendererAdapterFactory : IRendererAdapterFactory
{
    public IRendererAdapter Create(RendererOptions options, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.EnableQuestPdfAdapter)
        {
            if (!options.EnablePagination)
            {
                throw new InvalidOperationException("QuestPdfAdapter requires pagination.");
            }

            return new QuestPdfAdapter();
        }

        return new NullRendererAdapter();
    }
}
