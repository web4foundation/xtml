using System.Buffers;
using System.Numerics;

namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    public ref partial struct MessageBuilder(Bridge bridge, Batch batch)
    {
        public ParamsBuilder Method(string method)
        {
            batch.Utf8Json.WriteStartObject();
            batch.Utf8Json.WriteString("jsonrpc", "2.0");

            batch.Utf8Json.WriteString("method", method);

            return new ParamsBuilder(bridge, batch);
        }

        public ParamsBuilder Method(ValueTuple<string, byte[], string> method)
        {
            batch.Utf8Json.WriteStartObject();
            batch.Utf8Json.WriteString("jsonrpc", "2.0");

            batch.Utf8Json.WritePropertyName("method");
            batch.Utf8Json.WriteStringValueSegment(method.Item1, false);
            WriteRawKey(batch, method.Item2);
            batch.Utf8Json.WriteStringValueSegment(method.Item3, true);

            return new ParamsBuilder(bridge, batch);
        }

        public ResultBuilder Result(string result)
        {
            batch.Utf8Json.WriteStartObject();
            batch.Utf8Json.WriteString("jsonrpc", "2.0");

            batch.Utf8Json.WriteString("result", result);

            return new ResultBuilder(bridge, batch);
        }
        
        public ResultBuilder Result<T>(T value) where T : struct, INumber<T>
        {
            switch (value)
            {
                case int i: batch.Utf8Json.WriteNumber("result", i); break;
                case long l: batch.Utf8Json.WriteNumber("result", l); break;
                case float f: batch.Utf8Json.WriteNumber("result", f); break;
                case double d: batch.Utf8Json.WriteNumber("result", d); break;
                case decimal m: batch.Utf8Json.WriteNumber("result", m); break;
                default: throw new NotSupportedException($"Type {typeof(T)} is not supported");
            }

            return new ResultBuilder(bridge, batch);
        }

        public ErrorBuilder Error(int code, string message)
        {
            batch.Utf8Json.WriteStartObject();
            batch.Utf8Json.WriteString("jsonrpc", "2.0");

            batch.Utf8Json.WriteStartObject("error");

            batch.Utf8Json.WriteNumber("code", code);
            batch.Utf8Json.WriteString("message", message);

            return new ErrorBuilder(bridge, batch);
        }
    }
}