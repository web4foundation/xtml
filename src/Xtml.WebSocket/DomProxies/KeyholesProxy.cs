using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Xtml.Dom;
using Xtml.Runtime;
using Xtml.Runtime.Utilities;

namespace Xtml.WebSocket;

// Instead of a new-ing up another class, save an instantiation 
// and explicitly implement on Bridge.
public partial class Bridge : IRpcClient, IRpcServer, IKeyholes
{
    public IKeyholes Keyholes => this;

    public void SetValue(byte[] key, ref Keyhole keyhole)
    {
        JsonRpc.WriteNotification(
            method: ("keyholes['", key, "'].setValue"),
            param1: ref keyhole
        );
    }

    public void SetValue(byte[] key, Span<Keyhole> keyholes)
    {
        JsonRpc.WriteNotification(
            method: ("keyholes['", key, "'].setValue"),
            param1: keyholes
        );
    }

    public void SetNode(Keyhole[] buffer, byte[] key, Span<Keyhole> keyholes, ValueTuple<string, int>? viewTransitionNameNew = null, ValueTuple<string, int>? viewTransitionNameOld = null)
    {
        if (viewTransitionNameNew is not null && viewTransitionNameOld is not null)
            JsonRpc.WriteNotification(buffer,
                method: ("keyholes['", key, "'].setNode"),
                param1: keyholes,
                param2: viewTransitionNameNew.Value,
                param3: viewTransitionNameOld.Value
            );
        else
            JsonRpc.WriteNotification(buffer,
                method: ("keyholes['", key, "'].setNode"),
                param1: keyholes
            );
    }

    public void SetNode(Keyhole[] buffer, byte[] key, Span<Keyhole> keyholes, ValueTuple<string, byte[]> viewTransitionName)
    {
        JsonRpc.WriteNotification(buffer,
            method: ("keyholes['", key, "'].setNode"),
            param1: keyholes,
            param2: viewTransitionName
        );
    }

    public void PushNode(Keyhole[] buffer, byte[] key, Span<Keyhole> keyholes, byte[] newKey)
    {
        JsonRpc.WriteNotification(buffer,
            method: ("keyholes['", key, "'].pushNode"),
            param1: keyholes,
            param2: newKey
        );
    }

    public void PushNode(Keyhole[] buffer, byte[] key, Span<Keyhole> keyholes, byte[] newKey, ValueTuple<string, int> viewTransitionName)
    {
        JsonRpc.WriteNotification(buffer,
            method: ("keyholes['", key, "'].pushNode"),
            param1: keyholes,
            param2: newKey,
            param3: viewTransitionName
        );
    }

    public void PopNode(byte[] key)
    {
        JsonRpc.WriteNotification(
            method: ("keyholes['", key, "'].popNode")
        );
    }

    public void PopNode(byte[] key, ValueTuple<string, int> viewTransitionName)
    {
        JsonRpc.WriteNotification(
            method: ("keyholes['", key, "'].popNode"),
            param1: viewTransitionName
        );
    }

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

    public void Ping()
    {
        var stopwatch = LogMessageStarted("ping");
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
