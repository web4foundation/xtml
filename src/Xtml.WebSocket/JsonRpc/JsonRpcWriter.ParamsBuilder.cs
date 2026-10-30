using System.Buffers;
using System.Numerics;
using System.Threading.Channels;

namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    public ref partial struct ParamsBuilder(SyncCtx context)
    {
        private bool _isFirstParam = true;

        public ParamsBuilder Param(string? value)
        {
            TryStartParamsArray();

            context.JsonWriter.WriteStringValue(value);

            return this;
        }

        public ParamsBuilder Param(bool? value)
        {
            TryStartParamsArray();

            if (value.HasValue)
                context.JsonWriter.WriteBooleanValue(value.Value);
            else
                context.JsonWriter.WriteNullValue();

            return this;
        }

        public ParamsBuilder Param<T>(T? value) where T : struct, INumber<T>
        {
            TryStartParamsArray();

            if (value.HasValue)
                switch (value.Value)
                {
                    case int i: context.JsonWriter.WriteNumberValue(i); break;
                    case long l: context.JsonWriter.WriteNumberValue(l); break;
                    case float f: context.JsonWriter.WriteNumberValue(f); break;
                    case double d: context.JsonWriter.WriteNumberValue(d); break;
                    case decimal m: context.JsonWriter.WriteNumberValue(m); break;
                    default: throw new NotSupportedException($"Type {typeof(T)} is not supported");
                }
            else
                context.JsonWriter.WriteNullValue();

            return this;
        }

        public ParamsBuilder OptionalParam(string? value)
        {
            TryStartParamsArray();
            
            if (value is not null)
                context.JsonWriter.WriteStringValue(value);

            return this;
        }

        public ParamsBuilder OptionalParams(params Span<string> @params)
        {
            TryStartParamsArray();

            foreach (var param in @params)
                context.JsonWriter.WriteStringValue(param);

            return this;
        }

        public void AsNotification()
        {
            if (!_isFirstParam)
                context.JsonWriter.WriteEndArray();

            context.JsonWriter.WriteEndObject();
        }

        public Task<T> AsRequest<T>()
        {
            if (!_isFirstParam)
                context.JsonWriter.WriteEndArray();

            var id = Guid.NewGuid();
            context.JsonWriter.WriteString("id", id);

            context.JsonWriter.WriteEndObject();

            // TODO: Build JsonRpc Async!  Note: I guess this will automatically flush thanks to its SyncCtx.
            throw new NotImplementedException();
        }

        private void TryStartParamsArray()
        {
            if (_isFirstParam)
                context.JsonWriter.WriteStartArray("params");
            _isFirstParam = false;
        }
    }
}