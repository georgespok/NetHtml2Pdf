using System.Collections.Generic;
using System.IO;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace NetHtml2Pdf.Test
{
    public static class PdfTextExtractorHelper
    {
        public static IList<string> ExtractWords(byte[] pdfBytes)
        {
            using var ms = new MemoryStream(pdfBytes);
            using var doc = PdfDocument.Open(ms);
            var extractor = NearestNeighbourWordExtractor.Instance;

            var words = new List<string>();
            foreach (var page in doc.GetPages())
            {
                var pageWords = page.GetWords(extractor).Select(w => w.Text);
                words.AddRange(pageWords);
            }
            return words;
        }
    }
}
