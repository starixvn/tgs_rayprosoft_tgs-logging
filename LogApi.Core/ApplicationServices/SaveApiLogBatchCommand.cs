using System.Collections.Generic;

namespace Tgs.LogApi.ApplicationServices
{
	public record SaveApiLogBatchCommand(IEnumerable<ApiLogEvent> LogEvents);
}
