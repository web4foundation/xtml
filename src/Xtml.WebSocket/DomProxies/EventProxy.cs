
using System.Buffers;
using System.Drawing;
using System.Text.Json;
using Microsoft.Extensions.ObjectPool;
using Xtml.Dom;
using Xtml.Dom.Events;
using Xtml.Dom.Events.Subsets;
using Xtml.WebSocket.Buffers;

namespace Xtml.WebSocket.Dom;

/// <summary>
/// A WebSocket-optimized implementation of the Event interface.
/// Implementing as a regular POCO would be rather large from a memory standpoint 
/// since there are so many potential properties to cover, 
/// so this takes inspiration from JavaScript-style objects
/// by using a dictionary as the backing store in conjunction with lazy-reads from 
/// the ReadOnlySequence<byte> that came from the network.
/// Additionally, if the only properties lazily accessed are value types (e.g. bool, int, long, double)
/// then this struct will avoid allocating a surplus of memory on the heap by pooling its dictionary.
/// </summary>
public struct EventProxy(
    ReadOnlySequence<byte> rentedBuffer, 
    ReadOnlySequence<byte> eventBuffer, 
    IWindow window, 
    Propagation propagation
) : Event, IDisposable
{
    private static readonly ObjectPool<Dictionary<string, long>> _valueDictionaryPool = ObjectPool.Create<Dictionary<string, long>>();
    private Dictionary<string, long>? _values = null; // Here, longs are used to encode bools, ints, and doubles.
    private Dictionary<string, object>? _references = null;

    public void Dispose()
    {
        // TODO: Custom ObjectDisposedException? It'd be helpful to throw a custom message about trying to use LazyEvent beyond the scope of its event listener.  

        _values?.Clear();
        _references?.Clear();
        if (_values is not null)
            _valueDictionaryPool.Return(_values);
        rentedBuffer.ReturnToPool();
    }

    private void LazyParse(bool canIgnoreRefTypes = false)
    {
        if (_values is null)
            Parse(canIgnoreRefTypes);

        if (!canIgnoreRefTypes && _references is null)
            Parse(canIgnoreRefTypes);
    }

    private void Parse(bool canIgnoreRefTypes = false)
    {
        try
        {
            var reader = new Utf8JsonReader(eventBuffer);
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var (propertyName, type) = GetStringAndType(ref reader);
                    if (propertyName is not null)
                    {
                        reader.Read();
                        long? value = type switch
                        {
                            _ when type == typeof(bool) => reader.GetBoolean() ? 1 : 0,
                            _ when type == typeof(int) => reader.GetInt32(),
                            _ when type == typeof(long) => reader.GetInt64(),
                            _ when type == typeof(double) => BitConverter.DoubleToInt64Bits(reader.GetDouble()),
                            _ => null,
                        };
                        if (value is long l)
                        {
                            _values ??= _valueDictionaryPool.Get();
                            _values[propertyName] = l;
                        }

                        if (!canIgnoreRefTypes)
                        {
                            if (type == typeof(string))
                            {
                                _references ??= [];
                                var str = reader.GetString();
                                if (str is not null)
                                    _references[propertyName] = str;
                            }
                            else if (type == typeof(ABG))
                            {
                                _references ??= [];
                                // TODO: Implement
                            }
                            else if (type == typeof(DataTransferContainer))
                            {
                                _references ??= [];
                                // TODO: Implement
                            }
                            else if (type == typeof(DOMException))
                            {
                                _references ??= [];
                                // TODO: Implement
                            }
                            else if (type == typeof(EventTarget))
                            {
                                _references ??= [];
                                ParseEventTarget(reader, propertyName);
                            }
                            else if (type == typeof(TouchPoint[]))
                            {
                                _references ??= [];
                                // TODO: Implement
                            }
                            else if (type == typeof(XYZ))
                            {
                                _references ??= [];
                                // TODO: Implement
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new ApplicationException("There was an unexpected error parsing this event.", ex);
        }
    }

    private object? GetReference(string propName)
    {
        LazyParse(canIgnoreRefTypes: false);
        return _references?.GetValueOrDefault(propName);
    }

    private bool? GetBool(string propName)
    {
        LazyParse(canIgnoreRefTypes: true);
        return _values is null
            ? null
            : _values.TryGetValue(propName, out long value) ? value != 0 : null;
    }

    private int? GetInt(string propName)
    {
        LazyParse(canIgnoreRefTypes: true);
        return _values is null
            ? null
            : _values.TryGetValue(propName, out long value) ? (int)value : null;
    }

    private long? GetLong(string propName)
    {
        LazyParse(canIgnoreRefTypes: true);
        return _values is null
            ? null
            : _values.TryGetValue(propName, out long value) ? value : null;
    }

    private double? GetDouble(string propName)
    {
        LazyParse(canIgnoreRefTypes: true);
        return _values is null
            ? null
            : _values.TryGetValue(propName, out long value)
                ? BitConverter.Int64BitsToDouble(value)
                : null;
    }

    private EventTarget? GetTarget(string propName)
    {
        LazyParse(canIgnoreRefTypes: false);
        return _references?.GetValueOrDefault(propName) as EventTarget;
    }

    private void ParseEventTarget(Utf8JsonReader reader, string propertyName)
    {
        string id = "", type = "", value = "", name = "";
        bool @checked = false;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                _references![propertyName] = new EventTarget(id, name, type, @checked, value);
                return;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                _ = ParseString(reader, "id", ref id) ||
                    ParseString(reader, "type", ref type) ||
                    ParseString(reader, "value", ref value) ||
                    ParseString(reader, "name", ref name) ||
                    ParseBool(reader, "checked", ref @checked);
            }
        }
    }

    private bool ParseString(Utf8JsonReader reader, string propertyName, ref string value)
    {
        if (reader.ValueTextEquals(propertyName))
        {
            _references ??= [];
            reader.Read();
            value = reader.GetString() ?? "";
            return true;
        }
        return false;
    }

    private bool ParseBool(Utf8JsonReader reader, string propertyName, ref bool value)
    {
        if (reader.ValueTextEquals(propertyName))
        {
            _references ??= [];
            reader.Read();
            value = reader.GetBoolean();
            return true;
        }
        return false;
    }

    private static ValueTuple<string, Type> GetStringAndType(ref Utf8JsonReader reader)
    {
        if (!reader.HasValueSequence)
            return GetStringAndType(reader.ValueSpan);

        // Bummer.  This property name is split across multiple segments so stackalloc some memory to handle this.
        // Arbitrary max length to prevent stack-overflow attacks with huge property names.
        int length = (int)reader.ValueSequence.Length;
        if (length > 32)
            return (string.Empty, typeof(Exception));

        Span<byte> buffer = stackalloc byte[length];
        reader.ValueSequence.CopyTo(buffer);
        return GetStringAndType(buffer);
    }

    // TODO: Benchmarking needed here.  Is this faster than a Dictionay.AlternateLookup?  
    // Every property fits within 128-bit NEON SIMD-optimized comparison (i.e. even Apple's M1-M5) 
    // so does that make an ~90-case switch faster than a Dictionary lookup?
    // Don't forget that this is a highly-concurrent codepath so see if there are any 
    // Dictionaries that have lock-free reads.
    private static ValueTuple<string, Type> GetStringAndType(ReadOnlySpan<byte> p) => p switch
    {
        _ when p.SequenceEqual("absolute"u8) => ("absolute", typeof(bool)),
        _ when p.SequenceEqual("acceleration"u8) => ("acceleration", typeof(XYZ)),
        _ when p.SequenceEqual("accelerationIncludingGravity"u8) => ("accelerationIncludingGravity", typeof(XYZ)),
        _ when p.SequenceEqual("alpha"u8) => ("alpha", typeof(double)),
        _ when p.SequenceEqual("altitudeAngle"u8) => ("altitudeAngle", typeof(double)),
        _ when p.SequenceEqual("altKey"u8) => ("altKey", typeof(bool)),
        _ when p.SequenceEqual("animationName"u8) => ("animationName", typeof(string)),
        _ when p.SequenceEqual("azimuthAngle"u8) => ("azimuthAngle", typeof(double)),
        _ when p.SequenceEqual("beta"u8) => ("beta", typeof(double)),
        _ when p.SequenceEqual("bubbles"u8) => ("bubbles", typeof(bool)),
        _ when p.SequenceEqual("button"u8) => ("button", typeof(int)),
        _ when p.SequenceEqual("buttons"u8) => ("buttons", typeof(int)),
        _ when p.SequenceEqual("cancelable"u8) => ("cancelable", typeof(bool)),
        _ when p.SequenceEqual("changedTouches"u8) => ("changedTouches", typeof(TouchPoint[])),
        _ when p.SequenceEqual("clientX"u8) => ("clientX", typeof(double)),
        _ when p.SequenceEqual("clientY"u8) => ("clientY", typeof(double)),
        _ when p.SequenceEqual("code"u8) => ("code", typeof(string)),
        _ when p.SequenceEqual("colNo"u8) => ("colNo", typeof(int)),
        _ when p.SequenceEqual("composed"u8) => ("composed", typeof(bool)),
        _ when p.SequenceEqual("ctrlKey"u8) => ("ctrlKey", typeof(bool)),
        _ when p.SequenceEqual("currentTarget"u8) => ("currentTarget", typeof(EventTarget)),
        _ when p.SequenceEqual("data"u8) => ("data", typeof(string)),
        _ when p.SequenceEqual("dataTransfer"u8) => ("dataTransfer", typeof(DataTransferContainer)),
        _ when p.SequenceEqual("defaultPrevented"u8) => ("defaultPrevented", typeof(bool)),
        _ when p.SequenceEqual("deltaMode"u8) => ("deltaMode", typeof(int)),
        _ when p.SequenceEqual("deltaX"u8) => ("deltaX", typeof(double)),
        _ when p.SequenceEqual("deltaY"u8) => ("deltaY", typeof(double)),
        _ when p.SequenceEqual("deltaZ"u8) => ("deltaZ", typeof(double)),
        _ when p.SequenceEqual("detail"u8) => ("detail", typeof(long)),
        _ when p.SequenceEqual("elapsedTime"u8) => ("elapsedTime", typeof(double)),
        _ when p.SequenceEqual("error"u8) => ("error", typeof(DOMException)),
        _ when p.SequenceEqual("eventPhase"u8) => ("eventPhase", typeof(int)),
        _ when p.SequenceEqual("fileName"u8) => ("fileName", typeof(string)),
        _ when p.SequenceEqual("gamma"u8) => ("gamma", typeof(double)),
        _ when p.SequenceEqual("height"u8) => ("height", typeof(int)),
        _ when p.SequenceEqual("inputType"u8) => ("inputType", typeof(string)),
        _ when p.SequenceEqual("interval"u8) => ("interval", typeof(double)),
        _ when p.SequenceEqual("isComposing"u8) => ("isComposing", typeof(bool)),
        _ when p.SequenceEqual("isPrimary"u8) => ("isPrimary", typeof(bool)),
        _ when p.SequenceEqual("isTrusted"u8) => ("isTrusted", typeof(bool)),
        _ when p.SequenceEqual("key"u8) => ("key", typeof(string)),
        _ when p.SequenceEqual("length"u8) => ("length", typeof(int)),
        _ when p.SequenceEqual("lengthComputable"u8) => ("lengthComputable", typeof(bool)),
        _ when p.SequenceEqual("lineNo"u8) => ("lineNo", typeof(int)),
        _ when p.SequenceEqual("loaded"u8) => ("loaded", typeof(long)),
        _ when p.SequenceEqual("location"u8) => ("location", typeof(int)),
        _ when p.SequenceEqual("message"u8) => ("message", typeof(string)),
        _ when p.SequenceEqual("metaKey"u8) => ("metaKey", typeof(bool)),
        _ when p.SequenceEqual("movementX"u8) => ("movementX", typeof(double)),
        _ when p.SequenceEqual("movementY"u8) => ("movementY", typeof(double)),
        _ when p.SequenceEqual("newState"u8) => ("newState", typeof(string)),
        _ when p.SequenceEqual("newUrl"u8) => ("newUrl", typeof(string)),
        _ when p.SequenceEqual("offsetX"u8) => ("offsetX", typeof(double)),
        _ when p.SequenceEqual("offsetY"u8) => ("offsetY", typeof(double)),
        _ when p.SequenceEqual("oldState"u8) => ("oldState", typeof(string)),
        _ when p.SequenceEqual("oldUrl"u8) => ("oldUrl", typeof(string)),
        _ when p.SequenceEqual("pageX"u8) => ("pageX", typeof(double)),
        _ when p.SequenceEqual("pageY"u8) => ("pageY", typeof(double)),
        _ when p.SequenceEqual("persisted"u8) => ("persisted", typeof(bool)),
        _ when p.SequenceEqual("pointerID"u8) => ("pointerID", typeof(int)),
        _ when p.SequenceEqual("pointerType"u8) => ("pointerType", typeof(string)),
        _ when p.SequenceEqual("pressure"u8) => ("pressure", typeof(double)),
        _ when p.SequenceEqual("propertyName"u8) => ("propertyName", typeof(string)),
        _ when p.SequenceEqual("pseudoElement"u8) => ("pseudoElement", typeof(string)),
        _ when p.SequenceEqual("relatedTarget"u8) => ("relatedTarget", typeof(EventTarget)),
        _ when p.SequenceEqual("repeat"u8) => ("repeat", typeof(bool)),
        _ when p.SequenceEqual("rotationRate"u8) => ("rotationRate", typeof(ABG)),
        _ when p.SequenceEqual("screenX"u8) => ("screenX", typeof(double)),
        _ when p.SequenceEqual("screenY"u8) => ("screenY", typeof(double)),
        _ when p.SequenceEqual("shiftKey"u8) => ("shiftKey", typeof(bool)),
        _ when p.SequenceEqual("skipped"u8) => ("skipped", typeof(bool)),
        _ when p.SequenceEqual("submitter"u8) => ("submitter", typeof(EventTarget)),
        _ when p.SequenceEqual("tangentialPressure"u8) => ("tangentialPressure", typeof(double)),
        _ when p.SequenceEqual("target"u8) => ("target", typeof(EventTarget)),
        _ when p.SequenceEqual("targetTouches"u8) => ("targetTouches", typeof(TouchPoint[])),
        _ when p.SequenceEqual("timeStamp"u8) => ("timeStamp", typeof(double)),
        _ when p.SequenceEqual("tiltX"u8) => ("tiltX", typeof(double)),
        _ when p.SequenceEqual("tiltY"u8) => ("tiltY", typeof(double)),
        _ when p.SequenceEqual("total"u8) => ("total", typeof(long)),
        _ when p.SequenceEqual("touches"u8) => ("touches", typeof(TouchPoint[])),
        _ when p.SequenceEqual("twist"u8) => ("twist", typeof(double)),
        _ when p.SequenceEqual("type"u8) => ("type", typeof(string)),
        _ when p.SequenceEqual("width"u8) => ("width", typeof(int)),
        _ when p.SequenceEqual("x"u8) => ("x", typeof(double)),
        _ when p.SequenceEqual("y"u8) => ("y", typeof(double)),
        _ when p.SequenceEqual("id"u8) => ("id", typeof(string)),
        _ when p.SequenceEqual("type"u8) => ("type", typeof(string)),
        _ when p.SequenceEqual("value"u8) => ("value", typeof(string)),
        _ when p.SequenceEqual("name"u8) => ("name", typeof(string)),
        _ when p.SequenceEqual("checked"u8) => ("checked", typeof(bool)),
        _ => (string.Empty, typeof(Exception)),
    };

    public bool? Absolute => GetBool("absolute");
    bool IDeviceOrientationSubset.Absolute => Absolute ?? default;

    public XYZ? Acceleration { get; private set; } = null; // TODO:
    XYZ IDeviceMotionSubset.Acceleration => Acceleration ?? XYZ.Empty;

    public XYZ? AccelerationIncludingGravity { get; private set; } = null; // TODO:
    XYZ IDeviceMotionSubset.AccelerationIncludingGravity => AccelerationIncludingGravity ?? XYZ.Empty;

    public double? Alpha => GetDouble("alpha");
    double IDeviceOrientationSubset.Alpha => Alpha ?? default;

    public double? AltitudeAngle => GetDouble("altitudeAngle");
    double IAnglesSubset.AltitudeAngle => AltitudeAngle ?? default;

    public bool? AltKey => GetBool("altKey");
    bool IModifierAltSubset.AltKey => AltKey ?? default;

    public string? AnimationName => GetReference("animationName") as string;
    string IAnimationSubset.AnimationName => AnimationName ?? string.Empty;

    public double? AzimuthAngle => GetDouble("azimuthAngle");
    double IAnglesSubset.AzimuthAngle => AzimuthAngle ?? default;

    public double? Beta => GetDouble("beta");
    double IDeviceOrientationSubset.Beta => Beta ?? default;

    public bool? Bubbles => GetBool("bubbles");
    bool IEvent.Bubbles => Bubbles ?? default;

    public Button? Button => GetInt("button") switch { int v => (Button)v, _ => null };
    Button IButtonsSubset.Button => Button ?? Xtml.Dom.Events.Button.Main;

    public ButtonFlag? Buttons => GetInt("button") switch { int v => (ButtonFlag)v, _ => null };
    ButtonFlag IButtonsSubset.Buttons => Buttons ?? ButtonFlag.None;

    public bool? Cancelable => GetBool("cancelable");
    bool IEvent.Cancelable => Cancelable ?? default;

    public TouchPoint[]? ChangedTouches { get; private set; } = null; // TODO:
    TouchPoint[] ITouchesSubset.ChangedTouches => ChangedTouches ?? [];

    public double? ClientX => GetDouble("clientX");
    double IClientXYSubset.ClientX => ClientX ?? default;

    public double? ClientY => GetDouble("clientY");
    double IClientXYSubset.ClientY => ClientY ?? default;

    public string? Code => GetReference("code") as string;
    string IKeysSubset.Code => Code ?? string.Empty;

    public int? ColNo => GetInt("colNo");
    int IErrorSubset.ColNo => ColNo ?? default;

    public bool? Composed => GetBool("composed");
    bool IEvent.Composed => Composed ?? default;

    public bool? CtrlKey => GetBool("ctrlKey");
    bool IModifierCtrlSubset.CtrlKey => CtrlKey ?? default;

    public EventTarget? CurrentTarget => GetTarget("currentTarget");
    EventTarget ICurrentTargetSubset.CurrentTarget => CurrentTarget ?? EventTarget.Empty;

    public string? Data => GetReference("data") as string;
    string IDataSubset.Data => Data ?? string.Empty;

    public DataTransferContainer? DataTransfer { get; private set; } = null; // TODO:
    DataTransferContainer IDataTransferSubset.DataTransfer => DataTransfer ?? DataTransferContainer.Empty;

    public bool? DefaultPrevented => GetBool("defaultPrevented");
    bool IEvent.DefaultPrevented => DefaultPrevented ?? default;

    public DeltaMode? DeltaMode => GetInt("deltaMode") switch { int v => (DeltaMode)v, _ => null };
    DeltaMode IDeltasSubset.DeltaMode => DeltaMode ?? Xtml.Dom.Events.DeltaMode.Pixel;

    public double? DeltaX => GetDouble("deltaX");
    double IDeltasSubset.DeltaX => DeltaX ?? default;

    public double? DeltaY => GetDouble("deltaY");
    double IDeltasSubset.DeltaY => DeltaY ?? default;

    public double? DeltaZ => GetDouble("deltaZ");
    double IDeltasSubset.DeltaZ => DeltaZ ?? default;

    public long? Detail => GetLong("detail");
    long IDetailSubset.Detail => Detail ?? default;

    public double? ElapsedTime => GetDouble("elapsedTime");
    double IAnimationSubset.ElapsedTime => ElapsedTime ?? default;

    public DOMException? Error { get; private set; } = null; // TODO:
    DOMException IErrorSubset.Error => Error ?? DOMException.Empty;

    public EventPhase? EventPhase => GetInt("eventPhase") switch { int v => (EventPhase)v, _ => null };
    EventPhase IEvent.EventPhase => EventPhase ?? Xtml.Dom.Events.EventPhase.None;

    public string? FileName => GetReference("fileName") as string;
    string IErrorSubset.FileName => FileName ?? string.Empty;

    public double? Gamma => GetDouble("gamma");
    double IDeviceOrientationSubset.Gamma => Gamma ?? default;

    public int? Height => GetInt("height");
    int IWidthHeightSubset.Height => Height ?? default;

    public string? InputType => GetReference("inputType") as string;
    string IInputEvent<string>.InputType => InputType ?? string.Empty;
    string IInputEvent<bool>.InputType => InputType ?? string.Empty;
    string IInputEvent<Color>.InputType => InputType ?? string.Empty;
    string IInputEvent<Uri>.InputType => InputType ?? string.Empty;
    string IInputEvent<int>.InputType => InputType ?? string.Empty;
    string IInputEvent<long>.InputType => InputType ?? string.Empty;
    string IInputEvent<float>.InputType => InputType ?? string.Empty;
    string IInputEvent<double>.InputType => InputType ?? string.Empty;
    string IInputEvent<decimal>.InputType => InputType ?? string.Empty;
    string IInputEvent<DateTime>.InputType => InputType ?? string.Empty;
    string IInputEvent<DateOnly>.InputType => InputType ?? string.Empty;
    string IInputEvent<TimeOnly>.InputType => InputType ?? string.Empty;

    public double? Interval => GetDouble("interval");
    double IDeviceMotionSubset.Interval => Interval ?? default;

    public bool? IsComposing => GetBool("isComposing");
    bool IIsComposingSubset.IsComposing => IsComposing ?? default;

    public bool? IsPrimary => GetBool("isPrimary");
    bool IPointerSubset.IsPrimary => IsPrimary ?? default;

    public bool? IsTrusted => GetBool("isTrusted");
    bool IEvent.IsTrusted => IsTrusted ?? default;

    public string? Key => GetReference("key") as string;
    string IKeysSubset.Key => Key ?? string.Empty;

    public int? Length => GetInt("length");
    int ILengthSubset.Length => Length ?? default;

    public bool? LengthComputable => GetBool("lengthComputable");
    bool IProgressSubset.LengthComputable => LengthComputable ?? default;

    public int? LineNo => GetInt("lineNo");
    int IErrorSubset.LineNo => LineNo ?? default;

    public long? Loaded => GetLong("loaded");
    long IProgressSubset.Loaded => Loaded ?? default;

    public KeyLocation? Location => GetInt("location") switch { int v => (KeyLocation)v, _ => null };
    KeyLocation IKeysSubset.Location => Location ?? KeyLocation.Standard;

    public string? Message => GetReference("message") as string;
    string IErrorSubset.Message => Message ?? string.Empty;

    public bool? MetaKey => GetBool("metaKey");
    bool IModifierMetaSubset.MetaKey => MetaKey ?? default;

    public double? MovementX => GetDouble("movementX");
    double IMovementXYSubset.MovementX => MovementX ?? default;

    public double? MovementY => GetDouble("movementY");
    double IMovementXYSubset.MovementY => MovementY ?? default;

    public string? NewState => GetReference("newState") as string;
    string IStatesSubset.NewState => NewState ?? string.Empty;

    public string? NewUrl => GetReference("newUrl") as string;
    string IHashChangeSubset.NewUrl => NewUrl ?? string.Empty;

    public double? OffsetX => GetDouble("offsetX");
    double IOffsetXYSubset.OffsetX => OffsetX ?? default;

    public double? OffsetY => GetDouble("offsetY");
    double IOffsetXYSubset.OffsetY => OffsetY ?? default;

    public string? OldState => GetReference("oldState") as string;
    string IStatesSubset.OldState => OldState ?? string.Empty;

    public string? OldUrl => GetReference("oldUrl") as string;
    string IHashChangeSubset.OldUrl => OldUrl ?? string.Empty;

    public double? PageX => GetDouble("pageX");
    double IPageXYSubset.PageX => PageX ?? default;

    public double? PageY => GetDouble("pageY");
    double IPageXYSubset.PageY => PageY ?? default;

    public bool? Persisted => GetBool("persisted");
    bool IPersistedSubset.Persisted => Persisted ?? default;

    public int? PointerID => GetInt("pointerID");
    int IPointerSubset.PointerID => PointerID ?? default;

    public string? PointerType => GetReference("pointerType") as string;
    string IPointerSubset.PointerType => PointerType ?? string.Empty;

    public double? Pressure => GetDouble("pressure");
    double IPressuresSubset.Pressure => Pressure ?? default;

    public string? PropertyName => GetReference("propertyName") as string;
    string IAnimationSubset.PropertyName => PropertyName ?? string.Empty;

    public string? PseudoElement => GetReference("pseudoElement") as string;
    string IAnimationSubset.PseudoElement => PseudoElement ?? string.Empty;

    public EventTarget? RelatedTarget => GetTarget("relatedTarget");
    EventTarget IRelatedTargetSubset.RelatedTarget => RelatedTarget ?? EventTarget.Empty;

    public bool? Repeat => GetBool("repeat");
    bool IKeysSubset.Repeat => Repeat ?? default;

    public ABG? RotationRate { get; private set; } = null; // TODO:
    ABG IDeviceMotionSubset.RotationRate => RotationRate ?? ABG.Empty;

    public double? ScreenX => GetDouble("screenX");
    double IScreenXYSubset.ScreenX => ScreenX ?? default;

    public double? ScreenY => GetDouble("screenY");
    double IScreenXYSubset.ScreenY => ScreenY ?? default;

    public bool? ShiftKey => GetBool("shiftKey");
    bool IModifierShiftSubset.ShiftKey => ShiftKey ?? default;

    public bool? Skipped => GetBool("skipped");
    bool ISkippedSubset.Skipped => Skipped ?? default;

    public EventTarget? Submitter => GetTarget("submitter");
    EventTarget ISubmitterSubset.Submitter => Submitter ?? EventTarget.Empty;

    public double? TangentialPressure => GetDouble("tangentialPressure");
    double IPressuresSubset.TangentialPressure => TangentialPressure ?? default;

    public EventTarget? Target => GetTarget("target");
    EventTarget ITargetSubset.Target => Target ?? EventTarget.Empty;
    EventTarget<string> ITargetSubset<string>.Target => Target ?? EventTarget.Empty;
    EventTarget<bool> ITargetSubset<bool>.Target => Target ?? EventTarget.Empty;
    EventTarget<int> ITargetSubset<int>.Target => Target ?? EventTarget.Empty;
    EventTarget<long> ITargetSubset<long>.Target => Target ?? EventTarget.Empty;
    EventTarget<float> ITargetSubset<float>.Target => Target ?? EventTarget.Empty;
    EventTarget<double> ITargetSubset<double>.Target => Target ?? EventTarget.Empty;
    EventTarget<decimal> ITargetSubset<decimal>.Target => Target ?? EventTarget.Empty;
    EventTarget<DateTime> ITargetSubset<DateTime>.Target => Target ?? EventTarget.Empty;
    EventTarget<DateOnly> ITargetSubset<DateOnly>.Target => Target ?? EventTarget.Empty;
    EventTarget<TimeOnly> ITargetSubset<TimeOnly>.Target => Target ?? EventTarget.Empty;
    EventTarget<Color> ITargetSubset<Color>.Target => Target ?? EventTarget.Empty;
    EventTarget<Uri> ITargetSubset<Uri>.Target => Target ?? EventTarget.Empty;
    EventTarget<string> IInputEvent<string>.Target => Target ?? EventTarget.Empty;
    EventTarget<bool> IInputEvent<bool>.Target => Target ?? EventTarget.Empty;
    EventTarget<int> IInputEvent<int>.Target => Target ?? EventTarget.Empty;
    EventTarget<long> IInputEvent<long>.Target => Target ?? EventTarget.Empty;
    EventTarget<float> IInputEvent<float>.Target => Target ?? EventTarget.Empty;
    EventTarget<double> IInputEvent<double>.Target => Target ?? EventTarget.Empty;
    EventTarget<decimal> IInputEvent<decimal>.Target => Target ?? EventTarget.Empty;
    EventTarget<DateTime> IInputEvent<DateTime>.Target => Target ?? EventTarget.Empty;
    EventTarget<DateOnly> IInputEvent<DateOnly>.Target => Target ?? EventTarget.Empty;
    EventTarget<TimeOnly> IInputEvent<TimeOnly>.Target => Target ?? EventTarget.Empty;
    EventTarget<Color> IInputEvent<Color>.Target => Target ?? EventTarget.Empty;
    EventTarget<Uri> IInputEvent<Uri>.Target => Target ?? EventTarget.Empty;

    public TouchPoint[]? TargetTouches { get; private set; } = null; // TODO:
    TouchPoint[] ITouchesSubset.TargetTouches => TargetTouches ?? [];

    public double? TimeStamp => GetDouble("timeStamp");
    double IEvent.TimeStamp => TimeStamp ?? default;

    public double? TiltX => GetDouble("tiltX");
    double ITiltsSubset.TiltX => TiltX ?? default;

    public double? TiltY => GetDouble("tiltY");
    double ITiltsSubset.TiltY => TiltY ?? default;

    public long? Total => GetLong("total");
    long IProgressSubset.Total => Total ?? default;

    public TouchPoint[]? Touches { get; private set; } = null; // TODO:
    TouchPoint[] ITouchesSubset.Touches => Touches ?? [];

    public double? Twist => GetDouble("twist");
    double IPointerSubset.Twist => Twist ?? default;

    public string? Type => GetReference("type") as string;
    string IEvent.Type => Type ?? string.Empty;

    public IWindow? View => window;
    IWindow IViewSubset.View => View!;

    public int? Width => GetInt("width");
    int IWidthHeightSubset.Width => Width ?? default;

    public double? X => GetDouble("x");
    double IXYSubset.X => X ?? default;

    public double? Y => GetDouble("y");
    double IXYSubset.Y => Y ?? default;

    public void StopPropagation() => propagation.Stop();

    public void StopImmediatePropagation() => propagation.StopImmediate();
}
