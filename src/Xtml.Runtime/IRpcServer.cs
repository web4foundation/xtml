using Xtml.Dom;

namespace Xtml.Runtime;

public interface IRpcServer
{
    void DispatchEvent(Action listener);
    void DispatchEvent<T>(Action<Event> listener, T @event) where T : struct, Event;
    Task DispatchEvent(Func<Task> listener);
    Task DispatchEvent<T>(Func<Event, Task> listener, T @event) where T : struct, Event;
    void Ping(int requestId);
    void Dump();
    void Benchmark(int? threads);
}
