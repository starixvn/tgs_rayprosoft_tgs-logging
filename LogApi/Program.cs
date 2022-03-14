using TGS.Apis.LogApi;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("/config/appsettings.json", true);

var startup = new Startup();

startup.ConfigureServices(builder.Services);

var app = builder.Build();

app.MapControllers();

app.MapHealthChecks("/health");

await app.RunAsync();
