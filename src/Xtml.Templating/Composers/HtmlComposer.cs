using System.Buffers;
using System.Drawing;
using Xtml.Dom;

namespace Xtml.Templating.Composers;

public class HtmlComposer(IBufferWriter<byte> writer) : BaseComposer, IStreamingComposer
{
    public IBufferWriter<byte> Writer { get; set; } = writer;

    public override bool OnMarkup(ref Html parent, ref string literal, int relativeOrder = -1) => Writer.Write(literal);
    public override bool OnStringKeyhole(ref Html parent, string value) => Writer.Write(value);
    public override bool OnBoolKeyhole(ref Html parent, bool value) => Writer.Write(value ? "true" : "false");
    public override bool OnIntKeyhole(ref Html parent, int value, string? format = null) => Writer.Write(value, format);
    public override bool OnLongKeyhole(ref Html parent, long value, string? format = null) => Writer.Write(value, format);
    public override bool OnFloatKeyhole(ref Html parent, float value, string? format = null) => Writer.Write(value, format);
    public override bool OnDoubleKeyhole(ref Html parent, double value, string? format = null) => Writer.Write(value, format);
    public override bool OnDecimalKeyhole(ref Html parent, decimal value, string? format = null) => Writer.Write(value, format);
    public override bool OnDateTimeKeyhole(ref Html parent, DateTime value, string? format = null) => Writer.Write(value, format);
    public override bool OnDateOnlyKeyhole(ref Html parent, DateOnly value, string? format = null) => Writer.Write(value, format);
    public override bool OnTimeSpanKeyhole(ref Html parent, TimeSpan value, string? format = null) => Writer.Write(value, format);
    public override bool OnTimeOnlyKeyhole(ref Html parent, TimeOnly value, string? format = null) => Writer.Write(value, format);
    public override bool OnColorKeyhole(ref Html parent, Color value, string? format = null) => Writer.Write(value, format);
    public override bool OnUriKeyhole(ref Html parent, Uri value, string? format = null) => Writer.Write(value.ToString());
    // TODO: ^ Fix memory allocation happening here and incorporate format strings
    public override bool OnListener(ref Html parent, Action listener, string? trim = null, string? expression = null) => Writer.Write(expression ?? string.Empty);
    public override bool OnListener(ref Html parent, Action<Event> listener, string? trim = null, string? expression = null) => Writer.Write(expression ?? string.Empty);
    public override bool OnListener(ref Html parent, Func<Task> listener, string? trim = null, string? expression = null) => Writer.Write(expression ?? string.Empty);
    public override bool OnListener(ref Html parent, Func<Event, Task> listener, string? trim = null, string? expression = null) => Writer.Write(expression ?? string.Empty);

    public override void Reset()
    {
        Writer = null!;
        base.Reset();
    }

    [ThreadStatic] static HtmlComposer? t_reusable;
    public static HtmlComposer Reuse(IBufferWriter<byte> writer)
    {
        if (t_reusable is {} composer)
        {
            composer.Writer = writer;
            return composer;
        }

        return t_reusable = new HtmlComposer(writer);
    }
}