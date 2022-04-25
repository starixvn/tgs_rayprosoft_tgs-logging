using Microsoft.Extensions.Logging;

namespace Tgs.Logging;

internal class TgsLog : ITgsLog
{
    private readonly AsyncLocal<Guid> _asyncLocalTrackingId = new();
    private readonly TgsLogBackgroundProcessor<LogRecordEvent> _logRecordEventProcessor;
    private readonly TgsLogBackgroundProcessor<AdminLogEvent> _adminEventProcessor;
    private readonly TgsLogBackgroundProcessor<ApiLogEvent> _apiLogEventProcessor;
    private readonly ILogger<TgsLog> _logger;
    private string _loggerName = string.Empty;

    public Guid TrackingId
    {
        get => _asyncLocalTrackingId.Value;

        set
        {
            var slot = _asyncLocalTrackingId.Value;
            if (slot == Guid.Empty)
                _asyncLocalTrackingId.Value = value;
        }
    }

    public TgsLog(
        TgsLogBackgroundProcessor<LogRecordEvent> logRecordEventProcessor,
        TgsLogBackgroundProcessor<AdminLogEvent> adminEventProcessor,
        TgsLogBackgroundProcessor<ApiLogEvent> apiLogEventProcessor,
        ILogger<TgsLog> logger)
    {
        _logRecordEventProcessor = logRecordEventProcessor ?? throw new ArgumentNullException(nameof(logRecordEventProcessor));
        _adminEventProcessor = adminEventProcessor ?? throw new ArgumentNullException(nameof(adminEventProcessor));
        _apiLogEventProcessor = apiLogEventProcessor ?? throw new ArgumentNullException(nameof(apiLogEventProcessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void LogAdmin(AdminLogEvent adminEvent)
    {
        adminEvent.TrackingId = TrackingId;
        _logger.Log(
            LogLevel.Information,
            new EventId(100, "AdminLog"),
            adminEvent,
            null,
            (state, ex) => $"Admin Log Record.");
        _adminEventProcessor.AddRecord(adminEvent);
    }

    public void LogApi(ApiLogEvent apiLogEvent)
    {
        apiLogEvent.TrackingId = TrackingId;
        _logger.Log(
            LogLevel.Information,
            new EventId(200, "ApiLog"),
            apiLogEvent,
            null,
            (state, ex) => $"Api Log Record.");
        _apiLogEventProcessor.AddRecord(apiLogEvent);
    }

    public void LogDebug(
        string message,
        Exception? exception,
        string file,
        string member,
        int line)
        => SendLogRecord(
            TgsLogLevel.Debug,
            message,
            exception,
            file,
            member,
            line);

    public void LogError(
        string message,
        Exception? exception,
        string file,
        string member,
        int line)
        => SendLogRecord(
            TgsLogLevel.Error,
            message,
            exception,
            file,
            member,
            line);

    public void LogFatal(
        string message,
        Exception? exception,
        string file,
        string member,
        int line)
        => SendLogRecord(
            TgsLogLevel.Fatal,
            message,
            exception,
            file,
            member,
            line);

    public void LogInfo(
        string message,
        Exception? exception,
        string file,
        string member,
        int line)
        => SendLogRecord(
            TgsLogLevel.Info,
            message,
            exception,
            file,
            member,
            line);

    public void LogTrackingData(string trackingData)
        => _logRecordEventProcessor.AddRecord(new LogRecordEvent
        {
            TrackingId = TrackingId,
            RecordedOn = DateTimeOffset.UtcNow,
            ServerName = Environment.MachineName,
            Level = TgsLogLevel.Info,
            Logger = trackingData
        });

    public void LogWarning(
        string message,
        Exception? exception,
        string file,
        string member,
        int line)
        => SendLogRecord(
            TgsLogLevel.Warning,
            message,
            exception,
            file,
            member,
            line);

    public ITgsLog SetLogger(string name)
    {
        _loggerName = name;
        return this;
    }

    private void SendLogRecord(
        TgsLogLevel level,
        string message,
        Exception? exception,
        string callerFile,
        string callerMember,
        int lineNumber)
    {
        var logEvent = new LogRecordEvent
        {
            TrackingId = TrackingId,
            RecordedOn = DateTimeOffset.UtcNow,
            ServerName = Environment.MachineName,
            Level = level,
            Logger = _loggerName,
            Message = message,
            Exception = exception?.ToString() ?? string.Empty,
            File = callerFile,
            Member = callerMember,
            Line = lineNumber
        };

        _logRecordEventProcessor.AddRecord(logEvent);

        var logLevel = level switch
        {
            TgsLogLevel.Debug => LogLevel.Debug,
            TgsLogLevel.Verbose => LogLevel.Trace,
            TgsLogLevel.Info => LogLevel.Information,
            TgsLogLevel.Warning => LogLevel.Warning,
            TgsLogLevel.Error => LogLevel.Error,
            TgsLogLevel.Fatal => LogLevel.Critical,
            _ => LogLevel.None
        };

        if (_logger.IsEnabled(logLevel))
            _logger.Log(
                logLevel,
                new EventId(300, "LogRecord"),
                logEvent,
                exception,
                (state, ex) => $"LogEvent Record.");
    }
}
