using Xtml.Dom;

namespace Xtml.WebSocket.BrowserApis;

internal class Document(Bridge bridge) : IDocument
{
    public IWindow DefaultView => bridge.Browser.Window.Value;

    // TODO: Implement Title.
    public string? Title { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}