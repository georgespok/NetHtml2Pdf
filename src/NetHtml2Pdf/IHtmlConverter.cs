using NetHtml2Pdf.Core;

namespace NetHtml2Pdf;

[Obsolete("IHtmlConverter is deprecated. Use IPdfBuilder instead. This interface will be removed in a future version.")]
public interface IHtmlConverter
{
    byte[] ConvertToPdf(string html, ConverterOptions? options = null);
}
