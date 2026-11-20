using Xtml.Dom;
using Xtml.WebSocket.JsonRpc;

namespace Xtml.WebSocket.DocumentObjectModel;

internal partial struct Dom : IWindow
{
    public IWindow Window => this;
    IConsole IWindow.Console => this;
    IDocument IWindow.Document => this;
    IWindow IWindow.Window => this;

    // TODO: I need a concept of undefined, because window.prompt(undefined, "the default") is different from window.prompt(null, "the default")

    void IWindow.Alert(string message) => JsonRpcWriter
        .Batch(bridge)
        .Method("window.alert")
        .Param(message)
        .AsNotification();

    void IWindow.Focus() => JsonRpcWriter
        .Batch(bridge)
        .Method("window.focus")
        .AsNotification();

    Task<string?> IWindow.Prompt(string? message, string? defaultValue) => JsonRpcWriter
        .Batch(bridge)
        .Method("window.prompt")
        .Param(message)
        .OptionalParam(defaultValue)
        .AsRequest<string?>();
}
