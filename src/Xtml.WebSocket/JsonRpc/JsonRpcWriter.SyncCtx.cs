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
                        var previous = Current;
                        try
                        {
                            SetSynchronizationContext(stateHolder.SyncCtx);

                            // Just like in EventLoop(), set this to flush at the end of this block, 
                            // just before yielding this thread back to the pool.
                            using var flusher = JsonRpcWriter.FlushOnDispose(stateHolder.Bridge);
                            
                            // The awaiting is done.  Execute what comes after the `await`.
                            stateHolder.Callback(stateHolder.State);
                        }
                        finally
                        {
                            SetSynchronizationContext(previous);
                        }
                    }
                },
                state: new StateHolder(callback, state, bridge, this),
                preferLocal: false
            );
        }
    }
}