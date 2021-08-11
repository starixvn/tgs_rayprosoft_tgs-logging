using System.Collections.Generic;

namespace Tgs.LogApi.ApplicationServices
{
	public record SaveLogRecordBatchCommand(IEnumerable<LogRecordEvent> LogEvents);
}
