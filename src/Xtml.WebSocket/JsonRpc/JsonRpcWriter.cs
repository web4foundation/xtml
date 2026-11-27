namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    // TODO: Make this configurable?
    private const int COURTESY_FLUSH_THRESHOLD = 2 << 15; // 64k

    public static MessageBuilder Current(Bridge bridge)
    {
        var batch = t_current ?? Batch.Get();
        if (batch.Utf8Json.BytesCommitted + batch.Utf8Json.BytesPending == 0)
            batch.Utf8Json.WriteStartArray();
        return new(bridge, batch);
    }

    private static void Flush(Bridge bridge, Batch batch)
    {
        if (batch.Utf8Json.BytesCommitted == 0 && batch.Utf8Json.BytesPending <= 1) // i.e. the `[`
            return; // TODO: Verify how frequent this is.  I do know it happens for every async event listener.  Anywhere else?
        
        batch.Utf8Json.WriteEndArray();
        batch.Utf8Json.Flush();

        var output = batch.Buffer.GetSequenceAndReset();
        batch.Utf8Json.Reset();

        bridge.FlushToWebSocket(output);

        // batch.LastFlush.Restart();
        batch.Return();
    }

    private static void CourtesyFlush(Bridge bridge, Batch batch)
    {
        bool isUnbatched = !batch.IsBatching;
        bool tooSlow = false;//batch.LastFlush.Elapsed > bridge.RefreshRate;
        bool tooBig = batch.Utf8Json.BytesCommitted + batch.Utf8Json.BytesPending > COURTESY_FLUSH_THRESHOLD;

        if (isUnbatched || tooSlow || tooBig)
            Flush(bridge, batch);
    }

    // Intentionally a ref struct so it cannot be used across an await boundary.
    internal ref struct FlushDisposable : IDisposable
    {
        private readonly Bridge _bridge;
        private readonly Batch _batch;

        public FlushDisposable(Bridge bridge)
        {
            if (t_current is not null)
                throw new InvalidOperationException("A batch is already active on this thread.  This should be impossible");
            
            _bridge = bridge;
            _batch = t_current = Batch.Get();
            // _batch.LastFlush.Restart();
        }

        public void Dispose()
        {
            // Do not debounce the Reconciler here.  
            // The batch is over and the next event listener could be many dozens of milliseconds away.
            _bridge.Reconcile();
            Flush(_bridge, _batch);
            t_current = null;
        }
    }
}