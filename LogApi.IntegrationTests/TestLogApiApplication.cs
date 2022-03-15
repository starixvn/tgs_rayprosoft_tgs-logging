using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenTelemetry.Exporter;

namespace LogApi.IntegrationTests;

internal class TestLogApiApplication : WebApplicationFactory<Program>
{
    private readonly Action<IWebHostBuilder>? _webHostConfigure;

    public TestLogApiApplication(Action<IWebHostBuilder>? webHostConfigure = null)
    {
        _webHostConfigure = webHostConfigure;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _ = builder.ConfigureAppConfiguration(builder => builder.Sources.Clear());

        _ = builder.ConfigureServices(services => services
              .AddSingleton<IConfigureOptions<OtlpExporterOptions>>(
                  sp => new ConfigureNamedOptions<OtlpExporterOptions>(
                      Options.DefaultName,
                      exporterOptions =>
                      {
                          exporterOptions.Endpoint = null;
                      })));

        _webHostConfigure?.Invoke(builder);
    }
}
