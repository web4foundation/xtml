using System.Numerics;

namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    public ref partial struct ParamsBuilder(Bridge bridge, HybridWriter writer)
    {
        private bool _isFirstParam = true;

        public ParamsBuilder Param(string? value)
        {
            TryStartParamsArray();

            writer.Utf8Json.WriteStringValue(value);

            return this;
        }

        public ParamsBuilder Param(bool? value)
        {
            TryStartParamsArray();

            if (value.HasValue)
                writer.Utf8Json.WriteBooleanValue(value.Value);
            else
                writer.Utf8Json.WriteNullValue();

            return this;
        }

        public ParamsBuilder Param<T>(T? value) where T : struct, INumber<T>
        {
            TryStartParamsArray();

            if (value.HasValue)
                switch (value.Value)
                {
                    case int i: writer.Utf8Json.WriteNumberValue(i); break;
                    case long l: writer.Utf8Json.WriteNumberValue(l); break;
                    case float f: writer.Utf8Json.WriteNumberValue(f); break;
                    case double d: writer.Utf8Json.WriteNumberValue(d); break;
                    case decimal m: writer.Utf8Json.WriteNumberValue(m); break;
                    default: throw new NotSupportedException($"Type {typeof(T)} is not supported");
                }
            else
                writer.Utf8Json.WriteNullValue();

            return this;
        }

        public ParamsBuilder OptionalParam(string? value)
        {
            TryStartParamsArray();
            
            if (value is not null)
                writer.Utf8Json.WriteStringValue(value);

            return this;
        }

        public ParamsBuilder OptionalParams(params Span<string> @params)
        {
            TryStartParamsArray();

            foreach (var param in @params)
                writer.Utf8Json.WriteStringValue(param);

            return this;
        }

        public void AsNotification()
        {
            TryEndParamsArray();

            writer.Utf8Json.WriteEndObject();
            if (!writer.IsBatching)
                Flush(bridge, writer);
        }

        public Task<T> AsRequest<T>()
        {
            TryEndParamsArray();

            var id = Guid.NewGuid();
            var task = bridge.AwaitResult<T>(id);

            writer.Utf8Json.WriteString("id", id);
            writer.Utf8Json.WriteEndObject();

            if (!writer.IsBatching)
                Flush(bridge, writer);

            return task;
        }

        private void TryStartParamsArray()
        {
            if (_isFirstParam)
                writer.Utf8Json.WriteStartArray("params");
            _isFirstParam = false;
        }

        private void TryEndParamsArray()
        {
            if (!_isFirstParam)
                writer.Utf8Json.WriteEndArray();
        }
    }
}