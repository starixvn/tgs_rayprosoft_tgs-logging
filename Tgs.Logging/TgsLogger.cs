using Microsoft.Extensions.Logging;

namespace Tgs.Logging;

internal class TgsLogger : ILogger
{
    private readonly string _categoryName;
    private readonly TgsLogBackgroundProcessor<LogRecordEvent> _tgsLogBackgroundProcessor;

    public TgsLogger(string categoryName, TgsLogBackgroundProcessor<LogRecordEvent> tgsLogBackgroundProcessor)
    {
        _categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
        _tgsLogBackgroundProcessor = tgsLogBackgroundProcessor ?? throw new ArgumentNullException(nameof(tgsLogBackgroundProcessor));
    }

    public IDisposable BeginScope<TState>(TState state) => default!;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var record = new LogRecordEvent
        {
            RecordedOn = DateTimeOffset.UtcNow,
            ServerName = Environment.MachineName,
            Level = logLevel switch
            {
                LogLevel.Debug => TgsLogLevel.Debug,
                LogLevel.Information => TgsLogLevel.Info,
                LogLevel.Warning => TgsLogLevel.Warning,
                LogLevel.Error => TgsLogLevel.Error,
                LogLevel.Trace => TgsLogLevel.Verbose,
                LogLevel.Critical => TgsLogLevel.Fatal,
                _ => TgsLogLevel.All
            },
            Logger = _categoryName,
            Message = formatter?.Invoke(state, exception)!,
            Exception = exception?.ToString() ?? string.Empty
        };

        _tgsLogBackgroundProcessor.AddRecord(record);
    }
}
