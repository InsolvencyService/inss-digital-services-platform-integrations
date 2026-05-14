using Microsoft.Azure.Relay;
using Microsoft.Extensions.Options;
//using Platform.Integrations.RpsListener.Models;
using Platform.Integrations.RpsListener.Extensions;
using Platform.Integrations.RpsListener.Relay;
using Platform.Integrations.RpsListener.Configurations;

namespace Platform.Integrations.RpsListener.HostedServices;

public class AzureRelayListenerService : BackgroundService
{
    private readonly IOptions<RelayOptions> _relayOptions;
    private readonly ILogger<AzureRelayListenerService> _logger;
    private readonly RelayRequestProcessor _requestProcessor;

    private HybridConnectionListener? _listener;

    public AzureRelayListenerService(
        IOptions<RelayOptions> relayOptions,
        RelayRequestProcessor requestProcessor,
        ILogger<AzureRelayListenerService> logger)
    {
        _relayOptions = relayOptions;
        _requestProcessor = requestProcessor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TokenProvider tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(_relayOptions.Value.KeyName, _relayOptions.Value.Key);

        Uri listenerUri = new($"sb://{_relayOptions.Value.RelayNamespace}/{_relayOptions.Value.ConnectionName}");
        _listener = new HybridConnectionListener(listenerUri, tokenProvider);

        _listener.Connecting += (_, _) => _logger.RelayListenerStarted();
        _listener.Online += (_, _) => _logger.RelayListenerOnline();
        _listener.Offline += (_, _) => _logger.RelayListenerOffline();

        _listener.RequestHandler = context =>
        {
            _ = Task.Run(() => _requestProcessor.HandleRequestAsync(context, stoppingToken), stoppingToken);
        };

        await _listener.OpenAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken); // Keep the service running until cancellation is requested
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_listener is not null)
        {
            await _listener.CloseAsync(cancellationToken);
            _logger.RelayListenerStopped();
        }

        await base.StopAsync(cancellationToken);
    }
}


