using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Configuration;
using Tgs.Logging;
using Tgs.MessageQueue;

namespace Microsoft.Extensions.Logging;

public static class LoggingBuilderExtensions
{
    public static ILoggingBuilder AddTgsLog(this ILoggingBuilder builder)
    {
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, TgsLoggerProvider>());

        LoggerProviderOptions.RegisterProviderOptions<TgsLoggerOptions, TgsLoggerProvider>(builder.Services);

        builder.Services
            .TryAddSingleton(
                sp => new TgsLogBackgroundProcessor<LogRecordEvent>(
                    "tgs.log.log-record",
                    sp.GetRequiredService<IMessageSender>()));
        builder.Services
            .TryAddSingleton(
                sp => new TgsLogBackgroundProcessor<AdminLogEvent>(
                    "tgs.log.admin",
                    sp.GetRequiredService<IMessageSender>()));
        builder.Services
            .TryAddSingleton(
                sp => new TgsLogBackgroundProcessor<ApiLogEvent>(
                    "tgs.log.apilog",
                    sp.GetRequiredService<IMessageSender>()));

        _ = builder.Services
            .AddTransient<ITgsLog, TgsLog>();

        return builder;
    }
}
