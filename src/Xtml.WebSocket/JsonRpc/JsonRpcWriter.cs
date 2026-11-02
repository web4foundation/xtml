namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    public static MessageBuilder Batch(Bridge bridge)
    {
        var writer = HybridWriter.Get();
        if (writer.Buffer.WrittenCount + writer.Utf8Json.BytesPending == 0)
            writer.Utf8Json.WriteStartArray();
        return new MessageBuilder(bridge, writer);
    }

    public ref partial struct MessageBuilder(Bridge bridge, HybridWriter writer)
    {
        public void FlushOnAwait()
        {
            writer.Flushing = Flushing.OnDispose;
            SynchronizationContext.SetSynchronizationContext(bridge);
        }

        public FlushDisposable FlushOnDispose()
        {
            writer.Flushing = Flushing.OnDispose;
            return new FlushDisposable(bridge, writer);
        }

        // Intentionally a ref struct here so it cannot be used across an await boundary.
        public ref struct FlushDisposable(Bridge bridge, HybridWriter writer) : IDisposable
        {
            public void Dispose()
            {
                writer.Flush(bridge);
            }
        }
    }
}