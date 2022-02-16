using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tgs.Logging;

[ProviderAlias("TgsLog")]
internal class TgsLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, TgsLogger> _loggers = new();
    private readonly TgsLogBackgroundProcessor<LogRecordEvent> _tgsLogBackgroundProcessor;

    public TgsLoggerProvider(
        TgsLogBackgroundProcessor<LogRecordEvent> tgsLogBackgroundProcessor,
        IOptionsMonitor<TgsLoggerOptions> optionsAccessor)
    {
        _tgsLogBackgroundProcessor = tgsLogBackgroundProcessor ?? throw new System.ArgumentNullException(nameof(tgsLogBackgroundProcessor));
        _ = optionsAccessor.OnChange(ReloadLoggerOptions);
    }

    public ILogger CreateLogger(string categoryName)
        => _loggers.GetOrAdd(
            categoryName,
            name => new TgsLogger(categoryName, _tgsLogBackgroundProcessor));

    public void Dispose() => _loggers.Clear();

    private void ReloadLoggerOptions(TgsLoggerOptions options)
    {
        _loggers.Clear();
    }
}
