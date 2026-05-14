using Azure.Monitor.OpenTelemetry.AspNetCore;
using Platform.Integrations.RpsListener.Configurations;
using Platform.Integrations.RpsListener.HostedServices;
using Platform.Integrations.RpsListener.Relay;
using Platform.Integrations.RpsListener.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<AzureRelayListenerService>();
builder.Services.AddHealthChecks();
builder.Services.AddOptions<RelayOptions>().Bind(builder.Configuration.GetSection("Relay"));
builder.Services.AddOptions<WaderOptions>().Bind(builder.Configuration.GetSection("Wader"));

builder.Services.AddHttpClient<LookupCaseReferenceService>();
builder.Services.AddSingleton<RelayRequestProcessor>();
builder.Services.AddSingleton<RelayResponseWriter>();

builder.Services.AddOpenTelemetry().UseAzureMonitor();

WebApplication app = builder.Build();
app.UseHealthChecks("/health");

await app.RunAsync();