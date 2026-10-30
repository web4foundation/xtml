namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    public ref partial struct ResultBuilder(SyncCtx context)
    {
        public void AsResponse(int id)
        {
            context.JsonWriter.WriteNumber("id", id);
            context.JsonWriter.WriteEndObject();
        }
    }
}