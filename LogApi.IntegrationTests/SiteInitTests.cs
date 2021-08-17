using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Tgs.LogApi;
using Tgs.LogApi.Repositories.Entities;

namespace LogApi.IntegrationTests
{
    [TestFixture]
    public class SiteInitTests
    {
        [Test]
        public void Site_Initial_Should_No_Error()
        {
            var sut = Host.CreateDefaultBuilder(Array.Empty<string>())
                .ConfigureAppConfiguration(builder =>
                {
                    builder.Sources.Clear();
                    builder.AddInMemoryCollection(
                        new Dictionary<string, string>
                        {
                            ["ConnectionStrings:Tgs"] = "Server=localhost;Database=tgs;User Id=tgs;Password=1qaz2wsx!@"
                        });
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<TGS.Apis.LogApi.Startup>()
                        .UseTestServer()
                        .ConfigureTestServices(services =>
                        {
                            services
                                .Configure<LogApiOptions>(options =>
                                {
                                    options.LogLevel = Tgs.Logging.TgsLogLevel.All;
                                    options.UnrestrictedLoggers = Array.Empty<string>();
                                })
                                .AddFakeNatsMessageQueue();
                        });
                })
                .Build();

            var actual = sut.StartAsync();

            sut.Services.GetRequiredService<TgsContext>();

            Assert.IsFalse(actual.IsFaulted);
        }
    }
}
