using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.Logging;
using Tgs.MessageQueue;

namespace Tgs.LogApi.MessageQueue
{
    internal class ApiLogHandler : IMessageHandler
    {
        private readonly ILogger<ApiLogHandler> _logger;
        private readonly LogApiOptions _options;
        private readonly ApiLogSubject _subject;

        public ApiLogHandler(
            ApiLogSubject subject,
            IOptions<LogApiOptions> optionsAccessor,
            ILogger<ApiLogHandler> logger)
        {
            if (optionsAccessor is null) throw new ArgumentNullException(nameof(optionsAccessor));
            _subject = subject ?? throw new ArgumentNullException(nameof(subject));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = optionsAccessor.Value;
        }

        public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            var apiLog = MessagePackSerializer.Deserialize<ApiLogEvent>(
                 data,
                 _options.SerializerOptions);

            _subject.Send(apiLog);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation(JsonSerializer.Serialize(apiLog));

            return ValueTask.CompletedTask;
        }
    }
}
