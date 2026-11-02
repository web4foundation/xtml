using Xtml.Dom;

namespace Xtml.WebSocket;

// Instead of a new-ing up another class, save an instantiation 
// and explicitly implement on Bridge.
internal partial class Bridge : IDocument
{
    public IDocument Document => this;

    IWindow IDocument.DefaultView => throw new NotImplementedException();

    string? IDocument.Title { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}