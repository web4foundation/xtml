namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    // TODO: Make this configurable?
    private const int COURTESY_FLUSH_THRESHOLD = 2 << 15; // 64k

    public static MessageBuilder Current(Bridge bridge) => new(bridge, t_current ?? Batch.Get());
    public static MessageBuilder Current(Bridge bridge, Batch batch) => new(bridge, batch);

    private static void Flush(Bridge bridge, Batch batch)
    {
        batch.State = BatchState.Flushing;
        bridge.FlushToWebSocket(batch);
        t_current = null;
    }

    private static void CourtesyFlush(Bridge bridge, Batch batch)
    {
        switch (batch.State)
        {
            case BatchState.Flushing:
                // Already flushing.  Don't flush again.
                return;

            case BatchState.Immediate:
                Flush(bridge, batch);
                break;

            case BatchState.Batching:
                bool tooSlow = bridge.LastFlush.Elapsed > bridge.RefreshRate;
                bool tooBig = batch.Utf8Json.BytesCommitted + batch.Utf8Json.BytesPending > COURTESY_FLUSH_THRESHOLD;

                if (tooSlow || tooBig)
                {
                    Flush(bridge, batch);
                    // Since this batch was just stolen, we need to replace it.
                    batch = t_current = Batch.Get();
                    batch.State = BatchState.Batching;
                }
                break;
        }
    }

    // Intentionally a ref struct so it cannot be used across an await boundary.
    internal ref struct FlushDisposable : IDisposable
    {
        private readonly Bridge _bridge;

        public FlushDisposable(Bridge bridge)
        {
            if (t_current is not null)
                throw new InvalidOperationException("A batch is already active on this thread.  This should be impossible");
            
            _bridge = bridge;
            _bridge.LastFlush.Restart();
            var batch = t_current = Batch.Get();
            batch.State = BatchState.Batching;
        }

        public void Dispose()
        {
            var batch = t_current;
            ArgumentNullException.ThrowIfNull(batch);
            Flush(_bridge, batch);
        }
    }
}