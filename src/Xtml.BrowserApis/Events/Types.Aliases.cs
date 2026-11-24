#pragma warning disable IDE1006 // Naming Styles

using Xtml.BrowserApis.Events.Subsets;

namespace Xtml.BrowserApis.Events;

public interface Aliases
{
    public interface Animation : IAnimationEvent { }
    public interface BeforeUnload : IBeforeUnloadEvent { }
    public interface Clipboard : IClipboardEvent { }
    public interface Composition : ICompositionEvent { }
    public interface ContentVisibilityAutoStateChange : IContentVisibilityAutoStateChangeEvent { }
    public interface DeviceMotion : IDeviceMotionEvent { }
    public interface DeviceOrientation : IDeviceOrientationEvent { }
    public interface Drag : IDragEvent { }
    public interface Error : IErrorEvent { }
    public interface Focus : IFocusEvent { }
    public interface FormData : IFormDataEvent { }
    public interface Gamepad : IGamepadEvent { }
    public interface HashChange : IHashChangeEvent { }
    public interface Input<T> : IInputEvent<T> { }
    public interface Keyboard : IKeyboardEvent { }
    public interface Message : IMessageEvent { }
    public interface Mouse : IMouseEvent { }
    public interface PageTransition : IPageTransitionEvent { }
    public interface Pointer : IPointerEvent { }
    public interface PopState : IPopStateEvent { }
    public interface PreventDefault : IPreventDefaultEvent { }
    public interface Progress : IProgressEvent { }
    public interface PromiseRejection : IPromiseRejectionEvent { }
    public interface Storage : IStorageEvent { }
    public interface Submit : ISubmitEvent { }
    public interface Toggle : IToggleEvent { }
    public interface Touch : ITouchEvent { }
    public interface Transition : ITransitionEvent { }
    public interface Wheel : IWheelEvent { }
    public interface Subsets
    {
        public interface Angles : IAnglesSubset { }
        public interface Animation : IAnimationSubset { }
        public interface Buttons : IButtonsSubset { }
        public interface ClientXY : IClientXYSubset { }
        public interface Coordinates : ICoordinatesSubset { }
        public interface CurrentTarget : ICurrentTargetSubset { }
        public interface Data : IDataSubset { }
        public interface DataTransfer : IDataTransferSubset { }
        public interface Deltas : IDeltasSubset { }
        public interface Detail : IDetailSubset { }
        public interface DeviceMotion : IDeviceMotionSubset { }
        public interface DeviceOrientation : IDeviceOrientationSubset { }
        public interface Error : IErrorSubset { }
        public interface HashChange : IHashChangeSubset { }
        public interface IsComposing : IIsComposingSubset { }
        public interface Keys : IKeysSubset { }
        public interface Length : ILengthSubset { }
        public interface ModifierAlt : IModifierAltSubset { }
        public interface ModifierCtrl : IModifierCtrlSubset { }
        public interface ModifierMeta : IModifierMetaSubset { }
        public interface Modifiers : IModifiersSubset { }
        public interface ModifierShift : IModifierShiftSubset { }
        public interface MovementXY : IMovementXYSubset { }
        public interface OffsetXY : IOffsetXYSubset { }
        public interface PageXY : IPageXYSubset { }
        public interface Persisted : IPersistedSubset { }
        public interface Pointer : IPointerSubset { }
        public interface Pressures : IPressuresSubset { }
        public interface PreventDefault : IPreventDefaultSubset { }
        public interface Progress : IProgressSubset { }
        public interface RelatedTarget : IRelatedTargetSubset { }
        public interface ScreenXY : IScreenXYSubset { }
        public interface Skipped : ISkippedSubset { }
        public interface States : IStatesSubset { }
        public interface Submitter : ISubmitterSubset { }
        public interface Target : ITargetSubset { }
        public interface Target<T> : ITargetSubset<T> { }
        public interface Tilts : ITiltsSubset { }
        public interface Touches : ITouchesSubset { }
        public interface View : IViewSubset { }
        public interface WidthHeight : IWidthHeightSubset { }
        public interface XY : IXYSubset { }
    }
}

#pragma warning restore IDE1006 // Naming Styles
