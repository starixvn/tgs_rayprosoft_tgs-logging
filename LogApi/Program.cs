using Prometheus;
using Prometheus.DotNetRuntime;
using TGS.Apis.LogApi;

using var collector = DotNetRuntimeStatsBuilder
    .Customize()
    .WithContentionStats(sampleRate: SampleEvery.FiveEvents)
    .WithJitStats(sampleRate: SampleEvery.TenEvents)
    .WithGcStats()
    .WithExceptionStats()
    .StartCollecting();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("/config/appsettings.json", true);

var startup = new Startup();

startup.ConfigureServices(builder.Services);

var app = builder.Build();

app.MapControllers();

app.MapHealthChecks("/health");

app.MapMetrics();

await app.RunAsync();
