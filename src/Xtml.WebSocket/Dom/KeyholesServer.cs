using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Xtml.Dom;
using Xtml.Runtime;
using Xtml.Runtime.Utilities;
using Xtml.WebSocket.JsonRpc;

namespace Xtml.WebSocket.DocumentObjectModel;

// Instead of a new-ing up another class, save an instantiation 
// and explicitly implement on Bridge.
internal partial class Bridge : IKeyholesServer
{
    void IKeyholesServer.DispatchEvent(Action listener)
    {
        var stopwatch = LogMessageStarted("dispatchEvent");
        try
        {
            listener();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error in this event listener: {Message}", ex.Message);
            Console.Error(ex);
        }
        finally
        {
            LogMessageEnded("dispatchEvent", stopwatch);
        }
    }

    void IKeyholesServer.DispatchEvent<T>(Action<Event> listener, T @event)
        where T : struct
    {
        var stopwatch = LogMessageStarted("dispatchEvent");
        try
        {
            // TODO: Memory allocation casting from TEvent (struct) to Event interface.
            listener(@event);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error in this event listener: {Message}", ex.Message);
            Console.Error(ex);
        }
        finally
        {
            // Return buffer(s) to the pool
            @event.Dispose();
            LogMessageEnded("dispatchEvent", stopwatch);
        }
    }

    async Task IKeyholesServer.DispatchEvent(Func<Task> listener)
    {
        var stopwatch = LogMessageStarted("dispatchEvent");
        try
        {
            using var _ = JsonRpcWriter.FlushOnAwait(this);
            await listener();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error in this event listener: {Message}", ex.Message);
            Console.Error(ex);
        }
        finally
        {
            LogMessageEnded("dispatchEvent", stopwatch);
        }
    }

    async Task IKeyholesServer.DispatchEvent<T>(Func<Event, Task> listener, T @event)
        where T : struct
    {
        var stopwatch = LogMessageStarted("dispatchEvent");
        try
        {
            using var _ = JsonRpcWriter.FlushOnAwait(this);
            // TODO: Memory allocation casting from TEvent (struct) to Event interface.
            await listener(@event);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error in this event listener: {Message}", ex.Message);
            Console.Error(ex);
        }
        finally
        {
            // Return buffer(s) to the pool
            @event.Dispose();
            LogMessageEnded("dispatchEvent", stopwatch);
        }
    }

    async Task<string> IKeyholesServer.Ping()
    {
        var stopwatch = LogMessageStarted("ping");
        LogMessageEnded("ping", stopwatch);
        return "pong";
    }

    void IKeyholesServer.Dump()
    {
        var stopwatch = LogMessageStarted("dump");

        var buffer = CaptureSnapshot();
        new KeyholeDumper(Console, buffer).Dump();

        LogMessageEnded("dump", stopwatch);
    }

    void IKeyholesServer.Benchmark(int? threads)
    {
        var stopwatch = LogMessageStarted("benchmark");

        // TODO: Implement

        LogMessageEnded("benchmark", stopwatch);
    }

    private Stopwatch? LogMessageStarted(string method)
    {
        // TODO: Temporary until a proper IHttpApplication is ready to be built and include proper diagnostics.

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Request starting {Protocol} {Method} - {ContentType} {ContentLength}",
                "JSON-RPC/2.0",
                method,
                "application/json",
                ""
            );

            // TODO: Memory allocation!
            return Stopwatch.StartNew();
        }
        return null;
    }

    private void LogMessageEnded(string method, Stopwatch? stopwatch)
    {
        // TODO: Temporary until a proper IHttpApplication is ready to be built and include proper diagnostics.

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Request finished {Protocol} {Method} - {StatusCode} {ContentLength} {ContentType} {ElapsedMilliseconds}ms",
                "JSON-RPC/2.0",
                method,
                200,
                "",
                "application/json",
                stopwatch?.Elapsed.TotalMilliseconds
            );
        }
    }
}