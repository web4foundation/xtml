using Xtml.Dom;

namespace Xtml.WebSocket.DocumentObjectModel;

internal partial struct Dom : IDocument
{
    public IDocument Document => this;

    IWindow IDocument.DefaultView => throw new NotImplementedException();

    string? IDocument.Title { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}