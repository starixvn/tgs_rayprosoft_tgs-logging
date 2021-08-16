using System;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.Extensions.Options;
using Tgs.Logging;
using Tgs.MessageQueue;

namespace Tgs.LogApi.MessageQueue
{
	internal class ApiLogHandler : IMessageHandler
	{
		private readonly LogApiOptions _options;
		private readonly ApiLogSubject _subject;

		public ApiLogHandler(
			ApiLogSubject subject,
			IOptions<LogApiOptions> optionsAccessor)
		{
			if (optionsAccessor is null) throw new ArgumentNullException(nameof(optionsAccessor));
			_subject = subject ?? throw new System.ArgumentNullException(nameof(subject));
			_options = optionsAccessor.Value;
		}

		public ValueTask HandleAsync(byte[] data, CancellationToken cancellationToken = default)
		{
			var apiLog = MessagePackSerializer.Deserialize<ApiLogEvent>(
				 data,
				 _options.SerializerOptions);

			_subject.Send(apiLog);

			return ValueTask.CompletedTask;
		}
	}
}
