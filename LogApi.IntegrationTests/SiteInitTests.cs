using System;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Tgs.LogApi;

namespace LogApi.IntegrationTests;

[TestFixture]
public class SiteInitTests
{
    [Test]
    public void Site_Initial_Should_No_Error()
    {
        using var sut = new TestLogApiApplication(webBuilder => webBuilder.ConfigureServices(
            services => services
                .Configure<LogApiOptions>(options =>
                {
                    options.LogLevel = Tgs.Logging.TgsLogLevel.All;
                    options.UnrestrictedLoggers = Array.Empty<string>();
                })
                .AddFakeNatsMessageQueue()));

        Assert.DoesNotThrow(() => sut.CreateClient());
    }
}
