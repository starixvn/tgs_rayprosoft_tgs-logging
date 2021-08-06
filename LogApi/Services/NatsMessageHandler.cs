using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NATS.Client;
using TGS.Apis.LogApi.Interfaces;
using TGS.Core.Infrastructures.Extensions;
using TGS.Core.Infrastructures.Log.Events;
using TGS.Core.Infrastructures.Nats;
using TGS.Core.Models;

namespace TGS.Apis.LogApi.Services
{
    public interface IMessageHandler
    {
        void StartReceiveMessages();
        void StopReceiveMessages();
        DateTimeOffset LastStartUpTime { get; }
        DateTimeOffset LastReceivedEventTime { get; }
        DateTimeOffset LastLogInsertTime { get; }
        string LastLogInsertError { get; }
        LogBusEvent LastBusEvent { get; }
        ConcurrentDictionary<string, uint> LogReceivedCount { get; }
    }

    public class NatsMessageHandler : IMessageHandler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly INatsManager _natsManager;
        private IAsyncSubscription _apiLogSubscription;
        private IAsyncSubscription _adminLogSubscription;
        private IAsyncSubscription _logRecordSubscription;
        private DateTimeOffset _lastReceivedEventTime;
        private LogBusEvent _lastBusEvent;
        private DateTimeOffset _startupTime;
        private DateTimeOffset _lastInsertTime;
        private string _lastError;
        private IDisposable _subscription;
        private readonly ConcurrentDictionary<string, uint> _logReceivedCount = new ConcurrentDictionary<string, uint>();
        private readonly LoggerServiceConfig _loggerServiceConfig;

        public NatsMessageHandler(
            IServiceProvider serviceProvider,
            IOptions<LoggerServiceConfig> loggerServiceConfig,
            INatsManager natsManager)
        {
            _serviceProvider = serviceProvider;
            _natsManager = natsManager;
            _loggerServiceConfig = loggerServiceConfig.Value;
        }

        void IMessageHandler.StartReceiveMessages()
        {
            SubscribeApiLogReceived();
            SubscribeLogRecordReceived();
            SubscribeAdminLogReceived();
            _startupTime = DateTimeOffset.UtcNow;
        }

        public event Action<LogBusEvent> LogRecordReceived;

        public event Action<LogBusEvent> ApiLogReceived;

        public event Action<LogBusEvent> AdminLogReceived;

        private void SubscribeApiLogReceived()
        {
            _apiLogSubscription = _natsManager.SubscribeAsync<ApiLogEvent>(
                nameof(ApiLogEvent),
                Consts.Nats.NatsTgsLogQueueName,
                message =>
                {
                    _lastBusEvent = message.DataObject;
                    _lastReceivedEventTime = DateTimeOffset.UtcNow;
                    ApiLogReceived?.Invoke(message.DataObject);
                });

            var query =
                from logs in Observable.FromEvent<LogBusEvent>(
                        ev => ApiLogReceived += ev,
                        ev => ApiLogReceived -= ev)
                    .Buffer(_loggerServiceConfig.BatchInsertWaitingSeconds, _loggerServiceConfig.BatchInsertCounts)
                where logs.Count > 0
                select logs;

            _subscription = query
            .SubscribeOn(NewThreadScheduler.Default)
            .Subscribe(
                async logs =>
                {
                    var now = DateTimeOffset.UtcNow;
                    CountReceivedLogs(now, nameof(ApiLogEvent), logs.Count);
                    await WriteLogsAsync(nameof(ApiLogEvent), logs);
                },
                ex => Console.WriteLine($"error occurred when RX executed: {ex}"));
        }

        private void SubscribeAdminLogReceived()
        {
            _adminLogSubscription = _natsManager.SubscribeAsync<AdminLogEvent>(
                nameof(AdminLogEvent),
                Consts.Nats.NatsTgsLogQueueName,
                message =>
                {
                    _lastBusEvent = message.DataObject;
                    _lastReceivedEventTime = DateTimeOffset.UtcNow;
                    AdminLogReceived?.Invoke(message.DataObject);
                });

            var query =
                from logs in Observable.FromEvent<LogBusEvent>(
                        ev => AdminLogReceived += ev,
                        ev => AdminLogReceived -= ev)
                    .Buffer(_loggerServiceConfig.BatchInsertWaitingSeconds, _loggerServiceConfig.BatchInsertCounts)
                where logs.Count > 0
                select logs;

            _subscription = query
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(
                    async logs =>
                    {
                        var now = DateTimeOffset.UtcNow;
                        CountReceivedLogs(now, nameof(AdminLogEvent), logs.Count);
                        await WriteLogsAsync(nameof(AdminLogEvent), logs);
                    },
                    ex => Console.WriteLine("error occurred when RX executed:" + ex));
        }


        private void SubscribeLogRecordReceived()
        {
            _logRecordSubscription = _natsManager.SubscribeAsync<LogRecordEvent>(
                nameof(LogRecordEvent),
                Consts.Nats.NatsTgsLogQueueName,
                message =>
                {
                    _lastBusEvent = message.DataObject;
                    _lastReceivedEventTime = DateTimeOffset.UtcNow;
                    LogRecordReceived?.Invoke(message.DataObject);
                });

            var query =
                from logs in Observable.FromEvent<LogBusEvent>(
                        ev => LogRecordReceived += ev,
                        ev => LogRecordReceived -= ev)
                    .Buffer(_loggerServiceConfig.BatchInsertWaitingSeconds, _loggerServiceConfig.BatchInsertCounts)
                where logs.Count > 0
                select logs;

            _subscription = query
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(
                    async logs =>
                    {
                        var now = DateTimeOffset.UtcNow;
                        CountReceivedLogs(now, nameof(LogRecordEvent), logs.Cast<LogRecordEvent>().Count(log => log.level >= _loggerServiceConfig.LogLevel || _loggerServiceConfig.UnrestrictedLoggers.Contains(log.logger)));
                        await WriteLogsAsync(nameof(LogRecordEvent), logs);
                    },
                    ex => Console.WriteLine("error occurred when RX executed:" + ex));
        }

        void IMessageHandler.StopReceiveMessages()
        {
            _apiLogSubscription?.Dispose();
            _logRecordSubscription?.Dispose();
            _adminLogSubscription?.Dispose();
            Task.WaitAll(Task.Delay(_loggerServiceConfig.BatchInsertWaitingSeconds.Add(TimeSpan.FromSeconds(3))));
            _subscription?.Dispose();
        }

        DateTimeOffset IMessageHandler.LastStartUpTime => _startupTime;

        DateTimeOffset IMessageHandler.LastReceivedEventTime => _lastReceivedEventTime;

        DateTimeOffset IMessageHandler.LastLogInsertTime => _lastInsertTime;

        string IMessageHandler.LastLogInsertError => _lastError;

        LogBusEvent IMessageHandler.LastBusEvent => _lastBusEvent;

        ConcurrentDictionary<string, uint> IMessageHandler.LogReceivedCount => _logReceivedCount;

        private async Task WriteLogsAsync(string eventType, IList<LogBusEvent> events)
        {
            if (eventType.IsNullOrEmpty() || events == null)
                await Task.FromResult(0);

            using var scope = _serviceProvider.CreateScope();

            try
            {
                var factory = scope.ServiceProvider.GetRequiredService<Func<string, ILoggerWriter>>();

                var logger = factory(eventType);
                await logger.SaveLogsAsync(events);
                _lastInsertTime = DateTimeOffset.UtcNow;
            }
            catch (Exception ex)
            {
                Console.WriteLine("error occurred when batch insert logs:" + ex);
                _lastError = ex.ToString();
            }
        }

        private void CountReceivedLogs(DateTimeOffset now, string eventTypeName, int? count)
        {
            var key = $"{now:yyyy/MM/dd HH:00} {eventTypeName}";
            if (count == null || count == 0)
                return;
            _logReceivedCount.AddOrUpdate(
                key,
                (uint)count,
                (k, oldCount) => oldCount + (uint)count);
        }
    }
}
