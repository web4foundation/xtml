namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    public struct ResetSynchronizationContextDisposable(SynchronizationContext? previous) : IDisposable
    {
        public void Dispose() => SynchronizationContext.SetSynchronizationContext(previous);
    }

    // Intentionally a ref struct so it cannot be used across an await boundary.
    public ref struct FlushDisposable(Bridge bridge, HybridWriter writer) : IDisposable
    {
        public void Dispose() => Flush(bridge, writer);
    }
}