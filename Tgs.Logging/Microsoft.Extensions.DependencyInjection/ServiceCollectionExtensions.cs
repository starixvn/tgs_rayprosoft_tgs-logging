using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Tgs.Logging.Microsoft.Extensions.DependencyInjection
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
