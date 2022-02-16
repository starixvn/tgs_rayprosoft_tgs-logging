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
    internal class AdminLogHandler : IMessageHandler
    {
        private readonly ILogger<AdminLogHandler> _logger;
        private readonly LogApiOptions _options;
        private readonly AdminLogSubject _subject;

        public AdminLogHandler(
            AdminLogSubject subject,
            IOptions<LogApiOptions> optionsAccessor,
            ILogger<AdminLogHandler> logger)
        {
            if (optionsAccessor is null) throw new ArgumentNullException(nameof(optionsAccessor));
            _subject = subject ?? throw new ArgumentNullException(nameof(subject));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = optionsAccessor.Value;
        }

        public ValueTask HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            var adminLog = MessagePackSerializer.Deserialize<AdminLogEvent>(
                data,
                _options.SerializerOptions);

            _subject.Send(adminLog);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation(JsonSerializer.Serialize(adminLog));

            return ValueTask.CompletedTask;
        }
    }
}
