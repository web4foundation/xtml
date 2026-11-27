using System.Text.Encodings.Web;
using System.Text.Json;
using System.Diagnostics;
using Xtml.WebSocket.Buffers;
using Microsoft.Extensions.ObjectPool;

namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    [ThreadStatic]
    private static Batch? t_current;

    internal class Batch
    {
        // private static readonly ObjectPool<Batch> s_pool = ObjectPool.Create<Batch>();

        public PooledSequenceBufferWriter<byte> Buffer { get; init; }
        public Utf8JsonWriter Utf8Json { get; init; }
        // public bool IsBatching => t_current != null;
        public bool IsBatching { get; set; } = false;

        // public Stopwatch LastFlush { get; set; } = Stopwatch.StartNew();

        public Batch()
        {
            Buffer = new PooledSequenceBufferWriter<byte>();
            Utf8Json = new Utf8JsonWriter(Buffer, new () { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            // It's OK to relax escaping since WebSockets are ALWAYS UTF-8 (when not binary). ^
            // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/character-encoding
        }

        // public static Batch Get()
        // {
        //     return s_pool.Get();
        // }

        // public void Return()
        // {
        //     s_pool.Return(this);
        // }
    }
}