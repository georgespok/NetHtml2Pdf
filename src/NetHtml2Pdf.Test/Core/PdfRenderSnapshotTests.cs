using Shouldly;
using NetHtml2Pdf.Core;

namespace NetHtml2Pdf.Test.Core
{
    public class PdfRenderSnapshotTests
    {
        private static readonly DateTime DefaultTimestamp = new DateTime(2025, 1, 27, 10, 0, 0, DateTimeKind.Utc);
        private static readonly TimeSpan DefaultRenderDuration = TimeSpan.FromMilliseconds(1500);
        private static readonly string DefaultPlatform = "Windows";
        private static readonly long DefaultOutputSize = 1024L;
        private static readonly int DefaultInputHtmlSize = 2048;
        private static readonly int DefaultElementCount = 25;
        private static readonly int DefaultSupportedElementCount = 25;
        private static readonly int DefaultFallbackElementCount = 0;
        private static readonly int DefaultCssPropertyCount = 15;
        private static readonly long DefaultMemoryUsage = 1048576L;

        #region Test Data Builders

        private static PdfRenderSnapshot CreateSnapshot(
            TimeSpan? renderDuration = null,
            string? platform = null,
            List<string>? warnings = null,
            long? outputSize = null,
            DateTime? timestamp = null,
            List<string>? fallbackElements = null,
            int? inputHtmlSize = null,
            int? elementCount = null,
            int? supportedElementCount = null,
            int? fallbackElementCount = null,
            int? cssPropertyCount = null,
            long? memoryUsage = null)
        {
            return new PdfRenderSnapshot(
                renderDuration ?? DefaultRenderDuration,
                platform ?? DefaultPlatform,
                warnings ?? [],
                outputSize ?? DefaultOutputSize,
                timestamp ?? DefaultTimestamp,
                fallbackElements ?? [],
                inputHtmlSize ?? DefaultInputHtmlSize,
                elementCount ?? DefaultElementCount,
                supportedElementCount ?? DefaultSupportedElementCount,
                fallbackElementCount ?? DefaultFallbackElementCount,
                cssPropertyCount ?? DefaultCssPropertyCount,
                memoryUsage ?? DefaultMemoryUsage);
        }

        private static PdfRenderSnapshot CreateSnapshotWithNullWarnings()
        {
            return new PdfRenderSnapshot(
                DefaultRenderDuration,
                DefaultPlatform,
                null!,
                DefaultOutputSize,
                DefaultTimestamp,
                [],
                DefaultInputHtmlSize,
                DefaultElementCount,
                DefaultSupportedElementCount,
                DefaultFallbackElementCount,
                DefaultCssPropertyCount,
                DefaultMemoryUsage);
        }

        private static PdfRenderSnapshot CreateSnapshotWithNullFallbackElements()
        {
            return new PdfRenderSnapshot(
                DefaultRenderDuration,
                DefaultPlatform,
                [],
                DefaultOutputSize,
                DefaultTimestamp,
                null!,
                DefaultInputHtmlSize,
                DefaultElementCount,
                DefaultSupportedElementCount,
                DefaultFallbackElementCount,
                DefaultCssPropertyCount,
                DefaultMemoryUsage);
        }

        private static void AssertSnapshotProperties(PdfRenderSnapshot snapshot, 
            TimeSpan? renderDuration = null,
            string? platform = null,
            List<string>? warnings = null,
            long? outputSize = null,
            DateTime? timestamp = null,
            List<string>? fallbackElements = null,
            int? inputHtmlSize = null,
            int? elementCount = null,
            int? supportedElementCount = null,
            int? fallbackElementCount = null,
            int? cssPropertyCount = null,
            long? memoryUsage = null)
        {
            if (renderDuration.HasValue) snapshot.RenderDuration.ShouldBe(renderDuration.Value);
            if (platform != null) snapshot.Platform.ShouldBe(platform);
            if (warnings != null) snapshot.Warnings.ShouldBe(warnings);
            if (outputSize.HasValue) snapshot.OutputSize.ShouldBe(outputSize.Value);
            if (timestamp.HasValue) snapshot.Timestamp.ShouldBe(timestamp.Value);
            if (fallbackElements != null) snapshot.FallbackElements.ShouldBe(fallbackElements);
            if (inputHtmlSize.HasValue) snapshot.InputHtmlSize.ShouldBe(inputHtmlSize.Value);
            if (elementCount.HasValue) snapshot.ElementCount.ShouldBe(elementCount.Value);
            if (supportedElementCount.HasValue) snapshot.SupportedElementCount.ShouldBe(supportedElementCount.Value);
            if (fallbackElementCount.HasValue) snapshot.FallbackElementCount.ShouldBe(fallbackElementCount.Value);
            if (cssPropertyCount.HasValue) snapshot.CssPropertyCount.ShouldBe(cssPropertyCount.Value);
            if (memoryUsage.HasValue) snapshot.MemoryUsage.ShouldBe(memoryUsage.Value);
        }

        private static void AssertValidationNotSet(PdfRenderSnapshot snapshot)
        {
            snapshot.IsCrossPlatformValidated.ShouldBeFalse();
            snapshot.ValidationTimestamp.ShouldBeNull();
            snapshot.ValidationResult.ShouldBeNull();
        }

        #endregion

        #region Constructor Tests - Validation Errors

        [Theory]
        [InlineData(-100, "RenderDuration must be non-negative")]
        [InlineData(-1, "InputHtmlSize must be non-negative")]
        [InlineData(-1L, "MemoryUsage must be non-negative")]
        public void Constructor_WithNegativeValues_ShouldThrowArgumentException(object value, string expectedMessage)
        {
            // Act & Assert
            if (value is int intValue)
            {
                if (expectedMessage.Contains("RenderDuration"))
                {
                    Should.Throw<ArgumentException>(() => 
                        CreateSnapshot(renderDuration: TimeSpan.FromMilliseconds(intValue)))
                        .Message.ShouldContain(expectedMessage);
                }
                else if (expectedMessage.Contains("InputHtmlSize"))
                {
                    Should.Throw<ArgumentException>(() => 
                        CreateSnapshot(inputHtmlSize: intValue))
                        .Message.ShouldContain(expectedMessage);
                }
            }
            else if (value is long longValue && expectedMessage.Contains("MemoryUsage"))
            {
                Should.Throw<ArgumentException>(() => 
                    CreateSnapshot(memoryUsage: longValue))
                    .Message.ShouldContain(expectedMessage);
            }
        }

        [Fact]
        public void Constructor_WithInvalidPlatform_ShouldThrowArgumentException()
        {
            // Act & Assert
            Should.Throw<ArgumentException>(() => 
                CreateSnapshot(platform: "MacOS"))
                .Message.ShouldContain("Platform must be exactly 'Windows' or 'Linux'");
        }

        [Fact]
        public void Constructor_WithNullWarnings_ShouldThrowArgumentException()
        {
            // Act & Assert
            Should.Throw<ArgumentException>(CreateSnapshotWithNullWarnings)
                .Message.ShouldContain("Warnings cannot be null");
        }

        [Fact]
        public void Constructor_WithEmptyWarning_ShouldThrowArgumentException()
        {
            // Act & Assert
            Should.Throw<ArgumentException>(() => 
                CreateSnapshot(warnings: [""]))
                .Message.ShouldContain("Warnings must be non-empty strings");
        }

        [Theory]
        [InlineData(-1L, "OutputSize must be positive")]
        [InlineData(0L, "OutputSize must be positive")]
        [InlineData(101L * 1024 * 1024, "OutputSize must not exceed 100MB")] // 101MB - exceeds 100MB limit
        public void Constructor_WithInvalidOutputSize_ShouldThrowArgumentException(long outputSize, string expectedMessage)
        {
            // Act & Assert
            Should.Throw<ArgumentException>(() => 
                CreateSnapshot(outputSize: outputSize))
                .Message.ShouldContain(expectedMessage);
        }

        [Fact]
        public void Constructor_WithFutureTimestamp_ShouldThrowArgumentException()
        {
            // Act & Assert
            Should.Throw<ArgumentException>(() => 
                CreateSnapshot(timestamp: DateTime.UtcNow.AddDays(1))) // Future timestamp
                .Message.ShouldContain("Timestamp cannot be in the future");
        }

        [Fact]
        public void Constructor_WithTimestampBefore2020_ShouldThrowArgumentException()
        {
            // Act & Assert
            Should.Throw<ArgumentException>(() => 
                CreateSnapshot(timestamp: new DateTime(2019, 12, 31, 23, 59, 59, DateTimeKind.Utc))) // Before 2020
                .Message.ShouldContain("Timestamp cannot be before 2020");
        }

        [Fact]
        public void Constructor_WithNullFallbackElements_ShouldThrowArgumentException()
        {
            // Act & Assert
            Should.Throw<ArgumentException>(CreateSnapshotWithNullFallbackElements)
                .Message.ShouldContain("FallbackElements cannot be null");
        }

        [Fact]
        public void Constructor_WithInvalidFallbackElement_ShouldThrowArgumentException()
        {
            // Act & Assert
            Should.Throw<ArgumentException>(() => 
                CreateSnapshot(fallbackElements: ["video", "123invalid"])) // Invalid HTML tag
                .Message.ShouldContain("FallbackElements must be valid HTML tag names");
        }


        [Fact]
        public void Constructor_WithElementCountMismatch_ShouldThrowArgumentException()
        {
            // Act & Assert
            Should.Throw<ArgumentException>(() => 
                CreateSnapshot(
                    elementCount: 25,
                    supportedElementCount: 20,
                    fallbackElementCount: 10)) // 20 + 10 = 30, not 25
                .Message.ShouldContain("FallbackElementCount must equal FallbackElements.Count");
        }

        [Fact]
        public void Constructor_WithFallbackElementCountMismatch_ShouldThrowArgumentException()
        {
            // Act & Assert
            Should.Throw<ArgumentException>(() => 
                CreateSnapshot(
                    fallbackElements: ["video", "audio"], // 2 elements
                    supportedElementCount: 23,
                    fallbackElementCount: 3)) // Should be 2 to match FallbackElements.Count
                .Message.ShouldContain("FallbackElementCount must equal FallbackElements.Count");
        }


        #endregion

        #region SetValidation Tests

        [Fact]
        public void SetValidation_WithInvalidValidationResult_ShouldThrowArgumentException()
        {
            // Arrange
            var snapshot = CreateSnapshot();
            var validationTimestamp = new DateTime(2025, 1, 27, 10, 5, 0, DateTimeKind.Utc);
            var validationResult = "INVALID"; // Invalid validation result

            // Act & Assert
            Should.Throw<ArgumentException>(() => 
                snapshot.SetValidation(validationTimestamp, validationResult))
                .Message.ShouldContain("ValidationResult must be one of 'IDENTICAL', 'DIFFERENT', 'ERROR', or null");
        }

        [Fact]
        public void SetValidation_WithValidationTimestampBeforeTimestamp_ShouldThrowArgumentException()
        {
            // Arrange
            var snapshot = CreateSnapshot();
            var validationTimestamp = new DateTime(2025, 1, 27, 9, 59, 0, DateTimeKind.Utc); // Before timestamp

            // Act & Assert
            Should.Throw<ArgumentException>(() => 
                snapshot.SetValidation(validationTimestamp, "IDENTICAL"))
                .Message.ShouldContain("ValidationTimestamp must be after Timestamp");
        }

        #endregion
    }
}
