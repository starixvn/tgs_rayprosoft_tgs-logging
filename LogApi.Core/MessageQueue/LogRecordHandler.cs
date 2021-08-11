using System;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using Tgs.MessageQueue;

namespace Tgs.LogApi.MessageQueue
{
	internal class LogRecordHandler : IMessageHandler
	{
		private readonly MessagePackSerializerOptions _messagePackSerializerOptions =
			MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

		private readonly LogRecordSubject _subject;

		public LogRecordHandler(LogRecordSubject subject)
		{
			_subject = subject ?? throw new ArgumentNullException(nameof(subject));
		}

		public ValueTask HandleAsync(byte[] data, CancellationToken cancellationToken = default)
		{
			var logRecord = MessagePackSerializer.Deserialize<LogRecordEvent>(
				data,
				_messagePackSerializerOptions);

			_subject.Send(logRecord);

			return ValueTask.CompletedTask;
		}
	}
}
