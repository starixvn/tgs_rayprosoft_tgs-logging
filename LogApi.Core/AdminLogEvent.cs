using System;
using MessagePack;

namespace Tgs.LogApi
{
	[MessagePackObject()]
	public class AdminLogEvent
	{
		[Key("description")]
		public string Description { get; set; } = default!;

		[Key("exception")]
		public string? Exception { get; set; }

		[Key("executiontimems")]
		public int ExecutionTimeMs { get; set; }

		[Key("host")]
		public string Host { get; set; } = default!;

		[Key("innerexception")]
		public string? InnerException { get; set; }

		[Key("ipaddress")]
		public string IpAddress { get; set; } = default!;

		[Key("method")]
		public string Method { get; set; } = default!;

		[Key("path")]
		public string Path { get; set; } = default!;

		[Key("recordedon")]
		public DateTimeOffset RecordedOn { get; set; }

		[Key("requestcontent")]
		public string RequestContent { get; set; } = default!;

		[Key("requestheaders")]
		public string RequestHeaders { get; set; } = default!;

		[Key("responsecode")]
		public short ResponseCode { get; set; } = default!;

		[Key("responsecontent")]
		public string ResponseContent { get; set; } = default!;

		[Key("responseheaders")]
		public string ResponseHeaders { get; set; } = default!;

		[Key("servername")]
		public string ServerName { get; set; } = default!;

		[Key("StackTrace")]
		public string? StackTrace { get; set; }

		[Key("trackingid")]
		public Guid TrackingId { get; set; }
	}
}
