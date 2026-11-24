using System.Drawing;
using Xtml.BrowserApis;

namespace Xtml.Templating.Composers;

public abstract class BaseComposer
{
    public int LiteralLength { get; set; }
    public int KeyholeCount { get; set; }

    public virtual void Grow(int literalLength, int keyholeCount)
    {
        LiteralLength += literalLength;
        KeyholeCount += keyholeCount;
    }

    public virtual bool OnMarkup(ref Html parent, ref string literal, int relativeOrder = -1) => true;

    public virtual bool OnStringKeyhole(ref Html parent, string value) => true;
    public virtual bool OnBoolKeyhole(ref Html parent, bool value) => true;
    public virtual bool OnIntKeyhole(ref Html parent, int value, string? format = null) => true;
    public virtual bool OnLongKeyhole(ref Html parent, long value, string? format = null) => true;
    public virtual bool OnFloatKeyhole(ref Html parent, float value, string? format = null) => true;
    public virtual bool OnDoubleKeyhole(ref Html parent, double value, string? format = null) => true;
    public virtual bool OnDecimalKeyhole(ref Html parent, decimal value, string? format = null) => true;
    public virtual bool OnDateTimeKeyhole(ref Html parent, DateTime value, string? format = null) => true;
    public virtual bool OnDateOnlyKeyhole(ref Html parent, DateOnly value, string? format = null) => true;
    public virtual bool OnTimeSpanKeyhole(ref Html parent, TimeSpan value, string? format = null) => true;
    public virtual bool OnTimeOnlyKeyhole(ref Html parent, TimeOnly value, string? format = null) => true;
    public virtual bool OnColorKeyhole(ref Html parent, Color value, string? format = null) => true;
    public virtual bool OnUriKeyhole(ref Html parent, Uri value, string? format = null) => true;

    public virtual bool OnHtmlKeyhole(ref Html parent, scoped Html value, int relativeOrder = -1, string? transition = null, string? expression = null) => true;

    public virtual bool OnIteratorKeyhole<T>(ref Html parent, ref Html htmls, Html.Enumerable<T> enumerable, string? transition = null, string? expression = null)
    {
        foreach (var html in enumerable)
            OnHtmlKeyhole(ref htmls, html);
        return true;
    }

    public virtual bool OnListener(ref Html parent, Action listener, string? trim = null, string? expression = null) => true;
    public virtual bool OnListener(ref Html parent, Action<Event> listener, string? trim = null, string? expression = null) => true;
    public virtual bool OnListener(ref Html parent, Func<Task> listener, string? trim = null, string? expression = null) => true;
    public virtual bool OnListener(ref Html parent, Func<Event, Task> listener, string? trim = null, string? expression = null) => true;

    public virtual void Reset()
    {
        // Called from the root Html's Dispose()
        LiteralLength = 0;
        KeyholeCount = 0;
    }
}