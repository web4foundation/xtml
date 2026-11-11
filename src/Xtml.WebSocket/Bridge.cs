using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Xtml.Runtime.Composers;
using Xtml.WebSocket.Buffers;
using Xtml.Runtime;
using Xtml.Runtime.Utilities;
using Xtml.Templating;
using Xtml.WebSocket.JsonRpc;
using SystemWebSocket = System.Net.WebSockets.WebSocket;
using Xtml.WebSocket.Dom;

namespace Xtml.WebSocket;

internal partial class Bridge
{
    private readonly static ConcurrentDictionary<Guid, Bridge> s_bridges = [];
    private readonly Func<Html> _template;
    private readonly ILogger _logger;
    private readonly Propagation _propagation = new();
    private Keyhole[] _snapshot;
    private readonly Channel<JsonRpcMessageOrAsyncCallback> _eventLoopChannel;
    private readonly Channel<ReadOnlySequence<byte>> _outputChannel;
    private Dictionary<Guid, object>? _pendingRequests = null;

    public Guid WindowId { get; init; }
    public List<EventListener> RootEventListeners { get; init; }
    public TimeSpan RefreshRate { get; set; } = TimeSpan.FromMilliseconds(1000d / 60d); // 60fps
    public bool ForceSequentialAsync { get; set; } = false;
    
    private record struct JsonRpcMessageOrAsyncCallback(
        ReadOnlySequence<byte>? Message,
        SendOrPostCallback? Callback,
        object? State
    );

    private Bridge(Guid windowId, Func<Html> template, List<EventListener> listeners, ILogger logger)
    {
        WindowId = windowId;
        s_bridges[windowId] = this;
        _template = template;
        RootEventListeners = listeners;
        _logger = logger;
        _snapshot = CaptureSnapshot(); // TODO: Should this be passed in as an arg from the byproduct of HtmlKeyComposer?

        _eventLoopChannel = Channel.CreateBounded<JsonRpcMessageOrAsyncCallback>(
            options: new(1000) // TODO: Make this limit configurable?
            {
                AllowSynchronousContinuations = true,
                FullMode = BoundedChannelFullMode.DropWrite,
                SingleReader = true,
                SingleWriter = false,
            },
            itemDropped: messageOrCallback =>
            {
                // TODO: Handle:
                // - Send error to the browser.
                // - Gracefully close WebSocket.
            }
        );

        _outputChannel = Channel.CreateBounded<ReadOnlySequence<byte>>(
            options: new(1000) // TODO: Make this limit configurable?
            {
                AllowSynchronousContinuations = false, // Don't let network jitter impact the event loop.
                FullMode = BoundedChannelFullMode.DropWrite,
                SingleReader = true,
                SingleWriter = false,
            },
            itemDropped: sequence =>
            {
                // TODO: WebSocket is borked?  Handle:
                // - Skip the line and gracefully close it.
                // - Signal this bridge to unregister itself everywhere.
            }
        );
    }

    #if DEBUG
    static Bridge()
    {
        HotReload.UpdateApplicationEvent += updatedTypes =>
        {
            foreach (var bridge in s_bridges.Values)
            {
                bridge.Reconcile();
            }
        };
    }
    #endif

    public static async Task Bind(
        SystemWebSocket webSocket,
        Guid windowId,
        Func<Html> template,
        List<EventListener> listeners,
        ILogger logger,
        CancellationToken shutdown)
    {
        var bridge = new Bridge(windowId, template, listeners, logger);

        using var _ = RegisterShutdown(webSocket, shutdown);

        await Task.WhenAny(
            bridge.WebSocketRead(webSocket, CancellationToken.None),
            bridge.EventLoop(CancellationToken.None),
            bridge.WebSocketWrite(webSocket, CancellationToken.None)
        );

        logger.LogInformation("👋 Goodbye, {WindowId}!", windowId);

        s_bridges.Remove(windowId, out var _);
    }
    
    private async Task WebSocketRead(SystemWebSocket webSocket, CancellationToken cancel = default)
    {
        try
        {
            while (true)
            {
                var (result, message) = await webSocket.ReceiveAsync(cancel);
                if (result.MessageType == WebSocketMessageType.Close)
                    break;
                await _eventLoopChannel.Writer.WriteAsync(new(message, null, null), cancel);
            }
        }
        catch (WebSocketException wex)
        {
            _logger.LogInformation(wex, "WebSocket exception occurred.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred.");
        }
    }

    private async Task EventLoop(CancellationToken cancel = default)
    {
        while (await _eventLoopChannel.Reader.WaitToReadAsync(cancel))
        {
            using var _ = JsonRpcWriter.FlushOnDispose(this);
            while (_eventLoopChannel.Reader.TryRead(out var work))
            {
                if (work.Message is ReadOnlySequence<byte> message)
                {
                    ProcessMessage(message);
                }
                else if (work.Callback is SendOrPostCallback callback)
                {
                    using var __ = JsonRpcWriter.FlushOnAwait(this);
                    callback(work.State);
                }
            }
        }
    }

    private void ProcessMessage(ReadOnlySequence<byte> message)
    {
        bool shouldReturnToPool = true;
        try
        {
            var rpc = new JsonRpcReader(message);
            shouldReturnToPool = rpc switch
            {
                _ when rpc.Method.Length > 0
                    => ProcessRequestMessage(rpc, message),
                _ when rpc.Result.Length > 0 && rpc.IdAsGuid is Guid id
                    => ProcessResponseMessage(rpc, id),
                _ when rpc.Error.Length > 0
                    => ProcessErrorMessage(rpc),
                _ 
                    => throw new InvalidOperationException($"Unexpected message format: {Encoding.UTF8.GetString(message.ToArray())}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error handling JsonRpc message: {Message}", ex.Message);
        }
        finally
        {
            if (shouldReturnToPool)
                message.ReturnToPool();
        }
    }

    private bool ProcessRequestMessage(in JsonRpcReader rpc, ReadOnlySequence<byte> message)
    {
        switch (rpc.Method)
        {
            case var method when method.SequenceEqual("keyholes.dump"u8):
                Keyholes.Dump();
                break;

            case var method when method.SequenceEqual("keyholes.benchmark"u8):
                Keyholes.Benchmark(
                    threads: rpc.Params.GetNextOptionalAsInt()
                );
                break;

            case var method when method.SequenceEqual("keyholes.ping"u8) && rpc.IdAsInt is int id:
                Keyholes.Ping(id);
                break;

            case var method when method.EndsWith("dispatchEvent"u8):
                InvokeEventListener(
                    key: GetKey(method),
                    @event: rpc.Params.GetNextAsEvent(message, Window, _propagation)
                );
                return false; // @event will return the buffer(s) to the pool.

            default:
                _logger.LogError("Unrecognized method: {Method}.  This should be impossible and needs fixing.", Encoding.UTF8.GetString(rpc.Method));
                break;
        }

        return true;
    }

    private bool ProcessResponseMessage(in JsonRpcReader rpc, Guid id)
    {
        if (_pendingRequests?.TryGetValue(id, out var taskCompletionSource) ?? false)
        {
            _pendingRequests.Remove(id);
            var result = rpc.Result.ToArray(); // TODO: Allocates.  Use the SafeSpan?
            switch (taskCompletionSource)
            {
                case TaskCompletionSource<string?> tcs:
                    var isNull = result.SequenceEqual("null"u8);
                    tcs.SetResult(isNull
                        ? null
                        : Encoding.UTF8.GetString(result.AsSpan(1..^1))); // Strip surrounding quotes
                    break;
                case TaskCompletionSource<int> tcs:
                    tcs.SetResult(int.Parse(result));
                    break;
                case TaskCompletionSource<double> tcs:
                    tcs.SetResult(double.Parse(result));
                    break;
            }
        }
        else
        {
            _logger.LogError("No pending request found for result with ID: {Id}", id);
        }
        return true;
    }

    private bool ProcessErrorMessage(in JsonRpcReader rpc)
    {
        return true;
    }

    private void InvokeEventListener(ReadOnlySpan<byte> key, EventProxy @event)
    {
        // Do not handle this event if `stopPropagation()` or `stopImmediatePropagation()`
        // has previously been called on the browser's same event instance.
        if (_propagation.IsStopped)
        {
            @event.Dispose();
            return;
        }

        switch (GetEventListener(key))
        {
            case { Action: { } withoutEvent }:
                @event.Dispose();                                   // Event not needed.  Return buffer to pool early.
                Keyholes.DispatchEvent(withoutEvent);
                break;

            case { ActionEvent: { } withEvent }:
                Keyholes.DispatchEvent(withEvent, @event);
                break;

            case { Func: { } withoutEventAsync }:
                @event.Dispose();                                   // Event not needed.  Return buffer to pool early.
                _ = Keyholes.DispatchEvent(withoutEventAsync);      // Do not await here or it will block the event loop.
                break;

            case { FuncEvent: { } withEventAsync }:
                _ = Keyholes.DispatchEvent(withEventAsync, @event); // Do not await here or it will block the event loop.
                break;

            default:
                @event.Dispose();
                _logger.LogWarning("Event listener not found.  Possible race condition.");
                break;
        }

        // Return early to skip `message.ReturnToPool()` below.  
        // @event may need to survive beyond this point in async flows.
        return;
    }

    private async Task WebSocketWrite(SystemWebSocket webSocket, CancellationToken cancel = default)
    {
        while (!cancel.IsCancellationRequested && webSocket.State == WebSocketState.Open)
        {
            try
            {
                var output = await _outputChannel.Reader.ReadAsync(cancel);
                var bytesRemaining = output.Length;
                if (bytesRemaining == 0)
                    continue;
                
                foreach (ReadOnlyMemory<byte> buffer in output)
                {
                    bytesRemaining -= buffer.Length;
                    await webSocket.SendAsync(
                        buffer: buffer,
                        messageType: WebSocketMessageType.Text,
                        endOfMessage: bytesRemaining == 0,
                        cancellationToken: cancel
                    );
                }

                output.ReturnToPool();
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogInformation("HTTP request was aborted: {Message}", ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error writing to the WebSocket: {Message}", ex.Message);
            }
        }
    }

    public void FlushToWebSocket(ReadOnlySequence<byte> output)
    {
        if (!_outputChannel.Writer.TryWrite(output))
            throw new InvalidOperationException("Failed to write to output channel."); // TODO: How to fail?
    }

    public Task<T> AwaitResult<T>(Guid id)
    {
        if (_pendingRequests?.Count >= 1000)
            throw new InvalidOperationException("Too many pending requests (1000).");

        var tcs = new TaskCompletionSource<T>();
        var pendingRequests = _pendingRequests ??= [];
        pendingRequests[id] = tcs;
        return tcs.Task;
    }

    public void PostToEventLoop(SendOrPostCallback callback, object? state)
    {
        if (!_eventLoopChannel.Writer.TryWrite(new(null, callback, state)))
            throw new InvalidOperationException("Failed to write to the event loop channel.");
    }

    private static ReadOnlySpan<byte> GetKey(ReadOnlySpan<byte> method)
    {
        var split = method.Split((byte)'\'');
        return split.MoveNext() && split.MoveNext()
            ? method[split.Current]
            : [];
    }

    private EventListener GetEventListener(ReadOnlySpan<byte> key)
    {
        using var perf = Perf.Measure("GetEventListener"); // TODO: Remove PerfCheck

        return key switch
        {
            // If the key starts with "win:" or "doc:" that means it's a top-level window or document event.
            // These live in a simple List<> and can be looked up by index.
            _ when key.StartsWith("win:"u8) || key.StartsWith("doc:"u8) =>
                int.TryParse(key[4..], out var index) && index < RootEventListeners.Count
                    ? RootEventListeners[index]
                    : default,
                    
            // Otherwise, it starts with "key" and is bound to some element buried somewhere in the HTML.
            // The only way to find it is to compose the HTML and compare every keyhole to the supplied key.
            _ => FindKeyComposer.Shared.FindEventListener(key, _template),
        };
    }

    public void Reconcile()
    {
        try
        {
            Keyhole[] oldBuffer = _snapshot;
            Keyhole[] newBuffer = CaptureSnapshot();
            Reconciler.Diff(mutator: Keyholes, oldBuffer, newBuffer);

            // Keep this snapshot buffer around to use as the "before" next time.
            _snapshot = newBuffer;
            ArrayPool<Keyhole>.Shared.Return(oldBuffer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error reconciling diffs: {Message}", ex.Message);
        }
    }

    private Keyhole[] CaptureSnapshot()
    {
        using var perf = Perf.Measure("CaptureSnapshot"); // TODO: Remove PerfCheck

        return SnapshotKeyComposer.Shared.Capture(_template);
    }

    private static CancellationTokenRegistration RegisterShutdown(SystemWebSocket webSocket, CancellationToken shutdown) =>
        shutdown.Register(static async state =>
        {
            if (state is SystemWebSocket webSocket)
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Application ended...",
                    CancellationToken.None
                );
        }, webSocket);
}