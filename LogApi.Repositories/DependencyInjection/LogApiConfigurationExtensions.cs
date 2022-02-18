using System;
using Microsoft.EntityFrameworkCore;
using Tgs.LogApi.Configuration;
using Tgs.LogApi.Repositories;
using Tgs.LogApi.Repositories.Entities;
using Tgs.NotifyService.Client;

namespace Microsoft.Extensions.DependencyInjection;

public static class LogApiConfigurationExtensions
{
    public static LogApiConfiguration AddDbTgsRepository(
        this LogApiConfiguration configuration,
        Action<IServiceProvider, DbContextOptionsBuilder> efConfigure,
        Action<NotifyClientOptions, IServiceProvider> notifyConfigure)
    {
        _ = configuration.Services
            .AddDbContext<TgsContext>(efConfigure)
            .AddNotifyClient(notifyConfigure);

        return configuration
            .AddTgsRepository<TgsRepository>();
    }
}
