using System;

namespace Tgs.LogApi
{
	public class LogApiOptions
	{
		public int BatchInsertCounts { set; get; }

		public TgsLogLevel LogLevel { set; get; }

		public string[] UnrestrictedLoggers { set; get; } = Array.Empty<string>();
	}
}
