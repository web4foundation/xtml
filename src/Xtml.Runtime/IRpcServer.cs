using Xtml.Dom;

namespace Xtml.Runtime;

public interface IRpcServer
{
    void DispatchEvent(Action listener);
    void DispatchEvent<T>(Action<Event> listener, T @event) where T : struct, Event;
    Task DispatchEvent(Func<Task> listener);
    Task DispatchEvent<T>(Func<Event, Task> listener, T @event) where T : struct, Event;
    Task<string> Ping();
    void Dump();
    void Benchmark(int? threads);
}
