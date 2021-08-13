using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tgs.LogApi.Repositories.Entities;

namespace LogApi.IntegrationTests
{
	internal class TestSiteHelper
	{
		public static IHost GetLogApiTestHost()
			=> GetLogApiTestHost(services => { });

		public static IHost GetLogApiTestHost(Action<IServiceCollection> configureServices)
			=> Host.CreateDefaultBuilder(Array.Empty<string>())
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder
						.UseStartup<TGS.Apis.LogApi.Startup>()
						.ConfigureAppConfiguration((ctx, builder) =>
						{
							builder.Sources.Clear();
						})
						.UseTestServer()
						.ConfigureTestServices(services =>
						{
							services
								.AddFakeNatsMessageQueue()
								.AddSingleton<TgsContext>(sp =>
								{
									var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
									dbContextOptionsBuilder
										.UseInMemoryDatabase(Guid.NewGuid().ToString())
										.ConfigureWarnings(x => x.Ignore(
											InMemoryEventId.TransactionIgnoredWarning));

									return new TgsContext(dbContextOptionsBuilder.Options);
								});

							configureServices(services);
						});
				})
				.Build();
	}
}
