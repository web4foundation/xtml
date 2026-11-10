namespace Xtml.WebSocket.JsonRpc;

internal partial struct JsonRpcWriter
{
    private class SyncCtx(Bridge bridge) : SynchronizationContext
    {
        private record struct StateHolder(SendOrPostCallback Callback, object? State, Bridge Bridge, SyncCtx SyncCtx);

        public override void Post(SendOrPostCallback callback, object? state)
        {
            ThreadPool.QueueUserWorkItem<StateHolder>(
                callBack: static stateHolder => {
                    if (stateHolder.Bridge.ForceSequentialAsync)
                    {
                        stateHolder.Bridge.PostToEventLoop(stateHolder.Callback, stateHolder.State);
                    }
                    else
                    {
                        using var _ = JsonRpcWriter.FlushOnAwait(stateHolder.Bridge);
                        using var __ = JsonRpcWriter.FlushOnDispose(stateHolder.Bridge);
                        stateHolder.Callback(stateHolder.State);
                    }
                },
                state: new StateHolder(callback, state, bridge, this),
                preferLocal: false
            );
        }
    }
}