namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    // Intentionally a ref struct so it cannot be used across an await boundary.
    internal ref struct FlushDisposable : IDisposable
    {
        private readonly Bridge _bridge;
        private readonly JsonRpcWriter.HybridWriter _writer;

        public FlushDisposable(Bridge bridge)
        {
            _bridge = bridge;
            _writer = GetWriter();
            _writer.IsBatching = true;
            _writer.LastFlush.Restart();
        }

        public void Dispose()
        {
            // Do not debounce the Reconciler here.  
            // The batch is over and the next event listener could be many dozens of milliseconds away.
            _bridge.Reconcile();
            Flush(_bridge, _writer);
            _writer.IsBatching = false;
        }
    }
}