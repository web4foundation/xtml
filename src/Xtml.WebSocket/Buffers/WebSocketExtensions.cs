using System.Buffers;
using System.Net.WebSockets;
using Xtml.Runtime.Utilities;

namespace Xtml.WebSocket.Buffers;

internal static class WebSocketExtensions
{
    private const int RECEIVE_BUFFER_LENGTH = 4096;

    internal static async Task<(WebSocketReceiveResult, ReadOnlySequence<byte>)> ReceiveAsync(
        this System.Net.WebSockets.WebSocket webSocket, 
        CancellationToken cancel = default)
    {
        // TODO: How to read without allocating a buffer for each connection?
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

    internal static async Task SendAsync(
        this System.Net.WebSockets.WebSocket webSocket, 
        ReadOnlySequence<byte> output, 
        CancellationToken cancel = default)
    {
        var bytesRemaining = output.Length;
        if (bytesRemaining == 0)
            return;

        foreach (ReadOnlyMemory<byte> buffer in output)
        {
            bytesRemaining -= buffer.Length;
            await webSocket.SendAsync(
                buffer: buffer,
                messageType: WebSocketMessageType.Text,
                endOfMessage: bytesRemaining == 0,
                cancellationToken: cancel
            );
        }
    }
}