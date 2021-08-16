using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TGS.Apis.LogApi.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddRepositoriesAndExecuteMigrations(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
