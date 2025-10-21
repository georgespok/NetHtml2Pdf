using System.Net;
using System.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace NetHtml2Pdf.TestAzureFunction;

public class PdfBuilderFunction(ILogger<PdfBuilderFunction> logger)
{
    [Function("ConvertToPdf")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        try
        {
            // Read raw HTML from request body
            using var reader = new StreamReader(req.Body);
            var html = await reader.ReadToEndAsync();

            // Validate that HTML content is provided
            if (string.IsNullOrWhiteSpace(html))
                return await BadRequest(req, "HTML content is required in the request body.");

            // Get optional title from query parameter
            var query = HttpUtility.ParseQueryString(req.Url.Query);
            var title = query["title"] ?? "HTML to PDF";

            var builder = new PdfBuilder(logger);
            var bytes = builder.AddPage(html).Build();

            var fileName = $"{SanitizeFileName(title)}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf";
            var res = req.CreateResponse(HttpStatusCode.OK);

            // Headers for PDF download
            res.Headers.Add("Content-Type", "application/pdf");
            res.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");

            // Write bytes
            await res.WriteBytesAsync(bytes);
            return res;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "PDF generation failed.");
            var res = req.CreateResponse(HttpStatusCode.InternalServerError);
            await res.WriteStringAsync($"Error: {ex.Message}");
            return res;
        }
    }

    private static async Task<HttpResponseData> BadRequest(HttpRequestData req, string message)
    {
        var res = req.CreateResponse(HttpStatusCode.BadRequest);
        await res.WriteStringAsync(message);
        return res;
    }

    private static string SanitizeFileName(string input)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", input.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).Trim();
    }
}