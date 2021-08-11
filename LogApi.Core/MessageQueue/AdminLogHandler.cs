using System;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using Tgs.MessageQueue;

namespace Tgs.LogApi.MessageQueue
{
	internal class AdminLogHandler : IMessageHandler
	{
		private readonly MessagePackSerializerOptions _messagePackSerializerOptions =
			MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

		private readonly AdminLogSubject _subject;

		public AdminLogHandler(AdminLogSubject subject)
		{
			_subject = subject ?? throw new ArgumentNullException(nameof(subject));
		}

		public ValueTask HandleAsync(byte[] data, CancellationToken cancellationToken = default)
		{
			var adminLog = MessagePackSerializer.Deserialize<AdminLogEvent>(
				data,
				_messagePackSerializerOptions);

			_subject.Send(adminLog);

			return ValueTask.CompletedTask;
		}
	}
}
