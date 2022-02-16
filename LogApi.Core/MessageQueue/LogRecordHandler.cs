using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.Logging;
using Tgs.MessageQueue;

namespace Tgs.LogApi.MessageQueue;

internal class LogRecordHandler : IMessageHandler
{
    private readonly ILogger<LogRecordHandler> _logger;
    private readonly LogApiOptions _options;
    private readonly LogRecordSubject _subject;

    public LogRecordHandler(
        LogRecordSubject subject,
        IOptions<LogApiOptions> optionsAccessor,
        ILogger<LogRecordHandler> logger)
    {
        if (optionsAccessor is null) throw new ArgumentNullException(nameof(optionsAccessor));
        _subject = subject ?? throw new ArgumentNullException(nameof(subject));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = optionsAccessor.Value;
    }

    public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var logRecord = MessagePackSerializer.Deserialize<LogRecordEvent>(data, _options.SerializerOptions, cancellationToken);

        _subject.Send(logRecord);

        if (_logger.IsEnabled(LogLevel.Information))
            _logger.LogInformation(JsonSerializer.Serialize(logRecord));

        return ValueTask.CompletedTask;
    }
}
