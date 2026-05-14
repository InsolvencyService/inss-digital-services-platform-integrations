using Microsoft.Azure.Relay;
using System.Net;
using System.Text.Json;

namespace Platform.Integrations.RpsListener.Relay;

public sealed class RelayResponseWriter
{
    public async Task WriteResponseAsync(RelayedHttpListenerContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.Headers.Add("Content-Type", "application/json");

        string json = JsonSerializer.Serialize(new { message });

        await using StreamWriter writer = new(context.Response.OutputStream);
        await writer.WriteAsync(json);
        await writer.FlushAsync();

        await context.Response.CloseAsync();
    }

}
