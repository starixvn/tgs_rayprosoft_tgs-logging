using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tgs.LogApi
{
	public interface ITgsRepository
	{
		ValueTask SaveAdminLogsAsync(IEnumerable<AdminLogEvent> adminLogEvents);

		ValueTask SaveApiLogsAsync(IEnumerable<ApiLogEvent> apiLogEvents);

		ValueTask SaveLogRecordsAsync(IEnumerable<LogRecordEvent> logRecordEvents);
	}
}
