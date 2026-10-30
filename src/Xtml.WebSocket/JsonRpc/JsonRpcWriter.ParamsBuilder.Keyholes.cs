using System.Buffers;
using Xtml.Runtime;
using Xtml.Templating;

namespace Xtml.WebSocket.JsonRpc;

internal ref partial struct JsonRpcWriter
{
    private static void WriteRawKey(SyncCtx context, byte[] key)
    {
        context.JsonWriter.Flush();
        key.CopyTo(context.RawWriter.GetSpan(key.Length));
        context.RawWriter.Advance(key.Length);
    }

    public ref partial struct ParamsBuilder
    {
        public ParamsBuilder Param(byte[] key)
        {
            TryStartParamsArray();

            context.JsonWriter.WriteStringValue(key);
            return this;
        }

        public ParamsBuilder Param(in Keyhole value)
        {
            TryStartParamsArray();

            if (value.Type == KeyholeType.Boolean)
            {
                // HTML treats boolean attributes differently.  Send without quotes.
                context.JsonWriter.WriteBooleanValue(value.Boolean);
            }
            else
            {
                context.JsonWriter.WriteStringValueSegment("", false);
                WriteMutableKeyholeValue(in value);
                context.JsonWriter.WriteStringValueSegment("", true);
            }

            return this;
        }

        public ParamsBuilder Param(Span<Keyhole> value)
        {
            TryStartParamsArray();

            WriteRawSequence(value);
            context.JsonWriter.WriteStringValueSegment("", true);

            return this;
        }

        public ParamsBuilder Param(Keyhole[] snapshot, Span<Keyhole> htmlFragment)
        {
            TryStartParamsArray();

            WriteHtml(snapshot, keyholes: htmlFragment);
            context.JsonWriter.WriteStringValueSegment("", true);

            return this;
        }

        public ParamsBuilder Param(ValueTuple<string, byte[]> viewTransitionName)
        {
            TryStartParamsArray();

            context.JsonWriter.WriteStringValueSegment(viewTransitionName.Item1, false);
            WriteRawKey(context, viewTransitionName.Item2);
            context.JsonWriter.WriteStringValueSegment("", true);

            return this;
        }

        public ParamsBuilder Param(ValueTuple<string, int> viewTransitionName)
        {
            TryStartParamsArray();

            Span<char> strInt = stackalloc char[11]; // max int length

            context.JsonWriter.WriteStringValueSegment(viewTransitionName.Item1, false);
            if (viewTransitionName.Item2.TryFormat(strInt, out int length))
                context.JsonWriter.WriteStringValueSegment(strInt[..length], false);
            context.JsonWriter.WriteStringValueSegment("", true);

            return this;
        }

        private void WriteMutableKeyholeValue(in Keyhole keyhole)
        {
            // String and Boolean do not use format strings.
            switch (keyhole.Type)
            {
                case KeyholeType.String:
                    // Must use jsonWriter to write this string with the proper json encoding.
                    context.JsonWriter.WriteStringValueSegment(keyhole.String, false);
                    return;
                case KeyholeType.Boolean:
                    context.JsonWriter.WriteStringValueSegment(keyhole.Boolean ? "true" : "false", false);
                    return;
            }

            // All other mutable values might make use of a format string.  
            // Flush the JSON writer and switch to the raw buffer writer.
            // Use IUtf8SpanFormattable.TryFormat() to write without allocating memory.

            context.JsonWriter.Flush();
            int length = 0;
            int sizeHint = 30;
            switch (keyhole.Type)
            {
                case KeyholeType.Integer:
                    while (!keyhole.Integer.TryFormat(context.RawWriter.GetSpan(sizeHint), out length, keyhole.FormatModifier))
                        GrowSizeHint(ref sizeHint);
                    break;
                case KeyholeType.Long:
                    while (!keyhole.Long.TryFormat(context.RawWriter.GetSpan(sizeHint), out length, keyhole.FormatModifier))
                        GrowSizeHint(ref sizeHint);
                    break;
                case KeyholeType.Float:
                    while (!keyhole.Float.TryFormat(context.RawWriter.GetSpan(sizeHint), out length, keyhole.FormatModifier))
                        GrowSizeHint(ref sizeHint);
                    break;
                case KeyholeType.Double:
                    while (!keyhole.Double.TryFormat(context.RawWriter.GetSpan(sizeHint), out length, keyhole.FormatModifier))
                        GrowSizeHint(ref sizeHint);
                    break;
                case KeyholeType.Decimal:
                    while (!keyhole.Decimal.TryFormat(context.RawWriter.GetSpan(sizeHint), out length, keyhole.FormatModifier))
                        GrowSizeHint(ref sizeHint);
                    break;
                case KeyholeType.DateTime:
                    while (!keyhole.DateTime.TryFormat(context.RawWriter.GetSpan(sizeHint), out length, keyhole.FormatModifier))
                        GrowSizeHint(ref sizeHint);
                    break;
                case KeyholeType.DateOnly:
                    while (!keyhole.DateOnly.TryFormat(context.RawWriter.GetSpan(sizeHint), out length, keyhole.FormatModifier))
                        GrowSizeHint(ref sizeHint);
                    break;
                case KeyholeType.TimeSpan:
                    while (!keyhole.TimeSpan.TryFormat(context.RawWriter.GetSpan(sizeHint), out length, keyhole.FormatModifier))
                        GrowSizeHint(ref sizeHint);
                    break;
                case KeyholeType.TimeOnly:
                    while (!keyhole.TimeOnly.TryFormat(context.RawWriter.GetSpan(sizeHint), out length, keyhole.FormatModifier))
                        GrowSizeHint(ref sizeHint);
                    break;
                case KeyholeType.Color:
                    while (!keyhole.Color.TryFormat(context.RawWriter.GetSpan(sizeHint), out length, keyhole.FormatModifier))
                        GrowSizeHint(ref sizeHint);
                    break;
                case KeyholeType.Uri:
                    // TODO: Fix memory allocation and support format string?
                    context.RawWriter.Write(keyhole.Uri!.ToString());
                    break;
            }
            context.RawWriter.Advance(length);
        }

        private static void GrowSizeHint(ref int sizeHint)
        {
            sizeHint *= 2;
            if (sizeHint > (2 << 20)) // 1MB
                throw new NotSupportedException("🛑 It seems a keyhole value with a format string needed a buffer > 1MB.  Probably misuse?  Needs investigation.");
        }

        private void WriteHtml(Keyhole[] snapshot, Span<Keyhole> keyholes)
        {
            #if DEBUG
            var isHeaderOmitted = false;
            #endif

            for (int i = 1; i < keyholes.Length; i += 2)
            {
                ref var keyhole = ref keyholes[i];
                ref var immutable = ref keyholes[i - 1];
                var literal = immutable.StringLiteral!;

                #if DEBUG
                // This inefficient check only happens in DEBUG.  
                // It's for Hot Reload, when the whole page must change, which never happens in production.
                if (i == 1 && keyhole.Key.SequenceEqual(snapshot[1].Key))
                    isHeaderOmitted = InjectKernel(ref literal);
                #endif

                bool isAttribute = WriteStringLiteral(ref literal, in keyhole, out var attributeName);

                switch (keyhole.Type)
                {
                    case KeyholeType.Html:
                        WriteOpenSentinel(keyhole.Key);
                        WriteHtml(snapshot, snapshot.AsSpan(keyhole.Sequence));
                        WriteCloseSentinel(keyhole.Key);
                        WriteTransition(keyhole.Key, keyhole.FormatModifier);
                        break;
                    case KeyholeType.HtmlRaw:
                        WriteOpenSentinel(keyhole.Key, isAttribute);
                        WriteRawSequence(snapshot.AsSpan(keyhole.Sequence));
                        WriteCloseSentinel(keyhole.Key, isAttribute);
                        break;
                    case KeyholeType.EventListener:
                        WriteListener(keyhole.Key, keyhole.TrimModifier);
                        break;
                    case KeyholeType.Iterator:
                        int start = keyhole.Sequence.Start.Value;
                        int end = keyhole.Sequence.End.Value;
                        for (int i2 = start + 1; i2 < end; i2 += 2)
                        {
                            ref var k = ref snapshot[i2];
                            WriteOpenSentinel(k.Key);
                            WriteHtml(snapshot, snapshot.AsSpan(k.Sequence));
                            WriteCloseSentinel(k.Key);
                        }
                        WriteVoidSentinel(keyhole.Key);
                        break;
                    // The rest are the mutable keyhole values.  They might use format strings.
                    default:
                        if (isAttribute && keyhole.Type == KeyholeType.Boolean)
                        {
                            // Boolean attributes have no opening sentinel and their value 
                            // is written as a part of the prior string literal for the sake of performance.
                            WriteCloseSentinel(keyhole.Key, attributeName);
                        }
                        else
                        {
                            WriteOpenSentinel(keyhole.Key, isAttribute);
                            WriteMutableKeyholeValue(in keyhole);
                            WriteCloseSentinel(keyhole.Key, isAttribute);
                        }
                        break;
                }
            }

            ref var lastLiteral = ref keyholes[^1];
            context.JsonWriter.WriteStringValueSegment(lastLiteral.StringLiteral, false);

            #if DEBUG
            if (isHeaderOmitted)
                context.JsonWriter.WriteStringValueSegment("</body></html>", false);
            #endif
        }

        private bool WriteStringLiteral(ref string literal, in Keyhole keyhole, out ReadOnlySpan<char> attributeName)
        {
            var isAttribute = literal.EndsWith('=');
            if (!isAttribute || keyhole.Type != KeyholeType.Boolean)
            {
                context.JsonWriter.WriteStringValueSegment(literal, false);
                attributeName = default;
                return isAttribute;
            }

            // Boolean attributes are "special".  
            // For example <input checked="false" /> means checked is true.  🤦
            // The only way to make a boolean attribute false is to omit it entirely.
            var span = literal.AsSpan();
            int indexBeforeAttribute = span.LastIndexOf(' ');
            context.JsonWriter.WriteStringValueSegment(
                keyhole.Boolean 
                    ? span[..^1] // Writes the whole literal which includes the attribute name up to the equals sign.
                    : span[..indexBeforeAttribute], // Writes the literal but excludes the attribute name and equals sign
                false);
            attributeName = span[(indexBeforeAttribute + 1)..^1];
            return true;
        }

        private void WriteListener(byte[] key, string? trim)
        {
            context.JsonWriter.WriteStringValueSegment("\"keyholes['", false);
            WriteRawKey(context, key);
            if (trim == string.Empty)
            {
                context.JsonWriter.WriteStringValueSegment("'].dispatchEvent(event)\" key:", false);
            }
            else
            {
                context.JsonWriter.WriteStringValueSegment("'].dispatchEvent(event,'", false);
                context.JsonWriter.WriteStringValueSegment(trim ?? "*", false);
                context.JsonWriter.WriteStringValueSegment("')\" key:", false);
            }
            WriteRawKey(context, key);
        }

        private void WriteRawSequence(Span<Keyhole> keyholes)
        {
            for (int i = 0; i < keyholes.Length; i++)
            {
                ref var keyhole = ref keyholes[i];

                switch (keyhole.Type)
                {
                    case KeyholeType.StringLiteral:
                        context.JsonWriter.WriteStringValueSegment(keyhole.StringLiteral, false);
                        break;
                    // The rest are the mutable keyhole values.  They might use format strings.
                    default:
                        WriteMutableKeyholeValue(in keyhole);
                        break;
                }
            }
        }

        private void WriteOpenSentinel(byte[] key, bool isAttribute = false)
        {
            if (isAttribute)
            {
                context.JsonWriter.WriteStringValueSegment("\"", false);
            }
            else
            {
                context.JsonWriter.WriteStringValueSegment("<!--key:", false);
                WriteRawKey(context, key);
                context.JsonWriter.WriteStringValueSegment("-->", false);
            }
        }

        private void WriteCloseSentinel(byte[] key, bool isAttribute = false)
        {
            if (isAttribute)
            {
                context.JsonWriter.WriteStringValueSegment("\" key:", false);
                WriteRawKey(context, key);
            }
            else
            {
                context.JsonWriter.WriteStringValueSegment("<!--/key:", false);
                WriteRawKey(context, key);
                context.JsonWriter.WriteStringValueSegment("-->", false);
            }
        }

        private void WriteCloseSentinel(byte[] key, scoped ReadOnlySpan<char> booleanAttributeName)
        {
            // Booleans are "special" (see WriteStringLiteral).
            context.JsonWriter.WriteStringValueSegment(" key:", false);
            WriteRawKey(context, key);
            context.JsonWriter.WriteStringValueSegment("=\"", false);
            context.JsonWriter.WriteStringValueSegment(booleanAttributeName, false);
            context.JsonWriter.WriteStringValueSegment("\"", false);
        }

        private void WriteVoidSentinel(byte[] key)
        {
            context.JsonWriter.WriteStringValueSegment("<!--key:", false);
            WriteRawKey(context, key);
            context.JsonWriter.WriteStringValueSegment(" /-->", false);
        }

        private void WriteTransition(byte[] parentKey, string? transition)
        {
            if (transition is null)
                return;
            
            Span<byte> key = stackalloc byte[parentKey.Length];
            for (int i = 0; i < key.Length; i++)
                key[i] = parentKey[i] == ':' ? (byte)'-' : parentKey[i];
            
            // TODO: Many allocations below.
            string k = System.Text.Encoding.UTF8.GetString(key);
            context.JsonWriter.WriteStringValueSegment($$"""
                <style>
                    ::view-transition-group(xtml-fwd-{{k}}, xtml-rev-{{k}}) { animation: none; }
                    ::view-transition-new(xtml-fwd-{{k}}) { width: auto; height: auto; animation: 300ms ease-in-out {{transition}}-in; }
                    ::view-transition-old(xtml-fwd-{{k}}) { width: auto; height: auto; animation: 300ms ease-in-out {{transition}}-out; }
                    ::view-transition-new(xtml-rev-{{k}}) { width: auto; height: auto; animation: 300ms ease-in-out {{transition}}-out reverse; }
                    ::view-transition-old(xtml-rev-{{k}}) { width: auto; height: auto; animation: 300ms ease-in-out {{transition}}-in reverse; }
                </style>
                """, false);
        }

        #if DEBUG
        private bool InjectKernel(ref string literal)
        {
            // NOTE! This only ever runs in DEBUG mode.

            int headIndex = literal.IndexOf("<head>", StringComparison.Ordinal);
            bool isHeadOmitted = headIndex < 0;
            headIndex += 6;

            context.JsonWriter.WriteStringValueSegment(
                isHeadOmitted 
                    ? "<!doctype html><html><head>" 
                    : literal[..headIndex], 
                false);
            
            context.JsonWriter.WriteStringValueSegment("""

                    <!-- Injected by XTML -->
                    <script src="/_app/websocket/ui.js" defer></script>
                    <link href="/_app/base/ui.css" rel="stylesheet" />
                    <meta name="viewport" content="width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no" />
                    <meta charset="UTF-8">

            """, false);

            // Write event handlers set on window or document
            var listeners = context.Bridge?.RootEventListeners ?? [];
            if (listeners?.Count > 0)
            {
                context.JsonWriter.WriteStringValueSegment("\n\n<script>\n", false);

                foreach (var listener in listeners)
                    context.JsonWriter.WriteStringValueSegment($"  {listener.Html ?? ""}\n", false);

                context.JsonWriter.WriteStringValueSegment("</script>\n\n", false);
            }

            if (isHeadOmitted)
                context.JsonWriter.WriteStringValueSegment("</head><body>", false);
            else
                literal = literal[headIndex..];

            return isHeadOmitted;
        }
        #endif
    }
}