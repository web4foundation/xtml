using Xtml.Dom;
using Xtml.WebSocket.JsonRpc;

namespace Xtml.WebSocket.DocumentObjectModel;

internal class Window(Bridge bridge) : IWindow
{
    IWindow IWindow.Window => this;
    public IConsole Console => bridge.Dom.Console.Value;
    public IDocument Document => bridge.Dom.Document.Value;

    // TODO: I need a concept of undefined, because window.prompt(undefined, "the default") is different from window.prompt(null, "the default")

    public void Alert(string message) => JsonRpcWriter
        .Batch(bridge)
        .Method("window.alert")
        .Param(message)
        .AsNotification();

    public void Focus() => JsonRpcWriter
        .Batch(bridge)
        .Method("window.focus")
        .AsNotification();

    public Task<string?> Prompt(string? message, string? defaultValue) => JsonRpcWriter
        .Batch(bridge)
        .Method("window.prompt")
        .Param(message)
        .OptionalParam(defaultValue)
        .AsRequest<string?>();
}
