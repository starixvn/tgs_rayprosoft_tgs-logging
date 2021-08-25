using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.LogApi.ApplicationServices;
using TGS.ServiceUtility;

namespace Tgs.LogApi
{
    internal class LogRecordBackgroundService : BackgroundService
    {
        private readonly ILogger<LogRecordBackgroundService> _logger;
        private readonly LogRecordSubject _logRecordSubject;
        private readonly LogApiOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public LogRecordBackgroundService(
            LogRecordSubject logRecordSubject,
            IServiceProvider serviceProvider,
            IOptions<LogApiOptions> optionsAccessor,
            ILogger<LogRecordBackgroundService> logger)
        {
            if (optionsAccessor is null) throw new ArgumentNullException(nameof(optionsAccessor));
            _logRecordSubject = logRecordSubject ?? throw new ArgumentNullException(nameof(logRecordSubject));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = optionsAccessor.Value;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var batchSub = _logRecordSubject
                .Where(log => log.Level >= _options.LogLevel
                    || _options.UnrestrictedLoggers.Contains(log.Logger))
                .BatchSubscribe(
                    _options.BatchInsertCounts,
                    async logs =>
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var batchService = scope.ServiceProvider.GetRequiredService<ICommandService<SaveLogRecordBatchCommand>>();

                        await batchService
                            .ExecuteAsync(new SaveLogRecordBatchCommand(logs))
                            .ConfigureAwait(false);
                    },
                    _logger);

            stoppingToken.Register(() => batchSub.Dispose());

            return Task.CompletedTask;
        }
    }
}
