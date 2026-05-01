using Platform.Integrations.RpsListener;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<AzureRelayListenerService>();
builder.Services.AddHealthChecks();
builder.Services.AddOptions<RelayOptions>().Bind(builder.Configuration.GetSection("Relay"));
builder.Services.AddOptions<WaderOptions>().Bind(builder.Configuration.GetSection("Wader"));

var app = builder.Build();
app.UseHealthChecks("/health");

await app.RunAsync();