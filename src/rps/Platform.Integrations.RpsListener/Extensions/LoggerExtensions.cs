
namespace Platform.Integrations.RpsListener.Extensions;

public static partial class LoggerExtensions
{

    // Startup and shutdown events...
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Azure Relay Listener connecting....")]
    public static partial void RelayListenerStarted(this ILogger logger);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "Azure Relay Listener online.")]
    public static partial void RelayListenerOnline(this ILogger logger);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Warning,
        Message = "Azure Relay Listener offline.")]
    public static partial void RelayListenerOffline(this ILogger logger);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Warning,
        Message = "Azure Relay Listener has stopped.")]
    public static partial void RelayListenerStopped(this ILogger logger);

    // Request handling events...
    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Information,
        Message = "Request received: {HttpMethod} {Url}.")]
    public static partial void RelayRequestReceived(this ILogger logger, string HttpMethod, string Url);

    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Information,
        Message = "Sent case reference from sender / web: {body}.")]
    public static partial void RelayCaseRefSent(this ILogger logger, string body);

    [LoggerMessage(
    EventId = 1007,
    Level = LogLevel.Information,
    Message = "Looking up case reference {CaseRefNumber} in Wader API.")]
    public static partial void CallingWaderApi(this ILogger logger, string CaseRefNumber);

    [LoggerMessage(
    EventId = 1008,
    Level = LogLevel.Information,
    Message = "Received response from Wader API for case reference {CaseRefNumber}: {StatusCode}.")]
    public static partial void WaderApiSuccess(this ILogger logger, string caseRefNumber, string statusCode);

    // Request error handling events...
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Error,
        Message = "Error processing relay request.")]
    public static partial void RelayRequestFailed(this ILogger logger, Exception exception);

    [LoggerMessage(
    EventId = 2002,
    Level = LogLevel.Error,
    Message = "Error looking up case reference {CaseRefNumber} in the Wader API")]
    public static partial void WaderApiFailed(this ILogger logger, Exception exception, string caseRefNumber);
}


