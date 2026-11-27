namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    public ref partial struct ErrorBuilder(Bridge bridge, Batch batch)
    {
        public ErrorBuilder Data(object data)
        {
            throw new NotImplementedException();
        }

        public void AsNotification()
        {
            batch.Utf8Json.WriteEndObject();
            batch.Utf8Json.WriteEndObject();
            
            CourtesyFlush(bridge, batch);
        }
    }
}