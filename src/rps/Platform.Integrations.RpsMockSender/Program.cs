using Microsoft.Azure.Relay;
using System.Text;
using Microsoft.Extensions.Configuration;
using Platform.Integrations.RpsMockSender;

RunAsync().GetAwaiter().GetResult();

static async Task RunAsync()
{
    IConfigurationRoot config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false)
        .AddUserSecrets<Program>()
        .Build();
    RelayOptions relayOptions = config.GetRequiredSection("relay").Get<RelayOptions>()!;
    
    TokenProvider tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(relayOptions.KeyName, relayOptions.Key);
    
    Uri uri = new($"https://{relayOptions.RelayNamespace}/{relayOptions.ConnectionName}");
    string token = (await tokenProvider.GetTokenAsync(uri.AbsoluteUri, TimeSpan.FromHours(1))).TokenString;
    HttpClient client = new();

    const int numberOfRequests = 3;

    IEnumerable<Task> tasks = Enumerable.Range(1, numberOfRequests)
        .Select(requestNumber =>
            SendReferenceCheckAsync(client, uri, token, requestNumber));

    // Uncomment if you wish to see relay results in console app...
    //Console.WriteLine("Waiting for the listener to start....");
    //await Task.Delay(5000);

    await Task.WhenAll(tasks);

    Console.WriteLine("All requests finished.");

    // Uncomment if you wish to see results in console app...
    //Console.WriteLine("Press any key to exit...");
    //Console.ReadKey();

    static async Task SendReferenceCheckAsync(HttpClient client, Uri uri, string token, int requestNumber)
    {
        string incCaseRefNumber = $"CASE_REF{requestNumber:D3}";
        string json = $$"""
        {
            "caseRefNumber": "{{incCaseRefNumber}}"
        }
        """;

        using HttpRequestMessage request = new(HttpMethod.Post, uri)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        request.Headers.Add("ServiceBusAuthorization", token);

        HttpResponseMessage response = await client.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"{requestNumber}: {(int)response.StatusCode} {response.StatusCode} - {body}");
    }
}
