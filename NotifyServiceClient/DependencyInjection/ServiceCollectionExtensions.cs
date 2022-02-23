using System;
using Microsoft.Extensions.Options;
using Tgs.NotifyService.Client;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNotifyClient(
            this IServiceCollection services,
            Action<NotifyClientOptions, IServiceProvider> configure)
            => services
                .AddOptions<NotifyClientOptions>()
                .Configure(configure)
                .Services
                .AddHttpClient<NotifyClient>()
                .ConfigureHttpClient((sp, http) =>
                {
                    var options = sp.GetRequiredService<IOptions<NotifyClientOptions>>().Value;

                    http.BaseAddress = options.BaseUrl;
                })
                .Services
                .AddTransient<INotifyClient>(sp => sp.GetRequiredService<NotifyClient>());
    }
}
