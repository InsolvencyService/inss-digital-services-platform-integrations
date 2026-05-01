using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Azure.Relay;
using Microsoft.Extensions.Options;

namespace Platform.Integrations.RpsListener
{
    public class AzureRelayListenerService : BackgroundService
    {
        private readonly IOptions<RelayOptions> _relayOptions;
        private readonly IOptions<WaderOptions> _waderOptions;
        private HybridConnectionListener? _listener;
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
        //private const string BaseUrl = "https://localhost:7181";
        private const string RefEndpoint = "/api/redundancy/cases";

        public AzureRelayListenerService(IOptions<RelayOptions> relayOptions, IOptions<WaderOptions> waderOptions)
        {
            _relayOptions = relayOptions;
            _waderOptions = waderOptions;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(_relayOptions.Value.KeyName, _relayOptions.Value.Key);

            var listenerUri = new Uri($"sb://{_relayOptions.Value.RelayNamespace}/{_relayOptions.Value.ConnectionName}");
            _listener = new HybridConnectionListener(listenerUri, tokenProvider);

            _listener.Connecting += (_, _) => Console.WriteLine("Connecting...");
            _listener.Online += (_, _) => Console.WriteLine("Online");
            _listener.Offline += (_, _) => Console.WriteLine("Offline");

            _listener.RequestHandler = context =>
            {
                _ = Task.Run(() => HandleRequestAsync(context, _waderOptions.Value.BaseUrl, stoppingToken), stoppingToken);
            };

            await _listener.OpenAsync(stoppingToken);
            Console.WriteLine("Server listening. Press Enter to stop.");
            await Task.Delay(Timeout.Infinite, stoppingToken); // Keep the service running until cancellation is requested
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_listener is not null)
            {
                await _listener.CloseAsync(cancellationToken);
                Console.WriteLine("Relay listener stopped.");
            }

            await base.StopAsync(cancellationToken);
        }

        private static async Task HandleRequestAsync(
            RelayedHttpListenerContext context, 
            string baseUrl, 
            CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine($"Request received: {context.Request.HttpMethod} {context.Request.Url}");

                using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
                var body = await reader.ReadToEndAsync(cancellationToken);

                Console.WriteLine($"Sent case reference from sender / web: {body}");

                var request = JsonSerializer.Deserialize<CheckReferenceRequest>(body, _jsonSerializerOptions);

                if (request is null || string.IsNullOrWhiteSpace(request.CaseRefNumber))
                {
                    await WriteResponseAsync(context, HttpStatusCode.BadRequest, "CustomerReference is required.");
                    return;
                }

                var exists = await LookupReferenceAsync(request.CaseRefNumber, baseUrl, cancellationToken);

                await WriteResponseAsync(
                    context,
                    exists ? HttpStatusCode.OK : HttpStatusCode.NotFound,
                    exists ? "Found" : "Not found");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                await WriteResponseAsync(
                    context,
                    HttpStatusCode.InternalServerError,
                    "Internal server error.");
            }
        }

        private static async Task<bool> LookupReferenceAsync(
            string customerReference,
            string baseUrl,
            CancellationToken cancellationToken)
        {
            // TODO: Use the http client factory
            using var client = new HttpClient();

            var response = await client.GetAsync(
                $"{baseUrl}{RefEndpoint}/{customerReference}",
                cancellationToken);

            return response.IsSuccessStatusCode;
        }

        private static async Task WriteResponseAsync(
            RelayedHttpListenerContext context,
            HttpStatusCode statusCode,
            string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.Headers.Add("Content-Type", "application/json");

            var json = JsonSerializer.Serialize(new { message });

            using var writer = new StreamWriter(context.Response.OutputStream);
            await writer.WriteAsync(json);
            await writer.FlushAsync();

            context.Response.Close();
        }

    }

    public class CheckReferenceRequest
    {
        public string? CaseRefNumber { get; set; }
    }

}

