using Xtml.BrowserApis;

namespace Xtml.Runtime;

public record struct EventListener(
    Action? Action = null,
    Action<Event>? ActionEvent = null,
    Func<Task>? Func = null,
    Func<Event, Task>? FuncEvent = null,
    string? OnNotation = null,
    string? Html = null)
{
    public EventListener(Action listener) : this(Action: listener) { }
    public EventListener(Action<Event> listener) : this(ActionEvent: listener) { }
    public EventListener(Func<Task> listener) : this(Func: listener) { }
    public EventListener(Func<Event, Task> listener) : this(FuncEvent: listener) { }
}