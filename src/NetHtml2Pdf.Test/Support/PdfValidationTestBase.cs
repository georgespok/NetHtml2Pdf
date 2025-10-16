using Xunit;

namespace NetHtml2Pdf.Test.Support;

/// <summary>
/// Abstract base class providing PDF validation functionality for test classes.
/// Contains common PDF validation methods and constants.
/// </summary>
public abstract class PdfValidationTestBase
{
    #region PDF Validation Constants


    private static class PdfHeader
    {
        public const byte Percent = 0x25;  // %
        public const byte P = 0x50;        // P
        public const byte D = 0x44;        // D
        public const byte F = 0x46;        // F
        public const int MinimumLength = 4;
    }

    /// <summary>
    /// Standard PDF header bytes for mock PDF data.
    /// </summary>
    protected static readonly byte[] StandardPdfBytes = [PdfHeader.Percent, PdfHeader.P, PdfHeader.D, PdfHeader.F]; // %PDF


    #endregion

    #region PDF Validation Methods

    /// <summary>
    /// Asserts that the byte array is a valid PDF file by checking header signature.
    /// </summary>
    protected static void AssertValidPdf(byte[] pdfBytes)
    {
        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length >= PdfHeader.MinimumLength,
            $"PDF must be at least {PdfHeader.MinimumLength} bytes");

        Assert.Equal(PdfHeader.Percent, pdfBytes[0]);
        Assert.Equal(PdfHeader.P, pdfBytes[1]);
        Assert.Equal(PdfHeader.D, pdfBytes[2]);
        Assert.Equal(PdfHeader.F, pdfBytes[3]);
    }

    /// <summary>
    /// Validates that PDF bytes are not null, not empty, and have valid length.
    /// </summary>
    protected static void ValidatePdfBytes(byte[] pdfBytes)
    {
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
        Assert.True(pdfBytes.Length > 0);
    }

    #endregion
}
