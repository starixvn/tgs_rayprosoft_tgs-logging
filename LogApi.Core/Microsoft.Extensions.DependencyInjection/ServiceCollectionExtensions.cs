using System;
using System.Reflection;
using Tgs.LogApi;
using Tgs.LogApi.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
	public static class ServiceCollectionExtensions
	{
		public static LogApiConfiguration AddLogApi(
			this IServiceCollection services,
			Action<LogApiOptions, IServiceProvider> configure)
			=> new LogApiConfiguration(services
				.AddOptions<LogApiOptions>()
				.Configure(configure)
				.Services
				.AddSingleton<ApiLogSubject>()
				.AddSingleton<AdminLogSubject>()
				.AddSingleton<LogRecordSubject>()
				.AddHostedService<ApiLogBackgroundService>()
				.AddHostedService<AdminLogBackgroundService>()
				.AddHostedService<LogRecordBackgroundService>()
				.RegisterAllServiceUtilityTypes(
					Assembly.GetExecutingAssembly(),
					ServiceLifetime.Transient));
	}
}
