using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using TGS.Apis.LogApi.Filters;

namespace TGS.Apis.LogApi
{
	public class Startup
	{
		private readonly IConfiguration _configuration;

		public Startup(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime)
		{
			app.UseRouting();

			app.UseHttpMetrics();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();

				endpoints.MapHealthChecks("/health");

				endpoints.MapMetrics();
			});
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services
				.AddLogApi(
					(options, sp) =>
					{
						var config = sp.GetRequiredService<IConfiguration>();

						config.GetSection("LoggerServiceConfig").Bind(options);
					})
				.AddDbTgsRepository(
					(sp, dbBuilder) =>
					{
						var config = sp.GetRequiredService<IConfiguration>();

						dbBuilder.UseLazyLoadingProxies();

						dbBuilder.UseNpgsql(
							config.GetConnectionString("Tgs"),
							serverOption =>
							{
								serverOption.SetPostgresVersion(12, 5);
								serverOption.CommandTimeout(180);
								serverOption.UseTrigrams();
							});

						dbBuilder.UseSnakeCaseNamingConvention();
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
