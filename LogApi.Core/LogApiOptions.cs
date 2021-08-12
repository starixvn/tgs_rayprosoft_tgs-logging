using System;
using MessagePack;
using Tgs.Logging;

namespace Tgs.LogApi
{
	public class LogApiOptions
	{
		public int BatchInsertCounts { set; get; }

		public TgsLogLevel LogLevel { set; get; }

		public string[] UnrestrictedLoggers { set; get; } = Array.Empty<string>();

		public string NatsUrl { get; set; } = "nats://localhost:4222";

		public MessagePackSerializerOptions SerializerOptions { get; set; } =
			MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
	}
}
