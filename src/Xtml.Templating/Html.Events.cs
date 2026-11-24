using System.Drawing;
using System.Runtime.CompilerServices;
using Xtml.BrowserApis;
using Xtml.BrowserApis.Events.Subsets;

namespace Xtml.Templating;

public ref partial struct Html
{
    public bool AppendFormatted(
        Action<Event.Subsets.Angles> listener, 
        string? format = IAnglesSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Angles, Task> listener, 
        string? format = IAnglesSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Animation> listener, 
        string? format = IAnimationSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Animation, Task> listener, 
        string? format = IAnimationSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Buttons> listener, 
        string? format = IButtonsSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Buttons, Task> listener, 
        string? format = IButtonsSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.ClientXY> listener, 
        string? format = IClientXYSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.ClientXY, Task> listener, 
        string? format = IClientXYSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Coordinates> listener, 
        string? format = ICoordinatesSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Coordinates, Task> listener, 
        string? format = ICoordinatesSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.CurrentTarget> listener, 
        string? format = ICurrentTargetSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.CurrentTarget, Task> listener, 
        string? format = ICurrentTargetSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Data> listener, 
        string? format = IDataSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Data, Task> listener, 
        string? format = IDataSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.DataTransfer> listener, 
        string? format = IDataTransferSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.DataTransfer, Task> listener, 
        string? format = IDataTransferSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Deltas> listener, 
        string? format = IDeltasSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Deltas, Task> listener, 
        string? format = IDeltasSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Detail> listener, 
        string? format = IDetailSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Detail, Task> listener, 
        string? format = IDetailSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.DeviceMotion> listener, 
        string? format = IDeviceMotionSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.DeviceMotion, Task> listener, 
        string? format = IDeviceMotionSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.DeviceOrientation> listener, 
        string? format = IDeviceOrientationSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.DeviceOrientation, Task> listener, 
        string? format = IDeviceOrientationSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Error> listener, 
        string? format = IErrorSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Error, Task> listener, 
        string? format = IErrorSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.HashChange> listener, 
        string? format = IHashChangeSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.HashChange, Task> listener, 
        string? format = IHashChangeSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.IsComposing> listener, 
        string? format = IIsComposingSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.IsComposing, Task> listener, 
        string? format = IIsComposingSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Keys> listener, 
        string? format = IKeysSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Keys, Task> listener, 
        string? format = IKeysSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Length> listener, 
        string? format = ILengthSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Length, Task> listener, 
        string? format = ILengthSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.ModifierAlt> listener, 
        string? format = IModifierAltSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.ModifierAlt, Task> listener, 
        string? format = IModifierAltSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.ModifierCtrl> listener, 
        string? format = IModifierCtrlSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.ModifierCtrl, Task> listener, 
        string? format = IModifierCtrlSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.ModifierMeta> listener, 
        string? format = IModifierMetaSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.ModifierMeta, Task> listener, 
        string? format = IModifierMetaSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Modifiers> listener, 
        string? format = IModifiersSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Modifiers, Task> listener, 
        string? format = IModifiersSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.ModifierShift> listener, 
        string? format = IModifierShiftSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.ModifierShift, Task> listener, 
        string? format = IModifierShiftSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.MovementXY> listener, 
        string? format = IMovementXYSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.MovementXY, Task> listener, 
        string? format = IMovementXYSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.OffsetXY> listener, 
        string? format = IOffsetXYSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.OffsetXY, Task> listener, 
        string? format = IOffsetXYSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.PageXY> listener, 
        string? format = IPageXYSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.PageXY, Task> listener, 
        string? format = IPageXYSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Persisted> listener, 
        string? format = IPersistedSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Persisted, Task> listener, 
        string? format = IPersistedSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Pointer> listener, 
        string? format = IPointerSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Pointer, Task> listener, 
        string? format = IPointerSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Pressures> listener, 
        string? format = IPressuresSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Pressures, Task> listener, 
        string? format = IPressuresSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.PreventDefault> listener, 
        string? format = IPreventDefaultSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.PreventDefault, Task> listener, 
        string? format = IPreventDefaultSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Progress> listener, 
        string? format = IProgressSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Progress, Task> listener, 
        string? format = IProgressSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.RelatedTarget> listener, 
        string? format = IRelatedTargetSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.RelatedTarget, Task> listener, 
        string? format = IRelatedTargetSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.ScreenXY> listener, 
        string? format = IScreenXYSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.ScreenXY, Task> listener, 
        string? format = IScreenXYSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Skipped> listener, 
        string? format = ISkippedSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Skipped, Task> listener, 
        string? format = ISkippedSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.States> listener, 
        string? format = IStatesSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.States, Task> listener, 
        string? format = IStatesSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Submitter> listener, 
        string? format = ISubmitterSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Submitter, Task> listener, 
        string? format = ISubmitterSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Target> listener, 
        string? format = ITargetSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Target, Task> listener, 
        string? format = ITargetSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Action<Event.Subsets.Target<string>> listener,
        string? format = ITargetSubset.TRIM,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Action<Event.Subsets.Target<bool>> listener,
        string? format = ITargetSubset.TRIM,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Action<Event.Subsets.Target<int>> listener,
        string? format = ITargetSubset.TRIM,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(listener, format, expression);
    // TODO: I think converting `Func<Event.Subsets.Event, T>` to `Action<Event.Subsets.Event>` (e => listener(e)) is allocating.  
    // Verify this and fix it.  There are 4 below, and one more `e => c++` at the bottom.
    // How interesting.  Long, float, double, and decimal must use the signature
    // `Func<Event.Subsets.Target<T>, T>` instead of `Action<Event.Subsets.Target<T>>` or else the 
    // "call is ambiguous" due to these types' ability to cast to other types.
    public bool AppendFormatted(
        Func<Event.Subsets.Target<long>, long> listener,
        string? format = ITargetSubset.TRIM,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(e => listener(e), format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Target<float>, float> listener,
        string? format = ITargetSubset.TRIM,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(e => listener(e), format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Target<double>, double> listener,
        string? format = ITargetSubset.TRIM,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(e => listener(e), format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Target<decimal>, decimal> listener,
        string? format = ITargetSubset.TRIM,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(e => listener(e), format, expression);
    public bool AppendFormatted(
        Action<Event.Subsets.Target<DateTime>> listener,
        string? format = ITargetSubset.TRIM,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Action<Event.Subsets.Target<DateOnly>> listener,
        string? format = ITargetSubset.TRIM,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Action<Event.Subsets.Target<TimeOnly>> listener,
        string? format = ITargetSubset.TRIM,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Action<Event.Subsets.Target<Color>> listener,
        string? format = ITargetSubset.TRIM,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Action<Event.Subsets.Target<Uri>> listener,
        string? format = ITargetSubset.TRIM,
        [CallerArgumentExpression(nameof(listener))] string? expression = null)
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Tilts> listener, 
        string? format = ITiltsSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Tilts, Task> listener, 
        string? format = ITiltsSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.Touches> listener, 
        string? format = ITouchesSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.Touches, Task> listener, 
        string? format = ITouchesSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.View> listener, 
        string? format = IViewSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.View, Task> listener, 
        string? format = IViewSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.WidthHeight> listener, 
        string? format = IWidthHeightSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.WidthHeight, Task> listener, 
        string? format = IWidthHeightSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);

    public bool AppendFormatted(
        Action<Event.Subsets.XY> listener, 
        string? format = IXYSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
    public bool AppendFormatted(
        Func<Event.Subsets.XY, Task> listener, 
        string? format = IXYSubset.TRIM, 
        [CallerArgumentExpression(nameof(listener))] string? expression = null) 
            => AppendEventListener(listener, format, expression);
}