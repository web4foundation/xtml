namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    public ref partial struct ErrorBuilder(SyncCtx context)
    {
        public ErrorBuilder Data(object data)
        {
            throw new NotImplementedException();
        }

        public void AsNotification()
        {
            context.JsonWriter.WriteEndObject();
            context.JsonWriter.WriteEndObject();
        }
    }
}