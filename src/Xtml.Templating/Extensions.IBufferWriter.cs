using System.Buffers;
using System.Drawing;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using Xtml.Templating.Composers;

namespace Xtml.Templating;

public static partial class Extensions
{
    public static bool Write(this IBufferWriter<byte> bufferWriter, string text)
    {
        Span<byte> utf8buffer = bufferWriter.GetSpan(text.Length);
        int length = Encoding.UTF8.GetBytes(text, utf8buffer);
        bufferWriter.Advance(length);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Write(this IBufferWriter<byte> bufferWriter, ReadOnlySpan<byte> bytes)
    {
        // This performs slightly faster than the built-in Write<T> extension.
        // Is this because the generic one calls GetSpan without a length hint?

        Span<byte> utf8buffer = bufferWriter.GetSpan(bytes.Length);
        bytes.CopyTo(utf8buffer);
        bufferWriter.Advance(bytes.Length);
        return true;
    }

    public static bool Write<T>(this IBufferWriter<byte> bufferWriter, T value, string? format = null)
        where T : struct, IUtf8SpanFormattable
    {
        // TODO: Research the true max length of T.
        Span<byte> destination = bufferWriter.GetSpan(128);
        value.TryFormat(destination, out int length, format, null);
        bufferWriter.Advance(length);
        return true;
    }

    public static void Write(this IBufferWriter<byte> bufferWriter, ReadOnlyMemory<char> memory)
        => Write(bufferWriter, memory.Span);
        
    public static void Write(this IBufferWriter<byte> bufferWriter, ReadOnlySpan<char> span)
    {
        Span<byte> buffer = bufferWriter.GetSpan(span.Length);
        int length = Encoding.UTF8.GetBytes(span, buffer);
        bufferWriter.Advance(length);
    }
    
    public static bool Write(this IBufferWriter<byte> bufferWriter, Color color, string? format = null)
    {
        Span<byte> utf8buffer = bufferWriter.GetSpan(color.GetMaxPossibleLength());
        color.TryFormat(utf8buffer, out int length, format);
        bufferWriter.Advance(length);
        return true;
    }

    public static void Write(
        this IBufferWriter<byte> bufferWriter, 
        ReadOnlySpan<byte> text1,
        ReadOnlySpan<byte> text2,
        ReadOnlySpan<byte> text3)
    {
        int totalLength = text1.Length + text2.Length + text3.Length;
        Span<byte> utf8buffer = bufferWriter.GetSpan(totalLength);
        text1.CopyTo(utf8buffer);
        text2.CopyTo(utf8buffer[text1.Length..]);
        text3.CopyTo(utf8buffer[(text1.Length + text2.Length)..]);
        bufferWriter.Advance(totalLength);
    }

    public static void Write(
        this IBufferWriter<byte> writer, // This one defaults to HtmlComposer (see Html constructor below)
        [InterpolatedStringHandlerArgument("writer")] ref Html html)
    {
        html.Dispose();
    }

    public static void Write<T>(
        this IBufferWriter<byte> writer,
        T composer, // This one lets you supply your own composer.
        [InterpolatedStringHandlerArgument("composer")] ref Html html)
            where T : BaseComposer, IStreamingComposer
    {
        html.Dispose();
    }
}

public ref partial struct Html
{
    // Enables the pattern: pipeWriter.Write($"...");
    public Html(int literalLength, int formattedCount, IBufferWriter<byte> writer)
        : this(t_scopedComposer = HtmlComposer.Reuse(writer), literalLength, formattedCount)
    {
    }
}