using Microsoft.Azure.Relay;
using Platform.Integrations.RpsListener.Extensions;
using Platform.Integrations.RpsListener.HostedServices;
using Platform.Integrations.RpsListener.Models;
using Platform.Integrations.RpsListener.Services;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Platform.Integrations.RpsListener.Relay;

public class RelayRequestProcessor
{
    private readonly ILogger<AzureRelayListenerService> _logger;
    private readonly RelayResponseWriter _responseWriter;
    private readonly LookupCaseReferenceService _lookupService;

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public RelayRequestProcessor(ILogger<AzureRelayListenerService> logger, RelayResponseWriter responseWriter, LookupCaseReferenceService lookupCaseReferenceService)
    {
        _logger = logger;
        _responseWriter = responseWriter;
        _lookupService = lookupCaseReferenceService;
    }

    public async Task HandleRequestAsync(RelayedHttpListenerContext context, CancellationToken cancellationToken)
    {
        try
        {
            _logger.RelayRequestReceived(context.Request.HttpMethod, context.Request.Url.ToString());

            using StreamReader reader = new(context.Request.InputStream, Encoding.UTF8);
            string body = await reader.ReadToEndAsync(cancellationToken);

            _logger.RelayCaseRefSent(body);

            CaseReferenceRequest? request = JsonSerializer.Deserialize<CaseReferenceRequest>(body, _jsonSerializerOptions);

            if (request is null || string.IsNullOrWhiteSpace(request.CaseRefNumber))
            {
                await _responseWriter.WriteResponseAsync(context, HttpStatusCode.BadRequest, "Case Reference Number is required.");
                return;
            }

            bool exists = await _lookupService.LookupReferenceAsync(request.CaseRefNumber, cancellationToken);

            await _responseWriter.WriteResponseAsync(
                context,
                exists ? HttpStatusCode.OK : HttpStatusCode.NotFound,
                exists ? "Found" : "Not found");
        }
        catch (Exception ex)
        {
            _logger.RelayRequestFailed(ex);

            await _responseWriter.WriteResponseAsync(
                context,
                HttpStatusCode.InternalServerError,
                "Internal server error.");
        }
    }
}
