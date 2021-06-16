using System;
using System.Runtime.Serialization;
using MessagePack;

namespace Tgs.Logging
{
	[MessagePackObject]
	public class LogRecordEvent
	{
		[Key("exception")]
		[DataMember(EmitDefaultValue = false)]
		public string? Exception { get; set; }

		[Key("file")]
		[DataMember(EmitDefaultValue = false)]
		public string? File { get; set; }

		[Key("level")]
		public TgsLogLevel Level { get; set; } = TgsLogLevel.Info;

		[Key("line")]
		[DataMember(EmitDefaultValue = false)]
		public int Line { get; set; }

		[Key("logger")]
		public string Logger { get; set; } = default!;

		[Key("member")]
		[DataMember(EmitDefaultValue = false)]
		public string? Member { get; set; }

		[Key("message")]
		[DataMember(EmitDefaultValue = false)]
		public string? Message { get; set; }

		[Key("recordedon")]
		public DateTimeOffset RecordedOn { get; set; }

		[Key("servername")]
		public string ServerName { get; set; } = default!;

		[Key("trackingid")]
		public Guid TrackingId { get; set; }
	}
}
