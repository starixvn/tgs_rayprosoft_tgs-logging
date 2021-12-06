using System;
using System.Reflection;
using Microsoft.Extensions.Options;
using Tgs.LogApi;
using Tgs.LogApi.Configuration;
using Tgs.LogApi.MessageQueue;
using Tgs.Logging;

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
                .AddSingleton<TaskSubject>()
                .AddHostedService<ApiLogBackgroundService>()
                .AddHostedService<AdminLogBackgroundService>()
                .AddHostedService<LogRecordBackgroundService>()
                .AddHostedService<DatabaseProcessManagerBackgroundService>()
                .RegisterAllServiceUtilityTypes(
                    Assembly.GetExecutingAssembly(),
                    ServiceLifetime.Transient)
                .AddNatsMessageQueue(config =>
                {
                    config
                        .ConfigQueueOptions((options, sp) =>
                        {
                            var logApiOptions = sp.GetRequiredService<IOptions<LogApiOptions>>().Value;

                            options.Url = logApiOptions.NatsUrl;
                        })
                        .AddHandler<ApiLogHandler>("ApiLogEvent", "ApiLogEvent")
                        .AddHandler<AdminLogHandler>("AdminLogEvent", "AdminLogEvent")
                        .AddHandler<LogRecordHandler>("LogRecordEvent", "LogRecordEvent");
                }));
    }
}
