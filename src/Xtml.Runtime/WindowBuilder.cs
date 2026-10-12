using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Xtml.Dom.EventListeners;
using Xtml.Runtime.Composers;
using Xtml.Dom;
using Xtml.Templating;
using Xtml.Runtime.Utilities;

namespace Xtml.Runtime;

public class WindowBuilder : 
    IWindowEventListeners,
    IEventListeners,
    IAnimationEventListeners,
    IClipboardListeners,
    IContentVisibilityEventListeners,
    IDragEventListeners,
    IFocusEventListeners,
    IFormDataEventListeners,
    IKeyboardEventListeners,
    IMouseEventListeners,
    IPointerEventListeners,
    IScrollEventListeners,
    ISubmitEventListeners,
    IToggleEventListeners,
    ITouchEventListeners,
    ITransitionEventListeners
{
    public Func<Html> Template { get; init; }
    public DocumentBuilder Document { get; init; }
    public List<EventListener> Listeners { get; } = [];

    public WindowBuilder(Func<Html> template)
    {
        Template = template;
        Document = new(this);
    }

    public WindowBuilder MapGet(
        [StringSyntax("Route")] string pattern, 
        Action<IWindow> requestDelegate)
    {
        // TODO: Implement
        return this;
    }

    public EventListener GetEventListener(ReadOnlySpan<byte> key)
    {
        using var perf = Perf.Measure("GetEventListener"); // TODO: Remove PerfCheck

        return key switch
        {
            // If the key starts with "win:" or "doc:" that means it's a top-level window or document event.
            // These live in a simple List<> and can be looked up by index.
            _ when key.StartsWith("win:"u8) || key.StartsWith("doc:"u8) =>
                int.TryParse(key[4..], out var index) && index < Listeners.Count
                    ? Listeners[index]
                    : default,
                    
            // Otherwise, it starts with "key" and is bound to some element buried somewhere in the HTML.
            // The only way to find it is to compose the HTML and compare every keyhole to the supplied key.
            _ => FindKeyComposer.Shared.FindEventListener(key, Template),
        };
    }

    public Action<Event>? OnAfterPrint { set => AddEventListener(nameof(OnAfterPrint), value, null); }
    public Action<Event>? OnBeforePrint { set => AddEventListener(nameof(OnBeforePrint), value, null); }
    public Action<Event.BeforeUnload>? OnBeforeUnload { set => AddEventListener(nameof(OnBeforeUnload), value, null); }
    public Action<Event.DeviceMotion>? OnDeviceMotion { set => AddEventListener(nameof(OnDeviceMotion), value, null); }
    public Action<Event.DeviceOrientation>? OnDeviceOrientation { set => AddEventListener(nameof(OnDeviceOrientation), value, null); }
    public Action<Event.DeviceOrientation>? OnDeviceOrientationAbsolute { set => AddEventListener(nameof(OnDeviceOrientationAbsolute), value, null); }
    public Action<Event.Gamepad>? OnGamepadConnected { set => AddEventListener(nameof(OnGamepadConnected), value, null); }
    public Action<Event.Gamepad>? OnGamepadDisconnected { set => AddEventListener(nameof(OnGamepadDisconnected), value, null); }
    public Action<Event.HashChange>? OnHashChange { set => AddEventListener(nameof(OnHashChange), value, null); }
    public Action<Event>? OnLanguageChange { set => AddEventListener(nameof(OnLanguageChange), value, null); }
    public Action<Event.Message>? OnMessage { set => AddEventListener(nameof(OnMessage), value, null); }
    public Action<Event.Message>? OnMessageError { set => AddEventListener(nameof(OnMessageError), value, null); }
    public Action<Event>? OnOffline { set => AddEventListener(nameof(OnOffline), value, null); }
    public Action<Event>? OnOnline { set => AddEventListener(nameof(OnOnline), value, null); }
    public Action<Event.PageTransition>? OnPageHide { set => AddEventListener(nameof(OnPageHide), value, null); }
    public Action<Event.PageTransition>? OnPageShow { set => AddEventListener(nameof(OnPageShow), value, null); }
    public Action<Event.PopState>? OnPopState { set => AddEventListener(nameof(OnPopState), value, null); }
    public Action<Event.PromiseRejection>? OnRejectionHandled { set => AddEventListener(nameof(OnRejectionHandled), value, null); }
    public Action<Event>? OnResize { set => AddEventListener(nameof(OnResize), value, null); }
    public Action<Event.Storage>? OnStorage { set => AddEventListener(nameof(OnStorage), value, null); }
    public Action<Event.PromiseRejection>? OnUnhandledRejection { set => AddEventListener(nameof(OnUnhandledRejection), value, null); }

    public Action<Event>? OnBeforeInput { set => AddEventListener(nameof(OnBeforeInput), value, null); }
    public Action<Event>? OnCancel { set => AddEventListener(nameof(OnCancel), value, null); }
    public Action<Event>? OnChange { set => AddEventListener(nameof(OnChange), value, null); }
    public Action<Event>? OnCueChange { set => AddEventListener(nameof(OnCueChange), value, null); }
    public Action<Event>? OnInput { set => AddEventListener(nameof(OnInput), value, null); }
    public Action<Event>? OnInvalid { set => AddEventListener(nameof(OnInvalid), value, null); }
    public Action<Event>? OnLoad { set => AddEventListener(nameof(OnLoad), value, null); }
    public Action<Event>? OnReset { set => AddEventListener(nameof(OnReset), value, null); }
    public Action<Event>? OnSelect { set => AddEventListener(nameof(OnSelect), value, null); }
    public Action<Event>? OnSecurityPolicyViolation { set => AddEventListener(nameof(OnSecurityPolicyViolation), value, null); }
    public Action<Event>? OnSelectStart { set => AddEventListener(nameof(OnSelectStart), value, null); }
    public Action<Event>? OnSlotChange { set => AddEventListener(nameof(OnSlotChange), value, null); }

    public Action<Event.Animation>? OnAnimationCancel { set => AddEventListener(nameof(OnAnimationCancel), value, null); }
    public Action<Event.Animation>? OnAnimationEnd { set => AddEventListener(nameof(OnAnimationEnd), value, null); }
    public Action<Event.Animation>? OnAnimationIteration { set => AddEventListener(nameof(OnAnimationIteration), value, null); }
    public Action<Event.Animation>? OnAnimationStart { set => AddEventListener(nameof(OnAnimationStart), value, null); }

    public Action<Event.Clipboard>? OnCopy { set => AddEventListener(nameof(OnCopy), value, null); }
    public Action<Event.Clipboard>? OnCut { set => AddEventListener(nameof(OnCut), value, null); }
    public Action<Event.Clipboard>? OnPaste { set => AddEventListener(nameof(OnPaste), value, null); }

    public Action<Event.ContentVisibilityAutoStateChange>? OnContentVisibilityAutoStateChange { set => AddEventListener(nameof(OnContentVisibilityAutoStateChange), value, null); }

    public Action<Event.Drag>? OnDrag { set => AddEventListener(nameof(OnDrag), value, null); }
    public Action<Event.Drag>? OnDragEnd { set => AddEventListener(nameof(OnDragEnd), value, null); }
    public Action<Event.Drag>? OnDragEnter { set => AddEventListener(nameof(OnDragEnter), value, null); }
    public Action<Event.Drag>? OnDragLeave { set => AddEventListener(nameof(OnDragLeave), value, null); }
    public Action<Event.Drag>? OnDragOver { set => AddEventListener(nameof(OnDragOver), value, null); }
    public Action<Event.Drag>? OnDragStart { set => AddEventListener(nameof(OnDragStart), value, null); }
    public Action<Event.Drag>? OnDrop { set => AddEventListener(nameof(OnDrop), value, null); }

    public Action<Event.Focus>? OnBlur { set => AddEventListener(nameof(OnBlur), value, null); }
    public Action<Event.Focus>? OnFocus { set => AddEventListener(nameof(OnFocus), value, null); }
    public Action<Event.Focus>? OnFocusIn { set => AddEventListener(nameof(OnFocusIn), value, null); }
    public Action<Event.Focus>? OnFocusOut { set => AddEventListener(nameof(OnFocusOut), value, null); }

    public Action<Event.FormData>? OnFormData { set => AddEventListener(nameof(OnFormData), value, null); }

    public Action<Event.Keyboard>? OnKeyDown { set => AddEventListener(nameof(OnKeyDown), value, null); }
    public Action<Event.Keyboard>? OnKeyUp { set => AddEventListener(nameof(OnKeyUp), value, null); }

    public Action<Event.Mouse>? OnAuxClick { set => AddEventListener(nameof(OnAuxClick), value, null); }
    public Action<Event.Mouse>? OnClick { set => AddEventListener(nameof(OnClick), value, null); }
    public Action<Event.Mouse>? OnContextMenu { set => AddEventListener(nameof(OnContextMenu), value, null); }
    public Action<Event.Mouse>? OnDblClick { set => AddEventListener(nameof(OnDblClick), value, null); }
    public Action<Event.Mouse>? OnMouseDown { set => AddEventListener(nameof(OnMouseDown), value, null); }
    public Action<Event.Mouse>? OnMouseEnter { set => AddEventListener(nameof(OnMouseEnter), value, null); }
    public Action<Event.Mouse>? OnMouseLeave { set => AddEventListener(nameof(OnMouseLeave), value, null); }
    public Action<Event.Mouse>? OnMouseMove { set => AddEventListener(nameof(OnMouseMove), value, null); }
    public Action<Event.Mouse>? OnMouseOut { set => AddEventListener(nameof(OnMouseOut), value, null); }
    public Action<Event.Mouse>? OnMouseOver { set => AddEventListener(nameof(OnMouseOver), value, null); }
    public Action<Event.Mouse>? OnMouseUp { set => AddEventListener(nameof(OnMouseUp), value, null); }

    public Action<Event.Pointer>? OnGotPointerCapture { set => AddEventListener(nameof(OnGotPointerCapture), value, null); }
    public Action<Event.Pointer>? OnLostPointerCapture { set => AddEventListener(nameof(OnLostPointerCapture), value, null); }
    public Action<Event.Pointer>? OnPointerCancel { set => AddEventListener(nameof(OnPointerCancel), value, null); }
    public Action<Event.Pointer>? OnPointerDown { set => AddEventListener(nameof(OnPointerDown), value, null); }
    public Action<Event.Pointer>? OnPointerEnter { set => AddEventListener(nameof(OnPointerEnter), value, null); }
    public Action<Event.Pointer>? OnPointerLeave { set => AddEventListener(nameof(OnPointerLeave), value, null); }
    public Action<Event.Pointer>? OnPointerMove { set => AddEventListener(nameof(OnPointerMove), value, null); }
    public Action<Event.Pointer>? OnPointerOut { set => AddEventListener(nameof(OnPointerOut), value, null); }
    public Action<Event.Pointer>? OnPointerOver { set => AddEventListener(nameof(OnPointerOver), value, null); }
    public Action<Event.Pointer>? OnPointerUp { set => AddEventListener(nameof(OnPointerUp), value, null); }

    public Action<Event>? OnScroll { set => AddEventListener(nameof(OnScroll), value, null); }
    public Action<Event>? OnScrollEnd { set => AddEventListener(nameof(OnScrollEnd), value, null); }

    public Action<Event.Submit>? OnSubmit { set => AddEventListener(nameof(OnSubmit), value, null); }

    public Action<Event.Toggle>? OnBeforeToggle { set => AddEventListener(nameof(OnBeforeToggle), value, null); }
    public Action<Event.Toggle>? OnToggle { set => AddEventListener(nameof(OnToggle), value, null); }

    public Action<Event.Touch>? OnTouchCancel { set => AddEventListener(nameof(OnTouchCancel), value, null); }
    public Action<Event.Touch>? OnTouchEnd { set => AddEventListener(nameof(OnTouchEnd), value, null); }
    public Action<Event.Touch>? OnTouchMove { set => AddEventListener(nameof(OnTouchMove), value, null); }
    public Action<Event.Touch>? OnTouchStart { set => AddEventListener(nameof(OnTouchStart), value, null); }

    public Action<Event.Transition>? OnTransitionCancel { set => AddEventListener(nameof(OnTransitionCancel), value, null); }
    public Action<Event.Transition>? OnTransitionEnd { set => AddEventListener(nameof(OnTransitionEnd), value, null); }
    public Action<Event.Transition>? OnTransitionRun { set => AddEventListener(nameof(OnTransitionRun), value, null); }
    public Action<Event.Transition>? OnTransitionStart { set => AddEventListener(nameof(OnTransitionStart), value, null); }

    public WindowBuilder AddEventListener(string type, Action listener) => AddEventListener(type, e => listener(), string.Empty);
    public WindowBuilder AddEventListener(string type, Action<Event.Animation> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Composition> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.DeviceMotion> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.DeviceOrientation> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Drag> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Error> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Focus> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.HashChange> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<string>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<bool>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<int>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<long>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<float>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<double>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<decimal>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<DateTime>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<DateOnly>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<TimeOnly>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<Color>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Input<Uri>> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Keyboard> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Mouse> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Pointer> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Progress> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Submit> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Touch> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Transition> listener) => AddEventListener(type, listener, null);
    public WindowBuilder AddEventListener(string type, Action<Event.Wheel> listener) => AddEventListener(type, listener, null);

    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Angles> listener) => AddEventListener(type, listener, Event.Subsets.Angles.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Animation> listener) => AddEventListener(type, listener, Event.Subsets.Animation.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Buttons> listener) => AddEventListener(type, listener, Event.Subsets.Buttons.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.ClientXY> listener) => AddEventListener(type, listener, Event.Subsets.ClientXY.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Coordinates> listener) => AddEventListener(type, listener, Event.Subsets.Coordinates.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.CurrentTarget> listener) => AddEventListener(type, listener, Event.Subsets.CurrentTarget.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Data> listener) => AddEventListener(type, listener, Event.Subsets.Data.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.DataTransfer> listener) => AddEventListener(type, listener, Event.Subsets.DataTransfer.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Deltas> listener) => AddEventListener(type, listener, Event.Subsets.Deltas.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Detail> listener) => AddEventListener(type, listener, Event.Subsets.Detail.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.DeviceMotion> listener) => AddEventListener(type, listener, Event.Subsets.DeviceMotion.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.DeviceOrientation> listener) => AddEventListener(type, listener, Event.Subsets.DeviceOrientation.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Error> listener) => AddEventListener(type, listener, Event.Subsets.Error.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.HashChange> listener) => AddEventListener(type, listener, Event.Subsets.HashChange.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.IsComposing> listener) => AddEventListener(type, listener, Event.Subsets.IsComposing.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Keys> listener) => AddEventListener(type, listener, Event.Subsets.Keys.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.ModifierAlt> listener) => AddEventListener(type, listener, Event.Subsets.ModifierAlt.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.ModifierCtrl> listener) => AddEventListener(type, listener, Event.Subsets.ModifierCtrl.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.ModifierMeta> listener) => AddEventListener(type, listener, Event.Subsets.ModifierMeta.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Modifiers> listener) => AddEventListener(type, listener, Event.Subsets.Modifiers.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.ModifierShift> listener) => AddEventListener(type, listener, Event.Subsets.ModifierShift.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.MovementXY> listener) => AddEventListener(type, listener, Event.Subsets.MovementXY.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.OffsetXY> listener) => AddEventListener(type, listener, Event.Subsets.OffsetXY.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.PageXY> listener) => AddEventListener(type, listener, Event.Subsets.PageXY.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Pointer> listener) => AddEventListener(type, listener, Event.Subsets.Pointer.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Pressures> listener) => AddEventListener(type, listener, Event.Subsets.Pressures.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Progress> listener) => AddEventListener(type, listener, Event.Subsets.Progress.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.RelatedTarget> listener) => AddEventListener(type, listener, Event.Subsets.RelatedTarget.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.ScreenXY> listener) => AddEventListener(type, listener, Event.Subsets.ScreenXY.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Submitter> listener) => AddEventListener(type, listener, Event.Subsets.Submitter.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target> listener) => AddEventListener(type, listener, Event.Subsets.Target.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<string>> listener) => AddEventListener(type, listener, Event.Subsets.Target.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<bool>> listener) => AddEventListener(type, listener, Event.Subsets.Target.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<int>> listener) => AddEventListener(type, listener, Event.Subsets.Target.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<long>> listener) => AddEventListener(type, listener, Event.Subsets.Target.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<float>> listener) => AddEventListener(type, listener, Event.Subsets.Target.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<double>> listener) => AddEventListener(type, listener, Event.Subsets.Target.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<decimal>> listener) => AddEventListener(type, listener, Event.Subsets.Target.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<DateTime>> listener) => AddEventListener(type, listener, Event.Subsets.Target.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<DateOnly>> listener) => AddEventListener(type, listener, Event.Subsets.Target.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<TimeOnly>> listener) => AddEventListener(type, listener, Event.Subsets.Target.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<Color>> listener) => AddEventListener(type, listener, Event.Subsets.Target.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Target<Uri>> listener) => AddEventListener(type, listener, Event.Subsets.Target.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Tilts> listener) => AddEventListener(type, listener, Event.Subsets.Tilts.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.Touches> listener) => AddEventListener(type, listener, Event.Subsets.Touches.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.View> listener) => AddEventListener(type, listener, Event.Subsets.View.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.WidthHeight> listener) => AddEventListener(type, listener, Event.Subsets.WidthHeight.TRIM);
    public WindowBuilder AddEventListener(string type, Action<Event.Subsets.XY> listener) => AddEventListener(type, listener, Event.Subsets.XY.TRIM);

    public WindowBuilder AddEventListener(string type, Action<Event>? listener, string? trim = null)
        => AddEventListenerInternal(type, listener, trim, "window");

    internal WindowBuilder AddEventListenerInternal(
        string type, 
        Action<Event>? listener, 
        string? trim = null,
        string target = "window")
    {
        string? onNotation = null;
        if (type.StartsWith("On"))
        {
            // e.g. window.OnClick
            onNotation = $"{target}.{type}";
            
            // e.g. "OnClick" -> "click"
            type = type[2..].ToLower();

            // e.g. using window.onClick twice overwrites the first one
            for (int i = 0; i < Listeners.Count; i++)
                if (Listeners[i].OnNotation == onNotation && !Listeners[i].Html!.StartsWith("//"))
                    Listeners[i] = Listeners[i] with { Html = "// " + Listeners[i].Html };
        }
        
        if (listener is not null)
        {
            var key = $"{target[..3]}:{Listeners.Count}";

            // TODO: Support more event listener options
            var options = "{passive:true}";

            Listeners.Add(new(listener)
            {
                Html = CreateListenerString(trim, type, target, key, options),
                OnNotation = onNotation,
            });
        }

        return this;
    }

    private string CreateListenerString(string? format, string type, string target, string key, string options) => format switch
    {
        // Serialize nothing – the event object is never used
        "" => $"{target}.addEventListener('{type}', e => {{ keyholes.set('{key}', {target}); keyholes['{key}'].dispatchEvent(e); }});",

        // Serialize the event – the event object is needed
        null => $"{target}.addEventListener('{type}', e => {{ keyholes.set('{key}', {target}); keyholes['{key}'].dispatchEvent(e, '*'); }});",

        // Serialize selectively – only a few properties are ever used
        _ => $"{target}.addEventListener('{type}', e => {{ keyholes.set('{key}', {target}); keyholes['{key}'].dispatchEvent(e, '{format}'); }});",
    };
}