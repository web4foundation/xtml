using Xtml.BrowserApis;

namespace Xtml.Runtime;

public interface IKeyholesServer
{
    void DispatchEvent(Action listener);
    void DispatchEvent<T>(Action<Event> listener, T @event) where T : struct, Event;
    Task DispatchEvent(Func<Task> listener);
    Task DispatchEvent<T>(Func<Event, Task> listener, T @event) where T : struct, Event;
    void Ping();
    void Dump();
    void Benchmark(int? threads);
}
