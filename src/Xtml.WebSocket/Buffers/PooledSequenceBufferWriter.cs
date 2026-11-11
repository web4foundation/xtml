using System.Buffers;

namespace Xtml.WebSocket.Buffers;

internal class PooledSequenceBufferWriter<T> : IBufferWriter<T>
{
    private const int DEFAULT_BUFFER_SIZE = 16384;
    private T[]? _currentBuffer;
    private int _currentIndex;
    private SequenceSegment<T>? _segmentStart;
    private SequenceSegment<T>? _segmentEnd;
    public int WrittenCount { get; private set; }

    public ReadOnlySequence<T> GetSequenceAndReset()
    {
        var sequence = this switch
        {
            // Never written to
            _ when _currentBuffer is null => ReadOnlySequence<T>.Empty,

            // Single-segment
            _ when _segmentEnd is null => new ReadOnlySequence<T>(_currentBuffer, 0, _currentIndex),

            // Multi-segment
            _ => new ReadOnlySequence<T>(_segmentStart!, 0, _segmentEnd.Append(_currentBuffer!, 0.._currentIndex), _currentIndex)
        };

        Reset();

        return sequence;
    }

    private void Reset()
    {
        WrittenCount = 0;
        _currentBuffer = null;
        _currentIndex = 0;
        _segmentStart = null;
        _segmentEnd = null;
    }

    public void Advance(int count)
    {
        _currentIndex += count;
        WrittenCount += count;
    }

    public Memory<T> GetMemory(int sizeHint = 0)
    {
        GrowIfNeeded(sizeHint);
        return _currentBuffer.AsMemory(_currentIndex);
    }

    public Span<T> GetSpan(int sizeHint = 0)
    {
        GrowIfNeeded(sizeHint);
        return _currentBuffer.AsSpan(_currentIndex);
    }

    private void GrowIfNeeded(int sizeHint)
    {
        var unusedCapacity = (_currentBuffer?.Length ?? 0) - _currentIndex;
        var bufferLength = Math.Max(sizeHint, DEFAULT_BUFFER_SIZE);

        // Return early if we already have enough capacity.
        if (unusedCapacity > sizeHint)
            return;

        // If this is already multi-segment, append the current buffer into 
        // the linked list and rent a fresh one.
        if (_segmentEnd is not null)
        {
            _segmentEnd = _segmentEnd.Append(_currentBuffer!, 0.._currentIndex);
            _currentBuffer = ArrayPool<T>.Shared.Rent(bufferLength);
            _currentIndex = 0;
            return;
        }

        // Single-segment is out of space.  Convert this to multi-segment.
        if (_currentBuffer is not null)
        {
            // SequenceSegment allocates memory 
            // and evidently there's no way around it short of a grow-copy-buffer approach.
            _segmentStart = new SequenceSegment<T>(_currentBuffer, 0.._currentIndex);
            _segmentEnd = _segmentStart;
            _currentBuffer = ArrayPool<T>.Shared.Rent(bufferLength);
            _currentIndex = 0;
            return;
        }

        // Must be the first write.  Rent a fresh buffer.
        if (_currentBuffer is null)
        {
            _currentBuffer = ArrayPool<T>.Shared.Rent(bufferLength);
            _currentIndex = 0;
            return;
        }
    }
}