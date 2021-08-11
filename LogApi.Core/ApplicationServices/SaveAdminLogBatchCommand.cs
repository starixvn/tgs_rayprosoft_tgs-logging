using System.Collections.Generic;

namespace Tgs.LogApi.ApplicationServices
{
	public record SaveAdminLogBatchCommand(IEnumerable<AdminLogEvent> LogEvents);
}
