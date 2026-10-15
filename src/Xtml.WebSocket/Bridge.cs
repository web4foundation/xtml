using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xtml.Runtime.Composers;
using Xtml.Dom;
using Xtml.WebSocket.Buffers;
using Xtml.Runtime;
using Xtml.WebSocket.Dom;
using Xtml.Runtime.Utilities;
using Xtml.Templating;

namespace Xtml.WebSocket;

public partial class Bridge
{
    private static readonly ConcurrentDictionary<Guid, Bridge> _windows = [];
    public static SnapshotStrategy SnapshotStrategy { get; set; } = SnapshotStrategy.Retain;

    private Keyhole[]? _snapshot = null;
    private bool _isInvalidated = false;
    private readonly Propagation _propagation = new();
    
    private TimeSpan _diffInterval = TimeSpan.FromMilliseconds(1000d / 60d); // 60fps
    private readonly Channel<int> _diffChannel = CreateDiffChannel();
    private readonly Channel<ReadOnlySequence<byte>> _outputChannel = CreateOutputChannel();
    private JsonRpcWriter JsonRpc => JsonRpcWriter.Current(_outputChannel.Writer, listeners);
    private Func<Html> _template;
    private List<EventListener> _listeners;
    private ILogger _logger;
    public Bridge(Func<Html> template, List<EventListener> listeners, ILogger logger)
    {
        _template = template;
        _listeners = listeners;
        _logger = logger;
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

        // HotReload only works in Retain mode.
        SnapshotStrategy = SnapshotStrategy.Retain;
    }
    #endif

    public static async Task Bind(
        System.Net.WebSockets.WebSocket webSocket,
        Guid windowId,
        Func<Html> template,
        List<EventListener> listeners,
        ILogger logger,
        CancellationToken shutdown)
    {
        var bridge = new Bridge(template, listeners, logger);
        if (SnapshotStrategy == SnapshotStrategy.Retain)
            bridge._snapshot ??= bridge.CaptureSnapshot();

        using var _ = RegisterShutdown(webSocket, shutdown);

        _windows[windowId] = bridge;

        await Task.WhenAny(
            bridge.OutputToWebSocket(webSocket, CancellationToken.None),
            bridge.WebSocketToMethod(webSocket, CancellationToken.None),
            bridge.DebounceDiffs(CancellationToken.None)
        );

        logger.LogInformation("👋 Goodbye, {WindowId}!", windowId);

        _windows.Remove(windowId, out var _);
    }

    private static Channel<int> CreateDiffChannel() => Channel.CreateBounded<int>(new BoundedChannelOptions(1)
    {
        AllowSynchronousContinuations = true,
        SingleReader = true,
        SingleWriter = false,
        FullMode = BoundedChannelFullMode.DropWrite
    });

    private static Channel<ReadOnlySequence<byte>> CreateOutputChannel() => Channel.CreateBounded<ReadOnlySequence<byte>>(
        options: new BoundedChannelOptions(1000) // TODO: Make this limit configurable?
        {
            AllowSynchronousContinuations = true,
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

    private async Task OutputToWebSocket(System.Net.WebSockets.WebSocket webSocket, CancellationToken cancel = default)
    {
        while (!cancel.IsCancellationRequested && webSocket.State == WebSocketState.Open)
        {
            try
            {
                var sequence = await _outputChannel.Reader.ReadAsync(cancel);
                var bytesRemaining = sequence.Length;
                foreach (var memory in sequence)
                {
                    bytesRemaining -= memory.Length;
                    await webSocket.SendAsync(
                        buffer: memory,
                        messageType: WebSocketMessageType.Text,
                        endOfMessage: bytesRemaining == 0,
                        cancellationToken: cancel
                    );
                }

                sequence.ReturnToPool();
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

    private async Task WebSocketToMethod(System.Net.WebSockets.WebSocket webSocket, CancellationToken cancel = default)
    {
        const int RECEIVE_BUFFER_LENGTH = 4096;
        ReadOnlySequence<byte> sequence;
        while (true)
        {
            try
            {
                var buffer = ArrayPool<byte>.Shared.Rent(RECEIVE_BUFFER_LENGTH);
                var result = await webSocket.ReceiveAsync(buffer, cancel);

                using var perf = Perf.Measure("WebSocketToMethod (loop)"); // TODO: Remove PerfCheck

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                    break;
                }
                else if (result.EndOfMessage)
                {
                    sequence = new ReadOnlySequence<byte>(buffer, 0, result.Count);
                }
                else
                {
                    // TODO: new SequenceSegment() allocates memory
                    var segmentStart = new SequenceSegment<byte>(buffer, 0..result.Count);
                    var segmentEnd = segmentStart;
                    while (!result.EndOfMessage)
                    {
                        buffer = ArrayPool<byte>.Shared.Rent(RECEIVE_BUFFER_LENGTH);
                        result = await webSocket.ReceiveAsync(buffer, cancel);
                        segmentEnd = segmentEnd.Append(buffer, 0..result.Count);
                    }
                    sequence = new ReadOnlySequence<byte>(segmentStart, 0, segmentEnd, result.Count);
                }
            }
            catch (WebSocketException)
            {
                break;
            }
            catch (Exception)
            {
                break;
            }

            if (HandleJsonRpcMessage(sequence) is ReadOnlySequence<byte> toReturn)
                toReturn.ReturnToPool();
        }
    }

    private async Task DebounceDiffs(CancellationToken cancel = default)
    {
        // TODO: I don't like how it awaits Task.Delay on the first run.

        // TODO: Verify that Stopwatch isn't allocating due to inlining, escape analysis, and stack allocation.
        // https://github.com/dotnet/runtime/pull/111834
        var lastUpdate = Stopwatch.StartNew();
        while (!cancel.IsCancellationRequested)
        {
            // Debounce!  If the last update was less than 16 ms ago (60 fps),
            // then wait until the "next frame" before updating again
            // (i.e. no need to run Update() more frequently than the screen's refresh rate).
            // There could be (potentially) millions of state changes every 16 ms
            // and it'd be wasteful to run diffs or send mutations for each
            // when most screens can only handle 60 Hz.
            var timeUntilNextUpdate = _diffInterval.Subtract(lastUpdate.Elapsed);
            if (timeUntilNextUpdate > TimeSpan.Zero)
                await Task.Delay(timeUntilNextUpdate, cancel);
            lastUpdate.Restart();

            // If here, this app has a green light to do work again 
            // so await until an update is requested.  
            _ = await _diffChannel.Reader.ReadAsync(cancel);
            Reconcile();
        }
    }

    private ReadOnlySequence<byte>? HandleJsonRpcMessage(ReadOnlySequence<byte> sequence)
    {
        // TODO: This doesn't belong here.
        // TODO: Once you figure out where they belong, they need to be in try/catches.
        foreach (var t in _windows.Values)
            t.Invalidate();

        try
        {
            var rpc = new JsonRpcReader(sequence);
            var @params = rpc.ReadPositionalParams();

            switch (rpc.Method)
            {
                case var method when method.SequenceEqual("keyholes.dump"u8):
                    {
                        using var batchOutput = JsonRpc.BatchThisScope();
                        Keyholes.Dump();
                        break;
                    }

                case var method when method.SequenceEqual("keyholes.benchmark"u8):
                    {
                        using var batchOutput = JsonRpc.BatchThisScope();
                        var threads = @params.GetNextOptionalAsInt();
                        Keyholes.Benchmark(threads);
                        break;
                    }

                case var method when method.SequenceEqual("keyholes.ping"u8) && rpc.IdAsInt is int id:
                    {
                        Keyholes.Ping();
                        JsonRpc.WriteResponse(id);
                        break;
                    }

                case var method when method.EndsWith("dispatchEvent"u8):
                    {
                        var eventSequence = @params.GetNextAsSequence();
                        _propagation.CurrentID = @params.GetNextAsInt();
                        _propagation.CurrentLevel = @params.GetNextOptionalAsInt() ?? 0;

                        // Do not handle this event if `stopPropagation()` or `stopImmediatePropagation()`
                        // has previously been called on the browser's same event instance.
                        if (_propagation.IsStopped)
                            return sequence;

                        DispatchEvent(sequence, eventSequence, GetKey(method));

                        // Return nothing.  DispatchEvent is responsible for returning the buffer(s) to the pool.
                        return null;
                    }

                default:
                    _logger.LogError("Unrecognized method: {Method}.  This should be impossible and needs fixing.", Encoding.UTF8.GetString(rpc.Method));
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error handling JsonRpc message: {Message}", ex.Message);
        }
        finally
        {
            // TODO: This doesn't belong here.
            foreach (var t in _windows.Values)
                t.Update();
        }
        return sequence;
    }

    private void DispatchEvent(ReadOnlySequence<byte> sequence, ReadOnlySequence<byte> eventSequence, ReadOnlySpan<byte> key)
    {
        var eventListener = GetEventListener(key);

        if (eventListener.Action is Action listener)
        {
            using var batchOutput = JsonRpc.BatchThisScope();

            // Done with buffer(s), return to pool early.
            sequence.ReturnToPool();

            Keyholes.DispatchEvent(listener);
        }
        else if (eventListener.ActionEvent is Action<Event> listenerWithEvent)
        {
            using var batchOutput = JsonRpc.BatchThisScope();

            Keyholes.DispatchEvent(
                listenerWithEvent,
                new EventProxy(sequence, eventSequence, this, _propagation)
            // LazyEvent will return buffer(s) to the pool after it completes.
            );
        }
        else if (eventListener.Func is Func<Task> listenerAsync)
        {
            using var batchOutput = JsonRpc.BatchThisScope(continueOnCapturedContext: true);

            // Done with buffer(s), return to pool early.
            sequence.ReturnToPool();

            // Do not await event listeners here!  That would block the WebSocket reader.
            _ = Keyholes.DispatchEvent(listenerAsync);
        }
        else if (eventListener.FuncEvent is Func<Event, Task> listenerWithEventAsync)
        {
            using var batchOutput = JsonRpc.BatchThisScope(continueOnCapturedContext: true);

            // Do not await event listeners here!  That would block the WebSocket reader.
            _ = Keyholes.DispatchEvent(
                listenerWithEventAsync,
                new EventProxy(sequence, eventSequence, this, _propagation)
            // LazyEvent will return buffer(s) to the pool after it completes.
            );
        }
        else
        {
            sequence.ReturnToPool();
            _logger.LogWarning("Event listener not found for key: {Key}.  Possible race condition.", Encoding.UTF8.GetString(key));
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
                int.TryParse(key[4..], out var index) && index < _listeners.Count
                    ? _listeners[index]
                    : default,
                    
            // Otherwise, it starts with "key" and is bound to some element buried somewhere in the HTML.
            // The only way to find it is to compose the HTML and compare every keyhole to the supplied key.
            _ => FindKeyComposer.Shared.FindEventListener(key, _template),
        };
    }

    private static CancellationTokenRegistration RegisterShutdown(
        System.Net.WebSockets.WebSocket webSocket, 
        CancellationToken shutdown)
    {
        return shutdown.Register(static async state =>
        {
            if (state is System.Net.WebSockets.WebSocket webSocket)
                await webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Application ended...",
                    CancellationToken.None
                );
        }, webSocket);
    }

    public void SetRefreshRate(int milliseconds)
    {
        _diffInterval = TimeSpan.FromMilliseconds(milliseconds);
    }

    private Keyhole[] CaptureSnapshot()
    {
        using var perf = Perf.Measure("CaptureSnapshot"); // TODO: Remove PerfCheck

        return SnapshotKeyComposer.Shared.Capture(_template);
    }

    public void Invalidate()
    {
        if (_isInvalidated)
            return;

        _isInvalidated = true;

        // This only does work when SnapshotStrategy is in "Recapture" mode
        // since snapshot is never set to null while operating in "Retain" mode.
        _snapshot ??= CaptureSnapshot();
    }

    public void Update()
    {
        if (!_isInvalidated)
            return;

        while (!_diffChannel.Writer.TryWrite(0)) ;
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
            using (var batchOutput = JsonRpc.BatchThisScope())
            {
                Reconciler.Diff(mutator: Keyholes, oldBuffer, newBuffer);
            }

            var pool = ArrayPool<Keyhole>.Shared;
            switch (SnapshotStrategy)
            {
                case SnapshotStrategy.Recapture:
                    // Do not keep this snapshot buffer for later.
                    _snapshot = null;
                    pool.Return(oldBuffer);
                    pool.Return(newBuffer);
                    break;
                case SnapshotStrategy.Retain:
                    // Keep this snapshot buffer around to use as the "before" next time.
                    _snapshot = newBuffer;
                    pool.Return(oldBuffer);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error reconciling diffs: {Message}", ex.Message);
        }

        _isInvalidated = false;
    }
}