using System.Buffers;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.ObjectPool;
using Xtml.Runtime;
using Xtml.WebSocket.Buffers;

namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    internal enum BatchState { Immediate, Batching, Flushing }

    [ThreadStatic]
    private static Batch? t_current;

    internal class Batch
    {
        private static readonly ObjectPool<Batch> s_pool = ObjectPool.Create<Batch>();

        public PooledSequenceBufferWriter<byte> Buffer { get; init; }
        public Utf8JsonWriter Utf8Json { get; init; }
        public BatchState State { get; set; } = BatchState.Immediate;

        public Batch()
        {
            Buffer = new PooledSequenceBufferWriter<byte>();
            Utf8Json = new Utf8JsonWriter(Buffer, new () { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            // It's OK to relax escaping since WebSockets are ALWAYS UTF-8 (when not binary). ^
            // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/character-encoding
            TryBegin();
        }

        private void TryBegin()
        {
            if (Utf8Json.BytesCommitted + Utf8Json.BytesPending == 0)
                Utf8Json.WriteStartArray();
        }

        public static Batch Get()
        {
            var batch = s_pool.Get();
            batch.TryBegin();
            return batch;
        }

        public void Return()
        {
            s_pool.Return(this);
        }
        
        public ReadOnlySequence<byte> GetOutputAndReset()
        {
            if (Utf8Json.BytesCommitted == 0 && Utf8Json.BytesPending <= 1) // i.e. the `[`
                return ReadOnlySequence<byte>.Empty; // TODO: Verify how frequent this is.  I do know it happens for every async event listener.  Anywhere else?
            
            Utf8Json.WriteEndArray();
            Utf8Json.Flush();

            var output = Buffer.GetSequenceAndReset();
            Utf8Json.Reset();
            State = BatchState.Immediate;
            Return();
            
            return output;
        }
    }
}