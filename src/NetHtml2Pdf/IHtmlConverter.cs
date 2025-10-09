using NetHtml2Pdf.Core;

namespace NetHtml2Pdf;

public interface IHtmlConverter
{
    byte[] ConvertToPdf(string html, ConverterOptions? options = null);
}
