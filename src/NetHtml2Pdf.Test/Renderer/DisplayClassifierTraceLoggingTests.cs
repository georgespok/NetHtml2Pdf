using Microsoft.Extensions.Logging;
using NetHtml2Pdf.Core;
using NetHtml2Pdf.Core.Enums;
using NetHtml2Pdf.Layout.Display;
using NetHtml2Pdf.Renderer;
using Shouldly;
using Xunit;

namespace NetHtml2Pdf.Test.Renderer;

public class DisplayClassifierTraceLoggingTests
{
    [Fact]
    public void DisplayClassifier_WithTraceLoggingEnabled_LogsClassificationDecisions()
    {
        // Arrange
        var logger = new TestLogger<DisplayClassifier>();
        var options = new RendererOptions { EnableClassifierTraceLogging = true };
        var classifier = new DisplayClassifier(logger, options);
        
        var node = new DocumentNode(DocumentNodeType.Div);
        var style = CssStyleMap.Empty.WithDisplay(CssDisplay.Block);

        // Act
        var result = classifier.Classify(node, style);

        // Assert
        result.ShouldBe(DisplayClass.Block);
        logger.LogEntries.ShouldContain(entry => 
            entry.Level == LogLevel.Debug && 
            entry.Message.Contains("DisplayClassifier: Node Div classified as Block via CSS display Block"));
    }

    [Fact]
    public void DisplayClassifier_WithTraceLoggingDisabled_DoesNotLogClassificationDecisions()
    {
        // Arrange
        var logger = new TestLogger<DisplayClassifier>();
        var options = new RendererOptions { EnableClassifierTraceLogging = false };
        var classifier = new DisplayClassifier(logger, options);
        
        var node = new DocumentNode(DocumentNodeType.Div);
        var style = CssStyleMap.Empty.WithDisplay(CssDisplay.Block);

        // Act
        var result = classifier.Classify(node, style);

        // Assert
        result.ShouldBe(DisplayClass.Block);
        logger.LogEntries.ShouldNotContain(entry => 
            entry.Level == LogLevel.Debug && 
            entry.Message.Contains("DisplayClassifier: Node Div classified as Block via CSS display Block"));
    }

    [Fact]
    public void DisplayClassifier_WithSemanticDefault_LogsSemanticClassification()
    {
        // Arrange
        var logger = new TestLogger<DisplayClassifier>();
        var options = new RendererOptions { EnableClassifierTraceLogging = true };
        var classifier = new DisplayClassifier(logger, options);
        
        var node = new DocumentNode(DocumentNodeType.Paragraph);
        var style = CssStyleMap.Empty; // No explicit display

        // Act
        var result = classifier.Classify(node, style);

        // Assert
        result.ShouldBe(DisplayClass.Block);
        logger.LogEntries.ShouldContain(entry => 
            entry.Level == LogLevel.Debug && 
            entry.Message.Contains("DisplayClassifier: Node Paragraph classified as Block via semantic default"));
    }

    [Fact]
    public void DisplayClassifier_DefaultOptions_DoesNotEnableTraceLogging()
    {
        // Arrange
        var logger = new TestLogger<DisplayClassifier>();
        var classifier = new DisplayClassifier(logger); // No options provided
        
        var node = new DocumentNode(DocumentNodeType.Div);
        var style = CssStyleMap.Empty.WithDisplay(CssDisplay.Block);

        // Act
        var result = classifier.Classify(node, style);

        // Assert
        result.ShouldBe(DisplayClass.Block);
        logger.LogEntries.ShouldNotContain(entry => 
            entry.Level == LogLevel.Debug && 
            entry.Message.Contains("DisplayClassifier:"));
    }
}

// Test logger implementation
public class TestLogger<T> : ILogger<T>
{
    public List<LogEntry> LogEntries { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        LogEntries.Add(new LogEntry
        {
            Level = logLevel,
            Message = formatter(state, exception),
            Exception = exception
        });
    }
}

public class LogEntry
{
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
}
