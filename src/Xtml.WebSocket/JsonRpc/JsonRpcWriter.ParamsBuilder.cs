using System.Numerics;
using System.Runtime.CompilerServices;

namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    public ref partial struct ParamsBuilder(Bridge bridge, Batch batch)
    {
        private bool _isFirstParam = true;

        public ParamsBuilder Param(string? value)
        {
            TryStartParamsArray();

            batch.Utf8Json.WriteStringValue(value);

            return this;
        }

        public ParamsBuilder Param(bool? value)
        {
            TryStartParamsArray();

            if (value.HasValue)
                batch.Utf8Json.WriteBooleanValue(value.Value);
            else
                batch.Utf8Json.WriteNullValue();

            return this;
        }

        public ParamsBuilder Param<T>(T? value) where T : struct, INumber<T>
        {
            TryStartParamsArray();

            if (value.HasValue)
                switch (value.Value)
                {
                    case int i: batch.Utf8Json.WriteNumberValue(i); break;
                    case long l: batch.Utf8Json.WriteNumberValue(l); break;
                    case float f: batch.Utf8Json.WriteNumberValue(f); break;
                    case double d: batch.Utf8Json.WriteNumberValue(d); break;
                    case decimal m: batch.Utf8Json.WriteNumberValue(m); break;
                    default: throw new NotSupportedException($"Type {typeof(T)} is not supported");
                }
            else
                batch.Utf8Json.WriteNullValue();

            return this;
        }

        public ParamsBuilder OptionalParam(string? value)
        {
            TryStartParamsArray();
            
            if (value is not null)
                batch.Utf8Json.WriteStringValue(value);

            return this;
        }

        public ParamsBuilder OptionalParams(params Span<string> @params)
        {
            TryStartParamsArray();

            foreach (var param in @params)
                batch.Utf8Json.WriteStringValue(param);

            return this;
        }

        public void AsNotification()
        {
            TryEndParamsArray();

            batch.Utf8Json.WriteEndObject();

            CourtesyFlush(bridge, batch);
        }

        public Task<T> AsRequest<T>()
        {
            TryEndParamsArray();

            var id = Guid.NewGuid();
            var task = bridge.AwaitResult<T>(id);

            batch.Utf8Json.WriteString("id", id);
            batch.Utf8Json.WriteEndObject();

            CourtesyFlush(bridge, batch);

            return task;
        }

        private void TryStartParamsArray()
        {
            if (_isFirstParam)
                batch.Utf8Json.WriteStartArray("params");
            _isFirstParam = false;
        }

        private void TryEndParamsArray()
        {
            if (!_isFirstParam)
                batch.Utf8Json.WriteEndArray();
        }
    }
}