using System.Collections.Generic;
using System.Threading.Tasks;
using TGS.Core.Infrastructures.Log.Events;

namespace TGS.Apis.LogApi.Interfaces
{
    public interface ILoggerWriter
    {
        Task SaveLogsAsync(IList<LogBusEvent> events);
        Task SaveLogAsync(LogBusEvent @event);
    }
}
