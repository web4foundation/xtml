using System.Buffers;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Channels;
using Xtml.Templating;
using Xtml.Runtime;

namespace Xtml.WebSocket.Buffers;

public partial class JsonRpcWriter : IDisposable
{
    // OK to relax escaping since WebSockets are ALWAYS UTF-8 (when not binary).
    // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/character-encoding
    private readonly static JsonWriterOptions options = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    [ThreadStatic]
    private static JsonRpcWriter? _threadStaticWriter;
    private readonly PooledSequenceBufferWriter<byte> _bufferWriter;
    private readonly Utf8JsonWriter _jsonWriter;
    private ChannelWriter<ReadOnlySequence<byte>>? _flusher = null;
    private FlushOnAwait? _flushOnAwait;
    private bool _isBatch = false;
    private List<EventListener>? _listeners;

    private JsonRpcWriter()
    {
        _bufferWriter = new();
        _jsonWriter = new(_bufferWriter, options);
    }

    public static JsonRpcWriter Current(ChannelWriter<ReadOnlySequence<byte>> flusher, List<EventListener>? listeners = null)
    {
        var writer = _threadStaticWriter ??= new();
        writer._flusher = flusher;
        writer._listeners = listeners;

        if (SynchronizationContext.Current is FlushOnAwait)
            writer._isBatch = true;

        return writer;
    }

    public FlushOnDispose BatchThisScope(bool continueOnCapturedContext = false)
    {
        if (continueOnCapturedContext)
        {
            _flushOnAwait ??= new();
            _flushOnAwait.Flusher = _flusher;
            SynchronizationContext.SetSynchronizationContext(_flushOnAwait);
        }

        if (!_isBatch)
        {
            if (_bufferWriter.WrittenCount > 0)
                throw new InvalidOperationException("Cannot switch to batch.  Buffer already written to.");
            _isBatch = true;
        }

        return new FlushOnDispose(this, continueOnCapturedContext);
    }

    public void Flush()
    {
        _jsonWriter.Flush();

        if (_isBatch && _bufferWriter.WrittenCount > 0)
        {
            _jsonWriter.WriteEndArray();
            _jsonWriter.Flush();
        }

        _isBatch = false;

        if (_bufferWriter.WrittenCount == 0)
            return;

        var buffer = _bufferWriter.Sequence;
        _jsonWriter.Reset(_bufferWriter);

        if (_flusher is null)
            throw new InvalidOperationException("🛑 Trying to flush when flusher is null.  This should be impossible.  Needs investigating.");
        while (!_flusher.TryWrite(buffer)) ;
    }

    public void WriteNotification(string method)
    {
        OnMessageBegin();

        _jsonWriter.WriteStartObject();
        _jsonWriter.WriteString("jsonrpc", "2.0");
        _jsonWriter.WriteString("method", method);
        _jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    public void WriteNotification<T>(string method, T param1)
    {
        OnMessageBegin();

        _jsonWriter.WriteStartObject();
        _jsonWriter.WriteString("jsonrpc", "2.0");

        _jsonWriter.WriteString("method", method);

        _jsonWriter.WriteStartArray("params");
        WriteTValue(param1);
        _jsonWriter.WriteEndArray();

        _jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    public void WriteNotification(string method, string param1, params Span<string> @params)
    {
        OnMessageBegin();

        _jsonWriter.WriteStartObject();
        _jsonWriter.WriteString("jsonrpc", "2.0");

        _jsonWriter.WriteString("method", method);

        _jsonWriter.WriteStartArray("params");
        _jsonWriter.WriteStringValue(param1);
        foreach (var param in @params)
            _jsonWriter.WriteStringValue(param);
        _jsonWriter.WriteEndArray();

        _jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    public void WriteNotification(string method, params Span<object> @params)
    {
        OnMessageBegin();

        _jsonWriter.WriteStartObject();
        _jsonWriter.WriteString("jsonrpc", "2.0");

        _jsonWriter.WriteString("method", method);

        _jsonWriter.WriteStartArray("params");
        foreach (var param in @params)
            _jsonWriter.WriteStringValue(param.ToString());
        _jsonWriter.WriteEndArray();

        _jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    // Called from SetValue
    public void WriteNotification(ValueTuple<string, byte[], string> method, ref Keyhole param1)
    {
        OnMessageBegin();

        _jsonWriter.WriteStartObject();
        _jsonWriter.WriteString("jsonrpc", "2.0");

        _jsonWriter.WritePropertyName("method");
        _jsonWriter.WriteStringValueSegment(method.Item1, false);
        WriteKey(method.Item2);
        _jsonWriter.WriteStringValueSegment(method.Item3, true);

        _jsonWriter.WriteStartArray("params");

        if (param1.Type == KeyholeType.Boolean)
        {
            // HTML treats boolean attributes differently.  Send without quotes.
            _jsonWriter.WriteBooleanValue(param1.Boolean);
        }
        else
        {
            _jsonWriter.WriteStringValueSegment("", false);
            WriteMutableKeyholeValue(ref param1);
            _jsonWriter.WriteStringValueSegment("", true);
        }

        _jsonWriter.WriteEndArray();

        _jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    // Called from SetValue
    public void WriteNotification(ValueTuple<string, byte[], string> method, Span<Keyhole> param1)
    {
        OnMessageBegin();

        _jsonWriter.WriteStartObject();
        _jsonWriter.WriteString("jsonrpc", "2.0");

        _jsonWriter.WritePropertyName("method");
        _jsonWriter.WriteStringValueSegment(method.Item1, false);
        WriteKey(method.Item2);
        _jsonWriter.WriteStringValueSegment(method.Item3, true);

        _jsonWriter.WriteStartArray("params");

        WriteRawSequence(param1);
        _jsonWriter.WriteStringValueSegment("", true);

        _jsonWriter.WriteEndArray();

        _jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    // Called from SetNode
    public void WriteNotification(Keyhole[] buffer, ValueTuple<string, byte[], string> method, Span<Keyhole> param1, ValueTuple<string, byte[]>? param2 = null)
    {
        OnMessageBegin();

        _jsonWriter.WriteStartObject();
        _jsonWriter.WriteString("jsonrpc", "2.0");

        _jsonWriter.WritePropertyName("method");
        _jsonWriter.WriteStringValueSegment(method.Item1, false);
        WriteKey(method.Item2);
        _jsonWriter.WriteStringValueSegment(method.Item3, true);

        _jsonWriter.WriteStartArray("params");

        WriteHtml(buffer, keyholes: param1, key: method.Item2);
        _jsonWriter.WriteStringValueSegment("", true);

        if (param2.HasValue)
        {
            _jsonWriter.WriteStringValueSegment(param2.Value.Item1, false);
            WriteKey(method.Item2);
            _jsonWriter.WriteStringValueSegment("", true);
        }

        _jsonWriter.WriteEndArray();

        _jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    // Called from SetNode
    public void WriteNotification(Keyhole[] buffer, ValueTuple<string, byte[], string> method, Span<Keyhole> param1, ValueTuple<string, int> param2, ValueTuple<string, int> param3)
    {
        OnMessageBegin();

        _jsonWriter.WriteStartObject();
        _jsonWriter.WriteString("jsonrpc", "2.0");

        _jsonWriter.WritePropertyName("method");
        _jsonWriter.WriteStringValueSegment(method.Item1, false);
        WriteKey(method.Item2);
        _jsonWriter.WriteStringValueSegment(method.Item3, true);

        _jsonWriter.WriteStartArray("params");

        WriteHtml(buffer, param1);
        _jsonWriter.WriteStringValueSegment("", true);

        Span<char> strInt = stackalloc char[11]; // max int length

        _jsonWriter.WriteStringValueSegment(param2.Item1, false);
        if (param2.Item2.TryFormat(strInt, out int length))
            _jsonWriter.WriteStringValueSegment(strInt[..length], false);
        _jsonWriter.WriteStringValueSegment("", true);

        _jsonWriter.WriteStringValueSegment(param3.Item1, false);
        if (param3.Item2.TryFormat(strInt, out length))
            _jsonWriter.WriteStringValueSegment(strInt[..length], false);
        _jsonWriter.WriteStringValueSegment("", true);

        _jsonWriter.WriteEndArray();

        _jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    // Called from PushNode
    public void WriteNotification(Keyhole[] buffer, ValueTuple<string, byte[], string> method, Span<Keyhole> param1, byte[] param2, ValueTuple<string, int>? param3 = null)
    {
        OnMessageBegin();

        _jsonWriter.WriteStartObject();
        _jsonWriter.WriteString("jsonrpc", "2.0");

        _jsonWriter.WritePropertyName("method");
        _jsonWriter.WriteStringValueSegment(method.Item1, false);
        WriteKey(method.Item2);
        _jsonWriter.WriteStringValueSegment(method.Item3, true);

        _jsonWriter.WriteStartArray("params");

        WriteHtml(buffer, param1);
        _jsonWriter.WriteStringValueSegment("", true);

        _jsonWriter.WriteStringValue(param2);

        if (param3 is not null)
        {
            _jsonWriter.WriteStringValueSegment(param3.Value.Item1, false);
            Span<char> strInt = stackalloc char[11]; // max int length
            if (param3.Value.Item2.TryFormat(strInt, out int length))
                _jsonWriter.WriteStringValueSegment(strInt[..length], false);
            _jsonWriter.WriteStringValueSegment("", true);
        }

        _jsonWriter.WriteEndArray();

        _jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    // Called from PopNode
    public void WriteNotification(ValueTuple<string, byte[], string> method, ValueTuple<string, int>? param1 = null)
    {
        OnMessageBegin();

        _jsonWriter.WriteStartObject();
        _jsonWriter.WriteString("jsonrpc", "2.0");

        _jsonWriter.WritePropertyName("method");
        _jsonWriter.WriteStringValueSegment(method.Item1, false);
        WriteKey(method.Item2);
        _jsonWriter.WriteStringValueSegment(method.Item3, true);

        _jsonWriter.WriteStartArray("params");

        if (param1.HasValue)
        {
            _jsonWriter.WriteStringValueSegment(param1.Value.Item1, false);
            Span<char> strInt = stackalloc char[11]; // max int length
            if (param1.Value.Item2.TryFormat(strInt, out int length))
                _jsonWriter.WriteStringValueSegment(strInt[..length], false);
            _jsonWriter.WriteStringValueSegment("", true);
        }

        _jsonWriter.WriteEndArray();

        _jsonWriter.WriteEndObject();

        OnMessageEnd();
    }

    public void WriteRequest(int id)
    {
        // TODO: Implement
    }

    public void WriteResponse(int id)
    {
        OnMessageBegin();

        _jsonWriter.WriteStartObject();
        _jsonWriter.WriteString("jsonrpc", "2.0");
        _jsonWriter.WriteNull("result");
        _jsonWriter.WriteNumber("id", id);
        _jsonWriter.WriteEndObject();
        _jsonWriter.Flush();

        OnMessageEnd();
    }

    public void WriteResponse<T>(int id, T result)
    {
        OnMessageBegin();

        _jsonWriter.WriteStartObject();
        _jsonWriter.WriteString("jsonrpc", "2.0");
        _jsonWriter.WritePropertyName("result");
        WriteTValue(result);
        _jsonWriter.WriteNumber("id", id);
        _jsonWriter.WriteEndObject();
        _jsonWriter.Flush();

        OnMessageEnd();
    }
    
    private void WriteHtml(Keyhole[] buffer, Span<Keyhole> keyholes, byte[]? key = null)
    {
        #if DEBUG
        var isHeaderOmitted = false;
        #endif

        for (int i = 1; i < keyholes.Length; i += 2)
        {
            ref var keyhole = ref keyholes[i];
            ref var immutable = ref keyholes[i - 1];
            var literal = immutable.StringLiteral!;

            #if DEBUG
            if (i == 1 && key == Reconciler.ROOT_KEY)
                isHeaderOmitted = InjectKernel(ref literal);
            #endif

            bool isAttribute = WriteStringLiteral(ref literal, ref keyhole, out var attributeName);

            switch (keyhole.Type)
            {
                case KeyholeType.Html:
                    WriteOpenSentinel(keyhole.Key);
                    WriteHtml(buffer, buffer.AsSpan(keyhole.Sequence));
                    WriteCloseSentinel(keyhole.Key);
                    WriteTransition(keyhole.Key, keyhole.FormatModifier);
                    break;
                case KeyholeType.HtmlRaw:
                    WriteOpenSentinel(keyhole.Key, isAttribute);
                    WriteRawSequence(buffer.AsSpan(keyhole.Sequence));
                    WriteCloseSentinel(keyhole.Key, isAttribute);
                    break;
                case KeyholeType.EventListener:
                    WriteListener(keyhole.Key, keyhole.TrimModifier);
                    break;
                case KeyholeType.Iterator:
                    int start = keyhole.Sequence.Start.Value;
                    int end = keyhole.Sequence.End.Value;
                    for (int i2 = start + 1; i2 < end; i2 += 2)
                    {
                        ref var k = ref buffer[i2];
                        WriteOpenSentinel(k.Key);
                        WriteHtml(buffer, buffer.AsSpan(k.Sequence));
                        WriteCloseSentinel(k.Key);
                    }
                    WriteVoidSentinel(keyhole.Key);
                    break;
                // The rest are the mutable keyhole values.  They might use format strings.
                default:
                    if (isAttribute && keyhole.Type == KeyholeType.Boolean)
                    {
                        // Boolean attributes have no opening sentinel and their value 
                        // is written as a part of the prior string literal for the sake of performance.
                        WriteCloseSentinel(keyhole.Key, attributeName);
                    }
                    else
                    {
                        WriteOpenSentinel(keyhole.Key, isAttribute);
                        WriteMutableKeyholeValue(ref keyhole);
                        WriteCloseSentinel(keyhole.Key, isAttribute);
                    }
                    break;
            }
        }

        ref var lastLiteral = ref keyholes[^1];
        _jsonWriter.WriteStringValueSegment(lastLiteral.StringLiteral, false);

        #if DEBUG
        if (isHeaderOmitted)
            _jsonWriter.WriteStringValueSegment("</body></html>", false);
        #endif
    }

    private bool WriteStringLiteral(ref string literal, ref Keyhole keyhole, out ReadOnlySpan<char> attributeName)
    {
        var isAttribute = literal.EndsWith('=');
        if (!isAttribute || keyhole.Type != KeyholeType.Boolean)
        {
            _jsonWriter.WriteStringValueSegment(literal, false);
            attributeName = default;
            return isAttribute;
        }

        // Boolean attributes are "special".  
        // For example <input checked="false" /> means checked is true.  🤦
        // The only way to make a boolean attribute false is to omit it entirely.
        var span = literal.AsSpan();
        int indexBeforeAttribute = span.LastIndexOf(' ');
        _jsonWriter.WriteStringValueSegment(
            keyhole.Boolean 
                ? span[..^1] // Writes the whole literal which includes the attribute name up to the equals sign.
                : span[..indexBeforeAttribute], // Writes the literal but excludes the attribute name and equals sign
            false);
        attributeName = span[(indexBeforeAttribute + 1)..^1];
        return true;
    }

    private void WriteListener(byte[] key, string? trim)
    {
        _jsonWriter.WriteStringValueSegment("\"keyholes['", false);
        WriteKey(key);
        if (trim == string.Empty)
        {
            _jsonWriter.WriteStringValueSegment("'].dispatchEvent(event)\" key:", false);
        }
        else
        {
            _jsonWriter.WriteStringValueSegment("'].dispatchEvent(event,'", false);
            _jsonWriter.WriteStringValueSegment(trim ?? "*", false);
            _jsonWriter.WriteStringValueSegment("')\" key:", false);
        }
        WriteKey(key);
    }

    private void WriteRawSequence(Span<Keyhole> keyholes)
    {
        for (int i = 0; i < keyholes.Length; i++)
        {
            ref var keyhole = ref keyholes[i];

            switch (keyhole.Type)
            {
                case KeyholeType.StringLiteral:
                    _jsonWriter.WriteStringValueSegment(keyhole.StringLiteral, false);
                    break;
                // The rest are the mutable keyhole values.  They might use format strings.
                default:
                    WriteMutableKeyholeValue(ref keyhole);
                    break;
            }
        }
    }

    private void WriteOpenSentinel(byte[] key, bool isAttribute = false)
    {
        if (isAttribute)
        {
            _jsonWriter.WriteStringValueSegment("\"", false);
        }
        else
        {
            _jsonWriter.WriteStringValueSegment("<!--key:", false);
            WriteKey(key);
            _jsonWriter.WriteStringValueSegment("-->", false);
        }
    }

    private void WriteCloseSentinel(byte[] key, bool isAttribute = false)
    {
        if (isAttribute)
        {
            _jsonWriter.WriteStringValueSegment("\" key:", false);
            WriteKey(key);
        }
        else
        {
            _jsonWriter.WriteStringValueSegment("<!--/key:", false);
            WriteKey(key);
            _jsonWriter.WriteStringValueSegment("-->", false);
        }
    }

    private void WriteCloseSentinel(byte[] key, ReadOnlySpan<char> booleanAttributeName)
    {
        // Booleans are "special" (see WriteStringLiteral).
        _jsonWriter.WriteStringValueSegment(" key:", false);
        WriteKey(key);
        _jsonWriter.WriteStringValueSegment("=\"", false);
        _jsonWriter.WriteStringValueSegment(booleanAttributeName, false);
        _jsonWriter.WriteStringValueSegment("\"", false);
    }

    private void WriteVoidSentinel(byte[] key)
    {
        _jsonWriter.WriteStringValueSegment("<!--key:", false);
        WriteKey(key);
        _jsonWriter.WriteStringValueSegment(" /-->", false);
    }

    private void WriteKey(byte[] key)
    {
        _jsonWriter.Flush();
        key.CopyTo(_bufferWriter.GetSpan(key.Length));
        _bufferWriter.Advance(key.Length);
    }

    private void OnMessageBegin()
    {
        if (_isBatch && _jsonWriter.BytesCommitted + _jsonWriter.BytesPending + _bufferWriter.WrittenCount == 0)
            _jsonWriter.WriteStartArray();
    }

    private void OnMessageEnd()
    {
        if (!_isBatch)
            Flush();
    }

    private void WriteTValue<T>(T value)
    {
        switch (value)
        {
            case string s:
                _jsonWriter.WriteStringValue(s);
                break;
            case int i:
                _jsonWriter.WriteNumberValue(i);
                break;
            case bool b:
                _jsonWriter.WriteBooleanValue(b);
                break;
            // TODO: Support the rest.
            default:
                _jsonWriter.WriteNullValue();
                break;
        }
    }

    private void WriteMutableKeyholeValue(ref Keyhole keyhole)
    {
        // String and Boolean do not use format strings.
        switch (keyhole.Type)
        {
            case KeyholeType.String:
                // Must use jsonWriter to write this string with the proper json encoding.
                _jsonWriter.WriteStringValueSegment(keyhole.String, false);
                return;
            case KeyholeType.Boolean:
                _jsonWriter.WriteStringValueSegment(keyhole.Boolean ? "true" : "false", false);
                return;
        }

        // All other mutable values might make use of a format string.  
        // Flush the JSON writer and switch to the raw buffer writer.
        // Use IUtf8SpanFormattable.TryFormat() to write without allocating memory.

        _jsonWriter.Flush();
        int length = 0;
        int sizeHint = 30;
        switch (keyhole.Type)
        {
            case KeyholeType.Integer:
                while (!keyhole.Integer.TryFormat(_bufferWriter.GetSpan(sizeHint), out length, keyhole.FormatModifier))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.Long:
                while (!keyhole.Long.TryFormat(_bufferWriter.GetSpan(sizeHint), out length, keyhole.FormatModifier))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.Float:
                while (!keyhole.Float.TryFormat(_bufferWriter.GetSpan(sizeHint), out length, keyhole.FormatModifier))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.Double:
                while (!keyhole.Double.TryFormat(_bufferWriter.GetSpan(sizeHint), out length, keyhole.FormatModifier))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.Decimal:
                while (!keyhole.Decimal.TryFormat(_bufferWriter.GetSpan(sizeHint), out length, keyhole.FormatModifier))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.DateTime:
                while (!keyhole.DateTime.TryFormat(_bufferWriter.GetSpan(sizeHint), out length, keyhole.FormatModifier))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.DateOnly:
                while (!keyhole.DateOnly.TryFormat(_bufferWriter.GetSpan(sizeHint), out length, keyhole.FormatModifier))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.TimeSpan:
                while (!keyhole.TimeSpan.TryFormat(_bufferWriter.GetSpan(sizeHint), out length, keyhole.FormatModifier))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.TimeOnly:
                while (!keyhole.TimeOnly.TryFormat(_bufferWriter.GetSpan(sizeHint), out length, keyhole.FormatModifier))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.Color:
                while (!keyhole.Color.TryFormat(_bufferWriter.GetSpan(sizeHint), out length, keyhole.FormatModifier))
                    GrowSizeHint(ref sizeHint);
                break;
            case KeyholeType.Uri:
                // TODO: Fix memory allocation and support format string?
                _bufferWriter.Write(keyhole.Uri!.ToString());
                break;
        }
        _bufferWriter.Advance(length);
    }

    private static void GrowSizeHint(ref int sizeHint)
    {
        sizeHint *= 2;
        if (sizeHint > (2 << 20)) // 1MB
            throw new NotSupportedException("🛑 It seems a keyhole value with a format string needed a buffer > 1MB.  Probably misuse?  Needs investigation.");
    }

    private void WriteTransition(byte[] parentKey, string? transition)
    {
        if (transition is null)
            return;
        
        Span<byte> key = stackalloc byte[parentKey.Length];
        for (int i = 0; i < key.Length; i++)
            key[i] = parentKey[i] == ':' ? (byte)'-' : parentKey[i];
        
        // TODO: Many allocations below.
        string k = System.Text.Encoding.UTF8.GetString(key);
        _jsonWriter.WriteStringValueSegment($$"""
            <style>
                ::view-transition-group(xtml-fwd-{{k}}, xtml-rev-{{k}}) { animation: none; }
                ::view-transition-new(xtml-fwd-{{k}}) { width: auto; height: auto; animation: 300ms ease-in-out {{transition}}-in; }
                ::view-transition-old(xtml-fwd-{{k}}) { width: auto; height: auto; animation: 300ms ease-in-out {{transition}}-out; }
                ::view-transition-new(xtml-rev-{{k}}) { width: auto; height: auto; animation: 300ms ease-in-out {{transition}}-out reverse; }
                ::view-transition-old(xtml-rev-{{k}}) { width: auto; height: auto; animation: 300ms ease-in-out {{transition}}-in reverse; }
            </style>
            """, false);
    }

    public void Dispose()
    {
        _jsonWriter.Dispose();
    }

    #if DEBUG
    private bool InjectKernel(ref string literal)
    {
        // NOTE! This only ever runs in DEBUG mode.

        int headIndex = literal.IndexOf("<head>", StringComparison.Ordinal);
        bool isHeadOmitted = headIndex < 0;
        headIndex += 6;

        _jsonWriter.WriteStringValueSegment(
            isHeadOmitted 
                ? "<!doctype html><html><head>" 
                : literal[..headIndex], 
            false);
        
        _jsonWriter.WriteStringValueSegment("""

                <!-- Injected by XTML -->
                <script src="/_app/websocket/ui.js" defer></script>
                <link href="/_app/base/ui.css" rel="stylesheet" />
                <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no" />
                <meta charset="UTF-8">

        """, false);

        // Write event handlers set on window or document
        if (_listeners?.Count > 0)
        {
            _jsonWriter.WriteStringValueSegment("\n\n<script>\n", false);

            foreach (var listener in _listeners)
                _jsonWriter.WriteStringValueSegment($"  {listener.Html ?? ""}\n", false);

            _jsonWriter.WriteStringValueSegment("</script>\n\n", false);
        }

        if (isHeadOmitted)
            _jsonWriter.WriteStringValueSegment("</head><body>", false);
        else
            literal = literal[headIndex..];

        return isHeadOmitted;
    }
    #endif
}