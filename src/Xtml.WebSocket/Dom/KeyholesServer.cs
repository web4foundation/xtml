using Microsoft.Extensions.Logging;
using Xtml.Dom;
using Xtml.Runtime;
using Xtml.Runtime.Utilities;
using Xtml.WebSocket.JsonRpc;
using Xtml.WebSocket.Utilities;

namespace Xtml.WebSocket.BrowserApis;

internal struct KeyholesServer(Bridge bridge, ILogger logger) : IKeyholesServer
{
    private IConsole Console => bridge.Browser.Console.Value;
    private int? _requestId;

    public KeyholesServer AsResponse(int id)
    {
        _requestId = id;
        return this;
    }

    public void DispatchEvent(Action listener)
    {
        try
        {
            using var _ = RpcLogger.LogMessage(logger, "dispatchEvent");
            listener();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error in this event listener: {Message}", ex.Message);
            Console.Error(ex);
        }
    }

    public void DispatchEvent<T>(Action<Event> listener, T @event)
        where T : struct, Event
    {
        try
        {
            using var _ = RpcLogger.LogMessage(logger, "dispatchEvent");
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
        }
    }

    public async Task DispatchEvent(Func<Task> listener)
    {
        try
        {
            using var _ = RpcLogger.LogMessage(logger, "dispatchEvent");
            using var __ = JsonRpcWriter.FlushOnAwait(bridge);
            await listener();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error in this event listener: {Message}", ex.Message);
            Console.Error(ex);
        }
    }

    public async Task DispatchEvent<T>(Func<Event, Task> listener, T @event)
        where T : struct, Event
    {
        try
        {
            using var _ = RpcLogger.LogMessage(logger, "dispatchEvent");
            using var __ = JsonRpcWriter.FlushOnAwait(bridge);
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
        }
    }

    public void Ping()
    {
        using var _ = RpcLogger.LogMessage(logger, "ping");

        if (_requestId is not int id)
        {
            logger.LogError("Cannot send ping response without a request ID.");
            return;
        }
        _requestId = null;

        JsonRpcWriter
            .Batch(bridge)
            .Result("pong")
            .AsResponse(id);
    }

    public void Dump()
    {
        using var _ = RpcLogger.LogMessage(logger, "dump");

        var buffer = bridge.CaptureSnapshot();
        new KeyholeDumper(Console, buffer).Dump();
    }

    public void Benchmark(int? threads)
    {
        using var _ = RpcLogger.LogMessage(logger, "benchmark");

        // TODO: Implement
    }
}