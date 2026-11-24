using static Xtml.BrowserApis.Events.Aliases.Subsets;

namespace Xtml.BrowserApis.Events;

public interface IPreventDefaultEvent : IEvent, PreventDefault,
    IDragEvent,
    IPointerEvent,
    IWheelEvent,
    IMouseEvent,
    ICompositionEvent,
    IFocusEvent,
    IKeyboardEvent,
    ITouchEvent,
    IAnimationEvent,
    IBeforeUnloadEvent,
    IContentVisibilityAutoStateChangeEvent,
    IDeviceMotionEvent,
    IDeviceOrientationEvent,
    IErrorEvent,
    IHashChangeEvent,
    IPageTransitionEvent,
    IProgressEvent,
    IStorageEvent,
    ISubmitEvent,
    IToggleEvent,
    ITransitionEvent,
    IClipboardEvent,
    IFormDataEvent,
    IGamepadEvent,
    IMessageEvent,
    IPopStateEvent,
    IPromiseRejectionEvent
{
}
