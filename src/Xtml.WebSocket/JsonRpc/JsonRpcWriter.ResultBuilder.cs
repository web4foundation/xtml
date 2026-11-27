namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    public ref partial struct ResultBuilder(Bridge bridge, Batch batch)
    {
        public void AsResponse(int id)
        {
            batch.Utf8Json.WriteNumber("id", id);
            batch.Utf8Json.WriteEndObject();
            
            CourtesyFlush(bridge, batch);
        }
    }
}