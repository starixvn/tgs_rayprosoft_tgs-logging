using System.Collections.Generic;
using Tgs.Logging;

namespace Tgs.LogApi.ApplicationServices;

public record SaveLogRecordBatchCommand(IEnumerable<LogRecordEvent> LogEvents);
