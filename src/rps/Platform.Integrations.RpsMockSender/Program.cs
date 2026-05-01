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

    await Task.WhenAll(tasks);

    Console.WriteLine("All requests finished.");

    static async Task SendReferenceCheckAsync(HttpClient client, Uri uri, string token, int requestNumber)
    {
        var incCaseRefNumber = $"CASE_REF{requestNumber:D3}";
        var json = $$"""
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


    //var request = new HttpRequestMessage()
    //{
    //    RequestUri = uri,
    //    Method = HttpMethod.Get
    //};

    // POST request with Case Reference Number as the JSON content....
    //var request = new HttpRequestMessage(
    //    HttpMethod.Post, uri)
    //{
    //    Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
    //};

    //request.Headers.Add("ServiceBusAuthorization", token);
    
    //var response = await client.SendAsync(request);
    //var body = await response.Content.ReadAsStringAsync();

    //Console.WriteLine($"Status Code: {response.StatusCode}");
    //Console.WriteLine(body);

    //Console.WriteLine(await response.Content.ReadAsStringAsync());
    //Console.ReadLine();
}
