using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace NetHtml2Pdf.Test.Support;

/// <summary>
///     A logger implementation that writes to xUnit's ITestOutputHelper for debugging purposes.
/// </summary>
public class TestOutputLogger<T>(ITestOutputHelper output) : ILogger<T>
{
    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return NullDisposable.Instance;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string>? formatter)
    {
        if (formatter == null) return;
        var message = formatter(state, exception);
        output.WriteLine($"[{logLevel}] {message}");
        if (exception != null)
            output.WriteLine(exception.ToString());
    }

    private class NullDisposable : IDisposable
    {
        public static readonly NullDisposable Instance = new();

        public void Dispose()
        {
        }
    }
}