using System.Buffers;
using System.Drawing;
using Xtml.Templating;
using Xtml.Templating.Composers;
using Xtml.Dom;
using Xtml.Runtime.Utilities;

namespace Xtml.Runtime.Composers;

public class HtmlKeyComposer(IBufferWriter<byte> writer, WindowBuilder window)
    : BaseKeyComposer, IStreamingComposer
{
    private ReadOnlyMemory<char>? _deferredLiteral = null;
    private bool _isHeadOmitted = false;

    public IBufferWriter<byte> Writer { get; set; } = writer;
    public WindowBuilder Window { get; set; } = window;

    public override bool OnTemplateBegin(ref Html html, ref string markup)
    {
        InjectKernel(ref markup);

        return true;
    }

    public override bool OnTemplateEnd(ref Html html)
    {
        if (_isHeadOmitted)
        {
            Writer.Write("""
                    
                </body>
            </html>
            """u8);
        }

        return true;
    }

    public override bool OnMarkup(ref Html parent, ref string literal, int relativeOrder = -1)
    {
        base.OnMarkup(ref parent, ref literal, relativeOrder);

        // This makes the assumption that keyholes preceded with an '=' are always attributes.  
        if (literal.EndsWith('='))
            _deferredLiteral = literal.AsMemory();
        else
            Writer.Write(literal);

        return true;
    }

    public override bool OnStringKeyhole(ref Html parent, string value)
    {
        base.OnStringKeyhole(ref parent, value);

        if (parent.Type is HtmlType.Raw or HtmlType.Attribute)
        {
            Writer.Write(value);
        }
        else if (HandleDeferredLiteral())
        {
            // ex: `"{value}" key:{Key}`
            Writer.Write("\""u8);
            Writer.Write(value);
            Writer.Write("\" key:"u8);
            Writer.Write(Key);
        }
        else
        {
            // ex: `<!--key:{Key}-->{value}<!--/key:{Key}-->`
            Writer.Write("<!--key:"u8, Key, "-->"u8);
            Writer.Write(value);
            Writer.Write("<!--/key:"u8, Key, "-->"u8);
        }

        return true;
    }

    public override bool OnBoolKeyhole(ref Html parent, bool value)
    {
        base.OnBoolKeyhole(ref parent, value);

        if (parent.Type is HtmlType.Raw or HtmlType.Attribute)
        {
            Writer.Write(value ? "true" : "false");
        }
        else if (HandleDeferredLiteral(value, out var attributeName))
        {
            // ex: ` key:{Key}="{attributeName}"`
            Writer.Write(" key:"u8);
            Writer.Write(Key);
            Writer.Write("=\""u8);
            Writer.Write(attributeName);
            Writer.Write("\""u8);
        }
        else
        {
            // ex: `<!--key:{Key}-->{b}<!--/key:{Key}-->`
            Writer.Write("<!--key:"u8, Key, "-->"u8);
            Writer.Write(value ? "true" : "false");
            Writer.Write("<!--/key:"u8, Key, "-->"u8);
        }

        return true;
    }

    public override bool OnIntKeyhole(ref Html parent, int value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnLongKeyhole(ref Html parent, long value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnFloatKeyhole(ref Html parent, float value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnDoubleKeyhole(ref Html parent, double value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnDecimalKeyhole(ref Html parent, decimal value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnDateTimeKeyhole(ref Html parent, DateTime value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnDateOnlyKeyhole(ref Html parent, DateOnly value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnTimeSpanKeyhole(ref Html parent, TimeSpan value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    public override bool OnTimeOnlyKeyhole(ref Html parent, TimeOnly value, string? format = null) => OnUtf8SpanFormattable(ref parent, value, format);
    private bool OnUtf8SpanFormattable<T>(ref Html parent, T value, string? format = null)
        where T : struct, IUtf8SpanFormattable
    {
        base.OnKeyhole(ref parent);

        if (parent.Type is HtmlType.Raw or HtmlType.Attribute)
        {
            Writer.Write(value, format);
        }
        else if (HandleDeferredLiteral())
        {
            // ex: `"{value:format}" key:{Key}`
            Writer.Write("\""u8);
            Writer.Write(value, format);
            Writer.Write("\" key:"u8);
            Writer.Write(Key);            
        }
        else
        {
            // ex: `<!--key:{Key}-->{value:format}<!--/key:{Key}-->`
            Writer.Write("<!--key:"u8, Key, "-->"u8);
            Writer.Write(value, format);
            Writer.Write("<!--/key:"u8, Key, "-->"u8);            
        }

        return true;
    }

    public override bool OnColorKeyhole(ref Html parent, Color value, string? format = null)
    {
        base.OnColorKeyhole(ref parent, value, format);

        if (parent.Type is HtmlType.Raw or HtmlType.Attribute)
        {
            Writer.Write(value, format);
        }
        else if (HandleDeferredLiteral())
        {
            // ex: `"{value:format}" key:{Key}`
            Writer.Write("\""u8);
            Writer.Write(value, format);
            Writer.Write("\" key:"u8);
            Writer.Write(Key);
        }
        else
        {
            // ex: `<!--key:{Key}-->{value:format}<!--/key:{Key}-->`
            Writer.Write("<!--key:"u8, Key, "-->"u8);
            Writer.Write(value, format);
            Writer.Write("<!--/key:"u8, Key, "-->"u8);
        }

        return true;
    }

    public override bool OnUriKeyhole(ref Html parent, Uri value, string? format = null)
        => OnStringKeyhole(ref parent, value.ToString()); // TODO: Memory allocation!
        
    public override bool OnHtmlBegin(ref Html html, int relativeOrder = -1)
    {
        base.OnHtmlBegin(ref html, relativeOrder);

        // TODO: An attribute sequence or a raw tag might have a child HTML from an <if> tag
        // and therefore needs to somehow consider SuppressKeyholes... but there's no access to parent???

        if (HandleDeferredLiteral())
        {
            // The attribute's value is a combination of string literals and keyholes.
            // So they will be written without any sentinels but we still need to surround it with quotation marks.
            Writer.Write("\""u8);
            html.Type = HtmlType.Attribute;
        }
        else
        {
            // ex: `<!--key:{Key}-->`
            Writer.Write("<!--key:"u8, Key, "-->"u8);
        }

        return true;
    }

    public override bool OnHtmlEnd(ref Html parent, scoped Html html, int relativeOrder = -1, string? transition = null, string? expression = null)
    {
        base.OnHtmlEnd(ref parent, html, relativeOrder, transition, expression);

        if (html.Type is HtmlType.Attribute)
        {
            // ex: `" key:{Key}`
            Writer.Write("\" key:"u8);
            Writer.Write(Key);
        }
        else
        {
            // ex: `<!--/key:{Key}-->`
            Writer.Write("<!--/key:"u8, Key, "-->"u8);
            if (transition is {} trns)
                InjectTransition(trns);
        }

        return true;
    }

    public override bool OnIteratorBegin(ref Html parent, ref Html htmls, string? transition = null, string? expression = null)
    {
        base.OnIteratorBegin(ref parent, ref htmls, transition, expression);
        return true;
    }

    public override bool OnIteratorEnd(ref Html parent, ref Html htmls, string? transition = null, string? expression = null)
    {
        base.OnIteratorEnd(ref parent, ref htmls, transition, expression);
        
        // Keyhole to represent the loop itself, useful for zero-length use cases.
        // ex: `<!--key:{Key} /-->`
        Writer.Write("<!--key:"u8, Key, " /-->"u8);

        return true;
    }

    public override bool OnListener(ref Html parent, Action listener, string? trim = null, string? expression = null) => OnListener(ref parent, includeEventArg: false, trim);
    public override bool OnListener(ref Html parent, Action<Event> listener, string? trim = null, string? expression = null) => OnListener(ref parent, includeEventArg: true, trim);
    public override bool OnListener(ref Html parent, Func<Task> listener, string? trim = null, string? expression = null) => OnListener(ref parent, includeEventArg: false, trim);
    public override bool OnListener(ref Html parent, Func<Event, Task> listener, string? trim = null, string? expression = null) => OnListener(ref parent, includeEventArg: true, trim);
    private bool OnListener(ref Html parent, bool includeEventArg, string? trim = null)
    {
        base.OnKeyhole(ref parent);

        HandleDeferredLiteral();

        if (!includeEventArg)
        {
            // ex: `"keyholes['1:2:3'].dispatchEvent(event)" key:1:2:3`
            Writer.Write("\"keyholes['"u8);
            Writer.Write(Key);
            Writer.Write("'].dispatchEvent(event)\" key:"u8);
            Writer.Write(Key);
        }
        else if (trim is not null)
        {
            // ex: `"keyholes['1:2:3'].dispatchEvent(event,'x,y'))" key:1:2:3`
            Writer.Write("\"keyholes['"u8);
            Writer.Write(Key);
            Writer.Write("'].dispatchEvent(event,'"u8);
            Writer.Write(trim);
            Writer.Write("')\" key:"u8);
            Writer.Write(Key);
        }
        else if (trim is null)
        {
            // ex: `"keyholes['1:2:3'].dispatchEvent(event,'*'))" key:1:2:3`
            Writer.Write("\"keyholes['"u8);
            Writer.Write(Key);
            Writer.Write("'].dispatchEvent(event,'*')\" key:"u8);
            Writer.Write(Key);
        }

        return true;
    }

    private bool HandleDeferredLiteral()
    {
        if (!_deferredLiteral.HasValue)
            return false;

        Writer.Write(_deferredLiteral.Value);

        _deferredLiteral = null;
        return true;
    }

    /// <summary>
    /// This overload is specifically for boolean attributes and is responsible for the complicated logic regarding deferred literals.
    /// HTML has some quirky rules around boolean attributes that don't follow the normal `key="value"` pattern.
    /// https://developer.mozilla.org/en-US/docs/Glossary/Boolean/HTML
    /// Hence the reason to defer writing the literal until we see if the next keyhole will be a boolean.
    /// If so, and if the value is false, then the attribute name needs to NOT be written.
    /// But the keyhole's key still needs to be written.
    /// The prior string literal will look something like `...<input type="checkbox" checked=`
    /// Note: We know they always end with `=`.
    /// </summary>
    /// <param name="value">The value of the boolean attribute.</param>
    /// <param name="attributeName">The name of the boolean attribute.</param>
    /// <returns></returns>
    private bool HandleDeferredLiteral(bool value, out ReadOnlySpan<char> attributeName)
    {
        if (!_deferredLiteral.HasValue)
        {
            attributeName = [];
            return false;
        }

        var span = _deferredLiteral.Value.Span;
        int indexBeforeAttribute = span.LastIndexOf(' ');

        ArgumentOutOfRangeException.ThrowIfLessThan(indexBeforeAttribute, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(indexBeforeAttribute, span.Length - 2);

        if (value)
            Writer.Write(span[..^1]); // Writes the whole literal which includes the attribute name up to the equals sign.
        else
            Writer.Write(span[..indexBeforeAttribute]); // Writes the literal but excludes the attribute name and equals sign
            
        attributeName = span[(indexBeforeAttribute + 1)..^1];
        _deferredLiteral = null;

        return true;
    }

    private void InjectKernel(ref string literal)
    {
        int headIndex = literal.IndexOf("<head>", StringComparison.Ordinal);
        _isHeadOmitted = headIndex < 0;
        headIndex += 6;

        if (_isHeadOmitted)
        {
            Writer.Write("""
            <!doctype html>
            <html>
                <head>

            """u8);
        }
        else
        {
            Writer.Write(literal.AsSpan(..headIndex));
        }

        Writer.Write("""

                <!-- Injected by XTML -->
                <script src="/_app/websocket/ui.js" defer></script>
                <link href="/_app/base/ui.css" rel="stylesheet" />
                <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no" />

        """u8);

        // Write event handlers set on window or document
        if (Window.Listeners.Count > 0)
        {
            Writer.Write("\n\n<script>\n"u8);

            foreach (var listener in Window.Listeners)
            {
                // ex: `  {listener.Html}\n`
                Writer.Write("  "u8);
                Writer.Write(listener.Html ?? "");
                Writer.Write("\n"u8);
            }

            Writer.Write("</script>\n\n"u8);
        }

        if (_isHeadOmitted)
        {
            Writer.Write("""

                </head>
                <body>

            """u8);
        }
        else
        {
            // Pre-handle the work of OnMarkup, except consider `offset`.
            // Then set `literal` to "" so the next OnMarkup no-ops.
            int offset = _isHeadOmitted ? 0 : headIndex;
            if (literal.EndsWith('='))
                _deferredLiteral = literal.AsMemory(offset);
            else
                Writer.Write(literal.AsSpan(offset));

            // Clear the literal since we've already written it out, and we don't want it to be written again by the next OnMarkup.
            literal = string.Empty;
        }
    }

    private void InjectTransition(string transition)
    {
        Span<byte> key = stackalloc byte[Key.Length];
        for (int i = 0; i < key.Length; i++)
            key[i] = Key[i] == ':' ? (byte)'-' : Key[i];
        Writer.WriteRaw($$"""
            <style>
                ::view-transition-group(xtml-fwd-{{key}}, xtml-rev-{{key}}) { animation: none; }
                ::view-transition-new(xtml-fwd-{{key}}) { width: auto; height: auto; animation: 300ms ease-in-out {{transition}}-in; }
                ::view-transition-old(xtml-fwd-{{key}}) { width: auto; height: auto; animation: 300ms ease-in-out {{transition}}-out; }
                ::view-transition-new(xtml-rev-{{key}}) { width: auto; height: auto; animation: 300ms ease-in-out {{transition}}-out reverse; }
                ::view-transition-old(xtml-rev-{{key}}) { width: auto; height: auto; animation: 300ms ease-in-out {{transition}}-in reverse; }
            </style>
            """);
    }

    public override void Reset()
    {
        Writer = null!;
        Window = null!;
        base.Reset();
    }

    [ThreadStatic] static HtmlKeyComposer? reusable;
    public static HtmlKeyComposer Reuse(IBufferWriter<byte> writer, WindowBuilder window) 
    {
        if (reusable is {} composer)
        {
            composer.Writer = writer;
            composer.Window = window;
            return composer;
        }

        return reusable = new HtmlKeyComposer(writer, window);
    }
}