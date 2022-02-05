using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Tgs.LogApi
{
    internal class DatabaseProcessManagerBackgroundService : BackgroundService
    {
        private readonly TaskSubject _taskSubject;
        private readonly ILogger<DatabaseProcessManagerBackgroundService> _logger;
        private IDisposable? _taskSubscription;

        public DatabaseProcessManagerBackgroundService(TaskSubject taskSubject, ILogger<DatabaseProcessManagerBackgroundService> logger)
        {
            _taskSubject = taskSubject ?? throw new ArgumentNullException(nameof(taskSubject));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _taskSubscription = _taskSubject
                .Synchronize()
                .Select(callback => Observable.FromAsync(async () =>
                {
                    try
                    {
                        await callback().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Batch subscribe error !");
                    }
                }))
                .Concat()
                .Subscribe();

            stoppingToken.Register(() => _taskSubscription.Dispose());

            return Task.CompletedTask;
        }
    }
}
