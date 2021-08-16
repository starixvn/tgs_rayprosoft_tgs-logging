using System;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.Extensions.Options;
using Tgs.Logging;
using Tgs.MessageQueue;

namespace Tgs.LogApi.MessageQueue
{
	internal class LogRecordHandler : IMessageHandler
	{
		private readonly LogApiOptions _options;
		private readonly LogRecordSubject _subject;

		public LogRecordHandler(LogRecordSubject subject, IOptions<LogApiOptions> optionsAccessor)
		{
			if (optionsAccessor is null) throw new ArgumentNullException(nameof(optionsAccessor));
			_subject = subject ?? throw new ArgumentNullException(nameof(subject));
			_options = optionsAccessor.Value;
		}

		public ValueTask HandleAsync(byte[] data, CancellationToken cancellationToken = default)
		{
			var logRecord = MessagePackSerializer.Deserialize<LogRecordEvent>(
				data,
				_options.SerializerOptions);

			_subject.Send(logRecord);

			return ValueTask.CompletedTask;
		}
	}
}
