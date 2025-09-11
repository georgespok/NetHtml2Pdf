using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Web;

namespace NetHtml2Pdf.TestAzureFunction;

public class HtmlConverterFunction(ILogger<HtmlConverterFunction> logger)
{
    [Function("ConvertToPdf")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        try
        {
            string? html = null;

            var query = HttpUtility.ParseQueryString(req.Url.Query);
            bool inline = bool.TryParse(query["inline"], out var inl) && inl;

            // Optional inputs
            string title = query["title"] ?? "Azure Functions + QuestPDF";
            string? bodyText = null;
            

            // If POST, read JSON body like { "text":"...", "html":"..." }
            if (req.Method.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                using var reader = new StreamReader(req.Body);
                var raw = await reader.ReadToEndAsync();
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(raw);
                        var root = doc.RootElement;
                        if (root.TryGetProperty("text", out var t) && t.ValueKind == JsonValueKind.String)
                        {
                            bodyText = t.GetString();
                            html = $"<p>{bodyText}</p>";
                        }

                        if (root.TryGetProperty("html", out var h) && h.ValueKind == JsonValueKind.String)
                            html = h.GetString();
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Ignoring invalid JSON payload.");
                    }
                }
            }

            if (html == null)
            {
                throw new ApplicationException("Content is empty. Nothing to convert");
            }

            var converter = new HtmlConverter();
            var bytes = await converter.Convert(html);

            var fileName = $"{SanitizeFileName(title)}-{DateTime.UtcNow:yyyyMMdd-HHmmss}.pdf";
            var res = req.CreateResponse(HttpStatusCode.OK);

            // Headers for inline view or forced download
            res.Headers.Add("Content-Type", "application/pdf");
            res.Headers.Add(
                "Content-Disposition",
                inline ? $"inline; filename=\"{fileName}\"" : $"attachment; filename=\"{fileName}\""
            );

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