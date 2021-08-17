using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Tgs.LogApi.Repositories.Entities;
using Tgs.Logging;
using Tgs.NotifyService.Client;
using TGS.NotifyApi.Messages;

namespace Tgs.LogApi.Repositories
{
    internal class TgsRepository : ITgsRepository
    {
        private readonly TgsContext _dbContext;
        private readonly ILogger<TgsRepository> _logger;
        private readonly INotifyClient _notifyClient;

        public TgsRepository(TgsContext dbContext, INotifyClient notifyClient, ILogger<TgsRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _notifyClient = notifyClient ?? throw new ArgumentNullException(nameof(notifyClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async ValueTask SaveAdminLogsAsync(IEnumerable<AdminLogEvent> adminLogEvents)
        {
            _logger.LogInformation("Process AdminLog batch");

            var logs = adminLogEvents
                .Select(logEvent => new AdminLog
                {
                    Id = Guid.NewGuid(),
                    Description = logEvent.Description ?? string.Empty,
                    Exception = logEvent.Exception,
                    ExecutionTimeMs = logEvent.ExecutionTimeMs,
                    InnerException = logEvent.InnerException,
                    Ip = logEvent.IpAddress,
                    Method = logEvent.Method,
                    Path = logEvent.Path,
                    RecordedOn = logEvent.RecordedOn.UtcDateTime,
                    RequestContent = logEvent.RequestContent,
                    RequestHeaders = logEvent.RequestHeaders,
                    ResponseCode = logEvent.ResponseCode,
                    ResponseContent = logEvent.ResponseContent,
                    ResponseHeaders = logEvent.ResponseHeaders,
                    ServerName = logEvent.ServerName,
                    StackTrace = logEvent.StackTrace,
                    TrackingId = logEvent.TrackingId
                })
                .ToArray();

            await _dbContext.AdminLogs
                .AddRangeAsync(logs)
                .ConfigureAwait(false);

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            await Task.WhenAll(logs.Where(log => log.ResponseCode >= 500)
                .Select(log => Task.Run(async () =>
                {
                    var request = new NotifyLogRequest
                    {
                        Type = nameof(AdminLog),
                        Id = log.Id,
                        Path = log.Path,
                        Method = log.Method,
                        Prefix = log.Method,
                        ServerName = log.ServerName,
                        ResponseCode = log.ResponseCode,
                        RecordedOn = log.RecordedOn
                    };

                    await _notifyClient.SendAsync(request).ConfigureAwait(false);
                }))).ConfigureAwait(false);
        }

        public async ValueTask SaveApiLogsAsync(IEnumerable<ApiLogEvent> apiLogEvents)
        {
            _logger.LogInformation("Process ApiLog batch");

            var logs = apiLogEvents
                .Select(logEvent => new ApiLog
                {
                    Id = Guid.NewGuid(),
                    ExecutionTimeMs = logEvent.ExecutionTimeMs,
                    Ip = logEvent.Ip,
                    Method = logEvent.Method,
                    Path = logEvent.Path,
                    Prefix = logEvent.Prefix,
                    RecordedOn = logEvent.RecordedOn.UtcDateTime,
                    RequestContent = logEvent.RequestContent,
                    RequestHeaders = logEvent.RequestHeaders,
                    ResponseCode = logEvent.ResponseCode,
                    ResponseContent = logEvent.ResponseContent,
                    ResponseHeaders = logEvent.ResponseHeaders,
                    ServerName = logEvent.ServerName,
                    TrackingId = logEvent.TrackingId
                })
                .ToArray();

            await _dbContext.ApiLogs
                .AddRangeAsync(logs)
                .ConfigureAwait(false);

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);

            await Task.WhenAll(logs.Where(log => log.ResponseCode >= 500)
                .Select(log => Task.Run(async () =>
                {
                    var request = new NotifyLogRequest
                    {
                        Type = nameof(ApiLog),
                        Id = log.Id,
                        Path = log.Path,
                        Method = log.Method,
                        Prefix = log.Method,
                        ServerName = log.ServerName,
                        ResponseCode = log.ResponseCode,
                        RecordedOn = log.RecordedOn
                    };

                    await _notifyClient.SendAsync(request).ConfigureAwait(false);
                }))).ConfigureAwait(false);
        }

        public async ValueTask SaveLogRecordsAsync(IEnumerable<LogRecordEvent> logRecordEvents)
        {
            _logger.LogInformation("Process LogRecord batch");

            var logs = logRecordEvents
                .Select(log => new LogRecord
                {
                    Id = Guid.NewGuid(),
                    Exception = log.Exception ?? string.Empty,
                    Level = (int)log.Level,
                    Logger = log.Logger,
                    Message = log.Message,
                    RecordedOn = log.RecordedOn.UtcDateTime,
                    ServerName = log.ServerName,
                    TrackingId = log.TrackingId
                })
                .ToArray();

            await _dbContext.LogRecords
                .AddRangeAsync(logs)
                .ConfigureAwait(false);

            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}
