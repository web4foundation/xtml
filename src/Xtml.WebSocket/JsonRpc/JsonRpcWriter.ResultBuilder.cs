namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    public ref partial struct ResultBuilder(Bridge bridge, HybridWriter writer)
    {
        public void AsResponse(int id)
        {
            writer.Utf8Json.WriteNumber("id", id);
            writer.Utf8Json.WriteEndObject();
            
            CourtesyFlush(bridge, writer);
        }
    }
}