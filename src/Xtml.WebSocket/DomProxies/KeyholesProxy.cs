using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Xtml.Dom;
using Xtml.Runtime;
using Xtml.Runtime.Utilities;
using Xtml.WebSocket.JsonRpc;

namespace Xtml.WebSocket;

// Instead of a new-ing up another class, save an instantiation 
// and explicitly implement on Bridge.
public partial class Bridge : IRpcClient, IRpcServer, IKeyholes
{
    public IKeyholes Keyholes => this;

    public void SetValue(byte[] key, in Keyhole keyhole) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].setValue"))
        .Param(in keyhole)
        .AsNotification();

    public void SetValue(byte[] key, Span<Keyhole> keyholes) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].setValue"))
        .Param(keyholes)
        .AsNotification();

    public void SetNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].setNode"))
        .Param(snapshot, keyholes)
        .AsNotification();

    public void SetNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes, ValueTuple<string, byte[]> viewTransitionName) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].setNode"))
        .Param(snapshot, keyholes)
        .Param(viewTransitionName)
        .AsNotification();

    public void SetNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes, ValueTuple<string, int> viewTransitionNameNew, ValueTuple<string, int> viewTransitionNameOld) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].setNode"))
        .Param(snapshot, keyholes)
        .Param(viewTransitionNameNew)
        .Param(viewTransitionNameOld)
        .AsNotification();

    public void PushNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes, byte[] newKey) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].pushNode"))
        .Param(snapshot, keyholes)
        .Param(newKey)
        .AsNotification();

    public void PushNode(Keyhole[] snapshot, byte[] key, Span<Keyhole> keyholes, byte[] newKey, ValueTuple<string, int> viewTransitionName) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].pushNode"))
        .Param(snapshot, keyholes)
        .Param(newKey)
        .Param(viewTransitionName)
        .AsNotification();

    public void PopNode(byte[] key) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].popNode"))
        .AsNotification();
    
    public void PopNode(byte[] key, ValueTuple<string, int> viewTransitionName) => JsonRpcWriter
        .Batch(this)
        .Method(("keyholes['", key, "'].popNode"))
        .Param(viewTransitionName)
        .AsNotification();

    public void DispatchEvent(Action listener)
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

    public async Task DispatchEvent(Func<Task> listener)
    {
        var stopwatch = LogMessageStarted("dispatchEvent");
        try
        {
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

    public async Task DispatchEvent<T>(Func<Event, Task> listener, T @event)
        where T : struct, Event
    {
        var stopwatch = LogMessageStarted("dispatchEvent");
        try
        {
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

    public void Ping(int requestId)
    {
        var stopwatch = LogMessageStarted("ping");

        JsonRpcWriter
            .Batch(this)
            .Result("pong")
            .AsResponse(requestId);

        LogMessageEnded("ping", stopwatch);
    }

    public void Dump()
    {
        var stopwatch = LogMessageStarted("dump");

        var buffer = CaptureSnapshot();
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
