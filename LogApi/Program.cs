using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Prometheus.DotNetRuntime;

namespace TGS.Apis.LogApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            DotNetRuntimeStatsBuilder
               .Customize()
               .WithContentionStats(sampleRate: SampleEvery.FiveEvents)
               .WithJitStats(sampleRate: SampleEvery.TenEvents)
               .WithGcStats()
               .WithExceptionStats()
               .StartCollecting();

            var host = CreateHostBuilder(args).Build();

            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((ctx, builder) =>
                {
                    builder.AddJsonFile("/config/appsettings.json", true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
