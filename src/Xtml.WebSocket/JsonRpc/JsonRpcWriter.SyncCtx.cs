using System.Buffers;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Channels;
using Xtml.Runtime.Utilities;
using Xtml.WebSocket.Buffers;

namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    public class SyncCtx : SynchronizationContext
    {
        [ThreadStatic]
        private static SyncCtx? t_reusable;
        public Utf8JsonWriter JsonWriter { get; init; }
        public PooledSequenceBufferWriter<byte> RawWriter { get; init; }
        public Bridge? Bridge { get; set; }
        private record struct StateHolder(
            SendOrPostCallback Callback,
            object? State,
            SyncCtx Context
        );

        public SyncCtx()
        {
            RawWriter = new PooledSequenceBufferWriter<byte>();
            JsonWriter = new Utf8JsonWriter(RawWriter, new () { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            // It's OK to relax escaping since WebSockets are ALWAYS UTF-8 (when not binary). ^
            // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/character-encoding
        }

        public static SyncCtx Set(Bridge bridge)
        {
            var current = SynchronizationContext.Current;
            var context = current as SyncCtx;

            if (current is not null && context is null)
                throw new InvalidOperationException("Current SynchronizationContext is not a JsonRpcWriter.SyncCtx.");

            if (context is null)
            {
                context = t_reusable ??= new SyncCtx();
                SynchronizationContext.SetSynchronizationContext(context);
            }

            context.Bridge = bridge;
            return context;
        }

        public override void Post(SendOrPostCallback callback, object? state)
        {
            ThreadPool.QueueUserWorkItem<StateHolder>(
                callBack: static stateHolder => {
                    // The awaiting is done.  Execute what comes after the `await`.
                    stateHolder.Callback(stateHolder.State);
                    stateHolder.Context.Bridge = null;

                    // If here, we have reached the next `await`.  Courtesy flush.
                    Flush();
                },
                state: new StateHolder(
                    Callback: callback, 
                    State: state, 
                    Context: this
                ),
                preferLocal: false
            );
        }
    }
}