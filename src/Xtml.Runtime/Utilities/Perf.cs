using System.Diagnostics;

namespace Xtml.Runtime.Utilities;

public static class Perf
{
    public static PerfDumper Measure(string name = "unnamed") => new(name);

    public struct PerfDumper(string name) : IDisposable
    {
        readonly long gc1 = GC.GetAllocatedBytesForCurrentThread();
        readonly long sw1 = Stopwatch.GetTimestamp();

        public void Dispose()
        {
            var elapsed = Stopwatch.GetElapsedTime(sw1);
            long gc2 = GC.GetAllocatedBytesForCurrentThread();
            Debug.WriteLine($"{$"🚥 Perf({name}):",-45} elapsed:{$"{elapsed.TotalNanoseconds:n0} ns",-15} {$"allocations: {(gc2 - gc1):n0} bytes",-25}   thread:{Thread.CurrentThread.ManagedThreadId}");
        }
    }
}