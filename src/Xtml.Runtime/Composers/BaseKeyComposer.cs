using System.Drawing;
using Xtml.Templating;
using Xtml.Templating.Composers;
using Xtml.BrowserApis;

namespace Xtml.Runtime.Composers;

public abstract class BaseKeyComposer : BaseComposer
{
    private bool _isHtmlPrepared = true;
    protected readonly KeyCursor _keyCursor = new();
    public byte[] Key { get; private set; } = [];

    public override void Grow(int literalLength, int keyholeCount)
    {
        _isHtmlPrepared = false;
        base.Grow(literalLength, keyholeCount);
    }

    public virtual bool OnTemplateBegin(ref Html html, ref string markup) => true;
    public virtual bool OnTemplateEnd(ref Html html) => true;

    public override bool OnMarkup(ref Html parent, ref string literal, int relativeOrder = -1) => (_isHtmlPrepared, parent.Type) switch {
        (true, _) => true,
        (false, HtmlType.Template) => _isHtmlPrepared = OnTemplateBegin(ref parent, ref literal),
        (false, _) => _isHtmlPrepared = OnHtmlBegin(ref parent, relativeOrder),
    };

    public override bool OnStringKeyhole(ref Html parent, string value) => OnKeyhole(ref parent);
    public override bool OnBoolKeyhole(ref Html parent, bool value) => OnKeyhole(ref parent);
    public override bool OnIntKeyhole(ref Html parent, int value, string? format = null) => OnKeyhole(ref parent);
    public override bool OnLongKeyhole(ref Html parent, long value, string? format = null) => OnKeyhole(ref parent);
    public override bool OnFloatKeyhole(ref Html parent, float value, string? format = null) => OnKeyhole(ref parent);
    public override bool OnDoubleKeyhole(ref Html parent, double value, string? format = null) => OnKeyhole(ref parent);
    public override bool OnDecimalKeyhole(ref Html parent, decimal value, string? format = null) => OnKeyhole(ref parent);
    public override bool OnDateTimeKeyhole(ref Html parent, DateTime value, string? format = null) => OnKeyhole(ref parent);
    public override bool OnDateOnlyKeyhole(ref Html parent, DateOnly value, string? format = null) => OnKeyhole(ref parent);
    public override bool OnTimeSpanKeyhole(ref Html parent, TimeSpan value, string? format = null) => OnKeyhole(ref parent);
    public override bool OnTimeOnlyKeyhole(ref Html parent, TimeOnly value, string? format = null) => OnKeyhole(ref parent);
    public override bool OnColorKeyhole(ref Html parent, Color value, string? format = null) => OnKeyhole(ref parent);
    public override bool OnUriKeyhole(ref Html parent, Uri value, string? format = null) => OnKeyhole(ref parent);

    public virtual bool OnHtmlBegin(ref Html html, int relativeOrder = -1)
    {
        Key = _keyCursor.MoveNext();
        _keyCursor.MoveDown();
        return true;
    }

    public override bool OnHtmlKeyhole(ref Html parent, scoped Html html, int relativeOrder = -1, string? transition = null, string? expression = null)
        => html.Type switch {
            HtmlType.Wrapper => true, // ignore
            HtmlType.Template => OnTemplateEnd(ref html),
            _ => OnHtmlEnd(ref parent, html, relativeOrder, transition, expression),
        };

    public virtual bool OnHtmlEnd(ref Html parent, scoped Html html, int relativeOrder = -1, string? transition = null, string? expression = null)
    {
        Key = _keyCursor.MoveUp();
        return true;
    }

    public virtual bool OnIteratorBegin(ref Html parent, ref Html htmls, string? transition = null, string? expression = null)
    {
        Key = _keyCursor.MoveNext();
        _keyCursor.MoveDown();
        return true;
    }

    public override bool OnIteratorKeyhole<T>(ref Html parent, ref Html htmls, Html.Enumerable<T> enumerable, string? transition = null, string? expression = null)
        => OnIteratorBegin(ref parent, ref htmls, transition, expression)
        && base.OnIteratorKeyhole(ref parent, ref htmls, enumerable, transition, expression)
        && OnIteratorEnd(ref parent, ref htmls, transition, expression);

    public virtual bool OnIteratorEnd(ref Html parent, ref Html htmls, string? transition = null, string? expression = null)
    {
        Key = _keyCursor.MoveUp();
        return true;
    }

    public override bool OnListener(ref Html parent, Action listener, string? trim = null, string? expression = null) => OnKeyhole(ref parent);
    public override bool OnListener(ref Html parent, Action<Event> listener, string? trim = null, string? expression = null) => OnKeyhole(ref parent);
    public override bool OnListener(ref Html parent, Func<Task> listener, string? trim = null, string? expression = null) => OnKeyhole(ref parent);
    public override bool OnListener(ref Html parent, Func<Event, Task> listener, string? trim = null, string? expression = null) => OnKeyhole(ref parent);

    public override void Reset()
    {
        _isHtmlPrepared = true;
        _keyCursor.Reset();
        base.Reset();
    }

    protected virtual bool OnKeyhole(ref Html parent)
    {
        var discard = string.Empty;
        _ = (_isHtmlPrepared, parent.Type) switch {
            (true, _) => true,
            (false, HtmlType.Template) => _isHtmlPrepared = OnTemplateBegin(ref parent, ref discard),
            (false, _) => _isHtmlPrepared = OnHtmlBegin(ref parent),
        };

        Key = _keyCursor.MoveNext();
        return true;
    }
}