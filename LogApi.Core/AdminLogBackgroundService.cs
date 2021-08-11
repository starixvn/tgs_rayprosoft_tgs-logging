using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Tgs.LogApi.ApplicationServices;
using TGS.ServiceUtility;

namespace Tgs.LogApi
{
	internal class AdminLogBackgroundService : BackgroundService
	{
		private readonly AdminLogSubject _adminLogSubject;
		private readonly IServiceProvider _serviceProvider;
		private readonly LogApiOptions _options;

		public AdminLogBackgroundService(
			AdminLogSubject adminLogSubject,
			IServiceProvider serviceProvider,
			IOptions<LogApiOptions> optionsAccessor)
		{
			if (optionsAccessor is null) throw new ArgumentNullException(nameof(optionsAccessor));

			_adminLogSubject = adminLogSubject ?? throw new ArgumentNullException(nameof(adminLogSubject));
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_options = optionsAccessor.Value;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var batchSub = _adminLogSubject
				.BatchSubscribe(
					_options.BatchInsertCounts,
					async logs =>
					{
						using var scope = _serviceProvider.CreateScope();
						var batchService = scope.ServiceProvider.GetRequiredService<ICommandService<SaveAdminLogBatchCommand>>();

						await batchService
							.ExecuteAsync(new SaveAdminLogBatchCommand(logs))
							.ConfigureAwait(false);
					});

			stoppingToken.Register(() => batchSub.Dispose());

			return Task.CompletedTask;
		}
	}
}
