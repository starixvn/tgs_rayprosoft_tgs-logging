using System;
using MessagePack;
using Tgs.Logging;

namespace Tgs.LogApi
{
	public class LogApiOptions
	{
		public int BatchInsertCounts { set; get; } = 10;

		public TgsLogLevel LogLevel { set; get; } = TgsLogLevel.All;

		public string NatsUrl { get; set; } = "nats://localhost:4222";

		public MessagePackSerializerOptions SerializerOptions { get; set; } =
			MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);

		public string[] UnrestrictedLoggers { set; get; } = Array.Empty<string>();
	}
}
