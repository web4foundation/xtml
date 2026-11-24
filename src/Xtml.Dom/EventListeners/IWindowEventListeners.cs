namespace Xtml.BrowserApis.EventListeners;

public interface IWindowEventListeners
{
    /// <summary>
    /// Fired after the associated document has started printing or the print preview has been closed.
    /// </summary>
    Action<Event>? OnAfterPrint { set; }

    /// <summary>
    /// Fired when the associated document is about to be printed or previewed for printing.
    /// </summary>
    Action<Event>? OnBeforePrint { set; }

    /// <summary>
    /// 
    /// </summary>
    Action<Event.BeforeUnload>? OnBeforeUnload { set; }

    /// <summary>
    /// Fired when the window, the document and its resources are about to be unloaded.
    /// </summary>
    Action<Event.DeviceMotion>? OnDeviceMotion { set; }

    /// <summary>
    /// Fired at a regular interval, indicating the amount of physical force of acceleration the device is receiving and the rate of rotation, if available.
    /// </summary>
    Action<Event.DeviceOrientation>? OnDeviceOrientation { set; }

    /// <summary>
    /// Fired when fresh data is available from the magnetometer orientation sensor about the current orientation of the device as compared to the Earth coordinate frame.
    /// </summary>
    Action<Event.DeviceOrientation>? OnDeviceOrientationAbsolute { set; }

    /// <summary>
    /// Fired when the browser detects that a gamepad has been connected or the first time a button/axis of the gamepad is used.
    /// </summary>
    Action<Event.Gamepad>? OnGamepadConnected { set; }

    /// <summary>
    /// Fired when the browser detects that a gamepad has been disconnected.
    /// </summary>
    Action<Event.Gamepad>? OnGamepadDisconnected { set; }

    /// <summary>
    /// Fired when the fragment identifier of the URL has changed (the part of the URL beginning with and following the # symbol).
    /// </summary>
    Action<Event.HashChange>? OnHashChange { set; }

    /// <summary>
    /// Fired at the global scope object when the user's preferred language changes.
    /// </summary>
    Action<Event>? OnLanguageChange { set; }

    /// <summary>
    /// Fired when the window receives a message, for example from a call to Window.postMessage() from another browsing context.
    /// </summary>
    Action<Event.Message>? OnMessage { set; }

    /// <summary>
    /// Fired when a Window object receives a message that can't be deserialized.
    /// </summary>
    Action<Event.Message>? OnMessageError { set; }

    /// <summary>
    /// Fired when the browser has lost access to the network and the value of navigator.onLine has switched to false.
    /// </summary>
    Action<Event>? OnOffline { set; }

    /// <summary>
    /// Fired when the browser has gained access to the network and the value of navigator.onLine has switched to true.
    /// </summary>
    Action<Event>? OnOnline { set; }

    /// <summary>
    /// Sent when the browser hides the current document while in the process of switching to displaying in its place a different document from the session's history. This happens, for example, when the user clicks the Back button or when they click the Forward button to move ahead in session history.
    /// </summary>
    Action<Event.PageTransition>? OnPageHide { set; }

    /// <summary>
    /// Sent when the browser makes the document visible due to navigation tasks, including not only when the page is first loaded, but also situations such as the user navigating back to the page after having navigated to another within the same tab.
    /// </summary>
    Action<Event.PageTransition>? OnPageShow { set; }

    /// <summary>
    /// Fired when the active history entry changes.
    /// </summary>
    Action<Event.PopState>? OnPopState { set; }

    /// <summary>
    /// Sent every time a JavaScript Promise is rejected, regardless of whether or not there is a handler in place to catch the rejection.
    /// </summary>
    Action<Event.PromiseRejection>? OnRejectionHandled { set; }

    /// <summary>
    /// Fired when the window has been resized.
    /// </summary>
    Action<Event>? OnResize { set; }

    /// <summary>
    /// Fired when a storage area (localStorage or sessionStorage) has been modified in the context of another document.
    /// </summary>
    Action<Event.Storage>? OnStorage { set; }

    /// <summary>
    /// Sent when a JavaScript Promise is rejected but there is no handler in place to catch the rejection.
    /// </summary>
    Action<Event.PromiseRejection>? OnUnhandledRejection { set; }
}