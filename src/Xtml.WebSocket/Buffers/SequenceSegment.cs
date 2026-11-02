using System.Buffers;

namespace Xtml.WebSocket.Buffers;

internal class SequenceSegment<T> : ReadOnlySequenceSegment<T>
{
    public T[] RentedBuffer { get; init; }

    public SequenceSegment(T[] buffer, Range range)
    {
        RentedBuffer = buffer;
        Memory = buffer.AsMemory()[range];
    }

    public SequenceSegment<T> Append(T[] buffer, Range range)
    {
        // TODO: new SequenceSegment() allocates memory
        var segment = new SequenceSegment<T>(buffer, range)
        {
            RunningIndex = RunningIndex + Memory.Length
        };
        Next = segment;
        return segment;
    }
}

internal static class ReadOnlySequenceExtensions
{
    internal static void ReturnToPool<T>(this ReadOnlySequence<T> sequence)
    {
        if (sequence.IsSingleSegment)
        {
            if (sequence.Start.GetObject() is T[] buffer && buffer.Length > 0)
                ArrayPool<T>.Shared.Return(buffer);
        }
        else
        {
            var position = sequence.Start;
            do
            {
                var item = position.GetObject();
                if (item is SequenceSegment<T> segment)
                {
                    ArrayPool<T>.Shared.Return(segment.RentedBuffer);
                }
            }
            while (sequence.TryGet(ref position, out var memory, advance: true));
        }
    }
}