using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Xtml.Dom;
using Xtml.Runtime;
using Xtml.Runtime.Utilities;
using Xtml.WebSocket.JsonRpc;

namespace Xtml.WebSocket.DocumentObjectModel;

internal struct KeyholesServer(Bridge bridge, ILogger logger) : IKeyholesServer
{
    IConsole Console => new Dom(bridge);

    public void DispatchEvent(Action listener)
    {
        var stopwatch = LogMessageStarted("dispatchEvent");
        try
        {
            listener();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error in this event listener: {Message}", ex.Message);
            Console.Error(ex);
        }
        finally
        {
            LogMessageEnded("dispatchEvent", stopwatch);
        }
    }

    public void DispatchEvent<T>(Action<Event> listener, T @event)
        where T : struct, Event
    {
        var stopwatch = LogMessageStarted("dispatchEvent");
        try
        {
            // TODO: Memory allocation casting from TEvent (struct) to Event interface.
            listener(@event);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error in this event listener: {Message}", ex.Message);
            Console.Error(ex);
        }
        finally
        {
            // Return buffer(s) to the pool
            @event.Dispose();
            LogMessageEnded("dispatchEvent", stopwatch);
        }
    }

    public async Task DispatchEvent(Func<Task> listener)
    {
        var stopwatch = LogMessageStarted("dispatchEvent");
        try
        {
            using var _ = JsonRpcWriter.FlushOnAwait(bridge);
            await listener();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error in this event listener: {Message}", ex.Message);
            Console.Error(ex);
        }
        finally
        {
            LogMessageEnded("dispatchEvent", stopwatch);
        }
    }

    public async Task DispatchEvent<T>(Func<Event, Task> listener, T @event)
        where T : struct, Event
    {
        var stopwatch = LogMessageStarted("dispatchEvent");
        try
        {
            using var _ = JsonRpcWriter.FlushOnAwait(bridge);
            // TODO: Memory allocation casting from TEvent (struct) to Event interface.
            await listener(@event);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error in this event listener: {Message}", ex.Message);
            Console.Error(ex);
        }
        finally
        {
            // Return buffer(s) to the pool
            @event.Dispose();
            LogMessageEnded("dispatchEvent", stopwatch);
        }
    }

    public async Task<string> Ping()
    {
        var stopwatch = LogMessageStarted("ping");
        LogMessageEnded("ping", stopwatch);
        return "pong";
    }

    public void Dump()
    {
        var stopwatch = LogMessageStarted("dump");

        var buffer = bridge.CaptureSnapshot();
        new KeyholeDumper(Console, buffer).Dump();

        LogMessageEnded("dump", stopwatch);
    }

    public void Benchmark(int? threads)
    {
        var stopwatch = LogMessageStarted("benchmark");

        // TODO: Implement

        LogMessageEnded("benchmark", stopwatch);
    }

    private Stopwatch? LogMessageStarted(string method)
    {
        // TODO: Temporary until a proper IHttpApplication is ready to be built and include proper diagnostics.

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
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

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
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