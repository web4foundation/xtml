using System.Text.Encodings.Web;
using System.Text.Json;
using Xtml.WebSocket.Buffers;

namespace Xtml.WebSocket.JsonRpc;

public enum Flushing { Immediately, OnDispose }

public record HybridWriter(Utf8JsonWriter Utf8Json, PooledSequenceBufferWriter<byte> Buffer)
{
    public Flushing Flushing { get; set; } = Flushing.Immediately;

    [ThreadStatic]
    private static HybridWriter? t_writers;
    public static HybridWriter Get()
    {
        if (t_writers == null)
        {
            var bufferWriter = new PooledSequenceBufferWriter<byte>();
            var utf8JsonWriter = new Utf8JsonWriter(bufferWriter, new () { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
            // It's OK to relax escaping since WebSockets are ALWAYS UTF-8 (when not binary). ^
            // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/character-encoding
            t_writers = new HybridWriter(utf8JsonWriter, bufferWriter);
        }
        return t_writers;
    }


    public void Flush(Bridge bridge)
    {
        if (Buffer.WrittenCount + Utf8Json.BytesPending == 0)
            return;
        
        // TODO: If nothing is written (only `[`), return early.

        Utf8Json.WriteEndArray();
        Utf8Json.Flush();

        var output = Buffer.Sequence;
        Utf8Json.Reset();

        if (!bridge.OutputChannel.Writer.TryWrite(output))
            throw new InvalidOperationException("Failed to write to output channel."); // TODO: How to fail?
        
        Flushing = Flushing.Immediately;
    }
}