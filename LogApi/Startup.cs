using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using TGS.Apis.LogApi.Filters;

namespace TGS.Apis.LogApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddLogApi(
                    (options, sp) =>
                    {
                        var config = sp.GetRequiredService<IConfiguration>();

                        config.GetSection("LoggerServiceConfig").Bind(options);

                        options.NatsUrl = config.GetValue<string>("NatsSetting:Urls");
                    })
                .AddDbTgsRepository(
                    (sp, dbBuilder) =>
                    {
                        var config = sp.GetRequiredService<IConfiguration>();

                        dbBuilder.UseNpgsql(
                            config.GetConnectionString("Tgs"),
                            serverOption =>
                            {
                                serverOption.CommandTimeout(180);
                            });
                    },
                    (options, sp) =>
                    {
                        var config = sp.GetRequiredService<IConfiguration>();

                        config.GetSection("Notify").Bind(options);
                    });

            services
                .AddControllers(options =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                    options.Filters.Add(new HttpResponseExceptionFilter());
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var result = new BadRequestObjectResult(context.ModelState);

                        result.ContentTypes.Add(MediaTypeNames.Application.Json);
                        result.ContentTypes.Add(MediaTypeNames.Application.Xml);

                        return result;
                    };
                });

            services
                .AddHealthChecks()
                .ForwardToPrometheus();
        }
    }
}
