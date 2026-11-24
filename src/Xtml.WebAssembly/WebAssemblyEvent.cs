using System.Drawing;
using Xtml.BrowserApis;
using Xtml.BrowserApis.Events;
using Xtml.BrowserApis.Events.Subsets;

namespace Xtml.WebAssembly;

public partial record struct WebAssemblyEvent(
    bool? Absolute = null,
    XYZ? Acceleration = null,
    XYZ? AccelerationIncludingGravity = null,
    double? Alpha = null,
    double? AltitudeAngle = null,
    bool? AltKey = null,
    string? AnimationName = null,
    double? AzimuthAngle = null,
    double? Beta = null,
    bool? Bubbles = null,
    Button? Button = null,
    ButtonFlag? Buttons = null,
    bool? Cancelable = null,
    TouchPoint[]? ChangedTouches = null,
    double? ClientX = null,
    double? ClientY = null,
    string? Code = null,
    int? ColNo = null,
    bool? Composed = null,
    bool? CtrlKey = null,
    EventTarget? CurrentTarget = null,
    string? Data = null,
    DataTransferContainer? DataTransfer = null,
    bool? DefaultPrevented = null,
    DeltaMode? DeltaMode = null,
    double? DeltaX = null,
    double? DeltaY = null,
    double? DeltaZ = null,
    long? Detail = null,
    double? ElapsedTime = null,
    DOMException? Error = null,
    EventPhase? EventPhase = null,
    string? FileName = null,
    double? Gamma = null,
    int? Height = null,
    string? InputType = null,
    double? Interval = null,
    bool? IsComposing = null,
    bool? IsPrimary = null,
    bool? IsTrusted = null,
    string? Key = null,
    int? Length = null,
    bool? LengthComputable = null,
    int? LineNo = null,
    long? Loaded = null,
    KeyLocation? Location = null,
    string? Message = null,
    bool? MetaKey = null,
    double? MovementX = null,
    double? MovementY = null,
    string? NewState = null,
    string? NewUrl = null,
    double? OffsetX = null,
    double? OffsetY = null,
    string? OldState = null,
    string? OldUrl = null,
    double? PageX = null,
    double? PageY = null,
    bool? Persisted = null,
    int? PointerID = null,
    string? PointerType = null,
    double? Pressure = null,
    string? PropertyName = null,
    string? PseudoElement = null,
    EventTarget? RelatedTarget = null,
    bool? Repeat = null,
    ABG? RotationRate = null,
    double? ScreenX = null,
    double? ScreenY = null,
    bool? ShiftKey = null,
    bool? Skipped = null,
    EventTarget? Submitter = null,
    double? TangentialPressure = null,
    EventTarget? Target = null,
    TouchPoint[]? TargetTouches = null,
    double? TimeStamp = null,
    double? TiltX = null,
    double? TiltY = null,
    long? Total = null,
    TouchPoint[]? Touches = null,
    double? Twist = null,
    string? Type = null,
    int? Width = null,
    double? X = null,
    double? Y = null
) : Event
{
    bool IDeviceOrientationSubset.Absolute => Absolute ?? default;
    XYZ IDeviceMotionSubset.Acceleration => Acceleration ?? XYZ.Empty;
    XYZ IDeviceMotionSubset.AccelerationIncludingGravity => AccelerationIncludingGravity ?? XYZ.Empty;
    double IDeviceOrientationSubset.Alpha => Alpha ?? default;
    double IAnglesSubset.AltitudeAngle => AltitudeAngle ?? default;
    bool IModifierAltSubset.AltKey => AltKey ?? default;
    string IAnimationSubset.AnimationName => AnimationName ?? string.Empty;
    double IAnglesSubset.AzimuthAngle => AzimuthAngle ?? default;
    double IDeviceOrientationSubset.Beta => Beta ?? default;
    bool IEvent.Bubbles => Bubbles ?? default;
    Button IButtonsSubset.Button => Button ?? Xtml.BrowserApis.Events.Button.Main;
    ButtonFlag IButtonsSubset.Buttons => Buttons ?? ButtonFlag.None;
    bool IEvent.Cancelable => Cancelable ?? default;
    TouchPoint[] ITouchesSubset.ChangedTouches => ChangedTouches ?? [];
    double IClientXYSubset.ClientX => ClientX ?? default;
    double IClientXYSubset.ClientY => ClientY ?? default;
    string IKeysSubset.Code => Code ?? string.Empty;
    int IErrorSubset.ColNo => ColNo ?? default;
    bool IEvent.Composed => Composed ?? default;
    bool IModifierCtrlSubset.CtrlKey => CtrlKey ?? default;
    EventTarget ICurrentTargetSubset.CurrentTarget => CurrentTarget ?? EventTarget.Empty;
    string IDataSubset.Data => Data ?? string.Empty;
    DataTransferContainer IDataTransferSubset.DataTransfer => DataTransfer ?? DataTransferContainer.Empty;
    bool IEvent.DefaultPrevented => DefaultPrevented ?? default;
    DeltaMode IDeltasSubset.DeltaMode => DeltaMode ?? Xtml.BrowserApis.Events.DeltaMode.Pixel;
    double IDeltasSubset.DeltaX => DeltaX ?? default;
    double IDeltasSubset.DeltaY => DeltaY ?? default;
    double IDeltasSubset.DeltaZ => DeltaZ ?? default;
    long IDetailSubset.Detail => Detail ?? default;
    double IAnimationSubset.ElapsedTime => ElapsedTime ?? default;
    DOMException IErrorSubset.Error => Error ?? DOMException.Empty;
    EventPhase IEvent.EventPhase => EventPhase ?? Xtml.BrowserApis.Events.EventPhase.None;
    string IErrorSubset.FileName => FileName ?? string.Empty;
    double IDeviceOrientationSubset.Gamma => Gamma ?? default;
    int IWidthHeightSubset.Height => Height ?? default;
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
    double IDeviceMotionSubset.Interval => Interval ?? default;
    bool IIsComposingSubset.IsComposing => IsComposing ?? default;
    bool IPointerSubset.IsPrimary => IsPrimary ?? default;
    bool IEvent.IsTrusted => IsTrusted ?? default;
    string IKeysSubset.Key => Key ?? string.Empty;
    int ILengthSubset.Length => Length ?? default;
    bool IProgressSubset.LengthComputable => LengthComputable ?? default;
    int IErrorSubset.LineNo => LineNo ?? default;
    long IProgressSubset.Loaded => Loaded ?? default;
    KeyLocation IKeysSubset.Location => Location ?? KeyLocation.Standard;
    string IErrorSubset.Message => Message ?? string.Empty;
    bool IModifierMetaSubset.MetaKey => MetaKey ?? default;
    double IMovementXYSubset.MovementX => MovementX ?? default;
    double IMovementXYSubset.MovementY => MovementY ?? default;
    string IStatesSubset.NewState => NewState ?? string.Empty;
    string IHashChangeSubset.NewUrl => NewUrl ?? string.Empty;
    double IOffsetXYSubset.OffsetX => OffsetX ?? default;
    double IOffsetXYSubset.OffsetY => OffsetY ?? default;
    string IStatesSubset.OldState => OldState ?? string.Empty;
    string IHashChangeSubset.OldUrl => OldUrl ?? string.Empty;
    double IPageXYSubset.PageX => PageX ?? default;
    double IPageXYSubset.PageY => PageY ?? default;
    bool IPersistedSubset.Persisted => Persisted ?? default;
    int IPointerSubset.PointerID => PointerID ?? default;
    string IPointerSubset.PointerType => PointerType ?? string.Empty;
    double IPressuresSubset.Pressure => Pressure ?? default;
    string IAnimationSubset.PropertyName => PropertyName ?? string.Empty;
    string IAnimationSubset.PseudoElement => PseudoElement ?? string.Empty;
    EventTarget IRelatedTargetSubset.RelatedTarget => RelatedTarget ?? EventTarget.Empty;
    bool IKeysSubset.Repeat => Repeat ?? default;
    ABG IDeviceMotionSubset.RotationRate => RotationRate ?? ABG.Empty;
    double IScreenXYSubset.ScreenX => ScreenX ?? default;
    double IScreenXYSubset.ScreenY => ScreenY ?? default;
    bool IModifierShiftSubset.ShiftKey => ShiftKey ?? default;
    bool ISkippedSubset.Skipped => Skipped ?? default;
    EventTarget ISubmitterSubset.Submitter => Submitter ?? EventTarget.Empty;
    double IPressuresSubset.TangentialPressure => TangentialPressure ?? default;
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
    TouchPoint[] ITouchesSubset.TargetTouches => TargetTouches ?? [];
    double IEvent.TimeStamp => TimeStamp ?? default;
    double ITiltsSubset.TiltX => TiltX ?? default;
    double ITiltsSubset.TiltY => TiltY ?? default;
    long IProgressSubset.Total => Total ?? default;
    TouchPoint[] ITouchesSubset.Touches => Touches ?? [];
    double IPointerSubset.Twist => Twist ?? default;
    string IEvent.Type => Type ?? string.Empty;
    public IWindow View => null!; // TODO: Implement
    int IWidthHeightSubset.Width => Width ?? default;
    double IXYSubset.X => X ?? default;
    double IXYSubset.Y => Y ?? default;

    public void StopPropagation()
    {
        throw new NotImplementedException();
    }

    public void StopImmediatePropagation()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
    }
}
