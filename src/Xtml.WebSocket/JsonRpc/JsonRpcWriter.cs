using System.Text.Encodings.Web;
using System.Text.Json;
using Xtml.WebSocket.Buffers;

namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    [ThreadStatic]
    private static HybridWriter? t_current;
    public record HybridWriter(Utf8JsonWriter Utf8Json, PooledSequenceBufferWriter<byte> Buffer)
    {
        public bool IsBatching { get; set; } = false;
    }

    public static MessageBuilder Batch(Bridge bridge) => new(bridge, GetWriter());

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

        var writer = t_current;
        if (writer.Utf8Json.BytesCommitted + writer.Utf8Json.BytesPending == 0)
            writer.Utf8Json.WriteStartArray();

        return writer;
    }

    public static ResetSynchronizationContextDisposable FlushOnAwait(Bridge bridge)
    {
        var writer = GetWriter();
        writer.IsBatching = true;

        var flusher = new ResetSynchronizationContextDisposable(SynchronizationContext.Current);

        // TODO: SyncCtx allocates memory.  Is it moot to optimize this code path since async will likely allocate regardless?
        SynchronizationContext.SetSynchronizationContext(new SyncCtx(bridge));

        return flusher;
    }

    public static FlushDisposable FlushOnDispose(Bridge bridge)
    {
        var writer = GetWriter();
        writer.IsBatching = true;
        return new FlushDisposable(bridge, writer);
    }

    private static void Flush(Bridge bridge, HybridWriter writer)
    {
        bridge.Invalidate();
        bridge.Reconcile();

        if (writer.Utf8Json.BytesCommitted == 0 && writer.Utf8Json.BytesPending <= 1) // i.e. the `[`
            return;
        
        writer.Utf8Json.WriteEndArray();
        writer.Utf8Json.Flush();

        var output = writer.Buffer.Sequence;
        writer.Utf8Json.Reset();
        writer.IsBatching = false;

        bridge.FlushToWebSocket(output);
    }
}