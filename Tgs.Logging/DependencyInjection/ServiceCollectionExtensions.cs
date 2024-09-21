using Microsoft.Extensions.DependencyInjection.Extensions;
using Tgs.Logging;
using Valhalla.MessageQueue;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTgsLog(this IServiceCollection services)
    {
        services.TryAddSingleton(
            sp => new TgsLogBackgroundProcessor<LogRecordEvent>(
            "tgs.log.log-record",
            sp.GetRequiredService<IMessageSender>()));
        services.TryAddSingleton(
            sp => new TgsLogBackgroundProcessor<AdminLogEvent>(
            "tgs.log.admin",
            sp.GetRequiredService<IMessageSender>()));
        services.TryAddSingleton(
            sp => new TgsLogBackgroundProcessor<ApiLogEvent>(
            "tgs.log.apilog",
            sp.GetRequiredService<IMessageSender>()));
        services.TryAddTransient<ITgsLog, TgsLog>();

        return services;
    }

    public static IServiceCollection AddTgsLogWithHttpLogging(this IServiceCollection services, Action<TgsApiLoggerOptions> configureOptions)
        => services.AddTgsLog().Configure(configureOptions);
}
