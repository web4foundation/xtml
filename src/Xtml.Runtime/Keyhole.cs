using System.Drawing;
using System.Runtime.CompilerServices;

namespace Xtml.Runtime;

public struct Keyhole
{
    private byte[] _key;        // 8 bytes
    private object? _reference; // 8 bytes
    private long _value1;       // 8 bytes
    private int _value2;        // 4 bytes
    private KeyholeType _type;  // 4 bytes
    private string? _modifier;  // 8 bytes

    public byte[] Key { readonly get => _key; set => _key = value; }
    public KeyholeType Type { readonly get => _type; set => _type = value; }
    
    // --- shared backing field: modifier ---
    // These all come from the format string of string interpolation, the part after `:`
    // e.g. $"Your total cost is {amount:c} today."
    // However, some keyhole types use it for purposes other than formatting.
    // KeyholeType.EventListener uses it to trim event properties before serialization.
    // KeyholeType.Html/Iterator uses it to indicate viewTransitionName.
    public string? FormatModifier { readonly get => _modifier; set => _modifier = value; }
    public string? TransitionModifier { readonly get => _modifier; set => _modifier = value; }
    public string? TrimModifier { readonly get => _modifier; set => _modifier = value; }

    // --- shared backing field: reference ---
    // These properties all use `reference` as their backing field.  Since each keyhole
    // can only represent a single type at a time we can save a great deal of 
    // wasted RAM and boost memory locality of keyhole buffers by sharing a single backing store.
    public string? StringLiteral    { set => _reference = value; get => _reference as string; }
    public string? String           { set => _reference = value; get => _reference as string; }
    public Uri? Uri                 { set => _reference = value; get => _reference as Uri; }
    public string? Expression       { set => _reference = value; get => _reference as string; }
    public object? Tag              { set => _reference = value; get => _reference; }

    // --- shared backing field: value1 ---
    // These properties all use `value1` as their backing field.  Since each keyhole
    // can only represent a single type at a time we can save a great deal of 
    // wasted RAM and boost memory locality of keyhole buffers by sharing a single backing store 
    // and converting to and from a 64 bit number.  The primary use case is to check
    // equality between two keyholes and we can bypass type conversion and compare 
    // value1's directly (as long at the types match too).
    public bool Boolean { readonly get => _value1 != 0; set => _value1 = value ? 1 : 0; }
    public int Integer { readonly get => (int)_value1; set => _value1 = value; }
    public long Long { readonly get => _value1; set => _value1 = value; }
    public float Float { readonly get => (float)BitConverter.Int64BitsToDouble(_value1); set => _value1 = BitConverter.DoubleToInt64Bits(value); }
    public double Double { readonly get => BitConverter.Int64BitsToDouble(_value1); set => _value1 = BitConverter.DoubleToInt64Bits(value); }
    public decimal Decimal { readonly get => (decimal)BitConverter.Int64BitsToDouble(_value1); set => _value1 = BitConverter.DoubleToInt64Bits((double)value); } // Note: lossy precision here
    public DateTime DateTime { readonly get => new(_value1); set => _value1 = value.Ticks; }
    public DateOnly DateOnly { readonly get => DateOnly.FromDayNumber((int)_value1); set => _value1 = value.DayNumber; }
    public TimeSpan TimeSpan { readonly get => new(_value1); set => _value1 = value.Ticks; }
    public TimeOnly TimeOnly { readonly get => new(_value1); set => _value1 = value.Ticks; }
    public Color Color { readonly get => Color.FromArgb((int)_value1); set => _value1 = value.ToArgb(); }

    public Range Sequence { get => SequenceStart..(SequenceStart + SequenceLength); }
    public int SequenceStart { get => (int)((ulong)_value1 >> 32); set => _value1 = (long)((ulong)value << 32) | (uint)_value1; }
    public int SequenceLength { get => (int)_value1; set => _value1 = (long)((ulong)_value1 & 0xFFFFFFFF00000000) | (uint)value; }

    // --- shared backing field: value2 ---
    // These are "helper properties" and use `value2` as their backing field.  
    // Like the properties that use value1, they aim to conserve memory width in keyhole 
    // buffers by reusing one backing field across a number of properties that are only 
    // used depending on the keyhole type.
    public int RelativeOrder { get => _value2; set => _value2 = value; }
    public int WriteHead { get => _value2; set => _value2 = value; }

    public static bool Equals(ref Keyhole left, ref Keyhole right)
        => left.Type == right.Type && left.Type switch
        {
            KeyholeType.StringLiteral
                => Object.ReferenceEquals(left._reference, right._reference),
            KeyholeType.String
                => left.String == right.String,
            KeyholeType.Uri
                => left._reference == right._reference && left.FormatModifier == right.FormatModifier,
            KeyholeType.Boolean
                => left._value1 == right._value1,
            KeyholeType.Integer or
            KeyholeType.Long or
            KeyholeType.Float or
            KeyholeType.Double or
            KeyholeType.Decimal or
            KeyholeType.DateTime or
            KeyholeType.DateOnly or
            KeyholeType.TimeSpan or
            KeyholeType.TimeOnly or
            KeyholeType.Color
                => left._value1 == right._value1 && left.FormatModifier == right.FormatModifier,
            _ => throw new NotSupportedException()
        };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]    
    public void SetValue<T>(T value)
    {
        switch (value)
        {
            case string s: String = s; break;
            case bool b: Boolean = b; break;
            case int i: Integer = i; break;
            case long l: Long = l; break;
            case float f: Float = f; break;
            case double d: Double = d; break;
            case decimal m: Decimal = m; break;
            case DateTime dt: DateTime = dt; break;
            case DateOnly dO: DateOnly = dO; break;
            case TimeSpan ts: TimeSpan = ts; break;
            case TimeOnly tO: TimeOnly = tO; break;
            case Color c: Color = c; break;
            case Uri u: Uri = u; break;
            default: throw new NotSupportedException($"Type {typeof(T)} not supported");
        }
    }
}