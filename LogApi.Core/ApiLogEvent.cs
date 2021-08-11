using System;
using MessagePack;

namespace Tgs.LogApi
{
	[MessagePackObject]
	public class ApiLogEvent
	{
		[Key("executiontimems")]
		public int ExecutionTimeMs { get; set; }

		[Key("ip")]
		public string Ip { get; set; } = default!;

		[Key("method")]
		public string Method { get; set; } = default!;

		[Key("path")]
		public string Path { get; set; } = default!;

		[Key("prefix")]
		public string Prefix { get; set; } = default!;

		[Key("recordedon")]
		public DateTimeOffset RecordedOn { get; set; }

		[Key("requestcontent")]
		public string RequestContent { get; set; } = default!;

		[Key("requestheaders")]
		public string RequestHeaders { get; set; } = default!;

		[Key("responsecode")]
		public short ResponseCode { get; set; }

		[Key("responsecontent")]
		public string ResponseContent { get; set; } = default!;

		[Key("responseheaders")]
		public string ResponseHeaders { get; set; } = default!;

		[Key("servername")]
		public string ServerName { get; set; } = default!;

		[Key("trackingid")]
		public Guid TrackingId { get; set; }
	}
}
