using NetHtml2Pdf.Core;

namespace NetHtml2Pdf;

public interface IPdfBuilder
{
    IPdfBuilder Reset();
    IPdfBuilder SetHeader(string html);
    IPdfBuilder SetFooter(string html);
    IPdfBuilder AddPage(string htmlContent);
    byte[] Build(ConverterOptions? options = null);
}

