using Xtml.BrowserApis;

namespace Xtml.WebSocket.BrowserApis;

internal partial class BrowserProxy : IDocument
{
    IWindow IDocument.DefaultView => this;
}