using Microsoft.Extensions.DependencyInjection.Extensions;
using Tgs.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTgsLog(this IServiceCollection services)
        {
            services.TryAddTransient<ITgsLog, TgsLog>();

            return services;
        }
    }
}
