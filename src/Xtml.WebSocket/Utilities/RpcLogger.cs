using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Xtml.WebSocket.Utilities;

// TODO: Support OpenTelemetry, etc, and follow ASP.NET Core's HTTP request logging config.
internal readonly struct RpcLogger : IDisposable
{
    private readonly long _startTimestamp = Stopwatch.GetTimestamp();
    private readonly ILogger _logger;
    private readonly string _method;

    public static RpcLogger LogMessage(ILogger logger, string method) => new(logger, method);

    private RpcLogger(ILogger logger, string method)
    {
        _logger = logger;
        _method = method;

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Request starting {Protocol} {Method} - {ContentType} {ContentLength}",
                "JSON-RPC/2.0",
                method,
                "application/json",
                ""
            );
        }
    }

    public void Dispose()
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Request finished {Protocol} {Method} - {StatusCode} {ContentLength} {ContentType} {ElapsedMilliseconds}ms",
                "JSON-RPC/2.0",
                _method,
                200,
                "",
                "application/json",
                Stopwatch.GetElapsedTime(_startTimestamp).TotalMilliseconds
            );
        }            
    }
}