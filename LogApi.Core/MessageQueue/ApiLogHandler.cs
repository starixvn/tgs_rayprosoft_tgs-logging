using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using Tgs.Logging;
using Tgs.MessageQueue;

namespace Tgs.LogApi.MessageQueue
{
	internal class ApiLogHandler : IMessageHandler
	{
		private readonly MessagePackSerializerOptions _messagePackSerializerOptions =
			MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

		private readonly ApiLogSubject _subject;

		public ApiLogHandler(
			ApiLogSubject subject)
		{
			_subject = subject ?? throw new System.ArgumentNullException(nameof(subject));
		}

		public ValueTask HandleAsync(byte[] data, CancellationToken cancellationToken = default)
		{
			var apiLog = MessagePackSerializer.Deserialize<ApiLogEvent>(
				 data,
				 _messagePackSerializerOptions);

			_subject.Send(apiLog);

			return ValueTask.CompletedTask;
		}
	}
}
