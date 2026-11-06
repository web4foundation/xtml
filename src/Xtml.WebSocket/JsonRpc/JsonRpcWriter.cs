using System.Buffers;
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
    public static MessageBuilder Batch(Bridge bridge)
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
        return new MessageBuilder(bridge, writer);
    }

    public ref partial struct MessageBuilder(Bridge bridge, HybridWriter writer)
    {
        public void FlushOnAwait()
        {
            writer.IsBatching = true;
            // TODO: SyncCtx allocates memory.  Is it moot to optimize this code path since async will likely allocate regardless?
            SynchronizationContext.SetSynchronizationContext(new SyncCtx(bridge));
        }

        public FlushDisposable FlushOnDispose()
        {
            writer.IsBatching = true;
            return new FlushDisposable(bridge, writer);
        }

        // Intentionally a ref struct here so it cannot be used across an await boundary.
        public ref struct FlushDisposable(Bridge bridge, HybridWriter writer) : IDisposable
        {
            public void Dispose()
            {
                Flush(bridge, writer);
            }
        }
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

        bridge.Flush(output);
    }
}