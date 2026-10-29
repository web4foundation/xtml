using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xtml.Templating.Composers;
using Xtml.Dom;

namespace Xtml.Templating;

public enum HtmlType { Wrapper, Template, Node, Raw, Attribute }

[InterpolatedStringHandler]
[StructLayout(LayoutKind.Auto)]
public ref partial struct Html : IDisposable
{
    [ThreadStatic] static BaseComposer? t_scopedComposer;
    private readonly BaseComposer _composer;

    public int FormattedCount { get; private set; }
    public HtmlType Type { get; set; }

    /// <summary>
    /// --- ROOT Html ---
    /// Example:  composer.Compose($"...")
    /// This constructor is not intended to be called directly.  
    /// It's called by compiler-lowered code from methods that use [InterpolatedStringHandlerArgument].
    /// This constructor is for creating the root Html.
    /// </summary>
    public Html(int literalLength, int formattedCount, BaseComposer composer)
        : this(composer, literalLength, formattedCount)
    {
        t_scopedComposer = composer;
    }

    /// <summary>
    /// --- REUSABLE Html (component) ---
    /// Example:  $"...{ MyCustomHtml(c) }..."
    /// This constructor is not intended to be called directly.  
    /// It's called by compiler-lowered code from methods that use [InterpolatedStringHandlerArgument].
    /// This constructor is for reusable Html (think components).  
    /// It's relies on ThreadStatic to find its composer (which was established by the root Html).
    /// </summary>
    public Html(int literalLength, int formattedCount, [CallerLineNumber] int relativeOrder = 0)
        : this(t_scopedComposer ?? throw new NotSupportedException($"This thread's root Html must provide its own composer."), literalLength, formattedCount)
    {
    }

    /// <summary>
    /// --- INLINE Html ---
    /// Example:  $"...{$"...{c}..."}..."
    /// This constructor is not intended to be called directly.  
    /// It's called by compiler-lowered code from methods that use [InterpolatedStringHandlerArgument].
    /// This constructor is for inline Html.  It gets its composer from the parent Html.
    /// </summary>
    public Html(int literalLength, int formattedCount, Html parentHtml, out bool @continue, [CallerLineNumber] int relativeOrder = 0)
        : this(parentHtml._composer, literalLength, formattedCount)
    {
        @continue = true;
        Type = HtmlType.Raw;
    }

    private Html(BaseComposer composer, int literalLength, int formattedCount)
    {
        _composer = composer;
        FormattedCount = formattedCount;
        Type = (literalLength, composer.LiteralLength) switch {
            (0, 0) => HtmlType.Wrapper,
            (> 0, 0) => HtmlType.Template,
            _ => HtmlType.Node
        };

        composer.Grow(literalLength, formattedCount);

        // e.g. $"".  Compiler's lowered code calls no Append*() methods for this use case.
        if (literalLength == 0 && formattedCount == 0)
            AppendLiteral(string.Empty);
    }

    private Html(BaseComposer composer, int iteratorCount)
    {
        FormattedCount = iteratorCount;
        Type = HtmlType.Node;
        _composer = composer;
        // composer.Grow(0, iteratorCount);
    }


    // PARTIAL MARKUP
    // Ex (opening): <div id="something"><figure class="bg-slate-100 rounded-xl p-8 dark:bg-slate-800">
    // or (closing): </div></div></div></div></div></div></div>
    public bool AppendLiteral(string literal, [CallerLineNumber] int relativeOrder = 0)
        => _composer.OnMarkup(ref this, ref literal, relativeOrder);


    // MUTABLE VALUES
    // Ex: <p>Hello { name }, you have { count } clicks at { DateTime.Now }</p>
    public bool AppendFormatted(string value)
        => _composer.OnStringKeyhole(ref this, value);

    public bool AppendFormatted(bool value)
        => _composer.OnBoolKeyhole(ref this, value);

    public bool AppendFormatted(int value, string? format = null)
        => _composer.OnIntKeyhole(ref this, value, format);

    public bool AppendFormatted(long value, string? format = null)
        => _composer.OnLongKeyhole(ref this, value, format);
    
    public bool AppendFormatted(float value, string? format = null)
        => _composer.OnFloatKeyhole(ref this, value, format);
    
    public bool AppendFormatted(double value, string? format = null)
        => _composer.OnDoubleKeyhole(ref this, value, format);
    
    public bool AppendFormatted(decimal value, string? format = null)
        => _composer.OnDecimalKeyhole(ref this, value, format);
    
    public bool AppendFormatted(DateTime value, string? format = null)
        => _composer.OnDateTimeKeyhole(ref this, value, format);
    
    public bool AppendFormatted(DateOnly value, string? format = null)
        => _composer.OnDateOnlyKeyhole(ref this, value, format);
    
    public bool AppendFormatted(TimeSpan value, string? format = null)
        => _composer.OnTimeSpanKeyhole(ref this, value, format);
    
    public bool AppendFormatted(TimeOnly value, string? format = null)
        => _composer.OnTimeOnlyKeyhole(ref this, value, format);
    
    public bool AppendFormatted(Color value, string? format = null)
        => _composer.OnColorKeyhole(ref this, value, format);

    public bool AppendFormatted(Uri value, string? format = null)
        => _composer.OnUriKeyhole(ref this, value, format);


    // EVENT HANDLERS

    // Ex: <button onclick={ Increment }>Clicks: { c }</button>
    // Ex: <button onclick={ () => Increment() }>Clicks: { c }</button>
    public bool AppendFormatted(Action listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
        => AppendEventListener(listener, format, expression);
    private bool AppendEventListener(Action listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
        => _composer.OnListener(ref this, listener, format, expression);

    // Ex: <button onclick={ Increment }>Clicks: { c }</button>
    // Ex: <button onclick={ (Event e) => Increment(e) }>Clicks: { c }</button>
    public bool AppendFormatted(Action<Event> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
        => AppendEventListener(listener, format, expression);
    private bool AppendEventListener(Action<Event> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
        => _composer.OnListener(ref this, listener, format, expression);

    // Ex: <button onclick={ IncrementAsync }>Clicks: { c }</button>
    public bool AppendFormatted(Func<Task> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
        => AppendEventListener(listener, format, expression);
    private bool AppendEventListener(Func<Task> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
        => _composer.OnListener(ref this, listener, format, expression);

    // Ex: <button onclick={ IncrementFromEventAsync }>Clicks: { c }</button>
    public bool AppendFormatted(Func<Event, Task> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
        => AppendEventListener(listener, format, expression);
    private bool AppendEventListener(Func<Event, Task> listener, string? format = null, [CallerArgumentExpression(nameof(listener))] string? expression = null)
        => _composer.OnListener(ref this, listener, format, expression);


    // MUTABLE NODES

    // EX: <div>{ Avatar(user: user) }</div>
    public bool AppendFormatted(
        [InterpolatedStringHandlerArgument("")] scoped Html html, 
        int alignment = -1, // TODO: This doesn't work yet, probably because of empty/wrapper Htmls
        string? format = null, 
        [CallerArgumentExpression(nameof(html))] string? expression = null)
    {
        // Possible point of confusion: 
        // By this line, the `scoped Html html` has already set its own keyholes.

        return _composer.OnHtmlKeyhole(ref this, html, alignment, format, expression);
    }

    // EX: { names.Select(n => new MyComponent(name: n)) }
    public bool AppendFormatted<T>(
        Html.Enumerable<T> enumerable, 
        string? format = null, 
        [CallerArgumentExpression(nameof(enumerable))] string? expression = null)
    {
        var htmls = new Html(_composer, enumerable.Count);
        return _composer.OnIteratorKeyhole(ref this, ref htmls, enumerable, format, expression);
    }

    public readonly void Dispose()
    {
        t_scopedComposer?.Reset();
        t_scopedComposer = null;
    }
}