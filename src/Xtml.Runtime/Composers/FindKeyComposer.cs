using System.Runtime.CompilerServices;
using Xtml.Templating;
using Xtml.Dom;

namespace Xtml.Runtime.Composers;

public class FindKeyComposer : BaseKeyComposer
{
    [ThreadStatic] static FindKeyComposer? t_reusable;
    public static FindKeyComposer Shared => t_reusable ??= new FindKeyComposer();

    private EventListener _eventListener = default;
    private byte[] _keyBuffer = [];
    private Memory<byte> _searchKey;
    private bool _isFound = false;

    public EventListener FindEventListener(ReadOnlySpan<byte> key, Func<Html> template)
    {
        if (_keyBuffer.Length < key.Length)
            _keyBuffer = new byte[key.Length];
        key.CopyTo(_keyBuffer);
        return FindEventListener(_keyBuffer.AsMemory(..key.Length), template);
    }

    public EventListener FindEventListener(Memory<byte> key, Func<Html> template)
    {
        _searchKey = key;
        return Interpolate($"{template()}");
    }

    private EventListener Interpolate([InterpolatedStringHandlerArgument("")] Html html)
    {
        // ^ That's the root Html getting passed in above.
        // By the time you've reached this line, the templating work has already completed.
        
        // Hang onto the result before html.Dispose() resets this class.
        var result = _eventListener;

        // html.Dispose() calls composer.Reset() which sets everything back to null.
        html.Dispose();

        // Do something interesting with the result.
        return result;
    }

    // Note: Returning false short circuits InterpolatedStringHandler from calling any
    // subsequent AppendFormatted() or AppendLiteral() methods.  
    // And isFound is used to trickle that upwards to all parent Htmls.
    
    protected override bool OnKeyhole(ref Html parent)
        => !_isFound && base.OnKeyhole(ref parent);

    public override bool OnHtmlBegin(ref Html html, int relativeOrder = -1)
        => !_isFound && base.OnHtmlBegin(ref html, relativeOrder);

    public override bool OnHtmlKeyhole(ref Html parent, scoped Html html, int relativeOrder = -1, string? transition = null, string? expression = null)
        => !_isFound && base.OnHtmlKeyhole(ref parent, html, relativeOrder, transition, expression);

    public override bool OnHtmlEnd(ref Html parent, scoped Html html, int relativeOrder = -1, string? transition = null, string? expression = null)
        => !_isFound && base.OnHtmlEnd(ref parent, html, relativeOrder, transition, expression);

    public override bool OnIteratorBegin(ref Html parent, ref Html htmls, string? transition = null, string? expression = null)
        => !_isFound && base.OnIteratorBegin(ref parent, ref htmls, transition, expression);

    public override bool OnIteratorKeyhole<T>(ref Html parent, ref Html htmls, Html.Enumerable<T> enumerable, string? transition = null, string? expression = null)
        => !_isFound && base.OnIteratorKeyhole(ref parent, ref htmls, enumerable, transition, expression);

    public override bool OnIteratorEnd(ref Html parent, ref Html htmls, string? transition = null, string? expression = null)
        => !_isFound && base.OnIteratorEnd(ref parent, ref htmls, transition, expression);

    public override bool OnListener(ref Html parent, Action listener, string? trim = null, string? expression = null)
        => OnListener(ref parent, listener);
    public override bool OnListener(ref Html parent, Action<Event> listener, string? trim = null, string? expression = null)
        => OnListener(ref parent, listener);
    public override bool OnListener(ref Html parent, Func<Task> listener, string? trim = null, string? expression = null)
        => OnListener(ref parent, listener);
    public override bool OnListener(ref Html parent, Func<Event, Task> listener, string? trim = null, string? expression = null)
        => OnListener(ref parent, listener);

    private bool OnListener<T>(ref Html parent, T listener)
    {
        if (_isFound)
            return false;
            
        base.OnKeyhole(ref parent);
        
        if (Key.SequenceEqual(_searchKey.Span))
        {
            switch (listener)
            {
                case Action action:
                    _eventListener.Action = action;
                    break;
                case Action<Event> actionEvent:
                    _eventListener.ActionEvent = actionEvent;
                    break;
                case Func<Task> func:
                    _eventListener.Func = func;
                    break;
                case Func<Event, Task> funcEvent:
                    _eventListener.FuncEvent = funcEvent;
                    break;
            }
            _isFound = true;
            return false;
        }
        
        return true;
    }

    public override void Reset()
    {
        _searchKey = default;
        _isFound = false;
        _eventListener = default;
        base.Reset();
    }
}