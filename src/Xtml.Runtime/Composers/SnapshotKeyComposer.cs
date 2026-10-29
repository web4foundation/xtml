using System.Buffers;
using System.Drawing;
using System.Runtime.CompilerServices;
using Xtml.Templating;
using Xtml.Dom;

namespace Xtml.Runtime.Composers;

public class SnapshotKeyComposer : BaseKeyComposer
{
    // TODO: Don't forget to implement the high watermark logic.
    private static int _highWaterMark = 2048;

    private Keyhole[] _buffer = [];
    private int _writeHead = 0;
    private readonly List<int> _cursors = [0];
    private int Cursor
    {
        get => _cursors[_keyCursor.CurrentDepth];
        set
        {
            if (_cursors.Count == _keyCursor.CurrentDepth)
                _cursors.Add(value);
            else
                _cursors[_keyCursor.CurrentDepth] = value;
        }
    }

    public Keyhole[] Capture(Func<Html> template)
    {
        _buffer = ArrayPool<Keyhole>.Shared.Rent(_highWaterMark);
        return Interpolate($"{template()}");
    }
    
    public Keyhole[] Capture(Keyhole[] buffer, Func<Html> template)
    {
        _buffer = buffer;
        return Interpolate($"{template()}");
    }

    private Keyhole[] Interpolate([InterpolatedStringHandlerArgument("")] Html html)
    {
        // ^ That's the root Html getting passed in above.
        // By the time you've reached this line, the templating work has already completed.
        
        // Hang onto the result before html.Dispose() resets this class.
        var result = _buffer;

        // html.Dispose() calls composer.Reset() which sets snapshot to [].
        html.Dispose();

        // Do something interesting with the result.
        return result;
    }
    
    public override bool OnTemplateBegin(ref Html html, ref string markup)
    {
        base.OnTemplateBegin(ref html, ref markup);
        Cursor = 0;
        _writeHead += html.FormattedCount * 2 + 2;
        return true;
    }

    public override bool OnMarkup(ref Html parent, ref string literal, int relativeOrder = -1)
    {
        base.OnMarkup(ref parent, ref literal, relativeOrder);

        int index = Cursor;
        ref var keyhole = ref _buffer[index];
        keyhole.String = literal;
        keyhole.Type = KeyholeType.StringLiteral;
        keyhole.SequenceStart = index;
        keyhole.SequenceLength = parent.FormattedCount * 2 + 1;
       
        Cursor = index + 1;
        return true;
    }

    public override bool OnStringKeyhole(ref Html parent, string value) => OnKeyhole(ref parent, value, KeyholeType.String);
    public override bool OnBoolKeyhole(ref Html parent, bool value) => OnKeyhole(ref parent, value, KeyholeType.Boolean);
    public override bool OnIntKeyhole(ref Html parent, int value, string? format = null) => OnKeyhole(ref parent, value, KeyholeType.Integer, format);
    public override bool OnLongKeyhole(ref Html parent, long value, string? format = null) => OnKeyhole(ref parent, value, KeyholeType.Long, format);
    public override bool OnFloatKeyhole(ref Html parent, float value, string? format = null) => OnKeyhole(ref parent, value, KeyholeType.Float, format);
    public override bool OnDoubleKeyhole(ref Html parent, double value, string? format = null)  => OnKeyhole(ref parent, value, KeyholeType.Double, format);
    public override bool OnDecimalKeyhole(ref Html parent, decimal value, string? format = null) => OnKeyhole(ref parent, value, KeyholeType.Decimal, format);
    public override bool OnDateTimeKeyhole(ref Html parent, DateTime value, string? format = null) => OnKeyhole(ref parent, value, KeyholeType.DateTime, format);
    public override bool OnDateOnlyKeyhole(ref Html parent, DateOnly value, string? format = null) => OnKeyhole(ref parent, value, KeyholeType.DateOnly, format);
    public override bool OnTimeSpanKeyhole(ref Html parent, TimeSpan value, string? format = null) => OnKeyhole(ref parent, value, KeyholeType.TimeSpan, format);
    public override bool OnTimeOnlyKeyhole(ref Html parent, TimeOnly value, string? format = null) => OnKeyhole(ref parent, value, KeyholeType.TimeOnly, format);
    public override bool OnColorKeyhole(ref Html parent, Color value, string? format = null) => OnKeyhole(ref parent, value, KeyholeType.Color, format);
    public override bool OnUriKeyhole(ref Html parent, Uri value, string? format = null) => OnKeyhole(ref parent, value, KeyholeType.Uri, format);
    private bool OnKeyhole<T>(ref Html parent, T value, KeyholeType type, string? format = null)
    {
        EnsureOddIndex();
        base.OnKeyhole(ref parent);

        int index = Cursor;
        ref var keyhole = ref _buffer[index];
        keyhole.Key = Key;
        keyhole.SetValue(value);
        keyhole.Type = type;
        keyhole.FormatModifier = format;

        Cursor = index + 1;
        return true;
    }

    public override bool OnHtmlBegin(ref Html html, int relativeOrder = -1)
    {
        EnsureOddIndex();
        int index = Cursor;
        base.OnHtmlBegin(ref html, relativeOrder);

        ref var keyhole = ref _buffer[index];
        keyhole.Key = Key;
        keyhole.SequenceStart = _writeHead;
        keyhole.SequenceLength = html.FormattedCount * 2 + 1;
        keyhole.Type = html.Type == HtmlType.Raw ? KeyholeType.HtmlRaw : KeyholeType.Html;
        keyhole.RelativeOrder = relativeOrder;

        Cursor = _writeHead;
        _writeHead += html.FormattedCount * 2 + 2;
        return true;
    }

    public override bool OnHtmlEnd(ref Html parent, scoped Html html, int relativeOrder = -1, string? transition = null, string? expression = null)
    {
        // Prevent bleeding
        ref var tail = ref _buffer[Cursor];
        tail.String = string.Empty;
        tail.Type = KeyholeType.StringLiteral;

        base.OnHtmlEnd(ref parent, html, relativeOrder, transition, expression);

        int index = Cursor;
        ref var keyhole = ref _buffer[index];
        keyhole.TransitionModifier = transition;
        keyhole.Expression = expression;
        if (relativeOrder >= 0)
            keyhole.RelativeOrder = relativeOrder;

        Cursor = index + 1;
        return true;
    }

    public override bool OnIteratorBegin(ref Html parent, ref Html htmls, string? transition = null, string? expression = null)
    {
        EnsureOddIndex();
        int index = Cursor;
        base.OnIteratorBegin(ref parent, ref htmls, transition, expression);

        ref var keyhole = ref _buffer[index];
        keyhole.Key = Key;
        keyhole.Type = KeyholeType.Iterator;
        keyhole.TransitionModifier = transition;
        keyhole.Expression = expression;
        keyhole.SequenceStart = _writeHead;
        keyhole.SequenceLength = htmls.FormattedCount * 2 + 1;

        Cursor = _writeHead;
        _writeHead += htmls.FormattedCount * 2 + 2;
        return true;
    }

    public override bool OnIteratorKeyhole<T>(ref Html parent, ref Html htmls, Html.Enumerable<T> enumerable, string? transition = null, string? expression = null)
    {
        OnIteratorBegin(ref parent, ref htmls, transition, expression);

        var enumerator = enumerable.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var (selector, item) = enumerator.CurrentDeconstructed;
            var html = selector(item);
            htmls.AppendFormatted(html);

            _buffer[Cursor - 1].Tag = item; // TODO: Memory allocation?
        }

        OnIteratorEnd(ref parent, ref htmls, transition, expression);
        return true;
    }

    public override bool OnIteratorEnd(ref Html parent, ref Html htmls, string? transition = null, string? expression = null)
    {
        // Prevent bleeding
        ref var keyhole = ref _buffer[Cursor];
        keyhole.String = string.Empty;
        keyhole.Type = KeyholeType.StringLiteral;

        base.OnIteratorEnd(ref parent, ref htmls, transition, expression);

        Cursor++;
        return true;
    }

    public override bool OnListener(ref Html parent, Action listener, string? trim = null, string? expression = null) => OnListener(ref parent, string.Empty, expression);
    public override bool OnListener(ref Html parent, Action<Event> listener, string? trim = null, string? expression = null) => OnListener(ref parent, trim, expression);
    public override bool OnListener(ref Html parent, Func<Task> listener, string? trim = null, string? expression = null) => OnListener(ref parent, string.Empty, expression);
    public override bool OnListener(ref Html parent, Func<Event, Task> listener, string? trim = null, string? expression = null) => OnListener(ref parent, trim, expression);
    private bool OnListener(ref Html parent, string? format = null, string? expression = null)
    {
        EnsureOddIndex();
        base.OnKeyhole(ref parent);

        int index = Cursor;
        ref var keyhole = ref _buffer[index];
        keyhole.Key = Key;
        keyhole.Type = KeyholeType.EventListener;
        keyhole.TrimModifier = format;
        keyhole.Expression = expression;
        
        Cursor = index + 1;
        return true;
    }

    public override void Reset()
    {
        _buffer = [];
        _writeHead = 0;
        _cursors[0] = 0;
        base.Reset();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureOddIndex()
    {
        int index = Cursor;
        if (index % 2 == 0)
        {
            ref var keyhole = ref _buffer[index];
            keyhole.String = string.Empty;
            keyhole.Type = KeyholeType.StringLiteral;        
            Cursor = index + 1;
        }
    }

    [ThreadStatic] static SnapshotKeyComposer? t_reusable;
    public static SnapshotKeyComposer Shared => t_reusable ??= new SnapshotKeyComposer();
}