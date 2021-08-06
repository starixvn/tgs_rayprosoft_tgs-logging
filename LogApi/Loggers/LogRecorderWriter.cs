using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TGS.Apis.LogApi.Interfaces;
using TGS.Apis.LogApi.Services;
using TGS.Core.DataServices.Tgs;
using TGS.Core.DataServices.Tgs.Models;
using TGS.Core.Infrastructures.Log.Events;

namespace TGS.Apis.LogApi.Loggers
{
    public sealed class LogRecordWriter : ILoggerWriter
    {
        private readonly IOptions<LoggerServiceConfig> _loggerServiceConfig;
        private readonly Func<ITgsRepository> _repoFactory;

        public LogRecordWriter(
            Func<ITgsRepository> repoFactory,
            IOptions<LoggerServiceConfig> loggerServiceConfig)
        {
            _repoFactory = repoFactory;
            _loggerServiceConfig = loggerServiceConfig;
        }

        async Task ILoggerWriter.SaveLogsAsync(IList<LogBusEvent> events)
        {
            var config = _loggerServiceConfig.Value;

            var logs = events
                .Select(Convert)
                .Where(r => r.Level >= config.LogLevel ||
                    config.UnrestrictedLoggers.Contains(r.Logger))
                .ToArray();

            await using var repo = _repoFactory();
            await repo.LogRecords.AddRangeAsync(logs);
            await repo.SaveChangesAsync();
        }

        async Task ILoggerWriter.SaveLogAsync(LogBusEvent @event)
        {
            var log = Convert(@event);
            await using var repo = _repoFactory();
            await repo.LogRecords.AddAsync(log);
            await repo.SaveChangesAsync();
        }

        private static LogRecord Convert(LogBusEvent ent) => (LogRecord)ent;
    }
}