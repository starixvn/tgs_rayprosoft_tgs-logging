using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Tgs.LogApi.ApplicationServices;
using TGS.ServiceUtility;

namespace Tgs.LogApi
{
	internal class ApiLogBackgroundService : BackgroundService
	{
		private readonly ApiLogSubject _apiLogSubject;
		private readonly LogApiOptions _options;
		private readonly IServiceProvider _serviceProvider;

		public ApiLogBackgroundService(
			ApiLogSubject apiLogSubject,
			IServiceProvider serviceProvider,
			IOptions<LogApiOptions> optionsAccessor)
		{
			if (optionsAccessor is null) throw new ArgumentNullException(nameof(optionsAccessor));
			_apiLogSubject = apiLogSubject ?? throw new ArgumentNullException(nameof(apiLogSubject));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_options = optionsAccessor.Value;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var batchSub = _apiLogSubject.BatchSubscribe(
				_options.BatchInsertCounts,
				async logs =>
				{
					using var scope = _serviceProvider.CreateScope();
					var batchService = scope.ServiceProvider.GetRequiredService<ICommandService<SaveApiLogBatchCommand>>();

					await batchService
						.ExecuteAsync(new SaveApiLogBatchCommand(logs))
						.ConfigureAwait(false);
				});

			stoppingToken.Register(() => batchSub.Dispose());

			return Task.CompletedTask;
		}
	}
}
