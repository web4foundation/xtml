using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Xtml.Dom;
using Xtml.Runtime;
using Xtml.Runtime.Utilities;
using Xtml.WebSocket.JsonRpc;

namespace Xtml.WebSocket;

// Instead of a new-ing up another class, save an instantiation 
// and explicitly implement on Bridge.
internal partial class Bridge : IRpcClient, IRpcServer, IKeyholes
{
    public IKeyholes Keyholes => this;

    void IRpcClient.SetValue(byte[] key, in Keyhole keyhole) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].setValue"))
        .Param(in keyhole)
        .AsNotification();

    void IRpcClient.SetValue(byte[] key, Span<Keyhole> keyholes) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].setValue"))
        .Param(keyholes)
        .AsNotification();

    void IRpcClient.SetNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].setNode"))
        .Param(snapshot, keyholes)
        .AsNotification();

    void IRpcClient.SetNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes, ValueTuple<string, byte[]> viewTransitionName) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].setNode"))
        .Param(snapshot, keyholes)
        .Param(viewTransitionName)
        .AsNotification();

    void IRpcClient.SetNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes, ValueTuple<string, int> viewTransitionNameNew, ValueTuple<string, int> viewTransitionNameOld) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].setNode"))
        .Param(snapshot, keyholes)
        .Param(viewTransitionNameNew)
        .Param(viewTransitionNameOld)
        .AsNotification();

    void IRpcClient.PushNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes, byte[] newKey) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].pushNode"))
        .Param(snapshot, keyholes)
        .Param(newKey)
        .AsNotification();

    void IRpcClient.PushNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes, byte[] newKey, ValueTuple<string, int> viewTransitionName) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].pushNode"))
        .Param(snapshot, keyholes)
        .Param(newKey)
        .Param(viewTransitionName)
        .AsNotification();

    void IRpcClient.PopNode(byte[] key) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].popNode"))
        .AsNotification();
    
    void IRpcClient.PopNode(byte[] key, ValueTuple<string, int> viewTransitionName) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].popNode"))
        .Param(viewTransitionName)
        .AsNotification();

    void IRpcServer.DispatchEvent(Action listener)
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

    void IRpcServer.DispatchEvent<T>(Action<Event> listener, T @event)
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

    async Task IRpcServer.DispatchEvent(Func<Task> listener)
    {
        var stopwatch = LogMessageStarted("dispatchEvent");
        try
        {
            JsonRpcWriter.FlushOnAwait(this);
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

    async Task IRpcServer.DispatchEvent<T>(Func<Event, Task> listener, T @event)
        where T : struct
    {
        var stopwatch = LogMessageStarted("dispatchEvent");
        try
        {
            JsonRpcWriter.FlushOnAwait(this);
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

    void IRpcServer.Ping(int requestId)
    {
        var stopwatch = LogMessageStarted("ping");

        JsonRpcWriter
            .Batch(this)
            .Result("pong")
            .AsResponse(requestId);

        LogMessageEnded("ping", stopwatch);
    }

    void IRpcServer.Dump()
    {
        var stopwatch = LogMessageStarted("dump");

        var buffer = CaptureSnapshot();
        new KeyholeDumper(Console, buffer).Dump();

        LogMessageEnded("dump", stopwatch);
    }

    void IRpcServer.Benchmark(int? threads)
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
