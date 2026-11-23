using Xtml.Dom;

namespace Xtml.WebSocket.DocumentObjectModel;

internal class Document(Bridge bridge) : IDocument
{
    public IWindow DefaultView => bridge.Dom.Window.Value;

    // TODO: Implement Title.
    public string? Title { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}