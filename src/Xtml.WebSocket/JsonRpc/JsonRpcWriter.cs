using System.Buffers;
using System.Threading.Channels;

namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    public static MessageBuilder Batch(Bridge bridge)
    {
        var context = SyncCtx.Set(bridge);

        if (context.RawWriter.WrittenCount + context.JsonWriter.BytesPending == 0)
            context.JsonWriter.WriteStartArray();

        return new MessageBuilder(context);
    }

    public static void Flush()
    {
        if (SynchronizationContext.Current is not SyncCtx context)
            return;
        
        if (context.RawWriter.WrittenCount + context.JsonWriter.BytesPending == 0)
            return;
        
        if (context.Bridge is null)
            throw new InvalidOperationException("It should be impossible for SyncCtx.Bridge to be null here.");
        
        // TODO: If nothing is written (only `[`), return early.

        context.JsonWriter.WriteEndArray();
        context.JsonWriter.Flush();

        var output = context.RawWriter.Sequence;
        context.JsonWriter.Reset();

        if (!context.Bridge.OutputChannel.Writer.TryWrite(output))
            throw new InvalidOperationException("Failed to write to output channel."); // TODO: How to fail?
    }

    public static FlushOnDispose FlushWhenDone() => new();

    public ref struct FlushOnDispose : IDisposable
    {
        public void Dispose()
        {
            Flush();
        }
    }
}