using System.Text;
using Xtml.BrowserApis;
using Xtml.Templating;

namespace Xtml.Runtime.Utilities;

// TODO: Clean this up.  Many, many memory allocations.

public ref struct KeyholeDumper(IConsole Console, Keyhole[] buffer)
{
    const string CSS_NOTES = "font-size:10px;color:#808080;font-weight:normal;font-family:monospace,monospace;";
    const string CSS_DEFAULT = "font-weight:normal;font-family:monospace,monospace;";
    const string CSS_VARIABLE = "color:#aadbfb;font-weight:normal;font-family:monospace,monospace;";
    const string CSS_NUMBER = "color:#9581f7;font-weight:normal;font-family:monospace,monospace;";
    const string CSS_STRING = "color:#79c8ea;font-weight:normal;font-family:monospace,monospace;";
    const string CSS_TYPE = "color:#6fc3a7;font-weight:normal;font-family:monospace,monospace;";
    const string CSS_OPERATOR = "font-weight:normal;font-family:monospace,monospace;";
    const string CSS_LITERAL = "color:#666666;font-weight:normal;font-family:monospace,monospace;";
    const string CSS_LINK = "font-size:9px;color:#aadbfb;text-decoration:underline;font-weight:normal;font-family:monospace,monospace;";
    const string CSS_BRACE = "color:#ff6600;font-weight:normal;font-family:monospace,monospace;";
    const string CSS_FUNCTION = "color:#f3c349;font-weight:normal;font-family:monospace,monospace;";
    const string CSS_HTML = "font-weight:normal;font-family:monospace,monospace;";

    public void Dump()
    {
        Console.Group("Remote Keyhole Buffer");

        var rootLength = buffer[0].SequenceLength;
        for (int index = 0; index < rootLength; index++)
        {
            ref Keyhole keyhole = ref buffer[index];
            WriteKeyhole(index, keyhole);
        }

        // var keyholeSize = sizeof(Keyhole);
        Console.Log($"%cRemote RAM:\n  buffer:     {170 * 40:n0} bytes\n  keyholes:   {85}\n    values:   {45}\n    pointers: {40}", CSS_NOTES);
        Console.GroupEnd();
    }

    private void WriteKeyhole(int index, Keyhole keyhole, bool isParentAnAttribute = false)
    {
        var key = keyhole.Key != null ? Encoding.UTF8.GetString(keyhole.Key) : "";
        switch (keyhole.Type)
        {
            case KeyholeType.StringLiteral:
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%c ",-24} %c`{InlineString(keyhole.String)}`", CSS_VARIABLE, CSS_LITERAL);
                Console.Log($"\n{keyhole.String}\n\n");
                Console.GroupEnd();
                break;
            case KeyholeType.String:
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%ckey:{key} %c: %c{keyhole.Type}",-28} %o", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, ObjectString(key));
                Console.GroupEnd();
                break;
            case KeyholeType.Integer:
                if (isParentAnAttribute)
                    Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%ckey:{key} %c: %c{keyhole.Type}",-28} %c{keyhole.Integer}", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_NUMBER);
                else
                    Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%ckey:{key} %c: %c{keyhole.Type}",-28} %o", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, ObjectString(key));
                Console.GroupEnd();
                break;
            case KeyholeType.Boolean:
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%ckey:{key} %c: %c{keyhole.Type}",-28} %c{(keyhole.Boolean ? "true" : "false")}", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_NUMBER);
                Console.GroupEnd();
                break;
            case KeyholeType.Color:
                if (isParentAnAttribute)
                    Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%ckey:{key} %c: %c{keyhole.Type}",-28} %c◼ %c#{keyhole.Color.ToRgb():x6}", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, $"color:#{keyhole.Color.ToRgb():x6}", CSS_DEFAULT);
                else
                    Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%ckey:{key} %c: %c{keyhole.Type}",-28} %c◼ %o", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, $"color:#{keyhole.Color.ToRgb():x6}", ObjectString(key));
                Console.GroupEnd();
                break;
            // TODO: Support the other FormatTypes too
            case KeyholeType.EventListener:
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%ckey:{key} %c: %c{keyhole.Type}",-28} {$"%c{{ %c{keyhole.Expression} %c}}"}", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_BRACE, CSS_DEFAULT, CSS_BRACE);
                Console.GroupEnd();
                break;
            case KeyholeType.HtmlRaw:
                int start = keyhole.Sequence.Start.Value;
                int length = keyhole.Sequence.End.Value - start;
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%ckey:{key} %c: %c{keyhole.Type}",-28} %o %cbuffer[{keyhole.Sequence}]", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, ObjectString(key), CSS_LINK);

                for (int i = start; i < start + length; i++)
                {
                    ref var k = ref buffer[i];
                    WriteKeyhole(i, k, isParentAnAttribute: true);
                }
                Console.GroupEnd();
                break;
            case KeyholeType.Html:
                start = keyhole.Sequence.Start.Value;
                length = keyhole.Sequence.End.Value - start;
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%ckey:{key} %c: %c{keyhole.Type}",-28} {$"%c{{ %o %c}}"} %cbuffer[{keyhole.Sequence}]", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_BRACE, ObjectString(key), CSS_BRACE, CSS_LINK);

                for (int i = start; i < start + length; i++)
                {
                    ref var k = ref buffer[i];
                    WriteKeyhole(i, k);
                }
                Console.GroupEnd();
                break;
            case KeyholeType.Iterator:
                start = keyhole.Sequence.Start.Value;
                length = keyhole.Sequence.End.Value - start;
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%ckey:{key} %c: %c{keyhole.Type}",-28} {$"%c({length / 2} items)"} %cbuffer[{keyhole.Sequence}]", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_DEFAULT, CSS_LINK);

                for (int i = start; i < start + length; i++)
                {
                    ref var k = ref buffer[i];
                    WriteKeyhole(i, k);
                }
                Console.GroupEnd();
                break;
            default:
                Console.GroupCollapsed($"{$"[{index}]",-4}  {$"%ckey:{key} %c: %c{keyhole.Type}",-28} %c{keyhole.Double}", CSS_VARIABLE, CSS_OPERATOR, CSS_TYPE, CSS_NUMBER);
                Console.GroupEnd();
                break;
        }
    }

    private static string ObjectString(string key)
    {
        return $"%oglobalThis.keyholes['{key}'].node";
    }

    private static string InlineString(string? value)
    {
        if (value is null)
            return "";

        int maxLength = 100;
        var inlined = new StringBuilder(value)
            .Replace("\n", "")
            .Replace("\r", "")
            .Replace("  ", "")
            .ToString();
        return (inlined.Length > maxLength)
            ? inlined[..(maxLength - 3)] + "..."
            : inlined;
    }
}