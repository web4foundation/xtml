using Xtml.Dom;
using Xtml.WebSocket.JsonRpc;

namespace Xtml.WebSocket;

// Instead of a new-ing up another class, save an instantiation 
// and explicitly implement on Bridge.
public partial class Bridge : IWindow
{
    public IWindow Window => this;
    IConsole IWindow.Console => this;
    IDocument IWindow.Document => this;
    IWindow IWindow.Window => this;

    // TODO: I need a concept of undefined, because window.prompt(undefined, "the default") is different from window.prompt(null, "the default")

    void IWindow.Alert(string message) => JsonRpcWriter
        .Batch(this)
        .Method("window.alert")
        .Param(message)
        .AsNotification();

    void IWindow.Focus() => JsonRpcWriter
        .Batch(this)
        .Method("window.focus")
        .AsNotification();

    Task<string> IWindow.Prompt(string? message, string? defaultValue) => JsonRpcWriter
        .Batch(this)
        .Method("window.prompt")
        .Param(message)
        .Param(defaultValue)
        .AsRequest<string>();
}
