using System.Text.RegularExpressions;

namespace NetHtml2Pdf.Core
{
    public class PdfRenderSnapshot(
        TimeSpan renderDuration,
        string platform,
        List<string> warnings,
        long outputSize,
        DateTime timestamp,
        List<string> fallbackElements,
        int inputHtmlSize,
        int elementCount,
        int supportedElementCount,
        int fallbackElementCount,
        int cssPropertyCount,
        long memoryUsage)
    {
        private static readonly HashSet<string> ValidValidationResults = ["IDENTICAL", "DIFFERENT", "ERROR"];

        private static readonly HashSet<string> ValidPlatforms = ["Windows", "Linux"];

        private static readonly Regex HtmlTagNameRegex = new Regex(@"^[a-zA-Z][a-zA-Z0-9-]*$", RegexOptions.Compiled);

        public TimeSpan RenderDuration { get; } = ValidateRenderDuration(renderDuration);
        public string Platform { get; } = ValidatePlatform(platform);
        public List<string> Warnings { get; } = ValidateWarnings(warnings);
        public long OutputSize { get; } = ValidateOutputSize(outputSize);
        public DateTime Timestamp { get; } = ValidateTimestamp(timestamp);
        public List<string> FallbackElements { get; } = ValidateFallbackElements(fallbackElements);
        public int InputHtmlSize { get; } = ValidateInputHtmlSize(inputHtmlSize);
        public int ElementCount { get; } = ValidateElementCount(elementCount, supportedElementCount, fallbackElementCount, fallbackElements);
        public int SupportedElementCount { get; } = ValidateSupportedElementCount(supportedElementCount, elementCount);
        public int FallbackElementCount { get; } = ValidateFallbackElementCount(fallbackElementCount, fallbackElements);
        public int CssPropertyCount { get; } = ValidateCssPropertyCount(cssPropertyCount);
        public long MemoryUsage { get; } = ValidateMemoryUsage(memoryUsage);
        public bool IsCrossPlatformValidated { get; private set; } = false;
        public DateTime? ValidationTimestamp { get; private set; } = null;
        public string? ValidationResult { get; private set; } = null;

        public void SetValidation(DateTime validationTimestamp, string validationResult)
        {
            if (validationTimestamp <= Timestamp)
            {
                throw new ArgumentException("ValidationTimestamp must be after Timestamp", nameof(validationTimestamp));
            }

            if (validationResult != null && !ValidValidationResults.Contains(validationResult))
            {
                throw new ArgumentException($"ValidationResult must be one of '{string.Join("', '", ValidValidationResults)}', or null", nameof(validationResult));
            }

            ValidationTimestamp = validationTimestamp;
            ValidationResult = validationResult;
            IsCrossPlatformValidated = ValidationTimestamp != null && ValidationResult != null;
        }

        private static TimeSpan ValidateRenderDuration(TimeSpan renderDuration)
        {
            if (renderDuration < TimeSpan.Zero)
            {
                throw new ArgumentException("RenderDuration must be non-negative", nameof(renderDuration));
            }

            return renderDuration;
        }

        private static string ValidatePlatform(string platform)
        {
            if (string.IsNullOrWhiteSpace(platform))
            {
                throw new ArgumentException("Platform cannot be null or empty", nameof(platform));
            }

            if (!ValidPlatforms.Contains(platform))
            {
                throw new ArgumentException($"Platform must be exactly '{string.Join("' or '", ValidPlatforms)}'", nameof(platform));
            }

            return platform;
        }

        private static List<string> ValidateWarnings(List<string> warnings)
        {
            if (warnings == null)
            {
                throw new ArgumentException("Warnings cannot be null", nameof(warnings));
            }

            foreach (var warning in warnings)
            {
                if (string.IsNullOrEmpty(warning))
                {
                    throw new ArgumentException("Warnings must be non-empty strings", nameof(warnings));
                }
            }

            return warnings;
        }

        private static long ValidateOutputSize(long outputSize)
        {
            if (outputSize <= 0)
            {
                throw new ArgumentException("OutputSize must be positive", nameof(outputSize));
            }

            const long minSize = 1024; // 1KB
            const long maxSize = 100L * 1024 * 1024; // 100MB

            if (outputSize < minSize)
            {
                throw new ArgumentException("OutputSize must be at least 1KB", nameof(outputSize));
            }

            if (outputSize > maxSize)
            {
                throw new ArgumentException("OutputSize must not exceed 100MB", nameof(outputSize));
            }

            return outputSize;
        }

        private static DateTime ValidateTimestamp(DateTime timestamp)
        {
            var now = DateTime.UtcNow;
            var minDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            if (timestamp > now)
            {
                throw new ArgumentException("Timestamp cannot be in the future", nameof(timestamp));
            }

            if (timestamp < minDate)
            {
                throw new ArgumentException("Timestamp cannot be before 2020", nameof(timestamp));
            }

            return timestamp;
        }

        private static List<string> ValidateFallbackElements(List<string> fallbackElements)
        {
            if (fallbackElements == null)
            {
                throw new ArgumentException("FallbackElements cannot be null", nameof(fallbackElements));
            }

            foreach (var element in fallbackElements)
            {
                if (string.IsNullOrWhiteSpace(element))
                {
                    throw new ArgumentException("FallbackElements must be valid HTML tag names", nameof(fallbackElements));
                }

                if (!HtmlTagNameRegex.IsMatch(element))
                {
                    throw new ArgumentException("FallbackElements must be valid HTML tag names (letters, numbers, hyphens)", nameof(fallbackElements));
                }
            }

            return fallbackElements;
        }

        private static int ValidateInputHtmlSize(int inputHtmlSize)
        {
            if (inputHtmlSize < 0)
            {
                throw new ArgumentException("InputHtmlSize must be non-negative", nameof(inputHtmlSize));
            }

            return inputHtmlSize;
        }

        private static int ValidateElementCount(int elementCount, int supportedElementCount, int fallbackElementCount, List<string> fallbackElements)
        {
            if (elementCount < 0)
            {
                throw new ArgumentException("ElementCount must be non-negative", nameof(elementCount));
            }

            if (fallbackElementCount != fallbackElements.Count)
            {
                throw new ArgumentException("FallbackElementCount must equal FallbackElements.Count", nameof(fallbackElementCount));
            }

            if (elementCount != supportedElementCount + fallbackElementCount)
            {
                throw new ArgumentException("ElementCount must equal SupportedElementCount + FallbackElementCount", nameof(elementCount));
            }

            return elementCount;
        }

        private static int ValidateSupportedElementCount(int supportedElementCount, int elementCount)
        {
            if (supportedElementCount < 0)
            {
                throw new ArgumentException("SupportedElementCount must be non-negative", nameof(supportedElementCount));
            }

            if (supportedElementCount > elementCount)
            {
                throw new ArgumentException("SupportedElementCount cannot exceed ElementCount", nameof(supportedElementCount));
            }

            return supportedElementCount;
        }

        private static int ValidateFallbackElementCount(int fallbackElementCount, List<string> fallbackElements)
        {
            if (fallbackElementCount < 0)
            {
                throw new ArgumentException("FallbackElementCount must be non-negative", nameof(fallbackElementCount));
            }

            return fallbackElementCount;
        }

        private static int ValidateCssPropertyCount(int cssPropertyCount)
        {
            if (cssPropertyCount < 0)
            {
                throw new ArgumentException("CssPropertyCount must be non-negative", nameof(cssPropertyCount));
            }

            return cssPropertyCount;
        }

        private static long ValidateMemoryUsage(long memoryUsage)
        {
            if (memoryUsage < 0)
            {
                throw new ArgumentException("MemoryUsage must be non-negative", nameof(memoryUsage));
            }

            return memoryUsage;
        }
    }
}
