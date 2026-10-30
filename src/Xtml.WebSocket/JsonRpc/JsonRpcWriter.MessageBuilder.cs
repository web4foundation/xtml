using System.Buffers;
using System.Numerics;

namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    public ref struct MessageBuilder(SyncCtx context)
    {
        public ParamsBuilder Method(string method)
        {
            context.JsonWriter.WriteStartObject();
            context.JsonWriter.WriteString("jsonrpc", "2.0");

            context.JsonWriter.WriteString("method", method);

            return new ParamsBuilder(context);
        }

        public ParamsBuilder Method(ValueTuple<string, byte[], string> method)
        {
            context.JsonWriter.WriteStartObject();
            context.JsonWriter.WriteString("jsonrpc", "2.0");

            context.JsonWriter.WritePropertyName("method");
            context.JsonWriter.WriteStringValueSegment(method.Item1, false);
            WriteRawKey(context, method.Item2);
            context.JsonWriter.WriteStringValueSegment(method.Item3, true);

            return new ParamsBuilder(context);
        }

        public ResultBuilder Result(string result)
        {
            context.JsonWriter.WriteStartObject();
            context.JsonWriter.WriteString("jsonrpc", "2.0");

            context.JsonWriter.WriteString("result", result);

            return new ResultBuilder(context);
        }
        
        public ResultBuilder Result<T>(T value) where T : struct, INumber<T>
        {
            switch (value)
            {
                case int i: context.JsonWriter.WriteNumber("result", i); break;
                case long l: context.JsonWriter.WriteNumber("result", l); break;
                case float f: context.JsonWriter.WriteNumber("result", f); break;
                case double d: context.JsonWriter.WriteNumber("result", d); break;
                case decimal m: context.JsonWriter.WriteNumber("result", m); break;
                default: throw new NotSupportedException($"Type {typeof(T)} is not supported");
            }

            return new ResultBuilder(context);
        }

        public ErrorBuilder Error(int code, string message)
        {
            context.JsonWriter.WriteStartObject();
            context.JsonWriter.WriteString("jsonrpc", "2.0");

            context.JsonWriter.WriteStartObject("error");

            context.JsonWriter.WriteNumber("code", code);
            context.JsonWriter.WriteString("message", message);

            return new ErrorBuilder(context);
        }
    }
}