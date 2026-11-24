#pragma warning disable IDE1006 // Naming Styles

using System.Drawing;
using Xtml.BrowserApis.Events;

namespace Xtml.BrowserApis;

/// <summary>
/// A DOM event
/// </summary>
public partial interface Event : IDisposable,
    Aliases,
    Aliases.Animation,
    Aliases.BeforeUnload,
    Aliases.Clipboard,
    Aliases.Composition,
    Aliases.ContentVisibilityAutoStateChange,
    Aliases.DeviceMotion,
    Aliases.DeviceOrientation,
    Aliases.Drag,
    Aliases.Error,
    Aliases.Focus,
    Aliases.FormData,
    Aliases.Gamepad,
    Aliases.HashChange,
    Aliases.Input<string>,
    Aliases.Input<bool>,
    Aliases.Input<int>,
    Aliases.Input<long>,
    Aliases.Input<float>,
    Aliases.Input<double>,
    Aliases.Input<decimal>,
    Aliases.Input<DateTime>,
    Aliases.Input<DateOnly>,
    Aliases.Input<TimeOnly>,
    Aliases.Input<Color>,
    Aliases.Input<Uri>,
    Aliases.Keyboard,
    Aliases.Message,
    Aliases.Mouse,
    Aliases.PageTransition,
    Aliases.Pointer,
    Aliases.PopState,
    Aliases.PreventDefault,
    Aliases.Progress,
    Aliases.PromiseRejection,
    Aliases.Storage,
    Aliases.Submit,
    Aliases.Toggle,
    Aliases.Touch,
    Aliases.Transition,
    Aliases.Wheel
{
    /// <summary>
    /// A boolean that indicates whether or not the device is providing orientation data absolutely.
    /// </summary>
    new bool? Absolute { get; }

    /// <summary>
    /// An object giving the acceleration of the device on the three axis X, Y and Z. Acceleration is expressed in m/s².
    /// </summary>
    new XYZ? Acceleration { get; }

    /// <summary>
    /// An object giving the acceleration of the device on the three axis X, Y and Z with the effect of gravity. Acceleration is expressed in m/s².
    /// </summary>
    new XYZ? AccelerationIncludingGravity { get; }

    /// <summary>
    /// A number representing the motion of the device around the z axis, express in degrees with values ranging from 0 (inclusive) to 360 (exclusive).
    /// </summary>
    new double? Alpha { get; }

    /// <summary>
    /// Represents the angle between a transducer (a pointer or stylus) axis and the X-Y plane of a device screen.
    /// </summary>
    new double? AltitudeAngle { get; }

    /// <summary>
    /// Returns true if the alt key was down when the mouse event was fired.
    /// </summary>
    new bool? AltKey { get; }

    /// <summary>
    /// A string containing the value of the animation-name that generated the animation.
    /// </summary>
    new string? AnimationName { get; }

    /// <summary>
    /// Represents the angle between the Y-Z plane and the plane containing both the transducer (a pointer or stylus) axis and the Y axis.
    /// </summary>
    new double? AzimuthAngle { get; }

    /// <summary>
    /// A number representing the motion of the device around the x axis, express in degrees with values ranging from -180 (inclusive) to 180 (exclusive). This represents a front to back motion of the device.
    /// </summary>
    new double? Beta { get; }

    /// <summary>
    /// A nullable boolean value indicating whether or not the event bubbles up through the DOM.  
    /// </summary>
    new bool? Bubbles { get; }

    /// <summary>
    /// The button number that was pressed (if applicable) when the mouse event was fired.
    /// </summary>
    new Button? Button { get; }

    /// <summary>
    /// The buttons being pressed (if any) when the mouse event was fired.
    /// </summary>
    new ButtonFlag? Buttons { get; }

    /// <summary>
    /// A nullable boolean value indicating whether the event is cancelable.
    /// </summary>
    new bool? Cancelable { get; }

    /// <summary>
    /// A TouchList of all the Touch objects representing individual points of contact whose states changed between the previous touch event and this one.
    /// </summary>
    new TouchPoint[]? ChangedTouches { get; }

    /// <summary>
    /// The X coordinate of the mouse pointer in viewport coordinates.
    /// </summary>
    new double? ClientX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer in viewport coordinates.
    /// </summary>
    new double? ClientY { get; }

    /// <summary>
    /// Returns a string with the code value of the physical key represented by the event.
    /// </summary>
    new string? Code { get; }

    /// <summary>
    /// A nullable boolean indicating whether or not the event can bubble across the boundary between the shadow DOM and the regular DOM.
    /// </summary>
    new bool? Composed { get; }

    /// <summary>
    /// Returns true if the control key was down when the mouse event was fired.
    /// </summary>
    new bool? CtrlKey { get; }

    /// <summary>
    /// A nullable reference to the currently registered target for the event. This is the object to which the event is currently slated to be sent. It's possible this has been changed along the way through retargeting.
    /// </summary>
    new EventTarget? CurrentTarget { get; }

    /// <summary>
    /// Returns a string with the inserted characters. This may be an empty string if the change doesn't insert text (for example, when deleting characters).
    /// </summary>
    new string? Data { get; }

    /// <summary>
    /// Returns a DataTransfer object containing information about richtext or plaintext data being added to or removed from editable content.
    /// </summary>
    new DataTransferContainer? DataTransfer { get; }

    /// <summary>
    /// A nullable boolean that indicates whether or not the call to event.preventDefault() canceled the event.
    /// </summary>
    new bool? DefaultPrevented { get; }

    /// <summary>
    /// Returns an unsigned long representing the unit of the delta* values' scroll amount.
    /// </summary>
    new DeltaMode? DeltaMode { get; }

    /// <summary>
    /// Returns a double representing the horizontal scroll amount.
    /// </summary>
    new double? DeltaX { get; }

    /// <summary>
    /// Returns a double representing the vertical scroll amount.
    /// </summary>
    new double? DeltaY { get; }

    /// <summary>
    /// Returns a double representing the scroll amount for the z-axis.
    /// </summary>
    new double? DeltaZ { get; }

    /// <summary>
    /// Returns a nullable long with details about the event, depending on the event type.
    /// </summary>
    new long? Detail { get; }

    /// <summary>
    /// A float giving the amount of time the animation has been running, in seconds, when this event fired, excluding any time the animation was paused. For an animationstart event, elapsedTime is 0.0 unless there was a negative value for animation-delay, in which case the event will be fired with elapsedTime containing (-1 * delay).
    /// </summary>
    new double? ElapsedTime { get; }

    /// <summary>
    /// A nullable int which indicates which phase of the event flow is being processed. It is one of the following numbers: NONE, CAPTURING_PHASE, AT_TARGET, BUBBLING_PHASE.
    /// </summary>
    new EventPhase? EventPhase { get; }

    /// <summary>
    /// A number representing the motion of the device around the y axis, express in degrees with values ranging from -90 (inclusive) to 90 (exclusive). This represents a left to right motion of the device.
    /// </summary>
    new double? Gamma { get; }

    /// <summary>
    /// The height (magnitude on the Y axis), in CSS pixels, of the contact geometry of the pointer.
    /// </summary>
    new int? Height { get; }

    /// <summary>
    /// Returns the type of change for editable content such as, for example, inserting, deleting, or formatting text.
    /// </summary>
    new string? InputType { get; }

    /// <summary>
    /// A number representing the interval of time, in milliseconds, at which data is obtained from the device.
    /// </summary>
    new double? Interval { get; }

    /// <summary>
    /// Returns a boolean value that is true if the event is fired between after compositionstart and before compositionend.
    /// </summary>
    new bool? IsComposing { get; }

    /// <summary>
    /// Indicates if the pointer represents the primary pointer of this pointer type.
    /// </summary>
    new bool? IsPrimary { get; }

    /// <summary>
    /// A nullable boolean which indicates whether or not the event was initiated by the browser (after a user click, for instance) or by a script (using an event creation method, for example).
    /// </summary>
    new bool? IsTrusted { get; }

    /// <summary>
    /// Returns a string representing the key value of the key represented by the event.
    /// </summary>
    new string? Key { get; }

    /// <summary>
    /// A boolean flag indicating if the ratio between the size of the data already transmitted or processed (loaded), and the total size of the data (total), is calculable. In other words, it tells if the progress is measurable or not.
    /// </summary>
    new bool? LengthComputable { get; }

    /// <summary>
    /// A 64-bit unsigned integer indicating the size, in bytes, of the data already transmitted or processed. The ratio can be calculated by dividing ProgressEvent.total by the value of this property. When downloading a resource using HTTP, this only counts the body of the HTTP message, and doesn't include headers and other overhead. Note that for compressed requests of unknown total size, loaded might contain the size of the compressed, or decompressed, data, depending on the browser. As of 2024, it contains the size of the compressed data in Firefox, and the size of the uncompressed data in Chrome.
    /// </summary>
    new long? Loaded { get; }

    /// <summary>
    /// Returns a number representing the location of the key on the keyboard or other input device.
    /// </summary>
    new KeyLocation? Location { get; }

    /// <summary>
    /// Returns true if the meta key was down when the mouse event was fired.
    /// </summary>
    new bool? MetaKey { get; }

    /// <summary>
    /// The X coordinate of the mouse pointer relative to the position of the last mousemove event.
    /// </summary>
    new double? MovementX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer relative to the position of the last mousemove event.
    /// </summary>
    new double? MovementY { get; }

    /// <summary>
    /// The new URL to which the window is navigating.
    /// </summary>
    new string? NewUrl { get; }

    /// <summary>
    /// The X coordinate of the mouse pointer relative to the position of the padding edge of the target node.
    /// </summary>
    new double? OffsetX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer relative to the position of the padding edge of the target node.
    /// </summary>
    new double? OffsetY { get; }

    /// <summary>
    /// The previous URL from which the window was navigated.
    /// </summary>
    new string? OldUrl { get; }

    /// <summary>
    /// The X coordinate of the mouse pointer relative to the whole document.
    /// </summary>
    new double? PageX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer relative to the whole document.
    /// </summary>
    new double? PageY { get; }

    /// <summary>
    /// A unique identifier for the pointer causing the event.
    /// </summary>
    new int? PointerID { get; }

    /// <summary>
    /// Indicates the device type that caused the event (mouse, pen, touch, etc.).
    /// </summary>
    new string? PointerType { get; }

    /// <summary>
    /// The normalized pressure of the pointer input in the range 0 to 1, where 0 and 1 represent the minimum and maximum pressure the hardware is capable of detecting, respectively.
    /// </summary>
    new double? Pressure { get; }

    /// <summary>
    /// A string containing the name CSS property associated with the transition.
    /// </summary>
    new string? PropertyName { get; }

    /// <summary>
    /// A string, starting with '::', containing the name of the pseudo-element the animation runs on. If the animation doesn't run on a pseudo-element but on the element, an empty string: ''.
    /// </summary>
    new string? PseudoElement { get; }

    /// <summary>
    /// The secondary target for the event, if there is one.
    /// </summary>
    new EventTarget? RelatedTarget { get; }

    /// <summary>
    /// Returns a boolean value that is true if the key is being held down such that it is automatically repeating.
    /// </summary>
    new bool? Repeat { get; }

    /// <summary>
    /// An object giving the rate of change of the device's orientation on the three orientation axis alpha, beta and gamma. Rotation rate is expressed in degrees per seconds.
    /// </summary>
    new ABG? RotationRate { get; }
    
    /// <summary>
    /// The X coordinate of the mouse pointer in screen coordinates.
    /// </summary>
    new double? ScreenX { get; }

    /// <summary>
    /// The Y coordinate of the mouse pointer in screen coordinates.
    /// </summary>
    new double? ScreenY { get; }

    /// <summary>
    /// Returns true if the shift key was down when the mouse event was fired.
    /// </summary>
    new bool? ShiftKey { get; }

    /// <summary>
    /// An HTMLElement object which identifies the button or other element which was invoked to trigger the form being submitted.
    /// </summary>
    new EventTarget? Submitter { get; }

    /// <summary>
    /// The normalized tangential pressure of the pointer input (also known as barrel pressure or cylinder stress) in the range -1 to 1, where 0 is the neutral position of the control.
    /// </summary>
    new double? TangentialPressure { get; }

    /// <summary>
    /// A nullable reference to the object to which the event was originally dispatched.
    /// </summary>
    new EventTarget? Target { get; }

    /// <summary>
    /// A TouchList of all the Touch objects that are both currently in contact with the touch surface and were also started on the same element that is the target of the event.
    /// </summary>
    new TouchPoint[]? TargetTouches { get; }

    /// <summary>
    /// The nullable time at which the event was created (in milliseconds). By specification, this value is time since epoch—but in reality, browsers' definitions vary. In addition, work is underway to change this to be a DOMHighResTimeStamp instead.
    /// </summary>
    new double? TimeStamp { get; }

    /// <summary>
    /// The plane angle (in degrees, in the range of -90 to 90) between the Y–Z plane and the plane containing both the pointer (e.g. pen stylus) axis and the Y axis.
    /// </summary>
    new double? TiltX { get; }

    /// <summary>
    /// The plane angle (in degrees, in the range of -90 to 90) between the X–Z plane and the plane containing both the pointer (e.g. pen stylus) axis and the X axis.
    /// </summary>
    new double? TiltY { get; }

    /// <summary>
    /// A 64-bit unsigned integer indicating the total size, in bytes, of the data being transmitted or processed. When downloading a resource using HTTP, this value is taken from the Content-Length response header. It only counts the body of the HTTP message, and doesn't include headers and other overhead.
    /// </summary>
    new long? Total { get; }

    /// <summary>
    /// A TouchList of all the Touch objects representing all current points of contact with the surface, regardless of target or changed status.
    /// </summary>
    new TouchPoint[]? Touches { get; }

    /// <summary>
    /// The clockwise rotation of the pointer (e.g. pen stylus) around its major axis in degrees, with a value in the range 0 to 359.
    /// </summary>
    new double? Twist { get; }

    /// <summary>
    /// A nullable string indicating The name identifying the type of the event.
    /// </summary>
    new string? Type { get; }

    /// <summary>
    /// The width (magnitude on the X axis), in CSS pixels, of the contact geometry of the pointer.
    /// </summary>
    new int? Width { get; }

    /// <summary>
    /// Alias for MouseEvent.clientX.
    /// </summary>
    new double? X { get; }

    /// <summary>
    /// Alias for MouseEvent.clientY.
    /// </summary>
    new double? Y { get; }
}

#pragma warning restore IDE1006 // Naming Styles