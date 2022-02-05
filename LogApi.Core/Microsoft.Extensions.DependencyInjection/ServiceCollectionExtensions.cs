using System;
using System.Reflection;
using Microsoft.Extensions.Options;
using Tgs.LogApi;
using Tgs.LogApi.Configuration;
using Tgs.LogApi.MessageQueue;

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
                        .AddHandler<ApiLogHandler>("tgs.log.apilog", "tgs.log.apilog")
                        .AddHandler<AdminLogHandler>("AdminLogEvent", "AdminLogEvent")
                        .AddHandler<AdminLogHandler>("tgs.log.admin", "tgs.log.admin")
                        .AddHandler<LogRecordHandler>("LogRecordEvent", "LogRecordEvent")
                        .AddHandler<LogRecordHandler>("tgs.log.log-record", "tgs.log.log-record");
                }));
    }
}
