using System;
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
    internal class ApiLogBackgroundService : BackgroundService
    {
        private readonly ApiLogSubject _apiLogSubject;
        private readonly TaskSubject _taskSubject;
        private readonly ILogger<ApiLogBackgroundService> _logger;
        private readonly LogApiOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private IDisposable _batchSubscription = default!;

        public ApiLogBackgroundService(
            ApiLogSubject apiLogSubject,
            TaskSubject taskSubject,
            IServiceProvider serviceProvider,
            IOptions<LogApiOptions> optionsAccessor,
            ILogger<ApiLogBackgroundService> logger)
        {
            if (optionsAccessor is null) throw new ArgumentNullException(nameof(optionsAccessor));
            _apiLogSubject = apiLogSubject ?? throw new ArgumentNullException(nameof(apiLogSubject));
            _taskSubject = taskSubject ?? throw new ArgumentNullException(nameof(taskSubject));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = optionsAccessor.Value;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _batchSubscription = _apiLogSubject
                .BatchSubscribe(
                    _options.BatchInsertCounts,
                    async logs =>
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var batchService = scope.ServiceProvider.GetRequiredService<ICommandService<SaveApiLogBatchCommand>>();

                        await batchService
                            .ExecuteAsync(new SaveApiLogBatchCommand(logs))
                            .ConfigureAwait(false);
                    },
                    _logger)
                .Subscribe(_taskSubject);

            stoppingToken.Register(() => _batchSubscription.Dispose());

            return Task.CompletedTask;
        }
    }
}
