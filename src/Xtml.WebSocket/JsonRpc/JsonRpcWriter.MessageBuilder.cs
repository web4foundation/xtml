using System.Buffers;
using System.Numerics;

namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    public ref partial struct MessageBuilder(Bridge bridge, HybridWriter writer)
    {
        public ParamsBuilder Method(string method)
        {
            writer.Utf8Json.WriteStartObject();
            writer.Utf8Json.WriteString("jsonrpc", "2.0");

            writer.Utf8Json.WriteString("method", method);

            return new ParamsBuilder(bridge, writer);
        }

        public ParamsBuilder Method(ValueTuple<string, byte[], string> method)
        {
            writer.Utf8Json.WriteStartObject();
            writer.Utf8Json.WriteString("jsonrpc", "2.0");

            writer.Utf8Json.WritePropertyName("method");
            writer.Utf8Json.WriteStringValueSegment(method.Item1, false);
            WriteRawKey(writer, method.Item2);
            writer.Utf8Json.WriteStringValueSegment(method.Item3, true);

            return new ParamsBuilder(bridge, writer);
        }

        public ResultBuilder Result(string result)
        {
            writer.Utf8Json.WriteStartObject();
            writer.Utf8Json.WriteString("jsonrpc", "2.0");

            writer.Utf8Json.WriteString("result", result);

            return new ResultBuilder(bridge, writer);
        }
        
        public ResultBuilder Result<T>(T value) where T : struct, INumber<T>
        {
            switch (value)
            {
                case int i: writer.Utf8Json.WriteNumber("result", i); break;
                case long l: writer.Utf8Json.WriteNumber("result", l); break;
                case float f: writer.Utf8Json.WriteNumber("result", f); break;
                case double d: writer.Utf8Json.WriteNumber("result", d); break;
                case decimal m: writer.Utf8Json.WriteNumber("result", m); break;
                default: throw new NotSupportedException($"Type {typeof(T)} is not supported");
            }

            return new ResultBuilder(bridge, writer);
        }

        public ErrorBuilder Error(int code, string message)
        {
            writer.Utf8Json.WriteStartObject();
            writer.Utf8Json.WriteString("jsonrpc", "2.0");

            writer.Utf8Json.WriteStartObject("error");

            writer.Utf8Json.WriteNumber("code", code);
            writer.Utf8Json.WriteString("message", message);

            return new ErrorBuilder(bridge, writer);
        }
    }
}