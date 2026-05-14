using Microsoft.Extensions.Options;
using Platform.Integrations.RpsListener.Configurations;
using Platform.Integrations.RpsListener.Extensions;
using Platform.Integrations.RpsListener.HostedServices;

namespace Platform.Integrations.RpsListener.Services; 

public sealed class LookupCaseReferenceService
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<WaderOptions> _waderOptions;
    private readonly ILogger<AzureRelayListenerService> _logger;

    public LookupCaseReferenceService(HttpClient httpClient, IOptions<WaderOptions> waderOptions, ILogger<AzureRelayListenerService> logger)
    {
        _httpClient = httpClient;
        _waderOptions = waderOptions;
        _logger = logger;
    }

    public async Task<bool> LookupReferenceAsync(string caseRefNumber, CancellationToken cancellationToken)
    {
        try
        {
            _logger.CallingWaderApi(caseRefNumber);

            string url = $"{_waderOptions.Value.BaseUrl}{_waderOptions.Value.ReferenceEndpoint}/{caseRefNumber}";

            HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);

            _logger.WaderApiSuccess(caseRefNumber, response.StatusCode.ToString());
            return response.IsSuccessStatusCode;
        }
        catch (Exception error)
        {
            _logger.WaderApiFailed(error, caseRefNumber);
            throw;
        }
    }
}
