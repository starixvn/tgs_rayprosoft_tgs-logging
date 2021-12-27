using Microsoft.Extensions.DependencyInjection.Extensions;
using Tgs.Logging;
using Tgs.MessageQueue;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTgsLog(this IServiceCollection services)
        {
            services.TryAddSingleton(
                sp => new TgsLogBackgroundProcessor<LogRecordEvent>(
                "LogRecordEvent",
                sp.GetRequiredService<IMessageSender>()));
            services.AddSingleton(
                sp => new TgsLogBackgroundProcessor<AdminLogEvent>(
                "AdminLogEvent",
                sp.GetRequiredService<IMessageSender>()));
            services.AddSingleton(
                sp => new TgsLogBackgroundProcessor<ApiLogEvent>(
                "ApiLogEvent",
                sp.GetRequiredService<IMessageSender>()));
            services.TryAddTransient<ITgsLog, TgsLog>();

            return services;
        }
    }
}
