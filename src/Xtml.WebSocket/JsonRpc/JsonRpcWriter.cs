using System.Text.Encodings.Web;
using System.Text.Json;
using System.Diagnostics;
using Xtml.WebSocket.Buffers;

namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    // TODO: Make this configurable?
    private const int COURTESY_FLUSH_THRESHOLD = 2 << 15; // 64k

    [ThreadStatic]
    private static HybridWriter? t_current;
    public record HybridWriter(Utf8JsonWriter Utf8Json, PooledSequenceBufferWriter<byte> Buffer)
    {
        public bool IsBatching { get; set; } = false;
        public Stopwatch LastFlush { get; set; } = Stopwatch.StartNew();
    }

    public static MessageBuilder Batch(Bridge bridge)
    {
        var writer = GetWriter();
        if (writer.Utf8Json.BytesCommitted + writer.Utf8Json.BytesPending == 0)
            writer.Utf8Json.WriteStartArray();
        return new(bridge, writer);
    }

    private static HybridWriter GetWriter()
    {
        if (t_current == null)
        {
            var bufferWriter = new PooledSequenceBufferWriter<byte>();
            var utf8JsonWriter = new Utf8JsonWriter(bufferWriter, new () { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            // It's OK to relax escaping since WebSockets are ALWAYS UTF-8 (when not binary). ^
            // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/character-encoding
            t_current = new HybridWriter(utf8JsonWriter, bufferWriter);
        }
        return t_current;
    }

    public static FlushDisposable FlushOnDispose(Bridge bridge)
    {
        var writer = GetWriter();
        writer.IsBatching = true;
        writer.LastFlush.Restart();
        return new FlushDisposable(bridge, writer);
    }

    public static ResetSynchronizationContextDisposable FlushOnAwait(Bridge bridge)
    {
        var flusher = new ResetSynchronizationContextDisposable(SynchronizationContext.Current);
        SynchronizationContext.SetSynchronizationContext(new SyncCtx(bridge)); // Allocation, but moot because async
        return flusher;
    }

    private static void Flush(Bridge bridge, HybridWriter writer)
    {
        if (writer.Utf8Json.BytesCommitted == 0 && writer.Utf8Json.BytesPending <= 1) // i.e. the `[`
            return; // TODO: Verify how frequent this is.  I do know it happens for every async event listener.  Anywhere else?
        
        writer.Utf8Json.WriteEndArray();
        writer.Utf8Json.Flush();

        var output = writer.Buffer.GetSequenceAndReset();
        writer.Utf8Json.Reset();

        bridge.FlushToWebSocket(output);

        writer.LastFlush.Restart();
    }

    private static void CourtesyFlush(Bridge bridge, HybridWriter writer)
    {
        bool isUnbatched = !writer.IsBatching;
        bool tooSlow = writer.LastFlush.Elapsed > bridge.RefreshRate;
        bool tooBig = writer.Utf8Json.BytesCommitted + writer.Utf8Json.BytesPending > COURTESY_FLUSH_THRESHOLD;

        if (isUnbatched || tooSlow || tooBig)
            Flush(bridge, writer);
    }
}