namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    public ref partial struct ErrorBuilder(Bridge bridge, HybridWriter writer)
    {
        public ErrorBuilder Data(object data)
        {
            throw new NotImplementedException();
        }

        public void AsNotification()
        {
            writer.Utf8Json.WriteEndObject();
            writer.Utf8Json.WriteEndObject();
            
            CourtesyFlush(bridge, writer);
        }
    }
}