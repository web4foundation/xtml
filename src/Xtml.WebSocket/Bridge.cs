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

namespace Xtml.WebSocket;

public partial class Bridge
{
    private readonly static ConcurrentDictionary<Guid, Bridge> _windows = [];
    private readonly Func<Html> _template;
    private readonly ILogger _logger;
    private readonly Propagation _propagation = new();
    private bool _isInvalidated;
    private Keyhole[] _snapshot;

    public Guid WindowId { get; init; }
    public List<EventListener> RootEventListeners { get; init; }
    public Channel<ReadOnlySequence<byte>> EventLoopChannel { get; init; }
    public Channel<ReadOnlySequence<byte>> OutputChannel { get; init; }
    public TimeSpan RefreshRate { get; set; } = TimeSpan.FromMilliseconds(1000d / 60d); // 60fps
    
    private Bridge(Guid windowId, Func<Html> template, List<EventListener> listeners, ILogger logger)
    {
        _windows[windowId] = this;
        WindowId = windowId;
        _template = template;
        RootEventListeners = listeners;
        _logger = logger;
        _isInvalidated = false;
        _snapshot = CaptureSnapshot(); // TODO: Should this be passed in as an arg from the byproduct of HtmlKeyComposer?

        EventLoopChannel = Channel.CreateBounded<ReadOnlySequence<byte>>(
            options: new(1000) // TODO: Make this limit configurable?
            {
                AllowSynchronousContinuations = true,
                FullMode = BoundedChannelFullMode.DropWrite,
                SingleReader = true,
                SingleWriter = false,
            },
            itemDropped: sequence =>
            {
                // TODO: Handle:
                // - Send error to the browser.
                // - Gracefully close WebSocket.
            }
        );

        OutputChannel = Channel.CreateBounded<ReadOnlySequence<byte>>(
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
            foreach (var bridge in _windows.Values)
            {
                bridge.Invalidate();
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

        _windows.Remove(windowId, out var _);
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
                await EventLoopChannel.Writer.WriteAsync(message, cancel);
            }
        }
        catch (WebSocketException wex)
        {
            _logger.LogError(wex, "WebSocket error occurred.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred.");
        }
    }

    private async Task EventLoop(CancellationToken cancel = default)
    {
        var lastRun = Stopwatch.StartNew();
        while (!cancel.IsCancellationRequested)
        {
            // Debounce! 
            // If the last batch was less than 16ms ago (60 fps), wait until the "next frame"
            // (i.e. no need to update the DOM more frequently than the screen's own refresh rate).
            // There could be potentially millions of state changes every 16ms
            // and it'd be wasteful to calculate diffs or send mutations for each
            // when most screens can only handle 60-144 Hz.
            if (RefreshRate > TimeSpan.Zero)
            {
                // TODO: I don't like how it awaits Task.Delay on the first run.
                var timeRemaining = RefreshRate - lastRun.Elapsed;
                if (timeRemaining > TimeSpan.Zero)
                    await Task.Delay(timeRemaining, cancel);
                lastRun.Restart();
            }

            // If here, this connection has a green light to do work again so await 
            // until a message is received.  If multiple messages have been 
            // queued while awaiting or working, run them all before awaiting again.
            // If this batch takes longer than 16ms to run, the next iteration will 
            // run immediately, same thread, no awaiting.
            // Always remember to flush.  If any async work was kicked off, 
            // JsonRpcWriter's SyncCtx will flush every time it awaits.
            while (await EventLoopChannel.Reader.WaitToReadAsync(cancel))
                using (_ = JsonRpcWriter.FlushWhenDone())
                    while (EventLoopChannel.Reader.TryRead(out var message))
                        Process(message);
        }
    }

    private void Process(ReadOnlySequence<byte> message)
    {
        try
        {
            var rpc = new JsonRpcReader(message);

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
                    var @event = rpc.Params.GetNextAsEvent(message, Window, _propagation);

                    // Do not handle this event if `stopPropagation()` or `stopImmediatePropagation()`
                    // has previously been called on the browser's same event instance.
                    if (_propagation.IsStopped)
                    {
                        @event.Dispose();
                        return;
                    }

                    switch (GetEventListener(key: GetKey(method)))
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
                            return;
                    }

                    // Return early to skip `message.ReturnToPool()` below.  
                    // @event may need to survive beyond this point in async flows.
                    return;

                default:
                    _logger.LogError("Unrecognized method: {Method}.  This should be impossible and needs fixing.", Encoding.UTF8.GetString(rpc.Method));
                    break;
            }

            message.ReturnToPool();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error handling JsonRpc message: {Message}", ex.Message);
        }
    }

    private async Task WebSocketWrite(SystemWebSocket webSocket, CancellationToken cancel = default)
    {
        while (!cancel.IsCancellationRequested && webSocket.State == WebSocketState.Open)
        {
            try
            {
                var output = await OutputChannel.Reader.ReadAsync(cancel);


                var bytesRemaining = output.Length;
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

    public void Invalidate()
    {
        if (_isInvalidated)
            return;
        _isInvalidated = true;
    }

    private void Reconcile()
    {
        if (!_isInvalidated)
        {
            _logger.LogWarning("Cancelling Reconcile() because IsInvalidated is false (which is unexpected and should be investigated).");
            return;
        }

        if (_snapshot is null)
        {
            _logger.LogWarning("Cancelling Reconcile() because snapshot is null (which is unexpected and should be investigated).");
            return;
        }

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

        _isInvalidated = false;
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