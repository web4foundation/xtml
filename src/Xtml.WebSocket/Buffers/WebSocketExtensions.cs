using System.Buffers;
using System.Net.WebSockets;
using Xtml.Runtime.Utilities;

namespace Xtml.WebSocket.Buffers;

public static class WebSocketExtensions
{
    private const int RECEIVE_BUFFER_LENGTH = 4096;

    public static async Task<(WebSocketReceiveResult, ReadOnlySequence<byte>)> ReceiveAsync(
        this System.Net.WebSockets.WebSocket webSocket, 
        CancellationToken cancel = default)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(RECEIVE_BUFFER_LENGTH);
        var result = await webSocket.ReceiveAsync(buffer, cancel);

        using var perf = Perf.Measure("WebSocket.ReceiveAsync (loop)"); // TODO: Remove PerfCheck

        if (result.MessageType == WebSocketMessageType.Close)
        {
            ArrayPool<byte>.Shared.Return(buffer);
            return (result, default);
        }
        
        if (result.EndOfMessage)
        {
            return (result, new ReadOnlySequence<byte>(buffer, 0, result.Count));
        }
        else
        {
            // TODO: new SequenceSegment() allocates memory
            var segmentStart = new SequenceSegment<byte>(buffer, 0..result.Count);
            var segmentEnd = segmentStart;
            while (!result.EndOfMessage)
            {
                buffer = ArrayPool<byte>.Shared.Rent(RECEIVE_BUFFER_LENGTH);
                result = await webSocket.ReceiveAsync(buffer, cancel);
                segmentEnd = segmentEnd.Append(buffer, 0..result.Count);
            }
            return (result, new ReadOnlySequence<byte>(segmentStart, 0, segmentEnd, result.Count));
        }
    }
}